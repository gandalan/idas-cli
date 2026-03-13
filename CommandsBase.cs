using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Reflection;
using System.Collections;
using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;

public class CommandsBase
{
    private static readonly HashSet<Guid> _initializedAppTokens = new();
    private static readonly object _initLock = new();
    private const string TokenFilePath = "token";

    /// <summary>
    /// When true, suppresses all console output (used in MCP server mode)
    /// </summary>
    public static bool IsSilentMode { get; set; } = false;

    protected void SafeLog(string message)
    {
        if (IsSilentMode)
            return;

        try
        {
            Console.WriteLine(message);
        }
        catch (ObjectDisposedException)
        {
            // Console writer already closed - ignore in MCP mode
        }
        catch (IOException)
        {
            // Cannot write to closed TextWriter - ignore in MCP mode
        }
    }

    protected async Task<IWebApiConfig> getSettings(Guid? appGuid = null, string? env = null)
    {
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            throw new InvalidOperationException("Please provide a valid AppGuid via --appguid parameter or IDAS_APPGUID environment variable");
        }

        // Initialize the WebApiConfigurations if not already done for this app token
        await InitializeWebApiConfigurationsAsync(appGuid.Value);

        // Get the settings for the specified environment
        var settings = WebApiConfigurations.ByName(env);
        if (settings == null)
        {
            throw new InvalidOperationException($"Environment '{env}' not found. Available environments: {string.Join(", ", WebApiConfigurations.GetAll().Select(s => s.FriendlyName))}");
        }

        // Try to authenticate using stored token first
        var authResult = await TryAuthenticateWithStoredTokenAsync(settings, appGuid.Value);
        if (authResult.IsSuccessful)
        {
            return settings;
        }

