using System.CommandLine;
using System.Text.Json;

namespace IdasCli.Mcp;

/// <summary>
/// Provides runtime discovery of MCP tools using the System.CommandLine tree.
/// Scans registered CLI commands and creates MCP metadata from the command structure.
/// </summary>
public static class RuntimeMcpToolProvider
{
    private static readonly Dictionary<string, McpToolMetadata> _tools = new();
    private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Scans the root command for registered CLI commands.
    /// </summary>
    public static IReadOnlyDictionary<string, McpToolMetadata> DiscoverTools(RootCommand rootCommand)
    {
        if (_tools.Count > 0)
            return _tools;

        foreach (var command in rootCommand.Subcommands)
        {
            DiscoverCommand(command, new[] { command.Name });
        }

        return _tools;
    }

    /// <summary>
    /// Gets a specific tool by name.
    /// </summary>
    public static McpToolMetadata? GetTool(string toolName)
    {
        EnsureDiscovered();
        return _tools.TryGetValue(toolName, out var tool) ? tool : null;
    }

    /// <summary>
    /// Gets all discovered tools.
    /// </summary>
    public static IEnumerable<McpToolMetadata> GetAllTools()
    {
        EnsureDiscovered();
        return _tools.Values;
    }

    public static int ToolCount => _tools.Count;

    /// <summary>
    /// Clears the discovered tools cache. Useful for testing.
    /// </summary>
    public static void ClearCache()
    {
        _tools.Clear();
    }

    private static void EnsureDiscovered()
    {
        if (_tools.Count == 0)
            throw new InvalidOperationException("MCP tools have not been discovered yet. Initialize the MCP container first.");
    }

    private static void DiscoverCommand(Command command, IReadOnlyList<string> commandPath)
    {
        if (ShouldSkipCommand(commandPath))
            return;

        var subcommands = command.Subcommands.Where(subcommand => !ShouldSkipCommand(new[] { subcommand.Name })).ToList();
        if (subcommands.Count > 0)
        {
            foreach (var subcommand in subcommands)
            {
                DiscoverCommand(subcommand, commandPath.Concat(new[] { subcommand.Name }).ToArray());
            }

            return;
        }

        var toolMetadata = CreateToolMetadata(command, commandPath);
        _tools[toolMetadata.ToolName] = toolMetadata;
    }

    private static bool ShouldSkipCommand(IReadOnlyList<string> commandPath)
    {
        return commandPath.Count > 0 && NameComparer.Equals(commandPath[0], "mcp");
    }

    private static McpToolMetadata CreateToolMetadata(Command command, IReadOnlyList<string> commandPath)
    {
        var parameters = new List<McpParameterMetadata>();

        foreach (var option in command.Options)
        {
            parameters.Add(CreateOptionMetadata(option));
        }

        foreach (var argument in command.Arguments)
        {
            parameters.Add(CreateArgumentMetadata(argument));
        }

        return new McpToolMetadata
        {
            ToolName = string.Join("_", commandPath),
            CommandPath = commandPath.ToArray(),
            CommandGroup = commandPath[0],
            CommandName = commandPath[^1],
            Description = command.Description ?? $"Execute {string.Join(" ", commandPath)} command",
            Parameters = parameters
        };
    }

    private static McpParameterMetadata CreateOptionMetadata(Option option)
    {
        return new McpParameterMetadata
        {
            Name = NormalizeName(option.Name),
            CliName = option.Aliases.FirstOrDefault(alias => alias.StartsWith("--", StringComparison.Ordinal))
                ?? option.Aliases.FirstOrDefault()
                ?? $"--{NormalizeName(option.Name)}",
            Type = option.ValueType,
            Kind = McpParameterKind.Option,
            IsOptional = option.Arity.MinimumNumberOfValues == 0,
            Description = GetDescription(option),
            Aliases = option.Aliases.Select(NormalizeName).Distinct(NameComparer).ToList(),
            IsPutFileParameter = IsFileParameter(NormalizeName(option.Name), option.ValueType)
        };
    }

    private static McpParameterMetadata CreateArgumentMetadata(Argument argument)
    {
        var argumentName = NormalizeName(argument.Name);

        return new McpParameterMetadata
        {
            Name = argumentName,
            CliName = argumentName,
            Type = argument.ValueType,
            Kind = McpParameterKind.Argument,
            IsOptional = argument.Arity.MinimumNumberOfValues == 0,
            Description = GetDescription(argument),
            Aliases = new List<string> { argumentName },
            IsPutFileParameter = IsFileParameter(argumentName, argument.ValueType)
        };
    }

    private static string? GetDescription(Symbol symbol)
    {
        return symbol.Description;
    }

    private static bool IsFileParameter(string name, Type type)
    {
        return type == typeof(string) && NameComparer.Equals(name, "file");
    }

    private static string NormalizeName(string name)
    {
        return name.TrimStart('-');
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
    /// The command path segments (e.g., ["vorgang", "list"])
    /// </summary>
    public IReadOnlyList<string> CommandPath { get; set; } = Array.Empty<string>();

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
    /// Parameters of the tool
    /// </summary>
    public List<McpParameterMetadata> Parameters { get; set; } = new();
}

public enum McpParameterKind
{
    Option,
    Argument
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
    /// CLI name or alias used when invoking the command.
    /// </summary>
    public string CliName { get; set; } = "";

    /// <summary>
    /// Parameter type
    /// </summary>
    public Type Type { get; set; } = null!;

    /// <summary>
    /// Whether this is a positional argument or an option.
    /// </summary>
    public McpParameterKind Kind { get; set; }

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
    /// Accepted parameter names from MCP JSON input.
    /// </summary>
    public List<string> Aliases { get; set; } = new();

    /// <summary>
    /// Whether this parameter represents a file path for a put command
    /// </summary>
    public bool IsPutFileParameter { get; set; }
}
