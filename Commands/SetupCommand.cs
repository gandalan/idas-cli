using System.Diagnostics;
using IdasCli.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace IdasCli.Commands;

/// <summary>
/// Interactive setup: asks for environment, AppToken and login method (SSO or classic AuthToken),
/// persists the configuration to a .env file and logs the user in.
/// </summary>
public class SetupCommand : AsyncCommand<SetupCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly ILogger<SetupCommand> _logger;

    public SetupCommand(IIdasAuthService authService, ILogger<SetupCommand> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("--appguid")]
        public string? AppGuid { get; set; }

        [CommandOption("--env")]
        public string? Env { get; set; }

        [CommandOption("--method")]
        public string? Method { get; set; }

        [CommandOption("--token")]
        public string? Token { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Environment
            var env = settings.Env;
            if (string.IsNullOrWhiteSpace(env))
            {
                env = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Select [green]environment[/]:")
                    .AddChoices("prod", "stg", "dev"));
            }

            // 2. Login method
            var method = settings.Method?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(method))
            {
                var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                    .Title("Select [green]login method[/]:")
                    .AddChoices("SSO (browser)", "Classic AuthToken"));
                method = choice.StartsWith("SSO") ? "sso" : "token";
            }

            // Optional explicit AppToken override (only prompted for SSO, which needs it).
            Guid? appGuid = null;
            if (!string.IsNullOrWhiteSpace(settings.AppGuid))
            {
                if (!Guid.TryParse(settings.AppGuid, out var parsedAppGuid))
                {
                    AnsiConsole.MarkupLine("[red]Invalid AppGuid provided via --appguid[/]");
                    return 1;
                }
                appGuid = parsedAppGuid;
            }

            AuthResult result;
            Guid resolvedAppToken;

            if (method == "token")
            {
                // The classic AuthToken already contains the AppToken - no need to ask for it.
                var tokenInput = settings.Token;
                if (string.IsNullOrWhiteSpace(tokenInput))
                {
                    tokenInput = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]AuthToken[/]:")
                        .Secret()
                        .Validate(v => Guid.TryParse(v, out _)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Not a valid GUID[/]")));
                }
                if (!Guid.TryParse(tokenInput, out var authToken))
                {
                    AnsiConsole.MarkupLine("[red]Invalid AuthToken[/]");
                    return 1;
                }

                result = await _authService.LoginWithAuthTokenAsync(authToken, appGuid, env);
                resolvedAppToken = result.AppToken;
            }
            else if (method == "sso")
            {
                // SSO needs an AppToken up front.
                if (appGuid == null)
                {
                    var appGuidInput = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]AppToken (AppGuid)[/]:")
                        .Validate(v => Guid.TryParse(v, out _)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Not a valid GUID[/]")));
                    appGuid = Guid.Parse(appGuidInput);
                }

                bool OpenBrowser(string url)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Could not open browser automatically: {ex.Message}");
                        _logger.LogInformation($"Please open the SSO URL manually to continue the login: {url}");
                        return false;
                    }
                }

                AnsiConsole.MarkupLine("[grey]Starting SSO login flow...[/]");
                result = await _authService.LoginWithSsoAsync(appGuid, env, log: msg => _logger.LogInformation(msg), openBrowser: OpenBrowser);
                resolvedAppToken = appGuid.Value;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Unknown login method '{method}'. Use 'sso' or 'token'.[/]");
                return 1;
            }

            if (!result.IsSuccessful)
            {
                AnsiConsole.MarkupLine($"[red]Login failed: {result.ErrorMessage}[/]");
                return 1;
            }

            // Persist config (env + resolved AppToken) so subsequent invocations pick it up.
            if (resolvedAppToken != Guid.Empty)
            {
                var envFile = FirstRunManager.PersistConfiguration(resolvedAppToken.ToString(), env);
                Environment.SetEnvironmentVariable("IDAS_APPGUID", resolvedAppToken.ToString());
                Environment.SetEnvironmentVariable("IDAS_ENV", env);
                AnsiConsole.MarkupLine($"[grey]Configuration saved to {envFile}[/]");
            }

            AnsiConsole.MarkupLine($"[green]Setup complete[/] — logged in as [bold]{result.UserName}[/] (Mandant: {result.MandantName})");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}
