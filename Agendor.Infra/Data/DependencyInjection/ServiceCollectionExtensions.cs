using Agendor.Application.Interfaces;
using Agendor.Application.Services;
using Agendor.Core.Entities;
using Agendor.Core.Interfaces;
using Agendor.Infra.Data.Context;
using Agendor.Infra.Data.Uow;
using Agendor.Infra.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agendor.Infra.Data.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfraData(this IServiceCollection services, IConfiguration cfg)
        {
            services.Configure<DbOptions>(cfg.GetSection("Database"));

            services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();

            services.AddSingleton<ISchemaInitializer, SchemaInitializer>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IUsuarioService, UsuarioService>();

            services.AddScoped<IPacienteRepository, PacienteRepository>();
            services.AddScoped<IPacienteService, PacienteService>();

            services.AddScoped<IMedicoRepository, MedicoRepository>();
            services.AddScoped<IMedicoService, MedicoService>();

            services.AddScoped<IConsultaRepository, ConsultaRepository>();
            services.AddScoped<IConsultaService, ConsultaService>();

            return services;
        }
    }
}
