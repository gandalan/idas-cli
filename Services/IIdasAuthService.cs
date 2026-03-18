using Gandalan.IDAS.Client.Contracts.Contracts;

namespace IdasCli.Services;

/// <summary>
/// Service for IDAS authentication and settings management
/// </summary>
public interface IIdasAuthService
{
    /// <summary>
    /// Gets the WebApi configuration with authentication
    /// </summary>
    Task<IWebApiConfig> GetSettingsAsync(Guid? appGuid = null, string? env = null);
    
    /// <summary>
    /// Logs out the current user and clears the token
    /// </summary>
    Task LogoutAsync(string? env = null, Guid? appGuid = null);
    
    /// <summary>
    /// Tries to authenticate with a stored token
    /// </summary>
    Task<AuthResult> TryAuthenticateWithStoredTokenAsync(IWebApiConfig settings, Guid appGuid);
}

/// <summary>
/// Result of an authentication attempt
/// </summary>
public class AuthResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }

    public static AuthResult Succeeded() => new() { IsSuccessful = true };
    public static AuthResult Failed(string message) => new() { IsSuccessful = false, ErrorMessage = message };
}

