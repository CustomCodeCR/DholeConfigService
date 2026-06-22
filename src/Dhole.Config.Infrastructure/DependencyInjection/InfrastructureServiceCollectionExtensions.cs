using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Mongo.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Mongo;
using Dhole.Config.Infrastructure.Cache;
using Dhole.Config.Infrastructure.Mongo;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Config.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodeAuth(configuration);

        services.PostConfigure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeMongo(configuration);

        services.AddScoped<IConfigCacheService, ConfigCacheService>();
        services.AddScoped<IConfigChangeSnapshotWriter, ConfigChangeSnapshotWriter>();

        return services;
    }
}
