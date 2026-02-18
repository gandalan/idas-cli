using System.Runtime.CompilerServices;
using System.Text.Json;
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
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            throw new InvalidOperationException("Please provide a valid AppGuid via --appguid parameter or IDAS_APP_TOKEN environment variable");
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
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            throw new InvalidOperationException("Please provide a valid AppGuid via --appguid parameter or IDAS_APP_TOKEN environment variable");
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
                // Always write data output, even in silent mode (needed for MCP)
                try
                {
                    Console.WriteLine(data.ToString());
                }
                catch (ObjectDisposedException)
                {
                    // Console closed - ignore
                }
                catch (IOException)
                {
                    // Cannot write - ignore
                }
                break;
        }
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
}
