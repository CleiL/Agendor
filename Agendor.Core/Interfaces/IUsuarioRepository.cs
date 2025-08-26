using Agendor.Core.Entities;

namespace Agendor.Core.Interfaces
{
    public interface IUsuarioRepository
        : IBaseRepository<Usuario>
    {
        Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    }
}
