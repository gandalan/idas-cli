using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using IdasCli.Commands;
using ModelContextProtocol.Server;
using Spectre.Console.Cli;

namespace IdasCli.Mcp;

/// <summary>
/// Dynamic MCP tool container that exposes discovered Spectre CLI commands as MCP tools.
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
    /// Initializes the tool container with discovered tools from the Spectre command tree.
    /// Must be called before the MCP server starts.
    /// </summary>
    public static void Initialize(ICommandApp commandApp)
    {
        if (_initialized) return;

        lock (_initLock)
        {
            if (_initialized) return;

            // Discover all tools by reflecting on command types in the assembly
            var discoveredTools = DiscoverToolsFromAssembly();
            foreach (var (toolName, metadata) in discoveredTools)
            {
                _tools[toolName] = metadata;
            }

            // Create the handler
            _handler = new DynamicMcpToolHandler(commandApp, _tools);

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
    /// Discovers tools by scanning the assembly for Spectre command classes
    /// and extracting their settings/parameters.
    /// </summary>
    private static Dictionary<string, McpToolMetadata> DiscoverToolsFromAssembly()
    {
        var tools = new Dictionary<string, McpToolMetadata>(StringComparer.OrdinalIgnoreCase);
        var assembly = Assembly.GetExecutingAssembly();

        // Find all command types in the assembly
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && 
                       t.GetInterfaces().Any(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(ICommand<>)))
            .ToList();

        foreach (var commandType in commandTypes)
        {
            var metadata = CreateToolMetadata(commandType);
            if (metadata != null)
            {
                tools[metadata.ToolName] = metadata;
            }
        }

        return tools;
    }

    private static McpToolMetadata? CreateToolMetadata(Type commandType)
    {
        // Extract command name from type name (e.g., "VorgangListCommand" -> "vorgang_list")
        var typeName = commandType.Name.Replace("Command", "");
        
        // Parse branch names from the command type
        var commandPath = ParseCommandPath(typeName);
        if (commandPath == null || commandPath.Count == 0)
            return null;

        var toolName = string.Join("_", commandPath).ToLowerInvariant();
        
        // Get settings type from ICommand<T> interface
        var settingsType = commandType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
            ?.GetGenericArguments().FirstOrDefault();

        var parameters = new List<McpParameterMetadata>();

        if (settingsType != null && settingsType != typeof(GlobalSettings))
        {
            // Extract options and arguments from settings type
            foreach (var property in settingsType.GetProperties())
            {
                var optionAttr = property.GetCustomAttribute<CommandOptionAttribute>();
                if (optionAttr != null)
                {
                    var paramName = optionAttr.LongNames?.FirstOrDefault() 
                        ?? property.Name.ToLowerInvariant();
                    
                    // Check if this is the "format" option from GlobalSettings
                    if (paramName == "format" || paramName == "filename")
                        continue;
                    
                    parameters.Add(new McpParameterMetadata
                    {
                        Name = paramName.Replace("-", "").ToLowerInvariant(),
                        CliName = $"--{paramName}",
                        Type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType,
                        Kind = McpParameterKind.Option,
                        IsOptional = Nullable.GetUnderlyingType(property.PropertyType) != null || 
                                    !property.PropertyType.IsValueType,
                        Description = GetDescription(property)
                    });
                }

                var argAttr = property.GetCustomAttribute<CommandArgumentAttribute>();
                if (argAttr != null)
                {
                    var argName = !string.IsNullOrEmpty(argAttr.ValueName) 
                        ? argAttr.ValueName.ToLowerInvariant().Trim('<', '>')
                        : property.Name.ToLowerInvariant();
                    
                    parameters.Add(new McpParameterMetadata
                    {
                        Name = argName.Replace("-", "").ToLowerInvariant(),
                        CliName = argName,
                        Type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType,
                        Kind = McpParameterKind.Argument,
                        IsOptional = Nullable.GetUnderlyingType(property.PropertyType) != null ||
                                    !property.PropertyType.IsValueType,
                        Description = GetDescription(property)
                    });
                }
            }
        }

        return new McpToolMetadata
        {
            ToolName = toolName,
            CommandPath = commandPath.ToArray(),
            CommandGroup = commandPath[0],
            CommandName = commandPath[^1],
            Description = $"Execute {string.Join(" ", commandPath)} command",
            Parameters = parameters,
            CommandType = commandType
        };
    }

    private static List<string>? ParseCommandPath(string typeName)
    {
        // Convert "VorgangListCommand" to ["vorgang", "list"]
        // Use heuristics to determine command hierarchy
        
        var knownPrefixes = new[] { 
            "Benutzer", "Vorgang", "Artikel", "AV", "Kontakt", "Serie", 
            "Variante", "Rolle", "KonfigSatz", "UIDefinition", "Werteliste",
            "Lagerbestand", "Lagerbuchung", "GSQL", "Berechtigung", 
            "Warengruppe", "Beleg", "Sidecar", "McpServer", "Rollen"
        };

        foreach (var prefix in knownPrefixes)
        {
            if (typeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var action = typeName.Substring(prefix.Length);
                if (string.IsNullOrEmpty(action))
                    return null;
                
                // Skip McpServer command itself
                if (prefix.Equals("McpServer", StringComparison.OrdinalIgnoreCase))
                    return null;
                    
                return new List<string> { 
                    prefix.ToLowerInvariant(), 
                    action.ToLowerInvariant() 
                };
            }
        }

        // Fallback: return null for unknown command patterns
        return null;
    }

    private static string? GetDescription(PropertyInfo property)
    {
        var descAttr = property.GetCustomAttribute<DescriptionAttribute>();
        return descAttr?.Description;
    }

    /// <summary>
    /// Generic tool invoker that handles all CLI commands dynamically.
    /// This is registered as a catch-all tool that dispatches to the appropriate CLI command.
    /// </summary>
    [McpServerTool(Name = "idas")]
    [Description("Execute IDAS CLI commands discovered from the registered Spectre command tree.")] 
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

public class McpToolMetadata
{
    public string ToolName { get; set; } = string.Empty;
    public string[] CommandPath { get; set; } = Array.Empty<string>();
    public string CommandGroup { get; set; } = string.Empty;
    public string CommandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<McpParameterMetadata> Parameters { get; set; } = new();
    public Type? CommandType { get; set; }
}

public class McpParameterMetadata
{
    public string Name { get; set; } = string.Empty;
    public string CliName { get; set; } = string.Empty;
    public Type Type { get; set; } = typeof(string);
    public McpParameterKind Kind { get; set; }
    public bool IsOptional { get; set; }
    public string? Description { get; set; }
    public bool IsPutFileParameter { get; set; }
}

public enum McpParameterKind
{
    Option,
    Argument
}
