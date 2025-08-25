using System.Data;

namespace Agendor.Core.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }
        bool IsActive { get; }
        Task BeginAsync(CancellationToken cancellation = default);
        Task CommitAsync(CancellationToken cancellation = default);
        Task RollbackAsync(CancellationToken cancellation = default);
    }
}
