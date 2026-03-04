namespace IdasCli;

/// <summary>
/// Container for CLI attribute types.
/// Use with: using static IdasCli.CliAttributes;
/// </summary>
public static class CliAttributes
{
    /// <summary>
    /// Attribute to mark a method as a CLI command that can be exposed as an MCP tool.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CliCommandAttribute : Attribute
    {
        /// <summary>
        /// The name of the command (e.g., "list", "get", "put")
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Optional description of the command
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the CliCommandAttribute
        /// </summary>
        /// <param name="name">The command name</param>
        public CliCommandAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Attribute to mark a parameter as a command option (--option).
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CliOptionAttribute : Attribute
    {
        /// <summary>
        /// Optional description of the option
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Attribute to mark a parameter as a command argument (positional).
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CliArgumentAttribute : Attribute
    {
        /// <summary>
        /// Optional description of the argument
        /// </summary>
        public string? Description { get; set; }
    }
}
