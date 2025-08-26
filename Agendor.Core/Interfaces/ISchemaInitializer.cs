namespace Agendor.Core.Interfaces
{
    public interface ISchemaInitializer
    {
        Task EnsureCreatedAsync(CancellationToken cancellationToken = default);
    }
}
