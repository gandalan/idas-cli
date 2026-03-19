namespace IdasCli.Sidecars;

public sealed record SidecarDescriptor(
    string CommandName,
    string ExecutablePath,
    string DisplayName,
    string Description);