        throw new InvalidOperationException("No valid token found. Run `idas benutzer login-sso` first.");
    }

    private async Task InitializeWebApiConfigurationsAsync(Guid appGuid)
    {
        lock (_initLock)
        {
            if (_initializedAppTokens.Contains(appGuid))
            {
                return;
            }
        }

        await WebApiConfigurations.InitializeAsync(appGuid);

        lock (_initLock)
        {
            _initializedAppTokens.Add(appGuid);
        }
    }

    private async Task<AuthResult> TryAuthenticateWithStoredTokenAsync(IWebApiConfig settings, Guid appGuid)
    {
        if (!File.Exists(TokenFilePath))
        {
            return AuthResult.Failed("No stored token found");
        }

        try
        {
            var tokenJson = await File.ReadAllTextAsync(TokenFilePath);
            var storedToken = JsonSerializer.Deserialize<UserAuthTokenDTO>(tokenJson);

            if (storedToken == null || storedToken.Token == Guid.Empty)
            {
                return AuthResult.Failed("Invalid token file");
            }

            // Configure settings with the stored token
            settings.AuthToken = storedToken;
            settings.AppToken = appGuid;

            // Try to refresh/validate the token using the library's built-in method
            var client = new WebRoutinenBase(settings);
            var loginSuccess = await client.LoginAsync();

            if (loginSuccess)
            {
                // Update settings with the potentially refreshed token
                settings.AuthToken = client.AuthToken;
                // Keep user/pass unchanged for token-only login

                // Persist the updated settings using the library's Save method
                WebApiConfigurations.Save(settings);

                // Save the refreshed token to file
                await SaveTokenAsync(client.AuthToken);

                SafeLog($"Login from stored token successful: User={settings.UserName} Mandant={client.AuthToken?.Mandant?.Name}, Environment={settings.FriendlyName}");
                return AuthResult.Succeeded();
            }

            return AuthResult.Failed("Token validation failed");
        }
        catch (Exception ex)
        {
            SafeLog($"Token authentication failed: {ex.Message}");
            return AuthResult.Failed(ex.Message);
        }
    }

    protected async Task LogoutAsync(string? env = null, Guid? appGuid = null)
    {
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            throw new InvalidOperationException("Please provide a valid AppGuid via --appguid parameter or IDAS_APPGUID environment variable");
        }

        await InitializeWebApiConfigurationsAsync(appGuid.Value);
        var settings = WebApiConfigurations.ByName(env);

        if (settings == null)
        {
            throw new InvalidOperationException($"Environment '{env}' not found. Available environments: {string.Join(", ", WebApiConfigurations.GetAll().Select(s => s.FriendlyName))}");
        }

        if (!File.Exists(TokenFilePath))
        {
            SafeLog("No local token found to log out.");
            return;
        }

        var tokenJson = await File.ReadAllTextAsync(TokenFilePath);
        var storedToken = JsonSerializer.Deserialize<UserAuthTokenDTO>(tokenJson);
        if (storedToken == null || storedToken.Token == Guid.Empty)
        {
            SafeLog("Stored token file is invalid. Clearing local token.");
            File.Delete(TokenFilePath);
            return;
        }

        try
        {
            settings.AuthToken = storedToken;
            settings.AppToken = appGuid.Value;

            var client = new WebRoutinenBase(settings);
            await client.PostAsync("/api/Authenticate/Logout", new { }, null, false, null);
            SafeLog("Logged out on server. Local token removed.");

            if (File.Exists(TokenFilePath))
            {
                File.Delete(TokenFilePath);
            }
        }
        catch (Exception ex)
        {
            SafeLog($"Server logout failed: {ex.Message}. Local token retained.");
        }
    }



    private async Task SaveTokenAsync(UserAuthTokenDTO? authToken)
    {
        if (authToken == null)
        {
            return;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        await File.WriteAllTextAsync(TokenFilePath, JsonSerializer.Serialize(authToken, options));
    }

    protected async Task dumpOutput(CommonParameters commonParameters, object data)
    {
        switch (commonParameters.Format)
        {
            case "json":
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true
                };
                var output = JsonSerializer.Serialize(data, options);
                if (!string.IsNullOrEmpty(commonParameters.FileName))
                {
                    await File.WriteAllTextAsync(commonParameters.FileName, output);
                }
                else
                {
                    // Always write data output, even in silent mode (needed for MCP)
                    try
                    {
                        Console.WriteLine(output);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Console closed - ignore
                    }
                    catch (IOException)
                    {
                        // Cannot write - ignore
                    }
                }
                break;

            default:
                // CSV output - serialize arrays and objects properly
                var csvOutput = ConvertToCsv(data);
                if (!string.IsNullOrEmpty(commonParameters.FileName))
                {
                    await File.WriteAllTextAsync(commonParameters.FileName, csvOutput);
                }
                else
                {
                    // Always write data output, even in silent mode (needed for MCP)
                    try
                    {
                        Console.WriteLine(csvOutput);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Console closed - ignore
                    }
                    catch (IOException)
                    {
                        // Cannot write - ignore
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Converts an object or collection to CSV format using reflection
    /// </summary>
    private string ConvertToCsv(object data)
    {
        if (data == null)
            return string.Empty;

        // Check if data is a collection (but not a string)
        if (data is IEnumerable enumerable && data is not string)
        {
            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }

            if (items.Count == 0)
                return string.Empty;

            return ConvertCollectionToCsv(items);
        }

        // Single object - convert to single-row CSV
        return ConvertSingleObjectToCsv(data);
    }

    /// <summary>
    /// Converts a collection of objects to CSV format
    /// </summary>
    private string ConvertCollectionToCsv(List<object> items)
    {
        var sb = new StringBuilder();

        // Get properties from the first item
        var firstItem = items[0];
        var allProperties = firstItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        // Filter out properties that are complex collections (GetPropertyValue returns null for those)
        var properties = allProperties
            .Where(p => GetPropertyValue(p, firstItem) != null || !IsComplexCollection(p))
            .ToArray();

        if (properties.Length == 0)
            return string.Empty;

        // Write header
        var headers = properties.Select(p => p.Name);
        sb.AppendLine(string.Join(";", headers));

        // Write data rows
        foreach (var item in items)
        {
            var values = properties.Select(p => EscapeCsvValue(GetPropertyValue(p, item) ?? string.Empty));
            sb.AppendLine(string.Join(";", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a single object to CSV format (header + one data row)
    /// </summary>
    private string ConvertSingleObjectToCsv(object item)
    {
        var sb = new StringBuilder();

        var allProperties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        // Filter out properties that are complex collections (GetPropertyValue returns null for those)
        var properties = allProperties
            .Where(p => GetPropertyValue(p, item) != null || !IsComplexCollection(p))
            .ToArray();

        if (properties.Length == 0)
            return item.ToString() ?? string.Empty;

        // Write header
        var headers = properties.Select(p => p.Name);
        sb.AppendLine(string.Join(";", headers));

        // Write data row
        var values = properties.Select(p => EscapeCsvValue(GetPropertyValue(p, item) ?? string.Empty));
        sb.AppendLine(string.Join(";", values));

        return sb.ToString();
    }

    /// <summary>
    /// Checks if a property is a complex collection (non-primitive IEnumerable)
    /// </summary>
    private bool IsComplexCollection(PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        // Check if it's a collection (but not string)
        if (propertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(propertyType))
            return false;

        // Get the element type
        var elementType = GetCollectionElementType(propertyType);

        // If element type is not primitive, it's a complex collection
        return !IsPrimitiveType(elementType);
    }

    /// <summary>
    /// Gets the string value of a property. Returns null for complex collections that should be excluded.
    /// </summary>
    private string? GetPropertyValue(PropertyInfo property, object item)
    {
        try
        {
            var value = property.GetValue(item);
            if (value == null)
                return string.Empty;

            // Check if value is a collection (but not a string)
            if (value is IEnumerable enumerable && value is not string)
            {
                // Get the element type from the collection
                var elementType = GetCollectionElementType(property.PropertyType);

                // If it's a collection of primitive types, serialize as comma-separated values
                if (IsPrimitiveType(elementType))
                {
                    return SerializePrimitiveCollection(enumerable);
                }

                // It's a collection of complex types - return null to indicate exclusion
                return null;
            }

            // Handle special types
            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            if (value is DateTimeOffset dto)
                return dto.ToString("yyyy-MM-dd HH:mm:ss");
            if (value is Guid g)
                return g.ToString("D");
            if (value is bool b)
                return b ? "true" : "false";

            return value.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Determines if a type is a primitive type that should be serialized in CSV
    /// </summary>
    private bool IsPrimitiveType(Type? type)
    {
        if (type == null)
            return false;

        // Unwrap nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Check for primitive types
        if (underlyingType.IsPrimitive)
            return true;

        // Check for additional common types that should be treated as primitives
        if (underlyingType == typeof(string) ||
            underlyingType == typeof(Guid) ||
            underlyingType == typeof(DateTime) ||
            underlyingType == typeof(DateTimeOffset) ||
            underlyingType == typeof(decimal) ||
            underlyingType == typeof(TimeSpan))
        {
            return true;
        }

        // Enum types are also considered primitives for CSV purposes
        if (underlyingType.IsEnum)
            return true;

        return false;
    }

    /// <summary>
    /// Gets the element type of a collection type
    /// </summary>
    private Type? GetCollectionElementType(Type collectionType)
    {
        // Handle arrays
        if (collectionType.IsArray)
            return collectionType.GetElementType();

        // Handle generic collections
        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            if (genericArgs.Length > 0)
                return genericArgs[0];
        }

        // Handle interfaces like IEnumerable<T>
        foreach (var interfaceType in collectionType.GetInterfaces())
        {
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return interfaceType.GetGenericArguments()[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Serializes an IEnumerable of primitive types to a comma-separated string
    /// </summary>
    private string SerializePrimitiveCollection(IEnumerable enumerable)
    {
        var values = new List<string>();

        foreach (var item in enumerable)
        {
            if (item == null)
            {
                values.Add(string.Empty);
                continue;
            }

            // Handle different primitive types
            string formattedValue = item switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss"),
                Guid g => g.ToString("D"),
                bool b => b ? "true" : "false",
                _ => item.ToString() ?? string.Empty
            };

            values.Add(formattedValue);
        }

        // Join with comma and wrap in brackets for clarity
        if (values.Count == 0)
            return "[]";

        return "[" + string.Join(",", values.Select(v => EscapeCsvCollectionValue(v))) + "]";
    }

    /// <summary>
    /// Escapes a value within a collection to ensure it doesn't break the CSV structure
    /// </summary>
    private string EscapeCsvCollectionValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape commas and brackets within the value
        value = value.Replace("\\", "\\\\").Replace(",", "\\,").Replace("[", "\\[").Replace("]", "\\]");

        return value;
    }

    /// <summary>
    /// Escapes a CSV value if it contains special characters
    /// </summary>
    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // If value contains semicolon, quotes, or newlines, wrap in quotes and escape internal quotes
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    /// <summary>
    /// Helper class for authentication results
    /// </summary>
    private class AuthResult
    {
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }

        public static AuthResult Succeeded() => new() { IsSuccessful = true };
        public static AuthResult Failed(string message) => new() { IsSuccessful = false, ErrorMessage = message };
    }

    #region Exception Handling

    /// <summary>
    /// Executes an async action with centralized exception handling.
    /// Returns exit code: 0 for success, 1 for error.
    /// </summary>
    public static async Task<int> ExecuteWithErrorHandling(Func<Task> action)
    {
        try
        {
            await action();
            return 0;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return 1;
        }
    }

    /// <summary>
    /// Executes an async action with centralized exception handling and returns a result.
    /// </summary>
    protected static async Task<T?> ExecuteWithErrorHandling<T>(Func<Task<T>> action) where T : class
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            HandleException(ex);
            return null;
        }
    }

    /// <summary>
    /// Handles exceptions and prints user-friendly error messages.
    /// Suppresses stack traces and internal details.
    /// </summary>
    private static void HandleException(Exception ex)
    {
        // Don't output anything in silent mode (MCP)
        if (IsSilentMode)
        {
            return;
        }

        string userMessage = GetUserFriendlyErrorMessage(ex);
        
        // Output to stderr
        try
        {
            Console.Error.WriteLine($"Error: {userMessage}");
        }
        catch
        {
            // Console might be closed - ignore
        }
    }

    /// <summary>
    /// Extracts a user-friendly error message from an exception.
    /// Handles specific exception types from the IDAS library.
    /// </summary>
    private static string GetUserFriendlyErrorMessage(Exception ex)
    {
        if (ex == null)
        {
            return "An unknown error occurred.";
        }

        // Check for HTTP exceptions and specific error types
        var exceptionType = ex.GetType().Name;
        var message = ex.Message;

        // Handle ApiUnauthorizedException or 401 responses
        if (exceptionType.Contains("Unauthorized") || 
            message.Contains("401") || 
            message.Contains("Unauthorized"))
        {
            return "Authentication failed. Please run 'idas benutzer login' to authenticate.";
        }

        // Handle forbidden/access denied (403)
        if (message.Contains("403") || message.Contains("Forbidden"))
        {
            return "Access denied. You don't have permission to perform this action.";
        }

        // Handle not found (404)
        if (message.Contains("404") || message.Contains("Not Found"))
        {
            return "The requested resource was not found.";
        }

        // Handle server errors (5xx)
        if (message.Contains("500") || message.Contains("Internal Server Error"))
        {
            return "The server encountered an error. Please try again later.";
        }

        // Handle invalid operation exceptions (usually configuration issues)
        if (ex is InvalidOperationException)
        {
            return message;
        }

        // Handle file not found
        if (ex is FileNotFoundException fileEx)
        {
            return $"File not found: {fileEx.FileName}";
        }

        // Handle JSON parsing errors
        if (ex is JsonException)
        {
            return "Invalid JSON format. Please check your input file.";
        }

        // For other exceptions, show the message but truncate if it's too long
        // This prevents stack traces from leaking through
        if (message.Length > 200)
        {
            message = message.Substring(0, 200) + "...";
        }

        // Clean up the message - remove newlines and stack trace indicators
        message = message.Replace("\n", " ").Replace("\r", " ");
        
        // If message looks like a stack trace entry (contains " at " or file paths), provide generic message
        if (message.Contains("   at ") || message.Contains(".cs:line "))
        {
            return "An unexpected error occurred. Please try again.";
        }

        return message;
    }

    #endregion
}
