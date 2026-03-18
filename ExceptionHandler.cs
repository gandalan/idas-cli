using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Centralized exception handling for IDAS CLI commands
/// </summary>
public static class ExceptionHandler
{
    /// <summary>
    /// Executes an async action with centralized exception handling.
    /// Returns exit code: 0 for success, 1 for error.
    /// </summary>
    public static async Task<int> ExecuteWithErrorHandling(Func<Task> action, ILogger? logger = null)
    {
        try
        {
            await action();
            return 0;
        }
        catch (Exception ex)
        {
            HandleException(ex, logger);
            return 1;
        }
    }

    /// <summary>
    /// Executes an async action with centralized exception handling and returns a result.
    /// </summary>
    public static async Task<T?> ExecuteWithErrorHandling<T>(Func<Task<T>> action, ILogger? logger = null) where T : class
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            HandleException(ex, logger);
            return null;
        }
    }

    /// <summary>
    /// Handles exceptions by logging them via ILogger
    /// </summary>
    private static void HandleException(Exception ex, ILogger? logger)
    {
        var userMessage = GetUserFriendlyErrorMessage(ex);
        
        // Always log via ILogger (goes to file, not console)
        logger?.LogError(ex, "Command failed: {Message}", userMessage);
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
        if (message.Length > 200)
        {
            message = message.Substring(0, 200) + "...";
        }

        // Clean up the message
        message = message.Replace("\n", " ").Replace("\r", " ");
        
        // If message looks like a stack trace entry, provide generic message
        if (message.Contains("   at ") || message.Contains(".cs:line "))
        {
            return "An unexpected error occurred. Please try again.";
        }

        return message;
    }
}
