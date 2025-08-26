using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Agendor.Infra.Repositories
{
    public class PacienteRepository
       (
           ILogger<PacienteRepository> logger,
           IUnitOfWork uow
       )
       : BaseRepository(uow), IPacienteRepository
    {
        private readonly ILogger<PacienteRepository> _logger = logger;

        public async Task<Paciente> CreateAsync(Paciente entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.Create",
                ["PacienteId"] = entity.PacienteId
            }))
            {
                const string sql = """
                    insert into Pacientes (PacienteId, Nome, CPF, Email)
                    values (@PacienteId, @Nome, @CPF, @Email)
                    """;
                _logger.LogDebug("Executando inserte na tabela");
                await Conn.ExecuteAsync(new CommandDefinition(sql, entity, Tx, cancellationToken: cancellationToken));
                _logger.LogInformation("Registrado com sucesso");
                return entity;
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.Delete",
                ["PacienteId"] = id
            }))
            {
                const string sql = """
                delete from Pacientes
                where PacienteId = @id
                """;
                var rows = await Conn.ExecuteAsync(new CommandDefinition(sql, new { id }, Tx, cancellationToken: cancellationToken));
                _logger.LogInformation("Excluído {rows} registros", rows);
                return rows > 0;
            }
        }

        public async Task<bool> ExistsByCpfAsync(string cpf, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            const string sql = """
                select count(1)
                from Pacientes
                where CPF = @cpf AND (@excludeId is null or PacienteId <> @excludeId);
                """;
            var count = await Conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { cpf, excludeId }, Tx, cancellationToken: cancellationToken));
            _logger.LogDebug("Verificado existência por CPF {cpf}, excluindo {excludeId}: {exists}", cpf, excludeId, count > 0);
            return count > 0;
        }

        public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            const string sql = """
                select count(1)
                from Pacientes
                where Email = @email AND (@excludeId is null or PacienteId <> @excludeId);
                """;
            var count = await Conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { email, excludeId }, Tx, cancellationToken: cancellationToken));
            _logger.LogDebug("Verificado existência por Email {email}, excluindo {excludeId}: {exists}", email, excludeId, count > 0);
            return count > 0;
        }

        public async Task<IEnumerable<Paciente>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "select PacienteId, Nome, CPF, Email, Phone from Pacientes order by Nome;";
            _logger.LogDebug("Consultando todos os registros");
            return await Conn.QueryAsync<Paciente>(new CommandDefinition(sql, Tx, cancellationToken: cancellationToken));
        }

        public async Task<Paciente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            const string sql = "select PacienteId, Nome, CPF, Email, Phone from Pacientes where PacienteId = @id;";
            _logger.LogDebug("Consultando registro por ID {id}", id);
            return await Conn.QuerySingleOrDefaultAsync<Paciente>(new CommandDefinition(sql, new { id }, Tx, cancellationToken: cancellationToken));
        }

        public async Task<Paciente> UpdateAsync(Paciente entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Paciente.Update",
                ["PacienteId"] = entity.PacienteId
            }))
            {
                const string sql = """
                update Pacientes
                   set Nome = @Nome, CPF = @CPF, Email = @Email, Phone = @Phone
                 where PacienteId = @PacienteId;
                """;
                var rows = await Conn.ExecuteAsync(new CommandDefinition(sql, entity, Tx, cancellationToken: cancellationToken));
                return entity;
            }
        }
    }
}
