using Agendor.Core.Entities;

namespace Agendor.Core.Interfaces
{
    public interface IMedicoRepository
        : IBaseRepository<Medico>
    {
        Task<bool> ExistsByCrmAsync(string crm, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);        
    }
}
