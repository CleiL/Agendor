using Agendor.Application.Dto.Usuarios;
using Agendor.Application.Interfaces;
using Agendor.Application.Mapping;
using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using static Agendor.Application.Dto.Usuarios.UsuarioDto;

namespace Agendor.Application.Services
{
    public class UsuarioService
        (
            IUsuarioRepository repository,
            ILogger<UsuarioService> logger,
            IUnitOfWorkFactory uowFactory
        )
        : IUsuarioService
    {
        private readonly IUsuarioRepository _repository = repository;
        private readonly ILogger<UsuarioService> _logger = logger;
        private readonly IUnitOfWorkFactory _uowFactory = uowFactory;

        public async Task<UsuarioResponseDto> CreateAsync(UsuarioDto.UsuarioCreateDto entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.Create",
                ["Email"] = entity.Email
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);

                    var email = NormalizeEmail(entity.Email);
                    if (await EmailJaCadastradoAsync(email, null, cancellationToken))
                        throw new InvalidOperationException("Email já cadastrado.");

                    // Hash da senha (nunca salve plain text)
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(entity.Password);

                    var user = new Usuario
                    {
                        UsuarioId = Guid.NewGuid(),
                        Email = email,
                        PasswordHash = passwordHash,
                        Role = string.IsNullOrWhiteSpace(entity.Role) ? "User" : entity.Role.Trim()
                    };

                    await _repository.CreateAsync(user, cancellationToken);

                    await uow.CommitAsync(cancellationToken);
                    _logger.LogInformation("END criação de usuário {UsuarioId}", user.UsuarioId);

                    return user.ToDto();
       
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar usuário");
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.Delete",
                ["UsuarioId"] = id
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var ok = await _repository.DeleteAsync(id, cancellationToken);
                    await uow.CommitAsync(cancellationToken);
                    return ok;
                }
                catch
                {
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<IEnumerable<UsuarioDto.UsuarioResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.GetAll"
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var users = await _repository.GetAllAsync(cancellationToken);
                    await uow.CommitAsync(cancellationToken);

                    return users.Select(u => new UsuarioResponseDto
                    {
                        Email = u.Email,
                        Role = u.Role
                    });
                }
                catch
                {
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<UsuarioDto.UsuarioResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.GetById",
                ["UsuarioId"] = id
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var user = await _repository.GetByIdAsync(id, cancellationToken);
                    await uow.CommitAsync(cancellationToken);

                    return user.ToDto();
                }
                catch
                {
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<UsuarioDto.UsuarioResponseDto> UpdateAsync(UsuarioDto.UsuarioUpdateDto entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.Update",
                ["UsuarioId"] = entity.UsuarioId
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);

                    var user = await _repository.GetByIdAsync(entity.UsuarioId, cancellationToken)
                               ?? throw new KeyNotFoundException("Usuário não encontrado.");

                    static string? NullIfWhite(string? s) => string.IsNullOrWhiteSpace(s) ? null : s!.Trim();

                    // campos opcionais no update
                    var emailNovo = NullIfWhite(entity.Email)?.ToLowerInvariant();
                    var roleNova = NullIfWhite(entity.Role);
                    var senhaNova = NullIfWhite(entity.Password);

                    if (emailNovo is not null && !emailNovo.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        if (await EmailJaCadastradoAsync(emailNovo, entity.UsuarioId, cancellationToken))
                            throw new InvalidOperationException("Email já cadastrado.");
                        user.Email = emailNovo;
                    }

                    if (roleNova is not null) user.Role = roleNova;

                    if (senhaNova is not null)
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(senhaNova);
                    }

                    await _repository.UpdateAsync(user, cancellationToken);

                    await uow.CommitAsync(cancellationToken);

                    return user.ToDto();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar usuário {UsuarioId}", entity.UsuarioId);
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        private static string NormalizeEmail(string email)
            => (email ?? throw new ArgumentNullException(nameof(email))).Trim().ToLowerInvariant();

        private async Task<bool> EmailJaCadastradoAsync(string email, Guid? excludeId, CancellationToken ct)
        {
            var all = await _repository.GetAllAsync(ct);
            return all.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                (excludeId is null || u.UsuarioId != excludeId.Value));
        }
    }
}
