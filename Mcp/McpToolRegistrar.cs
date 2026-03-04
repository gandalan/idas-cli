using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace IdasCli.Mcp;

/// <summary>
/// Automatically converts CLI commands to MCP tools using runtime reflection.
/// Discovers tools at server startup by scanning for [CliCommand] attributes.
/// </summary>
public class McpToolRegistrar
{
    private static readonly Dictionary<string, CliToolMetadata> _toolMetadata = new();
    private static bool _verbose = true;
    private static bool _hasScanned = false;

    /// <summary>
    /// Scans the assembly for command classes with [CliCommand] attributes and registers them as MCP tools.
    /// </summary>
    public static void ScanAndRegisterTools(bool verbose = true)
    {
        _verbose = verbose;

        if (_hasScanned)
        {
            if (_verbose)
                Console.Error.WriteLine($"[McpToolRegistrar] Tools already registered ({_toolMetadata.Count} tools)");
            return;
        }

        if (_verbose)
            Console.Error.WriteLine("[McpToolRegistrar] Scanning for CLI commands with [CliCommand] attributes...");

        try
        {
            // Use the runtime tool provider to discover tools
            var discoveredTools = RuntimeMcpToolProvider.DiscoverTools();

            foreach (var (toolName, mcpTool) in discoveredTools)
            {
                // Convert to CliToolMetadata format for backward compatibility
                var metadata = new CliToolMetadata
                {
                    ToolName = mcpTool.ToolName,
                    CommandType = mcpTool.CommandType,
                    Method = mcpTool.Method,
                    CommandName = mcpTool.CommandName,
                    SubCommandName = mcpTool.CommandGroup,
                    Description = mcpTool.Description,
                    Parameters = mcpTool.Parameters.Select(p => new CliParameterMetadata
                    {
                        Name = p.Name,
                        Type = p.Type,
                        IsOptional = p.IsOptional,
                        DefaultValue = p.DefaultValue,
                        Description = p.Description ?? "",
                        IsPutFileParameter = p.IsPutFileParameter
                    }).ToList()
                };

                _toolMetadata[toolName] = metadata;

                if (_verbose)
                    Console.Error.WriteLine($"  Registered: {toolName}");
            }

            _hasScanned = true;

            if (_verbose)
                Console.Error.WriteLine($"[McpToolRegistrar] Registered {_toolMetadata.Count} MCP tools");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[McpToolRegistrar] Error during tool discovery: {ex.Message}");
            if (_verbose && ex.StackTrace != null)
                Console.Error.WriteLine(ex.StackTrace);
        }
    }

    /// <summary>
    /// Gets all registered tool metadata.
    /// </summary>
    public static IEnumerable<CliToolMetadata> GetAllTools()
    {
        if (!_hasScanned)
            ScanAndRegisterTools(verbose: false);
        return _toolMetadata.Values;
    }

    /// <summary>
    /// Gets tool metadata by name.
    /// </summary>
    public static CliToolMetadata? GetTool(string toolName)
    {
        if (!_hasScanned)
            ScanAndRegisterTools(verbose: false);
        return _toolMetadata.TryGetValue(toolName, out var metadata) ? metadata : null;
    }

    /// <summary>
    /// Clears the registered tools cache. Useful for testing.
    /// </summary>
    public static void Clear()
    {
        _toolMetadata.Clear();
        _hasScanned = false;
        RuntimeMcpToolProvider.ClearCache();
    }

    /// <summary>
    /// Gets the count of registered tools.
    /// </summary>
    public static int ToolCount => _toolMetadata.Count;
}

/// <summary>
/// Metadata about a CLI command converted to MCP tool.
/// </summary>
public class CliToolMetadata
{
    public string ToolName { get; set; } = "";
    public Type CommandType { get; set; } = null!;
    public MethodInfo Method { get; set; } = null!;
    public string CommandName { get; set; } = "";
    public string SubCommandName { get; set; } = "";
    public string Description { get; set; } = "";
    public List<CliParameterMetadata> Parameters { get; set; } = new();
}

/// <summary>
/// Metadata about a parameter.
/// </summary>
public class CliParameterMetadata
{
    public string Name { get; set; } = "";
    public Type Type { get; set; } = null!;
    public bool IsOptional { get; set; }
    public object? DefaultValue { get; set; }
    public string Description { get; set; } = "";
    public bool IsPutFileParameter { get; set; }
}
