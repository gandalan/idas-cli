using System.Text.Json;

// Sample sidecar for IDAS CLI
// This demonstrates the sidecar protocol and context handling

if (Environment.GetEnvironmentVariable("IDAS_SIDECAR_DESCRIBE") == "1")
{
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        Description = "Beispiel-Sidecar fuer Diagnose, Argumentweitergabe und Host-Dispatch."
    }));
    return 0;
}

var invocationContext = await ReadInvocationContextAsync();

// Simple command dispatch based on first argument
var command = args.Length > 0 ? args[0].ToLowerInvariant() : "help";

switch (command)
{
    case "diagnose":
        await RunDiagnoseCommand(args.Skip(1).ToArray());
        break;
    case "argumente":
        await RunArgumenteCommand(args.Skip(1).ToArray());
        break;
    case "hallo":
        await RunHalloCommand(args.Skip(1).ToArray());
        break;
    case "help":
    default:
        ShowHelp();
        break;
}

async Task RunDiagnoseCommand(string[] cmdArgs)
{
    var asText = cmdArgs.Contains("--text") || cmdArgs.Contains("-t");

    var payload = new SidecarDiagnosticResult(
        CommandName: "beispiel",
        CurrentDirectory: invocationContext.WorkingDirectory ?? Directory.GetCurrentDirectory(),
        ExecutableDirectory: AppContext.BaseDirectory,
        Environment: invocationContext.Environment ?? Environment.GetEnvironmentVariable("IDAS_ENV"),
        AppGuid: invocationContext.AppGuid ?? Environment.GetEnvironmentVariable("IDAS_APPGUID"),
        TokenFilePresent: !string.IsNullOrWhiteSpace(invocationContext.TokenJson) || File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "token")),
        ContextFromStdin: Environment.GetEnvironmentVariable("IDAS_SIDECAR_CONTEXT_STDIN") == "1",
        SidecarPath: Environment.ProcessPath,
        TimestampUtc: DateTime.UtcNow);

    if (asText)
    {
        Console.WriteLine($"Kommando: {payload.CommandName}");
        Console.WriteLine($"Arbeitsverzeichnis: {payload.CurrentDirectory}");
        Console.WriteLine($"Executable-Verzeichnis: {payload.ExecutableDirectory}");
        Console.WriteLine($"IDAS_ENV: {payload.Environment ?? "<nicht gesetzt>"}");
        Console.WriteLine($"IDAS_APPGUID: {payload.AppGuid ?? "<nicht gesetzt>"}");
        Console.WriteLine($"Token-Datei vorhanden: {payload.TokenFilePresent}");
        Console.WriteLine($"Kontext via stdin: {payload.ContextFromStdin}");
        Console.WriteLine($"Sidecar-Pfad: {payload.SidecarPath ?? "<unbekannt>"}");
        Console.WriteLine($"Zeitstempel UTC: {payload.TimestampUtc:O}");
        return;
    }

    Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}

async Task RunArgumenteCommand(string[] cmdArgs)
{
    var gross = cmdArgs.Contains("--gross") || cmdArgs.Contains("-g");
    var werte = cmdArgs.Where(arg => !arg.StartsWith("-")).ToArray();

    var normalized = gross
        ? werte.Select(value => value.ToUpperInvariant()).ToArray()
        : werte;

    var payload = new SidecarArgumentResult(
        CommandName: "beispiel",
        ReceivedArguments: normalized,
        Count: normalized.Length);

    Console.WriteLine(JsonSerializer.Serialize(payload, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}

async Task RunHalloCommand(string[] cmdArgs)
{
    var name = cmdArgs.Length > 0 ? cmdArgs[0] : "Welt";
    Console.WriteLine($"Hallo {name}, dieses Kommando kommt aus dem Sidecar 'idas-beispiel'.");
}

void ShowHelp()
{
    Console.WriteLine("Beispiel-Sidecar fuer IDAS CLI");
    Console.WriteLine();
    Console.WriteLine("Verwendung: idas beispiel <befehl> [optionen]");
    Console.WriteLine();
    Console.WriteLine("Befehle:");
    Console.WriteLine("  diagnose           Zeigt Diagnoseinformationen zur Sidecar-Ausfuehrung an");
    Console.WriteLine("    --text, -t       Gibt die Diagnose als lesbaren Text statt als JSON aus");
    Console.WriteLine();
    Console.WriteLine("  argumente [werte]  Gibt die an das Sidecar weitergereichten Argumente zurueck");
    Console.WriteLine("    --gross, -g      Gibt die Argumente in Grossbuchstaben aus");
    Console.WriteLine();
    Console.WriteLine("  hallo [name]       Zeigt eine einfache Begruessung");
    Console.WriteLine();
    Console.WriteLine("  help               Zeigt diese Hilfe an");
}

return 0;

static async Task<ExampleInvocationContext> ReadInvocationContextAsync()
{
    if (Environment.GetEnvironmentVariable("IDAS_SIDECAR_CONTEXT_STDIN") == "1")
    {
        var input = await Console.In.ReadToEndAsync();
        if (!string.IsNullOrWhiteSpace(input))
        {
            var context = JsonSerializer.Deserialize<ExampleInvocationContext>(input);
            if (context != null)
            {
                return context;
            }
        }
    }

    return new ExampleInvocationContext(
        AppGuid: Environment.GetEnvironmentVariable("IDAS_APPGUID"),
        Environment: Environment.GetEnvironmentVariable("IDAS_ENV"),
        TokenJson: File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "token")) ? File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "token")) : null,
        WorkingDirectory: Directory.GetCurrentDirectory(),
        TimestampUtc: DateTime.UtcNow);
}

internal sealed record SidecarDiagnosticResult(
    string CommandName,
    string CurrentDirectory,
    string ExecutableDirectory,
    string? Environment,
    string? AppGuid,
    bool TokenFilePresent,
    bool ContextFromStdin,
    string? SidecarPath,
    DateTime TimestampUtc);

internal sealed record SidecarArgumentResult(
    string CommandName,
    string[] ReceivedArguments,
    int Count);

internal sealed record ExampleInvocationContext(
    string? AppGuid,
    string? Environment,
    string? TokenJson,
    string? WorkingDirectory,
    DateTime TimestampUtc);
