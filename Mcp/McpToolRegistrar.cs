using System.ComponentModel;
using System.Reflection;

namespace IdasCli.Mcp;

/// <summary>
/// Automatically converts CLI commands to MCP tools using reflection
/// NOTE: This is a placeholder implementation. Task-018 will implement full runtime tool registration.
/// </summary>
public class McpToolRegistrar
{
    private static readonly Dictionary<string, CliToolMetadata> _toolMetadata = new();
    private static bool _verbose = true;

    /// <summary>
    /// Scans the assembly for command classes and extracts metadata
    /// NOTE: Currently returns empty - full implementation in Task-018
    /// </summary>
    public static void ScanAndRegisterTools(bool verbose = true)
    {
        _verbose = verbose;
        
        if (_verbose)
            Console.Error.WriteLine("[McpToolRegistrar] Runtime tool registration not yet implemented (Task-018)");
        
        // Placeholder: Task-018 will implement full runtime discovery using custom attributes
        // For now, we return empty to allow the build to succeed
        
        if (_verbose)
            Console.Error.WriteLine($"[McpToolRegistrar] Registered {_toolMetadata.Count} MCP tools");
    }

    /// <summary>
    /// Gets all registered tool metadata
    /// </summary>
    public static IEnumerable<CliToolMetadata> GetAllTools()
    {
        return _toolMetadata.Values;
    }

    /// <summary>
    /// Gets tool metadata by name
    /// </summary>
    public static CliToolMetadata? GetTool(string toolName)
    {
        return _toolMetadata.TryGetValue(toolName, out var metadata) ? metadata : null;
    }
}

/// <summary>
/// Metadata about a CLI command converted to MCP tool
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
/// Metadata about a parameter
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
