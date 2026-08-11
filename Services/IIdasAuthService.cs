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
    /// Logs in interactively via the SSO browser flow and persists the resulting token
    /// </summary>
    Task<AuthResult> LoginWithSsoAsync(Guid? appGuid = null, string? env = null, int timeout = 60, Action<string>? log = null, Func<string, bool>? openBrowser = null);

    /// <summary>
    /// Logs in non-interactively using an existing classic IDAS AuthToken and persists it
    /// </summary>
    Task<AuthResult> LoginWithAuthTokenAsync(Guid authToken, Guid? appGuid = null, string? env = null);

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
    public string? UserName { get; init; }
    public string? MandantName { get; init; }
    public Guid AppToken { get; init; }

    public static AuthResult Succeeded(string? userName = null, string? mandantName = null, Guid appToken = default)
        => new() { IsSuccessful = true, UserName = userName, MandantName = mandantName, AppToken = appToken };
    public static AuthResult Failed(string message) => new() { IsSuccessful = false, ErrorMessage = message };
}

