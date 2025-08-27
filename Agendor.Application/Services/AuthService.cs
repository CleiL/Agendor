using Agendor.Application.Dto.Auth;
using Agendor.Application.Interfaces;
using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace Agendor.Application.Services
{
    public class AuthService
        (
            IUsuarioRepository usuarioRepository,
            IPacienteRepository pacienteRepository,
            IMedicoRepository medicoRepository,
            IUnitOfWorkFactory uowFactory,
            IOptions<JwtOptions> jwtOptions,
            ILogger<AuthService> logger
        )
        : IAuthService
    {
        private readonly IUsuarioRepository _usuarios = usuarioRepository;
        private readonly IPacienteRepository _pacientes = pacienteRepository;
        private readonly IMedicoRepository _medico = medicoRepository;
        private readonly IUnitOfWorkFactory _uowFactory = uowFactory;
        private readonly JwtOptions _jwt = jwtOptions.Value;
        private readonly ILogger<AuthService> _log = logger;
        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto dto, CancellationToken cancellationToken = default)
        {
            using (_log.BeginScope(new Dictionary<string, object?> { ["Flow"] = "Auth.Login", ["Email"] = dto.Email }))
            {
                var email = NormalizeEmail(dto.Email!);

                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                await uow.BeginAsync(cancellationToken);

                // AGORA usamos o método certo:
                var user = await _usuarios.GetByEmailAsync(email, cancellationToken);

                if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password!, user.PasswordHash))
                {
                    await uow.RollbackAsync(cancellationToken);
                    _log.LogInformation("Login falhou para {Email}", email);
                    throw new UnauthorizedAccessException("Credenciais inválidas.");
                }

                var token = GenerateJwt(user);

                await uow.CommitAsync(cancellationToken);
                _log.LogInformation("Login bem-sucedido para {Email}", email);

                return new LoginResponseDto { Token = token, Nome = user.Email};
            }
        }

        public Task ConfirmRegisterAsync(RegisterDto.RegisterResponseDto dto)
        {
            _log.LogInformation("ConfirmRegister: {Nome}", dto.Nome);
            return Task.CompletedTask;
        }

        public async Task<bool> RegisterMedicoAsync(RegisterDto.RegisterMedicoDto dto)
        {
            using (_log.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Auth.RegisterMedico",
                ["Email"] = dto.Email,
                ["CRM"] = dto.CRM
            }))
            {
                await using var uow = await _uowFactory.CreateAsync();
                try
                {
                    await uow.BeginAsync();

                    var email = NormalizeEmail(dto.Email!);

                    // checa se já existe usuário com esse e-mail
                    var existingUsers = await _usuarios.GetAllAsync();
                    if (existingUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("Email já cadastrado para login.");

                    var medico = new Medico
                    {
                        MedicoId = Guid.NewGuid(),
                        Nome = dto.Nome!.Trim(),
                        Email = email,
                        Especialidade = dto.Especialidade!,
                        CRM = dto.CRM!
                    };

                    await _medico.CreateAsync(medico);

                    var user = new Usuario
                    {
                        UsuarioId = Guid.NewGuid(),
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password!),
                        Role = "Medico"
                    };
                    await _usuarios.CreateAsync(user);

                    await uow.CommitAsync();
                    _log.LogInformation("Usuário médico {UsuarioId} registrado", user.UsuarioId);
                    return true;
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Erro no registro de médico");
                    await uow.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> RegisterPacienteAsync(RegisterDto.RegisterPacienteDto dto)
        {
            using (_log.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Auth.RegisterPaciente",
                ["Email"] = dto.Email,
                ["CPF"] = dto.CPF
            }))
            {
                await using var uow = await _uowFactory.CreateAsync();
                try
                {
                    await uow.BeginAsync();

                    // unicidades
                    var email = NormalizeEmail(dto.Email);
                    if (await _pacientes.ExistsByEmailAsync(email, null))
                        throw new InvalidOperationException("Email já cadastrado para paciente.");
                    if (await _pacientes.ExistsByCpfAsync(dto.CPF.Trim(), null))
                        throw new InvalidOperationException("CPF já cadastrado para paciente.");

                    // evita e-mail duplicado em USUÁRIOS
                    var existingUsers = await _usuarios.GetAllAsync();
                    if (existingUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("Email já cadastrado para login.");

                    // cria Paciente
                    var paciente = new Agendor.Core.Entities.Paciente
                    {
                        PacienteId = Guid.NewGuid(),
                        Nome = dto.Nome.Trim(),
                        CPF = dto.CPF.Trim(),
                        Email = email
                    };
                    await _pacientes.CreateAsync(paciente);

                    // cria Usuario (role: Paciente)
                    var user = new Usuario
                    {
                        UsuarioId = Guid.NewGuid(),
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password!),
                        Role = "Paciente"
                    };
                    await _usuarios.CreateAsync(user);

                    await uow.CommitAsync();
                    _log.LogInformation("Paciente {PacienteId} e Usuario {UsuarioId} registrados", paciente.PacienteId, user.UsuarioId);
                    return true;
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Erro no registro de paciente");
                    await uow.RollbackAsync();
                    throw;
                }
            }
        }

        private static string NormalizeEmail(string email)
        => (email ?? throw new ArgumentNullException(nameof(email))).Trim().ToLowerInvariant();

        private string GenerateJwt(Usuario user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.UsuarioId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role) // "Paciente" | "Medico"
    };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwt.ExpireHours <= 0 ? 4 : _jwt.ExpireHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
