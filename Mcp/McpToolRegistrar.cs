using System.ComponentModel;
using System.Reflection;
using Cocona;

namespace IdasCli.Mcp;

/// <summary>
/// Automatically converts Cocona commands to MCP tools using reflection
/// </summary>
public class McpToolRegistrar
{
    private static readonly Dictionary<string, CoconaToolMetadata> _toolMetadata = new();
    private static bool _verbose = true;

    /// <summary>
    /// Scans the assembly for Cocona command classes and extracts metadata
    /// </summary>
    public static void ScanAndRegisterTools(bool verbose = true)
    {
        _verbose = verbose;
        var assembly = Assembly.GetExecutingAssembly();
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CommandsBase)))
            .ToList();

        if (_verbose)
            Console.Error.WriteLine($"[McpToolRegistrar] Found {commandTypes.Count} command classes");

        foreach (var commandType in commandTypes)
        {
            // Skip McpServerCommand itself
            if (commandType.Name == "McpServerCommand")
                continue;

            ScanCommandType(commandType);
        }

        if (_verbose)
            Console.Error.WriteLine($"[McpToolRegistrar] Registered {_toolMetadata.Count} MCP tools");
    }

    private static void ScanCommandType(Type commandType)
    {
        var methods = commandType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<CommandAttribute>() != null)
            .ToList();

        // Extract the subcommand name from the type name (e.g., VorgangCommands -> vorgang)
        var subCommandName = commandType.Name.Replace("Commands", "").ToLowerInvariant();

        foreach (var method in methods)
        {
            var commandAttr = method.GetCustomAttribute<CommandAttribute>();
            if (commandAttr == null) continue;

            var commandName = commandAttr.Name ?? method.Name.ToLowerInvariant();
            var toolName = $"{subCommandName}_{commandName}";

            var metadata = new CoconaToolMetadata
            {
                ToolName = toolName,
                CommandType = commandType,
                Method = method,
                CommandName = commandName,
                SubCommandName = subCommandName,
                Description = commandAttr.Description ?? $"Executes {commandName} command for {subCommandName}",
                Parameters = ExtractParameters(method)
            };

            _toolMetadata[toolName] = metadata;
            if (_verbose)
                Console.Error.WriteLine($"Registered: {toolName}");
        }
    }

    private static List<CoconaParameterMetadata> ExtractParameters(MethodInfo method)
    {
        var parameters = new List<CoconaParameterMetadata>();
        var methodParams = method.GetParameters();

        foreach (var param in methodParams)
        {
            // Skip CommonParameters - we'll handle this specially
            if (param.ParameterType == typeof(CommonParameters))
                continue;

            var paramMetadata = new CoconaParameterMetadata
            {
                Name = param.Name ?? "value",
                Type = param.ParameterType,
                IsOptional = param.IsOptional 
                    || param.HasDefaultValue
                    || Nullable.GetUnderlyingType(param.ParameterType) != null,
                DefaultValue = param.DefaultValue,
                Description = ExtractParameterDescription(param)
            };

            // Special handling for file parameters in "put" commands
            if (method.GetCustomAttribute<CommandAttribute>()?.Name == "put" &&
                (param.Name == "file" || param.Name == "fileName") &&
                param.ParameterType == typeof(string))
            {
                paramMetadata.Name = "jsonContent";
                paramMetadata.Description = "JSON string containing the data";
                paramMetadata.IsPutFileParameter = true;
            }

            parameters.Add(paramMetadata);
        }

        return parameters;
    }

    private static string ExtractParameterDescription(ParameterInfo param)
    {
        // Check for Option attribute
        var optionAttr = param.GetCustomAttribute<OptionAttribute>();
        if (optionAttr != null && !string.IsNullOrEmpty(optionAttr.Description))
            return optionAttr.Description;

        // Check for Argument attribute
        var argumentAttr = param.GetCustomAttribute<ArgumentAttribute>();
        if (argumentAttr != null && !string.IsNullOrEmpty(argumentAttr.Description))
            return argumentAttr.Description;

        // Check for Description attribute
        var descAttr = param.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
            return descAttr.Description;

        return $"Parameter {param.Name}";
    }

    /// <summary>
    /// Gets all registered tool metadata
    /// </summary>
    public static IEnumerable<CoconaToolMetadata> GetAllTools()
    {
        return _toolMetadata.Values;
    }

    /// <summary>
    /// Gets tool metadata by name
    /// </summary>
    public static CoconaToolMetadata? GetTool(string toolName)
    {
        return _toolMetadata.TryGetValue(toolName, out var metadata) ? metadata : null;
    }
}

/// <summary>
/// Metadata about a Cocona command converted to MCP tool
/// </summary>
public class CoconaToolMetadata
{
    public string ToolName { get; set; } = "";
    public Type CommandType { get; set; } = null!;
    public MethodInfo Method { get; set; } = null!;
    public string CommandName { get; set; } = "";
    public string SubCommandName { get; set; } = "";
    public string Description { get; set; } = "";
    public List<CoconaParameterMetadata> Parameters { get; set; } = new();
}

/// <summary>
/// Metadata about a parameter
/// </summary>
public class CoconaParameterMetadata
{
    public string Name { get; set; } = "";
    public Type Type { get; set; } = null!;
    public bool IsOptional { get; set; }
    public object? DefaultValue { get; set; }
    public string Description { get; set; } = "";
    public bool IsPutFileParameter { get; set; }
}
