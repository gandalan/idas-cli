using IdasCli;
using IdasCli.Commands;
using IdasCli.Sidecars;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;

public class Program
{
    private static readonly string[] BuiltInCommands = new[]
    {
        "benutzer", "vorgang", "artikel", "av", "kontakt", "serie", "rollen",
        "variante", "uidefinition", "konfigsatz", "werteliste", "lagerbestand",
        "lagerbuchung", "gsql", "berechtigung", "warengruppe", "beleg", "mcp",
        "sidecar"
    };

    public static async Task<int> Main(string[] args)
    {
        var exeDir = AppContext.BaseDirectory;
        var workDir = Directory.GetCurrentDirectory();

        // Resolve configuration using FirstRunManager
        var configManager = new FirstRunManager(args);
        if (!configManager.TryResolveConfiguration(out var appGuid, out var env))
        {
            return 1;
        }

        // Set environment variables for this process so commands can read them
        Environment.SetEnvironmentVariable("IDAS_APPGUID", appGuid);
        Environment.SetEnvironmentVariable("IDAS_ENV", env);

        // If called without arguments and no token file exists, auto-start login
        string[] effectiveArgs = args;
        if (args.Length == 0 && !File.Exists("token"))
        {
            Console.WriteLine("Starting SSO-Login...");
            Console.WriteLine();
            effectiveArgs = new[] { "benutzer", "login" };
        }

        // Check if first argument is a sidecar command
        var sidecar = TryResolveSidecar(effectiveArgs);
        if (sidecar != null)
        {
            return await SidecarExecutor.ExecuteAsync(sidecar, effectiveArgs.Skip(1).ToArray());
        }

        // Not a sidecar - run the Spectre.Console.Cli app
        return await RunSpectreAppAsync(effectiveArgs);
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

    private static SidecarDescriptor? TryResolveSidecar(string[] commandLineArgs)
    {
        if (commandLineArgs.Length == 0)
        {
            return null;
        }

        var firstCommand = GetFirstCommandToken(commandLineArgs);
        if (firstCommand == null)
        {
            return null;
        }

        if (BuiltInCommands.Contains(firstCommand, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        return SidecarRegistry.Find(firstCommand, BuiltInCommands);
    }

    private static string? GetFirstCommandToken(string[] commandLineArgs)
    {
        for (var index = 0; index < commandLineArgs.Length; index++)
        {
            var current = commandLineArgs[index];
            if (string.IsNullOrWhiteSpace(current))
            {
                continue;
            }

            if (current.Equals("--appguid", StringComparison.OrdinalIgnoreCase)
                || current.Equals("--env", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                continue;
            }

            if (current.StartsWith("--appguid=", StringComparison.OrdinalIgnoreCase)
                || current.StartsWith("--env=", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (current.StartsWith("-", StringComparison.Ordinal))
            {
                continue;
            }

            return current;
        }

        return null;
    }

    private static async Task<int> RunSpectreAppAsync(string[] args)
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
            logger.LogInformation("Configuration resolved");

            // Build and run the Spectre.Console.Cli app with DI
            var app = SpectreCommandAppFactory.CreateApp(services);
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");
            throw;
        }
    }
}
