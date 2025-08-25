using static Agendor.Application.Dto.Medicos.MedicosDto;

namespace Agendor.Application.Interfaces
{
    public interface IMedicoService
    {
        Task<MedicoResponseDto> CreateAsync(MedicoCreateDto entity, CancellationToken cancellationToken = default);
        Task<MedicoResponseDto> UpdateAsync(MedicoUpdateDto entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<MedicoResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<MedicoResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
