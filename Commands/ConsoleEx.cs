using Spectre.Console;

namespace IdasCli.Commands;

/// <summary>
/// Shared console for interactive prompts and human-facing status/error messages.
/// It writes to <c>stderr</c> so that <c>stdout</c> stays reserved exclusively for the
/// parameter-controlled data output (--format / --filename). Any command that prompts the
/// user must route its interaction and status here, and its data through <see cref="Services.IOutputService"/>.
/// </summary>
internal static class ConsoleEx
{
    public static IAnsiConsole Status { get; } = AnsiConsole.Create(new AnsiConsoleSettings
    {
        Out = new AnsiConsoleOutput(Console.Error),
        Interactive = InteractionSupport.Yes
    });
}
