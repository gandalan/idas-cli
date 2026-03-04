using IdasCli.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

public class McpServerCommand : CommandsBase
{
    public async Task Serve()
    {
        // Enable silent mode to suppress console output from commands
        CommandsBase.IsSilentMode = true;

        // Scan and register all commands as MCP tools (suppress verbose output)
        McpToolRegistrar.ScanAndRegisterTools(verbose: false);

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
