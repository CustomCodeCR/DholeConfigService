using CustomCodeFramework.Messaging.DependencyInjection;
using CustomCodeFramework.Messaging.Outbox.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Redis.Streams.DependencyInjection;
using CustomCodeFramework.Workers.DependencyInjection;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Infrastructure.Cache;
using Dhole.Config.Worker.Outbox;
using Dhole.Config.Worker.Streams;
using Dhole.Config.Worker.Workers;

namespace Dhole.Config.Worker.DependencyInjection;

public static class WorkerServiceCollectionExtensions
{
    public static IServiceCollection AddConfigWorker(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeRedisStreams(configuration);

        services.AddScoped<IConfigCacheService, ConfigCacheService>();

        services.AddCustomCodeMessaging(configuration);
        services.AddCustomCodeMessagingOutbox(configuration);

        services.AddCustomCodeOutboxProcessor<OutboxProcessor>();
        services.AddCustomCodeInboxProcessor<InboxProcessor>();
        services.AddCustomCodeMessagingOutboxHostedServices();

        services.AddCustomCodeRedisStreamConsumerBackgroundService();

        services.AddCustomCodeRedisStreamHandler<CatalogGroupCreatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogGroupUpdatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogGroupDeletedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogGroupActivatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogGroupInactivatedStreamHandler>();

        services.AddCustomCodeRedisStreamHandler<CatalogItemCreatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogItemUpdatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogItemDeletedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogItemActivatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogItemInactivatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<CatalogItemSortOrderChangedStreamHandler>();

        services.AddCustomCodeWorkers(configuration);
        services.AddCustomCodePeriodicWorker<ConfigCacheWarmupWorker>();

        return services;
    }
}
