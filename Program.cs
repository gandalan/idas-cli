using IdasCli;
using IdasCli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Parse output parameters early to configure output services
        var commonParameters = ParseCommonParameters(args);
        
        // Create service collection first
        var services = new ServiceCollection();
        
        // Add required framework services
        services.AddLogging(logging =>
        {
            // Ensure logs directory exists
            var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);

            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFile(Path.Combine(logDirectory, "idas-cli.log"), append: true);
        });
        
        // Register all IDAS CLI command classes and services
        services.AddIdasCommands(commonParameters);
        
        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("IDAS CLI starting...");

            // Resolve configuration using FirstRunManager
            var configManager = new FirstRunManager(args);
            if (!configManager.TryResolveConfiguration(out var appGuid, out var env))
            {
                logger.LogError("Configuration resolution failed");
                return 1;
            }

            logger.LogInformation("Configuration resolved: AppGuid={AppGuid}, Environment={Environment}", appGuid, env);

            // Set environment variables for this process so commands can read them
            Environment.SetEnvironmentVariable("IDAS_APPGUID", appGuid);
            Environment.SetEnvironmentVariable("IDAS_ENV", env);

            // If called without arguments and no token file exists, auto-start login
            string[] effectiveArgs = args;
            if (args.Length == 0 && !File.Exists("token"))
            {
                logger.LogInformation("No token found, auto-starting SSO login");
                Console.WriteLine("Starting SSO-Login...");
                Console.WriteLine();
                effectiveArgs = new[] { "benutzer", "login" };
            }

            // Build and run the Spectre.Console.Cli app with DI
            var app = SpectreCommandAppFactory.CreateApp(services);
            return await app.RunAsync(effectiveArgs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");
            throw;
        }
    }

    /// <summary>
    /// Creates a host for MCP server mode with the same DI configuration
    /// </summary>
    public static IHost CreateMcpHost()
    {
        // MCP server always uses JSON output with default parameters
        var commonParameters = new OutputParameters("json", null);
        
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddIdasCommands(commonParameters);
                services.AddMcpServer()
                    .WithStdioServerTransport()
                    .WithToolsFromAssembly();
            })
            .ConfigureLogging(logging =>
            {
                var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logDirectory);

                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddFile(Path.Combine(logDirectory, "idas-cli.log"), append: true);
            })
            .Build();
    }

    /// <summary>
    /// Parses the common parameters from command line arguments
    /// </summary>
    private static OutputParameters ParseCommonParameters(string[] args)
    {
        string format = "json";
        string? filename = null;
        
        for (int i = 0; i < args.Length; i++)
        {
            // Parse format
            if (args[i].Equals("--format", StringComparison.OrdinalIgnoreCase) ||
                args[i].Equals("-f", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    format = args[i + 1].ToLowerInvariant();
                }
            }
            else if (args[i].StartsWith("--format=", StringComparison.OrdinalIgnoreCase))
            {
                format = args[i]["--format=".Length..].ToLowerInvariant();
            }
            
            // Parse filename
            if (args[i].Equals("--filename", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    filename = args[i + 1];
                }
            }
            else if (args[i].StartsWith("--filename=", StringComparison.OrdinalIgnoreCase))
            {
                filename = args[i]["--filename=".Length..];
            }
        }
        
        return new OutputParameters(format, filename);
    }
}
