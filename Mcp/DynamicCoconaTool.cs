using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace IdasCli.Mcp;

/// <summary>
/// Dynamic MCP tool that invokes Cocona commands via reflection
/// </summary>
[McpServerToolType]
public class DynamicCoconaTool : CommandsBase
{
    private readonly IServiceProvider _serviceProvider;

    public DynamicCoconaTool(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Dynamically invokes a Cocona command method
    /// </summary>
    public async Task<object> InvokeCommand(string toolName, Dictionary<string, object?> parameters)
    {
        var metadata = McpToolRegistrar.GetTool(toolName);
        if (metadata == null)
        {
            return new
            {
                Success = false,
                Error = $"Tool {toolName} not found"
            };
        }

        try
        {
            // Create an instance of the command class
            var commandInstance = Activator.CreateInstance(metadata.CommandType);
            if (commandInstance == null)
            {
                return new
                {
                    Success = false,
                    Error = $"Failed to create instance of {metadata.CommandType.Name}"
                };
            }

            // Prepare method parameters
            var methodParams = PrepareMethodParameters(metadata, parameters);

            // Use output capture to intercept console output
            using var outputCapture = new OutputCapture();

            // Invoke the method
            var result = metadata.Method.Invoke(commandInstance, methodParams);

            // Handle async methods
            if (result is Task task)
            {
                await task;
                
                // Check if there's a captured output
                var capturedOutput = outputCapture.GetCapturedOutput();
                if (!string.IsNullOrEmpty(capturedOutput))
                {
                    // Try to parse as JSON
                    try
                    {
                        var jsonObject = JsonSerializer.Deserialize<object>(capturedOutput);
                        return new
                        {
                            Success = true,
                            Data = jsonObject
                        };
                    }
                    catch
                    {
                        return new
                        {
                            Success = true,
                            Data = capturedOutput
                        };
                    }
                }
            }

            return new
            {
                Success = true,
                Message = $"Command {toolName} executed successfully",
                Output = outputCapture.GetCapturedOutput()
            };
        }
        catch (TargetInvocationException ex)
        {
            var innerException = ex.InnerException ?? ex;
            return new
            {
                Success = false,
                Error = innerException.Message,
                StackTrace = innerException.StackTrace
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
        finally
        {
            // Cleanup temp files
            if (_tempFilesToCleanup.Value != null && _tempFilesToCleanup.Value.Any())
            {
                foreach (var tempFile in _tempFilesToCleanup.Value)
                {
                    try
                    {
                        if (File.Exists(tempFile))
                            File.Delete(tempFile);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
                _tempFilesToCleanup.Value.Clear();
            }
        }
    }

    private object?[] PrepareMethodParameters(CoconaToolMetadata metadata, Dictionary<string, object?> parameters)
    {
        var methodParams = metadata.Method.GetParameters();
        var paramValues = new List<object?>();
        var tempFilesToCleanup = new List<string>();

        foreach (var methodParam in methodParams)
        {
            // Handle CommonParameters
            if (methodParam.ParameterType == typeof(CommonParameters))
            {
                paramValues.Add(new CommonParameters(Format: "json", FileName: null));
                continue;
            }

            // Find the corresponding parameter value
            var paramMetadata = metadata.Parameters.FirstOrDefault(p => 
                p.Name.Equals(methodParam.Name, StringComparison.OrdinalIgnoreCase));

            if (paramMetadata == null)
            {
                // Use default value if available
                paramValues.Add(methodParam.DefaultValue);
                continue;
            }

            // Get the value from parameters dictionary
            var key = parameters.Keys.FirstOrDefault(k => 
                k.Equals(paramMetadata.Name, StringComparison.OrdinalIgnoreCase));

            object? value = null;
            if (key != null && parameters.TryGetValue(key, out var paramValue))
            {
                // Special handling for "put" command file parameters
                // Convert JSON string content to a temporary file
                if (paramMetadata.IsPutFileParameter && paramValue is string jsonContent)
                {
                    var tempFile = Path.GetTempFileName();
                    File.WriteAllText(tempFile, jsonContent);
                    tempFilesToCleanup.Add(tempFile);
                    value = tempFile;
                }
                else
                {
                    value = ConvertParameterValue(paramValue, methodParam.ParameterType);
                }
            }
            else if (methodParam.IsOptional)
            {
                value = methodParam.DefaultValue == DBNull.Value ? null : methodParam.DefaultValue;
            }

            paramValues.Add(value);
        }

        // Store temp files for cleanup (will be handled by caller)
        if (tempFilesToCleanup.Any())
        {
            // Store in thread-local storage for cleanup after invocation
            _tempFilesToCleanup.Value = tempFilesToCleanup;
        }

        return paramValues.ToArray();
    }

    // Thread-local storage for temp files
    private readonly ThreadLocal<List<string>> _tempFilesToCleanup = new(() => new List<string>());

    private object? ConvertParameterValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
            targetType = underlyingType;

        // If value is already the correct type
        if (targetType.IsInstanceOfType(value))
            return value;

        // Handle string to Guid conversion
        if (targetType == typeof(Guid) && value is string guidString)
        {
            if (Guid.TryParse(guidString, out var guid))
                return guid;
        }

        // Handle string to int conversion
        if (targetType == typeof(int) && value is string intString)
        {
            if (int.TryParse(intString, out var intValue))
                return intValue;
        }

        // Handle JSON element to string
        if (value is JsonElement jsonElement)
        {
            if (targetType == typeof(string))
                return jsonElement.GetString();
            if (targetType == typeof(int) || targetType == typeof(int?))
                return jsonElement.GetInt32();
            if (targetType == typeof(Guid))
                return jsonElement.GetGuid();
            if (targetType == typeof(bool))
                return jsonElement.GetBoolean();
        }

        // Try standard conversion
        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return value;
        }
    }
}
