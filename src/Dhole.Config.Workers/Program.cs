using CustomCodeFramework.Core.Abstractions;
using Dhole.Config.Application.DependencyInjection;
using Dhole.Config.Infrastructure.Time;
using Dhole.Config.Persistence.DependencyInjection;
using Dhole.Config.Worker.DependencyInjection;
using Dhole.Config.Workers.Security;

var contentRoot = Path.Combine(Directory.GetCurrentDirectory(), "src", "Dhole.Config.Workers");

if (!Directory.Exists(contentRoot))
{
    contentRoot = Directory.GetCurrentDirectory();
}

var builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings { Args = args, ContentRootPath = contentRoot }
);

builder.Configuration.Sources.Clear();

builder
    .Configuration.SetBasePath(contentRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine($"Postgres: {builder.Configuration["Postgres:ConnectionString"]}");

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<ICurrentUser, WorkerCurrentUser>();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddConfigWorker(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
