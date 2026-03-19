using System.Text.Json;
using Spectre.Console.Cli;

namespace IdasCli.Mcp;

/// <summary>
/// Handles dynamic invocation of Spectre CLI commands.
/// This allows MCP tools to invoke CLI commands at runtime.
/// </summary>
public class DynamicMcpToolHandler
{
    private readonly ICommandApp _commandApp;
    private readonly Dictionary<string, McpToolMetadata> _tools;

    public DynamicMcpToolHandler(ICommandApp commandApp, Dictionary<string, McpToolMetadata> tools)
    {
        _commandApp = commandApp ?? throw new ArgumentNullException(nameof(commandApp));
        _tools = tools ?? throw new ArgumentNullException(nameof(tools));
    }

    /// <summary>
    /// Invokes a CLI command dynamically with the provided parameters.
    /// </summary>
    public async Task<object> InvokeAsync(McpToolMetadata toolMetadata, Dictionary<string, object?> parameters)
    {
        try
        {
            // Build command line arguments from parameters
            var args = BuildCommandLineArgs(toolMetadata, parameters);

            // Capture output
            using var capture = new OutputCapture();

            // Run the command through the CommandApp
            var exitCode = await _commandApp.RunAsync(args);

            // Get the captured output
            var output = capture.GetCapturedOutput();

            // Try to parse output as JSON
            if (!string.IsNullOrWhiteSpace(output))
            {
                try
                {
                    var jsonObject = JsonSerializer.Deserialize<object>(output);
                    return new
                    {
                        Success = exitCode == 0,
                        ExitCode = exitCode,
                        Data = jsonObject
                    };
                }
                catch (JsonException)
                {
                    // Not valid JSON, return as string
                    return new
                    {
                        Success = exitCode == 0,
                        ExitCode = exitCode,
                        Data = output.Trim()
                    };
                }
            }

            return new
            {
                Success = exitCode == 0,
                ExitCode = exitCode,
                Message = "Command executed successfully"
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Success = false,
                Error = ex.Message,
                StackTrace = ex.StackTrace
            };
        }
    }

    private string[] BuildCommandLineArgs(McpToolMetadata toolMetadata, Dictionary<string, object?> parameters)
    {
        var args = new List<string>(toolMetadata.CommandPath);

        foreach (var param in toolMetadata.Parameters.Where(p => p.Kind == McpParameterKind.Option))
        {
            var matchingKey = FindMatchingKey(parameters, param);
            if (matchingKey == null)
                continue;

            var value = parameters[matchingKey];

            // Skip null values for optional parameters
            if (value == null && param.IsOptional)
                continue;

            args.Add(param.CliName);

            if (value != null)
            {
                var stringValue = ConvertParameterToString(value, param.Type);
                args.Add(stringValue);
            }
        }

        foreach (var param in toolMetadata.Parameters.Where(p => p.Kind == McpParameterKind.Argument))
        {
            var matchingKey = FindMatchingKey(parameters, param);
            if (matchingKey == null)
            {
                if (param.IsOptional)
                    continue;

                throw new ArgumentException($"Missing required parameter '{param.Name}' for command '{toolMetadata.ToolName}'.");
            }

            var value = parameters[matchingKey];
            
            if (value == null)
            {
                if (param.IsOptional)
                    continue;

                throw new ArgumentException($"Parameter '{param.Name}' cannot be null for command '{toolMetadata.ToolName}'.");
            }

            args.Add(ConvertParameterToString(value, param.Type));
        }

        return args.ToArray();
    }

    private string? FindMatchingKey(Dictionary<string, object?> parameters, McpParameterMetadata param)
    {
        // Try exact match first
        if (parameters.ContainsKey(param.Name))
            return param.Name;

        // Try case-insensitive match
        var key = parameters.Keys.FirstOrDefault(k => 
            string.Equals(k, param.Name, StringComparison.OrdinalIgnoreCase));
        
        return key;
    }

    private string ConvertParameterToString(object value, Type targetType)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
            targetType = underlyingType;

        // Handle JsonElement
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => jsonElement.GetRawText(),
                JsonValueKind.String => jsonElement.GetString() ?? "",
                JsonValueKind.Null => "",
                _ => jsonElement.GetRawText()
            };
        }

        // Handle boolean specially - use lowercase
        if (targetType == typeof(bool) && value is bool b)
            return b.ToString().ToLowerInvariant();

        // Handle DateTime with ISO format
        if (targetType == typeof(DateTime) && value is DateTime dt)
            return dt.ToString("O");

        // Handle DateOnly
        if (targetType == typeof(DateOnly) && value is DateOnly d)
            return d.ToString("yyyy-MM-dd");

        // Handle Guid
        if (targetType == typeof(Guid) && value is Guid g)
            return g.ToString();

        // Default: ToString
        return value?.ToString() ?? "";
    }
}
