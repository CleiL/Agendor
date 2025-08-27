using Agendor.Application.Interfaces;
using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using static Agendor.Application.Dto.Consultas.ConsultaDto;

namespace Agendor.Application.Services
{
    public class ConsultaService
        (
            IConsultaRepository repository,
            ILogger<ConsultaService> logger,
            IUnitOfWorkFactory uowFactory
        )
        : IConsultaService
    {
        private const int SlotMinutes = 30;
        private static readonly TimeOnly Inicio = new(8, 0);
        private static readonly TimeOnly FimExclusivo = new(18, 0);

        private readonly IConsultaRepository _repository = repository;
        private readonly ILogger<ConsultaService> _logger = logger;
        private readonly IUnitOfWorkFactory _uowFactory = uowFactory;

        public async Task<IEnumerable<AgendaSlotDto>> AgendaDoProfissionalAsync(Guid profissionalId, DateTime dia, CancellationToken ct)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Consulta.AgendaDoProfissionalAsync",
                ["ProfissionalId"] = profissionalId,
                ["Dia"] = dia
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(ct);
                try
                {
                    await uow.BeginAsync(ct);

                    // 1) gera slots [08:00, 18:00) de 30 em 30
                    var slots = GerarSlots(dia);

                    // 2) busca horários já ocupados
                    var ocupados = await _repository.ObterHorariosOcupadosDoProfissional(profissionalId, DateOnly.FromDateTime(dia), ct);
                    var setOcupados = new HashSet<DateTime>(ocupados);

                    // 3) monta a agenda
                    var agenda = new List<AgendaSlotDto>(slots.Count);
                    foreach (var dt in slots)
                        agenda.Add(new AgendaSlotDto { Horario = dt, Disponivel = !setOcupados.Contains(dt) });

                    await uow.CommitAsync(ct);
                    _logger.LogInformation("Agenda gerada com {Qtde} slots para o profissional {ProfissionalId} em {Dia}",
                        agenda.Count, profissionalId, dia.Date);

                    return agenda;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao montar agenda do profissional {ProfissionalId} em {Dia}", profissionalId, dia.Date);
                    await uow.RollbackAsync(ct);
                    throw;
                }
            }
        }

        public async Task<ConsultaResponseDto> AgendarAsync(ConsultaCreateDto dto, CancellationToken ct)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Consulta.AgendarAsync",
                ["MedicoId"] = dto.MedicoId,
                ["PacienteId"] = dto.PacienteId,
                ["DataHora"] = dto.DataHora
            }))
            {

                await using var uow = await _uowFactory.CreateAsync(ct);
                try
                {
                    await uow.BeginAsync(ct);

                    // **NORMALIZAÇÃO AQUI**
                    var data = ToLocalUnspecified(dto.DataHora);

                    // 1) Validações de negócio (usando 'data')
                    ValidarDiaUtil(data);
                    ValidarJanela(data);
                    ValidarMultiploDe30(data);

                    // 2) Conflitos (usando 'data')
                    if (await _repository.ExisteDoProfissionalNoHorario(dto.MedicoId, data, ct))
                        throw new InvalidOperationException("O profissional já possui consulta nesse horário.");

                    if (await _repository.PacienteJaTemNoDiaComProfissional(
                        dto.PacienteId,
                        dto.MedicoId,
                        DateOnly.FromDateTime(data),
                        ct))
                        throw new InvalidOperationException("O paciente já possui consulta com esse profissional neste dia.");

                    // 3) Persistência (gravando 'data' já normalizado)
                    var entity = new Consulta
                    {
                        ConsultaId = Guid.NewGuid(),
                        MedicoId = dto.MedicoId,
                        PacienteId = dto.PacienteId,
                        DataHora = data
                    };

                    await _repository.CreateAsync(entity, ct);
                    await uow.CommitAsync(ct);

                    _logger.LogInformation("Consulta {ConsultaId} agendada com sucesso", entity.ConsultaId);

                    return new ConsultaResponseDto
                    {
                        ConsultaId = entity.ConsultaId,
                        MedicoId = entity.MedicoId,
                        PacienteId = entity.PacienteId,
                        DataHora = entity.DataHora
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao agendar consulta (MedicoId={MedicoId}, PacienteId={PacienteId}, DataHora={DataHora})",
                        dto.MedicoId, dto.PacienteId, dto.DataHora);
                    await uow.RollbackAsync(ct);
                    throw;
                }                              

            }
        }

        private static void ValidarJanela(DateTime dataHora)
        {
            var t = TimeOnly.FromDateTime(dataHora);
            if (t < Inicio || t >= FimExclusivo)
                throw new InvalidOperationException("Horário fora da janela de atendimento (08:00–18:00).");
        }

        private static void ValidarDiaUtil(DateTime dataHora)
        {
            var dow = dataHora.DayOfWeek;
            if (dow is DayOfWeek.Saturday or DayOfWeek.Sunday)
                throw new InvalidOperationException("Agendamentos são permitidos apenas de segunda a sexta.");
        }

        private static void ValidarMultiploDe30(DateTime dataHora)
        {
            if (dataHora.Second != 0 || dataHora.Millisecond != 0 || dataHora.Ticks % TimeSpan.TicksPerMinute != 0)
                throw new InvalidOperationException("O horário deve estar alinhado em minutos exatos.");

            var minutes = dataHora.Minute + dataHora.Hour * 60;
            if (minutes % SlotMinutes != 0)
                throw new InvalidOperationException("Consultas têm duração de 30 minutos; escolha um horário em múltiplos de 30 (ex.: 08:00, 08:30…).");
        }

        private static ReadOnlyCollection<DateTime> GerarSlots(DateTime dia)
        {
            var baseDate = dia.Date;
            var inicioDt = baseDate.AddHours(Inicio.Hour).AddMinutes(Inicio.Minute);
            var fimDt = baseDate.AddHours(FimExclusivo.Hour).AddMinutes(FimExclusivo.Minute);

            var list = new List<DateTime>();
            for (var d = inicioDt; d < fimDt; d = d.AddMinutes(SlotMinutes))
                list.Add(d);

            return list.AsReadOnly();
        }

        private static DateTime ToLocalUnspecified(DateTime dt)
        {
            // Se vier UTC (ex.: 11:00Z), converte para local (ex.: 08:00)…
            if (dt.Kind == DateTimeKind.Utc)
                dt = dt.ToLocalTime();

            // …e fixa como "sem fuso" para todas as comparações no domínio
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        }


        public async Task<IEnumerable<ConsultaResponseDto>> GetConsultasPorProfissionalAsync(Guid medicoId, CancellationToken cancellationToken)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Consulta.GetConsultasPorProfissional",
                ["MedicoId"] = medicoId
            }))
            {
                await using var uow = await _uowFactory.CreateAsync(cancellationToken);
                try
                {
                    await uow.BeginAsync(cancellationToken);

                    var consultas = await _repository.GetAllByMedicoAsync(medicoId, cancellationToken);

                    await uow.CommitAsync(cancellationToken);

                    return consultas.Select(c => new ConsultaResponseDto
                    {
                        ConsultaId = c.ConsultaId,
                        MedicoId = c.MedicoId,
                        PacienteId = c.PacienteId,
                        DataHora = c.DataHora
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao listar consultas do médico {MedicoId}", medicoId);
                    await uow.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }
    }
}
