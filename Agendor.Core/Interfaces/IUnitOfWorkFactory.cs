namespace Agendor.Core.Interfaces
{
    public interface IUnitOfWorkFactory
    {
        Task<IUnitOfWork> CreateAsync(CancellationToken cancellation = default);
    }
}
