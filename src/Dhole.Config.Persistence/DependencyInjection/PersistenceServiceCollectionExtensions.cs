using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using Dhole.Config.Application.Abstractions.Auditing;
using Dhole.Config.Application.Abstractions.Messaging;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Persistence.Auditing;
using Dhole.Config.Persistence.DbContexts;
using Dhole.Config.Persistence.Messaging;
using Dhole.Config.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Config.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodePostgres(configuration);
        services.AddCustomCodePostgresEntityFramework<ServiceDbContext>();

        services.AddScoped<ICatalogGroupRepository, CatalogGroupRepository>();
        services.AddScoped<ICatalogItemRepository, CatalogItemRepository>();

        services.AddScoped<IIntegrationEventOutboxWriter, IntegrationEventOutboxWriter>();
        services.AddScoped<IConfigAuditService, ConfigAuditService>();

        return services;
    }
}
