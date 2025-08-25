using static Agendor.Application.Dto.Usuarios.UsuarioDto;

namespace Agendor.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioResponseDto> CreateAsync(UsuarioCreateDto entity, CancellationToken cancellationToken = default);
        Task<UsuarioResponseDto> UpdateAsync(UsuarioUpdateDto entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UsuarioResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<UsuarioResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
