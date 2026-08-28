using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Config.Api.Endpoints;
using Dhole.Config.Api.Grpc;
using Dhole.Config.Api.Middleware;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.DependencyInjection;
using Dhole.Config.Infrastructure.DependencyInjection;
using Dhole.Config.Infrastructure.Time;
using Dhole.Config.Persistence.DbContexts;
using Dhole.Config.Persistence.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "DholeWebCors";

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

builder.Services.AddCustomCodeApiWithSwagger(title: "Dhole Config Service", version: "v1");

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        CorsPolicyName,
        policy =>
        {
            policy
                .WithOrigins(
                    "https://dhole.customcodecr.com",
                    "https://sistema.logisticacastrofallas.com",
                    "https://logisticacastrofallas.com",
                    "https://www.logisticacastrofallas.com",
                    "http://localhost:5173",
                    "http://127.0.0.1:5173",
                    "http://192.168.1.193:5173",
                    "http://192.168.1.12:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddGrpc();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCustomCodeApi();

app.UseCors(CorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.MapGet(
        "/health",
        () =>
        {
            return Results.Ok(
                new
                {
                    service = "DholeConfigService",
                    status = "Healthy",
                    timestamp = DateTimeOffset.UtcNow,
                }
            );
        }
    )
    .AllowAnonymous();

app.UseAuthentication();
app.UseMiddleware<AuditExecutionContextMiddleware>();
app.UseAuthorization();
app.UseMiddleware<AuditEndpointMiddleware>();

app.MapGrpcService<ConfigCatalogGrpcService>();

app.MapCatalogGroupEndpoints();
app.MapCatalogItemEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await dbContext.Database.MigrateAsync();

    // Some catalog migrations seed reference data directly in PostgreSQL. Clear the
    // Redis-backed catalog caches after migrations so newly seeded WHS/items are
    // visible immediately to Pricing instead of serving a stale empty select list.
    var cache = scope.ServiceProvider.GetRequiredService<IConfigCacheService>();
    foreach (var slug in new[]
             {
                 "pricing-warehouses",
                 "pricing-clients",
                 "pricing-sales-executives",
                 "country-vat-rates",
                 "pol",
                 "poe",
             })
    {
        await cache.RemoveCatalogGroupCacheAsync(slug);
    }
}

app.Run();
