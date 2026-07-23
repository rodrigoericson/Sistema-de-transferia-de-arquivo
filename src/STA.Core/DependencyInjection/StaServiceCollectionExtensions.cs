using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using STA.Core.Data;
using STA.Core.Data.Repositories;
using STA.Core.Services;
using STA.Core.Services.Transports;

namespace STA.Core.DependencyInjection;

public static class StaServiceCollectionExtensions
{
    public static IServiceCollection AddStaDatabase(
        this IServiceCollection services,
        string connectionString,
        string? migrationsAssembly = null)
    {
        services.AddDbContext<StaDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(120);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
                if (migrationsAssembly is not null)
                    npgsql.MigrationsAssembly(migrationsAssembly);
            }));
        return services;
    }

    public static IServiceCollection AddStaRepositories(this IServiceCollection services)
    {
        services.AddScoped<IParametroRepository, ParametroRepository>();
        services.AddScoped<ILogRepository, LogRepository>();
        services.AddScoped<IEtapaRepository, EtapaRepository>();
        services.AddScoped<ILogArquivoRepository, LogArquivoRepository>();
        services.AddScoped<ILogSftpRepository, LogSftpRepository>();
        return services;
    }

    public static IServiceCollection AddStaSftpTransports(this IServiceCollection services)
    {
        services.AddSingleton<ICredencialProtector, DpapiCredencialProtector>();
        services.AddSingleton<ISftpClientFactory, SftpClientFactory>();
        services.AddSingleton<ITransportFactory>(sp => new TransportFactory(
            sp.GetRequiredService<ISftpClientFactory>(),
            sp.GetRequiredService<ICredencialProtector>(),
            sp.GetRequiredService<ILoggerFactory>()));
        return services;
    }
}