using System.CommandLine;
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

        // Build the root command (same as Program.cs)
        var rootCommand = BuildRootCommand();

        // Initialize the dynamic MCP tool container with the root command
        DynamicMcpToolContainer.Initialize(rootCommand);

        // Debug output to stderr (won't interfere with MCP stdio protocol)
        Console.Error.WriteLine("[McpServer] MCP server starting...");
        Console.Error.WriteLine($"[McpServer] Registered {McpToolRegistrar.ToolCount} tools");

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

    private static RootCommand BuildRootCommand()
    {
        var rootCommand = new RootCommand("IDAS CLI - MCP Server");

        // Add global options
        rootCommand.AddGlobalOption(GlobalOptions.Format);
        rootCommand.AddGlobalOption(GlobalOptions.FileName);

        // Add all command builders
        rootCommand.AddCommand(VorgangCommandBuilder.Build());
        rootCommand.AddCommand(GSQLCommandBuilder.Build());
        rootCommand.AddCommand(KontaktCommandBuilder.Build());
        rootCommand.AddCommand(ArtikelCommandBuilder.Build());
        rootCommand.AddCommand(AVCommandBuilder.Build());
        rootCommand.AddCommand(LagerbestandCommandBuilder.Build());
        rootCommand.AddCommand(LagerbuchungCommandBuilder.Build());
        rootCommand.AddCommand(WarengruppeCommandBuilder.Build());
        rootCommand.AddCommand(BenutzerCommandBuilder.Build());
        rootCommand.AddCommand(SerieCommandBuilder.Build());
        rootCommand.AddCommand(RollenCommandBuilder.Build());
        rootCommand.AddCommand(VarianteCommandBuilder.Build());
        rootCommand.AddCommand(UIDefinitionCommandBuilder.Build());
        rootCommand.AddCommand(KonfigSatzCommandBuilder.Build());
        rootCommand.AddCommand(WertelisteCommandBuilder.Build());
        rootCommand.AddCommand(BelegCommandBuilder.Build());
        // Note: McpServerCommand is not added to avoid recursion

        return rootCommand;
    }
}
