using Agendor.Core.Entities;

namespace Agendor.Core.Interfaces
{
    public interface IConsultaRepository
    {
        Task<bool> ExisteDoProfissionalNoHorario(Guid medicoId, DateTime dataHora, CancellationToken cancellationToken = default);
        Task<bool> PacienteJaTemNoDiaComProfissional(Guid pacienteId, Guid profissionalId, DateOnly dia, CancellationToken cancellationToken = default);
        Task CreateAsync(Consulta entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<DateTime>> ObterHorariosOcupadosDoProfissional(Guid medicoId, DateOnly dia, CancellationToken cancellationToken = default);
    }
}
