using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using static IdasCli.CliAttributes;

namespace IdasCli.Mcp;

/// <summary>
/// Provides runtime discovery of MCP tools using reflection.
/// Scans assembly for methods with [CliCommand] attribute and creates metadata.
/// </summary>
public static class RuntimeMcpToolProvider
{
    private static readonly Dictionary<string, McpToolMetadata> _tools = new();

    /// <summary>
    /// Scans the assembly for classes inheriting from CommandsBase and discovers
    /// methods with [CliCommand] attribute.
    /// </summary>
    public static IReadOnlyDictionary<string, McpToolMetadata> DiscoverTools()
    {
        if (_tools.Count > 0)
            return _tools;

        var assembly = Assembly.GetExecutingAssembly();
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CommandsBase)));

        foreach (var commandType in commandTypes)
        {
            var typeName = commandType.Name;
            // Remove "Commands" suffix to get the command group name
            var commandGroup = typeName.EndsWith("Commands")
                ? typeName[..^"Commands".Length].ToLowerInvariant()
                : typeName.ToLowerInvariant();

            var methods = commandType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var cliCommandAttr = method.GetCustomAttribute<CliCommandAttribute>();
                if (cliCommandAttr == null)
                    continue;

                // Skip methods that shouldn't be exposed as MCP tools
                if (ShouldSkipMethod(method))
                    continue;

                var toolMetadata = CreateToolMetadata(commandGroup, cliCommandAttr, method, commandType);
                var toolName = $"{commandGroup}_{cliCommandAttr.Name}";

                // Handle duplicate names (e.g., for overloads)
                var uniqueToolName = toolName;
                var counter = 1;
                while (_tools.ContainsKey(uniqueToolName))
                {
                    uniqueToolName = $"{toolName}_{counter}";
                    counter++;
                }

                _tools[uniqueToolName] = toolMetadata;
            }
        }

        return _tools;
    }

    /// <summary>
    /// Gets a specific tool by name.
    /// </summary>
    public static McpToolMetadata? GetTool(string toolName)
    {
        DiscoverTools();
        return _tools.TryGetValue(toolName, out var tool) ? tool : null;
    }

    /// <summary>
    /// Gets all discovered tools.
    /// </summary>
    public static IEnumerable<McpToolMetadata> GetAllTools()
    {
        DiscoverTools();
        return _tools.Values;
    }

    /// <summary>
    /// Clears the discovered tools cache. Useful for testing.
    /// </summary>
    public static void ClearCache()
    {
        _tools.Clear();
    }

    private static bool ShouldSkipMethod(MethodInfo method)
    {
        // Skip property getters/setters
        if (method.IsSpecialName)
            return true;

        // Skip methods from Object class
        if (method.DeclaringType == typeof(object))
            return true;

        // Skip methods from CommandsBase that aren't commands
        if (method.DeclaringType == typeof(CommandsBase) && method.Name != "InvokeCommand")
            return true;

        // Skip async infrastructure methods
        if (method.Name.Contains("<") || method.Name.Contains(">"))
            return true;

        return false;
    }

    private static McpToolMetadata CreateToolMetadata(
        string commandGroup,
        CliCommandAttribute cliCommandAttr,
        MethodInfo method,
        Type commandType)
    {
        var parameters = new List<McpParameterMetadata>();

        foreach (var param in method.GetParameters())
        {
            // Skip CommonParameters - it's handled specially
            if (param.ParameterType == typeof(CommonParameters))
                continue;

            var paramMetadata = CreateParameterMetadata(param);
            parameters.Add(paramMetadata);
        }

        return new McpToolMetadata
        {
            ToolName = $"{commandGroup}_{cliCommandAttr.Name}",
            CommandGroup = commandGroup,
            CommandName = cliCommandAttr.Name,
            Description = cliCommandAttr.Description ?? GetMethodDescription(method),
            Method = method,
            CommandType = commandType,
            Parameters = parameters
        };
    }

    private static McpParameterMetadata CreateParameterMetadata(ParameterInfo param)
    {
        var description = GetParameterDescription(param);
        var isPutFileParameter = param.Name?.Equals("file", StringComparison.OrdinalIgnoreCase) == true &&
                                 param.ParameterType == typeof(string);

        return new McpParameterMetadata
        {
            Name = param.Name ?? "param",
            Type = param.ParameterType,
            IsOptional = param.IsOptional,
            DefaultValue = param.IsOptional ? param.DefaultValue : null,
            Description = description,
            IsPutFileParameter = isPutFileParameter
        };
    }

    private static string? GetParameterDescription(ParameterInfo param)
    {
        // Check for CliOption attribute
        var optionAttr = param.GetCustomAttribute<CliOptionAttribute>();
        if (optionAttr?.Description != null)
            return optionAttr.Description;

        // Check for CliArgument attribute
        var argAttr = param.GetCustomAttribute<CliArgumentAttribute>();
        if (argAttr?.Description != null)
            return argAttr.Description;

        // Check for Description attribute
        var descAttr = param.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr?.Description != null)
            return descAttr.Description;

        return null;
    }

    private static string GetMethodDescription(MethodInfo method)
    {
        // Check for Description attribute
        var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
        if (descAttr?.Description != null)
            return descAttr.Description;

        // Check for XML documentation (would require additional infrastructure)
        return $"Execute {method.Name} command";
    }
}

/// <summary>
/// Metadata about a discovered MCP tool.
/// </summary>
public class McpToolMetadata
{
    /// <summary>
    /// The full tool name (e.g., "vorgang_list")
    /// </summary>
    public string ToolName { get; set; } = "";

    /// <summary>
    /// The command group (e.g., "vorgang")
    /// </summary>
    public string CommandGroup { get; set; } = "";

    /// <summary>
    /// The command name (e.g., "list")
    /// </summary>
    public string CommandName { get; set; } = "";

    /// <summary>
    /// Description of the tool
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The method to invoke
    /// </summary>
    public MethodInfo Method { get; set; } = null!;

    /// <summary>
    /// The type containing the method
    /// </summary>
    public Type CommandType { get; set; } = null!;

    /// <summary>
    /// Parameters of the tool
    /// </summary>
    public List<McpParameterMetadata> Parameters { get; set; } = new();
}

/// <summary>
/// Metadata about a tool parameter.
/// </summary>
public class McpParameterMetadata
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Parameter type
    /// </summary>
    public Type Type { get; set; } = null!;

    /// <summary>
    /// Whether the parameter is optional
    /// </summary>
    public bool IsOptional { get; set; }

    /// <summary>
    /// Default value if optional
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Parameter description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this parameter represents a file path for a put command
    /// </summary>
    public bool IsPutFileParameter { get; set; }
}
