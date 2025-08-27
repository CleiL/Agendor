using static Agendor.Application.Dto.Consultas.ConsultaDto;

namespace Agendor.Application.Interfaces
{
    public interface IConsultaService
    {
        Task<IEnumerable<AgendaSlotDto>> AgendaDoProfissionalAsync(Guid profissionalId, DateTime dia, CancellationToken cancellationToken);
        Task<ConsultaResponseDto> AgendarAsync(ConsultaCreateDto dto, CancellationToken cancellationToken);
        Task<IEnumerable<ConsultaResponseDto>> GetConsultasPorProfissionalAsync(Guid medicoId, CancellationToken cancellationToken);
    }
}
