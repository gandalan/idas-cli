using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

/// <summary>
/// MCP Server command - provides Model Context Protocol server functionality.
/// This is a simplified version that starts the MCP server without dynamic tool registration.
/// </summary>
public class McpServerCommand : AsyncCommand<GlobalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        // Enable silent mode to suppress console output from commands
        CommandsBase.IsSilentMode = true;

        var rootCommand = CliRootCommandFactory.BuildRootCommand(
            "IDAS CLI - MCP Server",
            includeMcpServer: false);

        // Initialize the dynamic MCP tool container with the root command
        DynamicMcpToolContainer.Initialize(rootCommand);

        // Debug output to stderr (won't interfere with MCP stdio protocol)
        Console.Error.WriteLine("[McpServer] MCP server starting...");
        Console.Error.WriteLine($"[McpServer] Registered {RuntimeMcpToolProvider.ToolCount} tools");

        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddMcpServer()
                    .WithStdioServerTransport()
                    .WithToolsFromAssembly();
            })
            .ConfigureLogging(logging =>
            {
                // Disable all console logging for MCP server
                logging.ClearProviders();
            });

        await builder.Build().RunAsync();
    }
}
