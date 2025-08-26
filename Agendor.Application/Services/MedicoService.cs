using Agendor.Application.Dto.Medicos;
using Agendor.Application.Interfaces;
using Agendor.Application.Mapping;
using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Agendor.Application.Services
{
    public class MedicoService
        (
            IMedicoRepository repository,
            ILogger<MedicoService> logger,
            IUnitOfWorkFactory uowFactory
        )
        : IMedicoService
    {
        private readonly IMedicoRepository _repository = repository;
        private readonly ILogger<MedicoService> _logger = logger;
        private readonly IUnitOfWorkFactory _uowFactory = uowFactory;

        public async Task<MedicosDto.MedicoResponseDto> CreateAsync(MedicosDto.MedicoCreateDto entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.Create"
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);

                    var medico = new Medico
                    {
                        MedicoId = Guid.NewGuid(),
                        Nome = entity.Nome?.Trim() ?? string.Empty,
                        CRM = entity.CRM?.Trim() ?? string.Empty,
                        Email = entity.Email?.Trim() ?? string.Empty,
                        Especialidade = entity.Especialidade?.Trim() ?? string.Empty
                    };
                    await _repository.CreateAsync(medico, cancellationToken);
                    await uow.CommitAsync(cancellationToken);
                    _logger.LogInformation("END criação de médico {MedicoId}", medico.MedicoId);
                    return medico.ToDto();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar médico");
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.Delete",
                ["MedicoId"] = id
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var result = await _repository.DeleteAsync(id, cancellationToken);
                    await uow.CommitAsync(cancellationToken);
                    _logger.LogInformation("Excluído médico {MedicoId}", id);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao excluir médico {MedicoId}", id);
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<IEnumerable<MedicosDto.MedicoResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.GetAll"
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var medicos = await _repository.GetAllAsync(cancellationToken);
                    await uow.CommitAsync(cancellationToken);
                    return medicos.Select(m => m.ToDto());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter médicos");
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<MedicosDto.MedicoResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.GetById",
                ["MedicoId"] = id
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);
                    var medico = await _repository.GetByIdAsync(id, cancellationToken);
                    await uow.CommitAsync(cancellationToken);
                    if (medico == null)
                    {
                        _logger.LogWarning("Médico {MedicoId} não encontrado", id);
                        return null;
                    }
                    return medico.ToDto();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter médico {MedicoId}", id);
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<MedicosDto.MedicoResponseDto> UpdateAsync(MedicosDto.MedicoUpdateDto entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.Update",
                ["MedicoId"] = entity.MedicoId
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);

                    var medico = await _repository.GetByIdAsync(entity.MedicoId, cancellationToken)
                        ?? throw new KeyNotFoundException("Medico não encontrado");

                    static string? NullIfWhite(string? s) => string.IsNullOrWhiteSpace(s) ? null : s!.Trim();
                    static string? OnlyCrmOrNull(string? s)
                    {
                        if (string.IsNullOrWhiteSpace(s))
                            return null;

                        var pattern = @"^CRM/[A-Z]{2}\s\d{1,6}$";
                        return Regex.IsMatch(s.Trim(), pattern, RegexOptions.IgnoreCase)
                            ? s.Trim().ToUpperInvariant()
                            : null;
                    }


                    var nomeNovo = NullIfWhite(entity.Nome);
                    var crmNovo = OnlyCrmOrNull(entity.CRM);
                    var emailNovo = NullIfWhite(entity.Email)?.ToLowerInvariant();
                    var phoneNovo = NullIfWhite(entity.Phone);
                    var especialidadeNovo = NullIfWhite(entity.Especialidade);

                    if (crmNovo is not null && !crmNovo.Equals(medico?.CRM, StringComparison.OrdinalIgnoreCase))
                    {
                        var crmExists = await _repository.ExistsByCrmAsync(crmNovo, entity.MedicoId, cancellationToken);
                        if (crmExists)
                            throw new InvalidOperationException("CRM já cadastrado.");
                    }

                    if (emailNovo is not null && !emailNovo.Equals(medico?.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        if (await _repository.ExistsByEmailAsync(emailNovo, entity.MedicoId, cancellationToken))
                            throw new InvalidOperationException("Email já cadastrado.");

                        if (medico is not null && emailNovo is not null && !emailNovo.Equals(medico.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            if (await _repository.ExistsByEmailAsync(emailNovo, entity.MedicoId, cancellationToken))
                                throw new InvalidOperationException("Email já cadastrado.");
                            medico.Email = emailNovo;
                        }

                    }

                    if (nomeNovo is not null && medico is not null) medico.Nome = nomeNovo;
                    if (crmNovo is not null && medico is not null) medico.CRM = crmNovo;
                    if (emailNovo is not null && medico is not null) medico.Email = emailNovo;
                    if (phoneNovo is not null && medico is not null) medico.Phone = phoneNovo;
                    if (especialidadeNovo is not null && medico is not null) medico.Especialidade = especialidadeNovo;

                    if (medico is null)
                        throw new KeyNotFoundException("Medico não encontrado para atualização.");

                    _ = await _repository.UpdateAsync(medico, cancellationToken);

                    await uow.CommitAsync(cancellationToken);
                    _logger.LogInformation("Atualizado médico {MedicoId}", entity.MedicoId);
                    return medico.ToDto();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar médico {MedicoId}", entity.MedicoId);
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }
    }
}
