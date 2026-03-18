using IdasCli.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdasCli;

/// <summary>
/// Extension methods for registering IDAS CLI command services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all IDAS CLI services including commands and support services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="commonParameters">The common parameters containing format and filename options</param>
    public static IServiceCollection AddIdasCommands(this IServiceCollection services, OutputParameters commonParameters)
    {
        // Register support services
        services.AddTransient<IIdasAuthService, IdasAuthService>();

        // Register CommonParameters as singleton so output services can access it
        services.AddSingleton(commonParameters);

        // Register only the relevant output service based on the format parameter
        if (commonParameters.Format.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            services.AddTransient<IOutputService, CsvOutputService>();
        }
        else
        {
            services.AddTransient<IOutputService, JsonOutputService>();
        }

        // Command classes are no longer registered here - they are instantiated directly by Spectre.Console.Cli
        // via constructor injection of IIdasAuthService, IOutputService, and ILogger<T>

        return services;
    }
}
