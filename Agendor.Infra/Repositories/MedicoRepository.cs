using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;


namespace Agendor.Infra.Repositories
{
    public class MedicoRepository
       (
           ILogger<MedicoRepository> logger,
           IUnitOfWork uow
       )
       : BaseRepository(uow), IMedicoRepository
    {
        private readonly ILogger<MedicoRepository> _logger = logger;

        public async Task<Medico> CreateAsync(Medico entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.Create",
                ["MedicoId"] = entity.MedicoId
            }))
            {
                const string sql = """
                    insert into Medicos (MedicoId, Nome, CRM, Email, Phone, Especialidade)
                    values (@MedicoId, @Nome, @CRM, @Email, @Phone, @Especialidade)
                    """;
                _logger.LogDebug("Executando inserte na tabela");
                await Conn.ExecuteAsync(new CommandDefinition(sql, entity, Tx, cancellationToken: cancellationToken));
                _logger.LogInformation("Registrado com sucesso");
                return entity;
            }
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.Delete",
                ["MedicoId"] = id
            }))
            {
                const string sql = """
                delete from Medicos
                where MedicoId = @id
                """;
                var rows = Conn.ExecuteAsync(new CommandDefinition(sql, new { id }, Tx, cancellationToken: cancellationToken));
                _logger.LogInformation("Excluído {rows} registros", rows);
                return rows.ContinueWith(t => t.Result > 0, cancellationToken);
            }
        }

        public async Task<bool> ExistsByCrmAsync(string crm, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            const string sql = """
                select count(1)
                from Medicos
                where CRM = @crm AND (@excludeId is null or MedicoId <> @excludeId);
                """;
            var count = await Conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { crm, excludeId }, Tx, cancellationToken: cancellationToken));
            _logger.LogDebug("Verificado existência por CRM {crm}, excluindo {excludeId}: {exists}", crm, excludeId, count > 0);
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

        public Task<IEnumerable<Medico>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.GetAll"
            }))
            {
                const string sql = """
                    select MedicoId, Nome, CRM, Email, Phone, Especialidade
                    from Medicos
                    """;
                _logger.LogDebug("Consultando todos os registros");
                return Conn.QueryAsync<Medico>(new CommandDefinition(sql, transaction: Tx, cancellationToken: cancellationToken));
            }
        }

        public Task<Medico?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.GetById",
                ["MedicoId"] = id
            }))
            {
                const string sql = """
                    select MedicoId, Nome, CRM, Email, Phone, Especialidade
                    from Medicos
                    where MedicoId = @id
                    """;
                _logger.LogDebug("Consultando por ID");
                return Conn.QuerySingleOrDefaultAsync<Medico>(new CommandDefinition(sql, new { id }, Tx, cancellationToken: cancellationToken));
            }
        }

        public Task<Medico> UpdateAsync(Medico entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Medico.Update",
                ["MedicoId"] = entity.MedicoId
            }))
            {
                const string sql = """
                    update Medicos
                    set Nome = @Nome,
                        CRM = @CRM,
                        Email = @Email,
                        Phone = @Phone,
                        Especialidade = @Especialidade
                    where MedicoId = @MedicoId
                    """;
                _logger.LogDebug("Executando update na tabela");
                return Conn.ExecuteAsync(new CommandDefinition(sql, entity, Tx, cancellationToken: cancellationToken))
                    .ContinueWith(t =>
                    {
                        _logger.LogInformation("Atualizado com sucesso");
                        return entity;
                    }, cancellationToken);
            }
        }
    }
}
