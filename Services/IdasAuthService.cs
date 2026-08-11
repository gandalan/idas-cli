using Gandalan.IDAS.Client.Contracts.Contracts;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.Client.SSO;
using Gandalan.IDAS.WebApi.DTO;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace IdasCli.Services;

/// <summary>
/// Implementation of IDAS authentication service
/// </summary>
public class IdasAuthService(ILogger<IdasAuthService> logger) : IIdasAuthService
{
    private static readonly HashSet<Guid> _initializedAppTokens = [];
    private static readonly object _initLock = new();
    private const string TokenFilePath = "token";

    public async Task<IWebApiConfig> GetSettingsAsync(Guid? appGuid = null, string? env = null)
    {
        env ??= Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        // AppToken is optional here: URLs are resolved by env, and the AppToken is carried in
        // the stored token (see TryAuthenticateWithStoredTokenAsync). It is only required for
        // the interactive SSO login, not for token-based sessions.
        appGuid ??= Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

        await InitializeWebApiConfigurationsAsync(appGuid.Value);

        var settings = WebApiConfigurations.ByName(env);
        if (settings == null)
        {
            throw new InvalidOperationException($"Environment '{env}' not found. Available environments: {string.Join(", ", WebApiConfigurations.GetAll().Select(s => s.FriendlyName))}");
        }

        var authResult = await TryAuthenticateWithStoredTokenAsync(settings, appGuid.Value);
        if (authResult.IsSuccessful)
        {
            // Exchange the (long-lived) classic AuthToken for a fresh JWT on every run.
            // Returning an IJwtWebApiConfig makes WebRoutinenBase authenticate via Bearer,
            // so all commands transparently use the JWT without any further changes.
            return await settings.ToJwtWebApiSettings();
        }

        throw new InvalidOperationException("No valid token found. Run `idas benutzer login` first.");
    }

    public async Task<AuthResult> LoginWithAuthTokenAsync(Guid authToken, Guid? appGuid = null, string? env = null)
    {
        env ??= Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        // AppToken is optional: the environment URLs are resolved by env, and the AppToken
        // (plus Mandant) is contained in the classic token and returned by the backend.
        appGuid ??= Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

        if (authToken == Guid.Empty)
        {
            return AuthResult.Failed("Please provide a valid AuthToken");
        }

        await InitializeWebApiConfigurationsAsync(appGuid.Value);

        var settings = WebApiConfigurations.ByName(env);
        if (settings == null)
        {
            return AuthResult.Failed($"Environment '{env}' not found. Available environments: {string.Join(", ", WebApiConfigurations.GetAll().Select(s => s.FriendlyName))}");
        }

        settings.AuthToken = new UserAuthTokenDTO { Token = authToken };

        // Validate and enrich the classic AuthToken directly against the backend.
        // We call /api/Login/Update explicitly instead of LoginAsync()/RefreshTokenAsync():
        // for a bare token GUID (Expires unset) LoginAsync makes no backend call at all, and
        // RefreshTokenAsync swallows the exception - a direct PutAsync surfaces the real error.
        UserAuthTokenDTO? full;
        try
        {
            var client = new WebRoutinenBase(settings);
            full = await client.PutAsync<UserAuthTokenDTO>("/api/Login/Update", new UserAuthTokenDTO { Token = authToken }, null, skipAuth: true);
        }
        catch (Exception ex)
        {
            return AuthResult.Failed($"AuthToken validation failed: {ex.Message}");
        }

        if (full == null || full.Token == Guid.Empty)
        {
            return AuthResult.Failed("AuthToken validation failed: backend returned no valid token");
        }

        settings.AuthToken = full;
        settings.AppToken = full.AppToken;
        WebApiConfigurations.Save(settings);
        await SaveTokenAsync(full);

        logger.LogInformation("Login via AuthToken successful: User={UserName} Mandant={MandantName} AppToken={AppToken}, Environment={Environment}",
            settings.UserName, full.Mandant?.Name, full.AppToken, settings.FriendlyName);
        return AuthResult.Succeeded(settings.UserName, full.Mandant?.Name, full.AppToken);
    }

