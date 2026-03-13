using System.CommandLine;
using System.Text.Json;

namespace IdasCli.Mcp;

/// <summary>
/// Handles dynamic invocation of CLI commands via the root command.
/// This allows MCP tools to invoke CLI commands at runtime.
/// </summary>
public class DynamicMcpToolHandler
{
    private readonly RootCommand _rootCommand;

    public DynamicMcpToolHandler(RootCommand rootCommand)
    {
        _rootCommand = rootCommand ?? throw new ArgumentNullException(nameof(rootCommand));
    }

    /// <summary>
    /// Invokes a CLI command dynamically with the provided parameters.
    /// </summary>
    /// <param name="toolMetadata">The tool metadata containing command info</param>
    /// <param name="parameters">Dictionary of parameter names to values</param>
    /// <returns>The result of the command invocation</returns>
    public async Task<object> InvokeAsync(McpToolMetadata toolMetadata, Dictionary<string, object?> parameters)
    {
        try
        {
            // Build command line arguments from parameters
            var args = BuildCommandLineArgs(toolMetadata, parameters);

            // Capture output
            var originalOutput = Console.Out;
            var stringWriter = new StringWriter();

            try
            {
                Console.SetOut(stringWriter);

                // Invoke the root command with the constructed arguments
                var exitCode = await _rootCommand.InvokeAsync(args);

                // Get the captured output
                var output = stringWriter.ToString();

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
            finally
            {
                Console.SetOut(originalOutput);
            }
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
        finally
        {
            Cleanup();
        }
    }

    private string[] BuildCommandLineArgs(McpToolMetadata toolMetadata, Dictionary<string, object?> parameters)
    {
        var args = new List<string>(toolMetadata.CommandPath);

        foreach (var param in toolMetadata.Parameters.Where(parameter => parameter.Kind == McpParameterKind.Option))
        {
            var matchingKey = FindMatchingKey(parameters, param);

            if (matchingKey == null)
            {
                continue;
            }

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

        foreach (var param in toolMetadata.Parameters.Where(parameter => parameter.Kind == McpParameterKind.Argument))
        {
            var matchingKey = FindMatchingKey(parameters, param);
            if (matchingKey == null)
            {
                if (param.IsOptional)
                    continue;

                throw new ArgumentException($"Missing required parameter '{param.Name}' for command '{toolMetadata.ToolName}'.");
            }

            var value = parameters[matchingKey];
            if (param.IsPutFileParameter && value is string jsonContent)
            {
                var tempFile = Path.GetTempFileName();
                File.WriteAllText(tempFile, jsonContent);
                ScheduleCleanup(tempFile);
                args.Add(tempFile);
                continue;
            }

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

    private string ConvertParameterToString(object value, Type targetType)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
            targetType = underlyingType;

        // Handle JsonElement - extract actual value first
        if (value is JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.True:
                    return "true";
                case JsonValueKind.False:
                    return "false";
                case JsonValueKind.Number:
                    return jsonElement.GetRawText();
                case JsonValueKind.String:
                    return jsonElement.GetString() ?? "";
                case JsonValueKind.Null:
                    return "";
                default:
                    return jsonElement.GetRawText();
            }
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
            return g.ToString("D");

        return Convert.ToString(value) ?? "";
    }

    private object? ConvertParameterValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
            targetType = underlyingType;

        // If already correct type
        if (targetType.IsInstanceOfType(value))
            return value;

        // Handle string to Guid
        if (targetType == typeof(Guid) && value is string guidStr)
        {
            if (Guid.TryParse(guidStr, out var guid))
                return guid;
        }

        // Handle string to int
        if (targetType == typeof(int) && value is string intStr)
        {
            if (int.TryParse(intStr, out var intVal))
                return intVal;
        }

        // Handle string to bool
        if (targetType == typeof(bool) && value is string boolStr)
        {
            if (bool.TryParse(boolStr, out var boolVal))
                return boolVal;
        }

        // Handle string to DateTime
        if (targetType == typeof(DateTime) && value is string dateStr)
        {
            if (DateTime.TryParse(dateStr, out var dateVal))
                return dateVal;
        }

        // Handle JsonElement
        if (value is JsonElement jsonElement)
        {
            if (targetType == typeof(string))
                return jsonElement.GetString();
            if (targetType == typeof(int) || targetType == typeof(int?))
                return jsonElement.GetInt32();
            if (targetType == typeof(Guid))
            {
                var str = jsonElement.GetString();
                return str != null ? Guid.Parse(str) : Guid.Empty;
            }
            if (targetType == typeof(bool))
                return jsonElement.GetBoolean();
        }

        // Standard conversion
        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return value;
        }
    }

    private static string? FindMatchingKey(Dictionary<string, object?> parameters, McpParameterMetadata parameter)
    {
        return parameters.Keys.FirstOrDefault(key =>
            parameter.Aliases.Any(alias => alias.Equals(NormalizeKey(key), StringComparison.OrdinalIgnoreCase))
            || parameter.Name.Equals(NormalizeKey(key), StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().TrimStart('-');
    }

    // Track temp files for cleanup
    private readonly List<string> _tempFiles = new();

    private void ScheduleCleanup(string tempFile)
    {
        _tempFiles.Add(tempFile);
    }

    /// <summary>
    /// Cleans up any temporary files created during invocation.
    /// </summary>
    public void Cleanup()
    {
        foreach (var file in _tempFiles)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        _tempFiles.Clear();
    }
}
