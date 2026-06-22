using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;
using Dhole.Config.Application.Abstractions.Codes;
using Dhole.Config.Application.Abstractions.Slugs;
using Dhole.Config.Application.Codes;
using Dhole.Config.Application.Slugs;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Config.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCustomCodeValidation(AssemblyReference.Assembly);

        services.AddCustomCodeCqrs(AssemblyReference.Assembly);
        services.AddCustomCodeCqrsBehaviors();

        services.AddScoped<ICodeGenerator, CodeGenerator>();
        services.AddScoped<ISlugGenerator, SlugGenerator>();

        return services;
    }
}
