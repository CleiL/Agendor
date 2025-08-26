using Agendor.Core.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Agendor.Infra.Repositories
{
    public sealed class SchemaInitializer(
        IDbConnectionFactory factory,
        ILogger<SchemaInitializer> logger
    ) : ISchemaInitializer
    {
        private readonly IDbConnectionFactory _factory = factory;
        private readonly ILogger<SchemaInitializer> _logger = logger;

        public async Task EnsureCreatedAsync(CancellationToken ct = default)
        {
            // a factory já abre a conexão
            using var conn = await _factory.Create(ct);     // IDbConnection
            using var tx = conn.BeginTransaction();         // IDbTransaction

            try
            {
                var sql = ReadEmbedded("Agendor.Infra.Data.Sql.schema.sql"); // ajuste o resource name
                await conn.ExecuteAsync(new CommandDefinition(sql, transaction: tx, cancellationToken: ct));

                tx.Commit(); // síncrono, porque estamos em IDbTransaction
                _logger.LogInformation("Schema SQLite aplicado com sucesso.");
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { /* noop */ }
                _logger.LogError(ex, "Falha ao aplicar o schema SQLite.");
                throw;
            }
        }

        private static string ReadEmbedded(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var s = asm.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Recurso não encontrado: {resourceName}");
            using var r = new StreamReader(s);
            return r.ReadToEnd();
        }
    }
}
