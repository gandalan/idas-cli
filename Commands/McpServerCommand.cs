using IdasCli.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

/// <summary>
/// MCP Server command - provides Model Context Protocol server functionality.
/// </summary>
public class McpServerCommand : AsyncCommand<GlobalSettings>
{
    private readonly ICommandApp _commandApp;

    public McpServerCommand(ICommandApp commandApp)
    {
        _commandApp = commandApp;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        // Debug output to stderr (won't interfere with MCP stdio protocol)
        Console.Error.WriteLine("[McpServer] MCP server starting...");

        // Initialize the dynamic MCP tool container with the Spectre command app
        DynamicMcpToolContainer.Initialize(_commandApp);

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
        return 0;
    }
}
