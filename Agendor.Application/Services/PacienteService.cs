using Agendor.Application.Dto.Pacientes;
using Agendor.Application.Interfaces;
using Agendor.Application.Mapping;
using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Agendor.Application.Services
{
    public class PacienteService
        (
            IPacienteRepository repository,
            ILogger<PacienteService> logger,
            IUnitOfWorkFactory uowFactory
        )
        : IPacienteService
    {
        private readonly IPacienteRepository _repository = repository;
        private readonly ILogger<PacienteService> _logger = logger;
        private readonly IUnitOfWorkFactory _uowFactory = uowFactory;

        public async Task<PacienteDto.PacienteResponseDto> CreateAsync(PacienteDto.PacienteCreateDto entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.Create"
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);

                    if (await _repository.ExistsByCpfAsync(entity.CPF.Trim(), null, cancellationToken))
                        throw new InvalidOperationException("CPF já cadastrado.");

                    if (await _repository.ExistsByEmailAsync(entity.Email.Trim().ToLowerInvariant(), null, cancellationToken))
                        throw new InvalidOperationException("Email já cadastrado.");

                    var paciente = new Paciente
                    {
                        PacienteId = Guid.NewGuid(),
                        Nome = entity.Nome.Trim(),
                        CPF = entity.CPF.Trim(),
                        Email = entity.Email.Trim(),
                        Phone = entity.Phone.Trim()
                    };

                    await _repository.CreateAsync(paciente, cancellationToken);

                    await uow.CommitAsync(cancellationToken);
                    _logger.LogInformation("END criação de paciente {PacienteId}", paciente.PacienteId);
                    return paciente.ToDto();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar paciente");
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.Delete",
                ["PacienteId"] = id
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

        public async Task<IEnumerable<PacienteDto.PacienteResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.GetAll"
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var pacientes = await _repository.GetAllAsync(cancellationToken);
                    await uow.CommitAsync(cancellationToken);
                    _logger.LogInformation("List retornou entidades");
                    return pacientes.Select(p => p.ToDto());
                }
                catch
                {
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<PacienteDto.PacienteResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.GetById",
                ["PacienteId"] = id
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var paciente = await _repository.GetByIdAsync(id, cancellationToken);
                    await uow.CommitAsync(cancellationToken);
                    _logger.LogInformation("GetById({Id}) {Found}", id, paciente is null ? "não encontrado" : "ok");
                    return paciente?.ToDto();
                }
                catch
                {
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<PacienteDto.PacienteResponseDto> UpdateAsync(PacienteDto.PacienteUpdateDto entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.Update",
                ["PacienteId"] = entity.PacienteId
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);

                    var paciente = await _repository.GetByIdAsync(entity.PacienteId, cancellationToken)
                                   ?? throw new KeyNotFoundException("Paciente não encontrado.");

                    static string? NullIfWhite(string? s) => string.IsNullOrWhiteSpace(s) ? null : s!.Trim();
                    static string? OnlyDigitsOrNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : Regex.Replace(s!, @"\D", "");

                    var nomeNovo = NullIfWhite(entity.Nome);
                    var cpfNovo = OnlyDigitsOrNull(entity.CPF);
                    var emailNovo = NullIfWhite(entity.Email)?.ToLowerInvariant();
                    var phoneNovo = NullIfWhite(entity.Phone);

                    if (cpfNovo is not null && !cpfNovo.Equals(paciente.CPF, StringComparison.Ordinal))
                    {
                        if (await _repository.ExistsByCpfAsync(cpfNovo, entity.PacienteId, cancellationToken))
                            throw new InvalidOperationException("CPF já cadastrado.");
                        paciente.CPF = cpfNovo;
                    }

                    if (emailNovo is not null && !emailNovo.Equals(paciente.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        if (await _repository.ExistsByEmailAsync(emailNovo, entity.PacienteId, cancellationToken))
                            throw new InvalidOperationException("Email já cadastrado.");
                        paciente.Email = emailNovo;
                    }

                    if (nomeNovo is not null) paciente.Nome = nomeNovo;
                    if (emailNovo is not null) paciente.Email = emailNovo;
                    if (phoneNovo is not null) paciente.Phone = phoneNovo;

                    await _repository.UpdateAsync(paciente, cancellationToken);

                    await uow.CommitAsync(cancellationToken);
                    return paciente.ToDto();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar paciente {PacienteId}", entity.PacienteId);
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }
    }
}
