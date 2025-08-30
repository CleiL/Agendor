using Agendor.Core.Entities;

namespace Agendor.Core.Interfaces
{
    public interface IPacienteRepository
        : IBaseRepository<Paciente>
    {
        Task<bool> ExistsByCpfAsync(string cpf, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<Guid?> GetIdByEmailAsync(string email, CancellationToken cancellationToken = default);

    }
}