    public async Task<AuthResult> LoginWithSsoAsync(Guid? appGuid = null, string? env = null, int timeout = 60, Action<string>? log = null, Func<string, bool>? openBrowser = null)
    {
        env ??= Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        appGuid ??= Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            return AuthResult.Failed("Please provide a valid AppGuid via --appguid parameter or IDAS_APPGUID environment variable");
        }

        await InitializeWebApiConfigurationsAsync(appGuid.Value);

        var settings = WebApiConfigurations.ByName(env);
        if (settings == null)
        {
            return AuthResult.Failed($"Environment '{env}' not found. Available environments: {string.Join(", ", WebApiConfigurations.GetAll().Select(s => s.FriendlyName))}");
        }

        log ??= _ => { };
        openBrowser ??= DefaultOpenBrowser;

        var ssoService = new SsoLoginService(settings, timeout);
        var result = await ssoService.LoginAsync(appGuid.Value, log, openBrowser);

        if (!result.Success)
        {
            return AuthResult.Failed(result.ErrorMessage ?? "SSO login failed");
        }

        settings.AuthToken = result.AuthToken;
        WebApiConfigurations.Save(settings);
        await SaveTokenAsync(result.AuthToken);

        logger.LogInformation("SSO login successful: User={UserName} Mandant={MandantName}, Environment={Environment}",
            result.UserName, result.MandantName, settings.FriendlyName);
        return AuthResult.Succeeded(result.UserName, result.MandantName);
    }

    private static bool DefaultOpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync(string? env = null, Guid? appGuid = null)
    {
        env ??= Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        appGuid ??= Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

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
            logger.LogInformation("No local token found to log out.");
            return;
        }

        var tokenJson = await File.ReadAllTextAsync(TokenFilePath);
        var storedToken = JsonSerializer.Deserialize<UserAuthTokenDTO>(tokenJson);
        if (storedToken == null || storedToken.Token == Guid.Empty)
        {
            logger.LogWarning("Stored token file is invalid. Clearing local token.");
            File.Delete(TokenFilePath);
            return;
        }

        try
        {
            settings.AuthToken = storedToken;
            settings.AppToken = appGuid.Value;

            var client = new WebRoutinenBase(settings);
            await client.PostAsync("/api/Authenticate/Logout", new { }, null, false, null);
            logger.LogInformation("Logged out on server. Local token removed.");

            if (File.Exists(TokenFilePath))
            {
                File.Delete(TokenFilePath);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Server logout failed: {Message}. Local token retained.", ex.Message);
        }
    }

    public async Task<AuthResult> TryAuthenticateWithStoredTokenAsync(IWebApiConfig settings, Guid appGuid)
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

            settings.AuthToken = storedToken;
            // Derive the AppToken from the stored token when none was provided explicitly.
            settings.AppToken = appGuid != Guid.Empty ? appGuid : storedToken.AppToken;

            var client = new WebRoutinenBase(settings);
            var loginSuccess = await client.LoginAsync();

            if (loginSuccess)
            {
                settings.AuthToken = client.AuthToken;
                WebApiConfigurations.Save(settings);
                await SaveTokenAsync(client.AuthToken);

                logger.LogInformation("Login from stored token successful: User={UserName} Mandant={MandantName}, Environment={Environment}", 
                    settings.UserName, client.AuthToken?.Mandant?.Name, settings.FriendlyName);
                return AuthResult.Succeeded();
            }

            return AuthResult.Failed("Token validation failed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Token authentication failed: {Message}", ex.Message);
            return AuthResult.Failed(ex.Message);
        }
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

    private async Task SaveTokenAsync(UserAuthTokenDTO? authToken)
    {
        if (authToken == null)
        {
            return;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(TokenFilePath, JsonSerializer.Serialize(authToken, options));
    }
}
