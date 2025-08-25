using Agendor.Infra.Data.Dapper;
using Agendor.Infra.Data.DependencyInjection;
using Agendor.Infra.Data.Services;
using Agendor.Server.Infra;
using Dapper;
using Serilog;
using Serilog.Events;

// Bootstrap do Serilog (console + arquivo)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .WriteTo.File("logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true));

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen();

    builder.Services.AddInfraData(builder.Configuration);

    builder.Services.AddJwtAuthentication(builder.Configuration);

    SqlMapper.AddTypeHandler(new SqliteGuidTextHandler());

    var app = builder.Build();

    {
        var cs = builder.Configuration["Database:ConnectionString"] ?? "Data Source=./data/agenda.db";
        const string pfx = "Data Source=";
        var dbPath = cs.StartsWith(pfx, StringComparison.OrdinalIgnoreCase) ? cs[pfx.Length..].Trim() : cs;
        var dir = Path.GetDirectoryName(Path.GetFullPath(dbPath));
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    // Handler global de erro (ProblemDetails) + log
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async ctx =>
        {
            var feat = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            var ex = feat?.Error;

            Log.Error(ex, "Unhandled exception em {Path}", ctx.Request.Path);

            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/500",
                title = "Erro interno",
                status = 500,
                traceId = ctx.TraceIdentifier
            });
        });
    });

    // CorrelationId + log automático de requisições
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (http, elapsed, ex) =>
            ex != null ? LogEventLevel.Error :
            http.Response.StatusCode >= 500 ? LogEventLevel.Error :
            http.Response.StatusCode >= 400 ? LogEventLevel.Warning :
            LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diag, ctx) =>
        {
            diag.Set("RequestHost", ctx.Request.Host);
            diag.Set("RequestScheme", ctx.Request.Scheme);
            diag.Set("UserAgent", ctx.Request.Headers["User-Agent"].ToString());
            diag.Set("CorrelationId", ctx.Response.Headers[CorrelationIdMiddleware.Header].ToString());
        };
    });

    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.MapFallbackToFile("/index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}