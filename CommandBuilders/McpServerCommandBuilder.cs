using System.CommandLine;
using IdasCli.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

public static class McpServerCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("mcp", "Manage MCP server");

        // serve subcommand
        var serveCmd = new Command("serve", "Start the MCP server with dynamically registered commands");

        serveCmd.SetHandler(async () =>
        {
            // Use the McpServerCommand class directly
            var serverCommand = new McpServerCommand();
            await serverCommand.Serve();
        });

        cmd.AddCommand(serveCmd);

        return cmd;
    }
}
