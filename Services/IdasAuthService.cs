using Gandalan.IDAS.Client.Contracts.Contracts;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;
using Microsoft.Extensions.Logging;
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

        var authResult = await TryAuthenticateWithStoredTokenAsync(settings, appGuid.Value);
        if (authResult.IsSuccessful)
        {
            return settings;
        }

        throw new InvalidOperationException("No valid token found. Run `idas benutzer login` first.");
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
            settings.AppToken = appGuid;

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
