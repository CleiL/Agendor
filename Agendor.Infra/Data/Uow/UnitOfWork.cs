using Agendor.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Agendor.Infra.Data.Uow
{
    public sealed class UnitOfWork
        (
            IDbConnectionFactory factory,
            ILogger<UnitOfWork> logger
        ) : IUnitOfWork
    {
        private readonly IDbConnectionFactory _factory = factory;
        private readonly ILogger<UnitOfWork> _logger = logger;

        private IDbConnection? _conn;
        private IDbTransaction? _tx;
        private bool _disposed;

        public IDbConnection Connection => _conn ??
            throw new InvalidOperationException("UoW não iniciado. Chame BeginAsync antes de usar a Connection.");

        public IDbTransaction? Transaction => _tx;
        public bool IsActive => _tx is not null;

        public async Task BeginAsync(CancellationToken cancellationToken = default)
        {
            if (_conn is null)
                _conn = await _factory.Create(cancellationToken); // usa SUA fábrica (já abre e aplica PRAGMAs)

            if (_tx is null)
            {
                _logger.LogDebug("UoW Begin");
                _tx = _conn.BeginTransaction();
            }
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_tx is null) return;
            _logger.LogDebug("UoW Commit");
            _tx.Commit();
            _tx.Dispose();
            _tx = null;
            await Task.CompletedTask;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_tx is null) return;
            _logger.LogWarning("UoW Rollback");
            _tx.Rollback();
            _tx.Dispose();
            _tx = null;
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try { _tx?.Dispose(); } catch { }
            try
            {
                if (_conn?.State is not ConnectionState.Closed) _conn?.Close();
                _conn?.Dispose();
            }
            catch { }
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
