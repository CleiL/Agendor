using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Agendor.Infra.Repositories
{
    public class UsuarioRepository
        (
            ILogger<UsuarioRepository> logger,
            IUnitOfWork uow
        )
        : BaseRepository(uow), IUsuarioRepository
    {
        private readonly ILogger<UsuarioRepository> _logger = logger;

        public async Task<Usuario> CreateAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.Create",
                ["UsuarioId"] = entity.UsuarioId,
                ["Email"] = entity.Email
            }))
            {
                const string sql = """
                    insert into Usuarios (UsuarioId, Email, PasswordHash, Role)
                    values (@UsuarioId, @Email, @PasswordHash, @Role);
                    """;
                _logger.LogDebug("Executando insert na tabela Usuarios");
                await Conn.ExecuteAsync(new CommandDefinition(sql, entity, Tx, cancellationToken: cancellationToken));
                _logger.LogInformation("Usuário {Email} registrado com sucesso", entity.Email);
                return entity;
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.Delete",
                ["UsuarioId"] = id
            }))
            {
                const string sql = """
                    delete from Usuarios
                     where UsuarioId = @id;
                    """;
                var rows = await Conn.ExecuteAsync(new CommandDefinition(sql, new { id }, Tx, cancellationToken: cancellationToken));
                _logger.LogInformation("Excluídos {rows} registro(s)", rows);
                return rows > 0;
            }
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            const string sql = """
                select UsuarioId, Email, PasswordHash, Role
                  from Usuarios
                  order by Email;
                """;
            _logger.LogDebug("Consultando todos os usuários");
            return await Conn.QueryAsync<Usuario>(new CommandDefinition(sql, Tx, cancellationToken: cancellationToken));
        }

        public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            const string sql = """
                select UsuarioId, Email, PasswordHash, Role
                  from Usuarios
                 where UsuarioId = @id;
                """;
            _logger.LogDebug("Consultando usuário por ID {id}", id);
            return await Conn.QuerySingleOrDefaultAsync<Usuario>(new CommandDefinition(sql, new { id }, Tx, cancellationToken: cancellationToken));
        }

        public async Task<Usuario> UpdateAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
            {
                ["Flow"] = "Usuario.Update",
                ["UsuarioId"] = entity.UsuarioId
            }))
            {
                const string sql = """
                    update Usuarios
                       set Email = @Email,
                           PasswordHash = @PasswordHash,
                           Role = @Role
                     where UsuarioId = @UsuarioId;
                    """;
                var rows = await Conn.ExecuteAsync(new CommandDefinition(sql, entity, Tx, cancellationToken: cancellationToken));
                _logger.LogInformation("Atualizado {rows} registro(s) do usuário {UsuarioId}", rows, entity.UsuarioId);
                return entity;
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            const string sql = """
                select count(1)
                from Usuarios
                where Email = @email AND (@excludeId is null or UsuarioId <> @excludeId);
                """;
            var count = await Conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { email, excludeId }, Tx, cancellationToken: cancellationToken));
            _logger.LogDebug("Verificado existência por Email {email}, excluindo {excludeId}: {exists}", email, excludeId, count > 0);
            return count > 0;
        }

        public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            const string sql = """
                    SELECT UsuarioId, Email, PasswordHash, Role
                    FROM Usuarios
                    WHERE lower(Email) = lower(@Email)
                    LIMIT 1;
                """;
            return await Conn.QuerySingleOrDefaultAsync<Usuario>(
                new CommandDefinition(sql, new { Email = email }, Tx, cancellationToken: ct));
        }
    }
}
