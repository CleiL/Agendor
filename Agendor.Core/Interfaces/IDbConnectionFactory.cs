using System.Data;

namespace Agendor.Core.Interfaces
{
    public interface IDbConnectionFactory
    {
        Task<IDbConnection> Create(CancellationToken cancellationToken = default);
    }
}
