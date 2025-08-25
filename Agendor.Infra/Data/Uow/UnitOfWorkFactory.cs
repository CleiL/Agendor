using Agendor.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agendor.Infra.Data.Uow
{
    public sealed class UnitOfWorkFactory
        (
            IServiceProvider sp,
            ILogger<UnitOfWorkFactory> logger
        ) : IUnitOfWorkFactory
    {
        private readonly IServiceProvider _sp = sp;
        private readonly ILogger<UnitOfWorkFactory> _logger = logger;

        public Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken = default)
        {
            var uow = _sp.GetRequiredService<IUnitOfWork>();
            _logger.LogDebug("UoW criado via Factory");
            return Task.FromResult(uow);
        }
    }
}
