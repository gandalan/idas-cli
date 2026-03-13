using System.CommandLine;
using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace IdasCli.Mcp;

/// <summary>
/// Dynamic MCP tool container that exposes discovered CLI commands as MCP tools.
/// This class is discovered by the MCP SDK via WithToolsFromAssembly() and
/// delegates tool invocations to the dynamically discovered CLI commands.
/// </summary>
[McpServerToolType]
public class DynamicMcpToolContainer
{
    private static readonly Dictionary<string, McpToolMetadata> _tools = new();
    private static DynamicMcpToolHandler? _handler;
    private static bool _initialized = false;
    private static readonly object _initLock = new();

    /// <summary>
    /// Initializes the tool container with discovered tools and the root command.
    /// Must be called before the MCP server starts.
    /// </summary>
    public static void Initialize(RootCommand rootCommand)
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;

            // Discover all tools
            var discoveredTools = RuntimeMcpToolProvider.DiscoverTools(rootCommand);
            foreach (var (toolName, metadata) in discoveredTools)
            {
                _tools[toolName] = metadata;
            }

            // Create the handler
            _handler = new DynamicMcpToolHandler(rootCommand);

            _initialized = true;

            Console.Error.WriteLine($"[DynamicMcpToolContainer] Initialized with {_tools.Count} tools");
            foreach (var toolName in _tools.Keys.Take(10))
            {
                Console.Error.WriteLine($"  - {toolName}");
            }
            if (_tools.Count > 10)
            {
                Console.Error.WriteLine($"  ... and {_tools.Count - 10} more");
            }
        }
    }

    /// <summary>
    /// Generic tool invoker that handles all CLI commands dynamically.
    /// This is registered as a catch-all tool that dispatches to the appropriate CLI command.
    /// </summary>
    [McpServerTool(Name = "idas")]
    [Description("Execute IDAS CLI commands discovered from the registered System.CommandLine command tree.")]
    public async Task<object> InvokeIdasCommand(
        [Description("The command to execute (e.g., 'vorgang_list', 'kontakt_get')")] string command,
        [Description("JSON object with command parameters")] string? parameters = null)
    {
        if (!_initialized)
        {
            return new { Success = false, Error = "MCP tool container not initialized" };
        }

        if (string.IsNullOrWhiteSpace(command))
        {
            return new { Success = false, Error = "Command name is required" };
        }

        // Normalize command name
        command = command.ToLowerInvariant().Trim();

        // Find the tool
        if (!_tools.TryGetValue(command, out var toolMetadata))
        {
            // Try to find partial match
            var matchingTools = _tools.Keys.Where(k => k.Contains(command)).ToList();
            if (matchingTools.Count == 1)
            {
                toolMetadata = _tools[matchingTools[0]];
            }
            else if (matchingTools.Count > 1)
            {
                return new
                {
                    Success = false,
                    Error = $"Multiple commands match '{command}': {string.Join(", ", matchingTools)}"
                };
            }
            else
            {
                return new
                {
                    Success = false,
                    Error = $"Unknown command: '{command}'. Available commands: {string.Join(", ", _tools.Keys.Take(20))}..."
                };
            }
        }

        // Parse parameters
        Dictionary<string, object?> paramDict;
        try
        {
            paramDict = string.IsNullOrWhiteSpace(parameters)
                ? new Dictionary<string, object?>()
                : JsonSerializer.Deserialize<Dictionary<string, object?>>(parameters) ?? new Dictionary<string, object?>();
        }
        catch (JsonException ex)
        {
            return new { Success = false, Error = $"Invalid parameters JSON: {ex.Message}" };
        }

        // Invoke the tool
        try
        {
            var result = await _handler!.InvokeAsync(toolMetadata, paramDict);
            return result;
        }
        catch (Exception ex)
        {
            return new { Success = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// Gets a list of all available tools and their parameters.
    /// </summary>
    [McpServerTool(Name = "idas_help")]
    [Description("Get help for IDAS CLI commands. Returns a list of all available commands.")]
    public Task<object> GetIdasHelp()
    {
        if (!_initialized)
        {
            return Task.FromResult<object>(new { Success = false, Error = "MCP tool container not initialized" });
        }

        var tools = _tools.Values.Select(t => new
        {
            Name = t.ToolName,
            Description = t.Description,
            Parameters = t.Parameters.Select(p => new
            {
                Name = p.Name,
                Type = p.Type.Name,
                Optional = p.IsOptional,
                Description = p.Description
            }).ToList()
        }).ToList();

        return Task.FromResult<object>(new
        {
            Success = true,
            ToolCount = tools.Count,
            Tools = tools
        });
    }


}
