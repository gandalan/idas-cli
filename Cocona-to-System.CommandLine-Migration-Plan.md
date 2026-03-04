# Migration Plan: Cocona → System.CommandLine

## Overview
This document outlines the plan to migrate the idas-cli project from Cocona 2.2.0 to System.CommandLine while maintaining exact functionality.

---

## Phase 1: Project Setup

### Changes to `idas.csproj`

**Remove:**
```xml
<PackageReference Include="Cocona" Version="2.2.0" />
```

**Add:**
```xml
<PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
<PackageReference Include="System.CommandLine.NamingConventionBinder" Version="2.0.0-beta4.22272.1" />
```

---

## Phase 2: CommonParameters Refactoring

### Current (`Program.cs:49-52`)
```csharp
public record CommonParameters(
    [Option("format", Description = "Output format")] string Format = "json",
    [Option("filename", Description = "Dump output to file")] string? FileName = null
) : ICommandParameterSet;
```

### New
```csharp
public record CommonParameters(
    string Format = "json",
    string? FileName = null
);
```

- Remove `ICommandParameterSet` inheritance (Cocona-specific)
- Will be constructed manually from parsed options in command handlers

---

## Phase 3: Command Architecture Changes

### Current Pattern (Cocona attribute-based)
```csharp
public class VorgangCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList(CommonParameters commonParams,
        [Option("jahr")] int? jahr = null,
        [Option("includeArchive")] bool includeArchive = true) { ... }
}
```

### New Pattern (System.CommandLine explicit)

Create a `CommandBuilder` static class for each command group:

```csharp
public static class VorgangCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("vorgang", "Manage Vorgänge");
        
        // list command
        var listCmd = new Command("list", "List all Vorgänge");
        var jahrOption = new Option<int?>("--jahr", "Year to list (0 = all years)");
        var includeArchiveOption = new Option<bool>("--include-archive", () => true, "Include archived Vorgänge");
        var includeOthersDataOption = new Option<bool>("--include-others-data", () => true, "Include data from other users");
        var includeASPOption = new Option<bool>("--include-asp", () => true, "Include application specific properties");
        var includeAdditionalPropertiesOption = new Option<bool>("--include-additional-properties", () => true, "Include additional properties");
        
        listCmd.AddOption(jahrOption);
        listCmd.AddOption(includeArchiveOption);
        listCmd.AddOption(includeOthersDataOption);
        listCmd.AddOption(includeASPOption);
        listCmd.AddOption(includeAdditionalPropertiesOption);
        
        listCmd.SetHandler(async (format, filename, jahr, includeArchive, includeOthersData, includeASP, includeAdditionalProperties) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.GetList(commonParams, jahr, includeArchive, includeOthersData, includeASP, includeAdditionalProperties);
        }, 
        GlobalOptions.Format, GlobalOptions.FileName, jahrOption, includeArchiveOption, 
        includeOthersDataOption, includeASPOption, includeAdditionalPropertiesOption);
        
        cmd.AddCommand(listCmd);
        
        // get command
        var getCmd = new Command("get", "Get a single Vorgang by GUID");
        var vorgangArg = new Argument<Guid>("vorgang", "Vorgang-GUID");
        getCmd.AddArgument(vorgangArg);
        
        getCmd.SetHandler(async (format, filename, vorgang) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.GetVorgang(commonParams, vorgang);
        },
        GlobalOptions.Format, GlobalOptions.FileName, vorgangArg);
        
        cmd.AddCommand(getCmd);
        
        // put command
        var putCmd = new Command("put", "Create/update a Vorgang from JSON file");
        var fileArg = new Argument<string>("file", "JSON file with Vorgang data");
        putCmd.AddArgument(fileArg);
        
        putCmd.SetHandler(async (format, filename, file) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.PutVorgang(commonParams, file);
        },
        GlobalOptions.Format, GlobalOptions.FileName, fileArg);
        
        cmd.AddCommand(putCmd);
        
        // sample command
        var sampleCmd = new Command("sample", "Create a sample VorgangDTO");
        sampleCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.CreateSample(commonParams);
        },
        GlobalOptions.Format, GlobalOptions.FileName);
        
        cmd.AddCommand(sampleCmd);
        
        // archive command
        var archiveCmd = new Command("archive", "Archive a single Vorgang");
        var archiveVorgangArg = new Argument<Guid>("vorgang", "Vorgang-GUID");
        archiveCmd.AddArgument(archiveVorgangArg);
        
        archiveCmd.SetHandler(async (format, filename, vorgang) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.ArchiveVorgang(commonParams, vorgang);
        },
        GlobalOptions.Format, GlobalOptions.FileName, archiveVorgangArg);
        
        cmd.AddCommand(archiveCmd);
        
        // archive-bulk command
        var archiveBulkCmd = new Command("archive-bulk", "Archive multiple Vorgänge at once");
        var vorgaengeArg = new Argument<string>("vorgaenge", "Comma-separated list of Vorgang-GUIDs");
        archiveBulkCmd.AddArgument(vorgaengeArg);
        
        archiveBulkCmd.SetHandler(async (format, filename, vorgaenge) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.ArchiveVorgangBulk(commonParams, vorgaenge);
        },
        GlobalOptions.Format, GlobalOptions.FileName, vorgaengeArg);
        
        cmd.AddCommand(archiveBulkCmd);
        
        // activate command
        var activateCmd = new Command("activate", "Activate (unarchive) a Vorgang");
        var activateVorgangArg = new Argument<Guid>("vorgang", "Vorgang-GUID");
        activateCmd.AddArgument(activateVorgangArg);
        
        activateCmd.SetHandler(async (format, filename, vorgang) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.ActivateVorgang(commonParams, vorgang);
        },
        GlobalOptions.Format, GlobalOptions.FileName, activateVorgangArg);
        
        cmd.AddCommand(activateCmd);
        
        return cmd;
    }
}
```

### Command Groups to Migrate (16 total)
1. `VorgangCommands` → `VorgangCommandBuilder`
2. `gSQLCommands` → `GSQLCommandBuilder`
3. `KontaktCommands` → `KontaktCommandBuilder`
4. `ArtikelCommands` → `ArtikelCommandBuilder`
5. `AVCommands` → `AVCommandBuilder`
6. `LagerbestandCommands` → `LagerbestandCommandBuilder`
7. `LagerbuchungCommands` → `LagerbuchungCommandBuilder`
8. `WarengruppeCommands` → `WarengruppeCommandBuilder`
9. `BenutzerCommands` → `BenutzerCommandBuilder`
10. `SerieCommands` → `SerieCommandBuilder`
11. `RollenCommands` → `RollenCommandBuilder`
12. `VarianteCommands` → `VarianteCommandBuilder`
13. `UIDefinitionCommands` → `UIDefinitionCommandBuilder`
14. `KonfigSatzCommands` → `KonfigSatzCommandBuilder`
15. `WertelisteCommands` → `WertelisteCommandBuilder`
16. `BelegCommands` → `BelegCommandBuilder`
17. `McpServerCommand` → `McpServerCommandBuilder`

---

## Phase 4: Global Options Setup

Create a `GlobalOptions` class for shared parameters:

```csharp
public static class GlobalOptions
{
    public static Option<string> Format = new("--format", () => "json", "Output format (json, csv, gsql)");
    public static Option<string?> FileName = new("--filename", "Dump output to file");
}
```

---

## Phase 5: Program.cs Rewrite

### Current (`Program.cs:27-47`)
```csharp
var builder = CoconaApp.CreateBuilder(effectiveArgs);
var app = builder.Build();
app.AddSubCommand("vorgang", x => x.AddCommands<VorgangCommands>());
app.AddSubCommand("gsql", x => x.AddCommands<gSQLCommands>());
app.AddSubCommand("kontakt", x => x.AddCommands<KontaktCommands>());
app.AddSubCommand("artikel", x => x.AddCommands<ArtikelCommands>());
app.AddSubCommand("av", x => x.AddCommands<AVCommands>());
app.AddSubCommand("lagerbestand", x => x.AddCommands<LagerbestandCommands>());
app.AddSubCommand("lagerbuchung", x => x.AddCommands<LagerbuchungCommands>());
app.AddSubCommand("warengruppe", x => x.AddCommands<WarengruppeCommands>());
app.AddSubCommand("benutzer", x => x.AddCommands<BenutzerCommands>());
app.AddSubCommand("serie", x => x.AddCommands<SerieCommands>());
app.AddSubCommand("rollen", x => x.AddCommands<RollenCommands>());
app.AddSubCommand("variante", x => x.AddCommands<VarianteCommands>());
app.AddSubCommand("uidefinition", x => x.AddCommands<UIDefinitionCommands>());
app.AddSubCommand("konfigsatz", x => x.AddCommands<KonfigSatzCommands>());
app.AddSubCommand("werteliste", x => x.AddCommands<WertelisteCommands>());
app.AddSubCommand("mcp", x => x.AddCommands<McpServerCommand>());
app.AddSubCommand("beleg", x => x.AddCommands<BelegCommands>());
app.Run();
```

### New
```csharp
using System.CommandLine;

var rootCommand = new RootCommand("IDAS CLI - Command line interface for IDAS/i3 ERP system");

// Add global options
rootCommand.AddGlobalOption(GlobalOptions.Format);
rootCommand.AddGlobalOption(GlobalOptions.FileName);

// Add subcommands
rootCommand.AddCommand(VorgangCommandBuilder.Build());
rootCommand.AddCommand(GSQLCommandBuilder.Build());
rootCommand.AddCommand(KontaktCommandBuilder.Build());
rootCommand.AddCommand(ArtikelCommandBuilder.Build());
rootCommand.AddCommand(AVCommandBuilder.Build());
rootCommand.AddCommand(LagerbestandCommandBuilder.Build());
rootCommand.AddCommand(LagerbuchungCommandBuilder.Build());
rootCommand.AddCommand(WarengruppeCommandBuilder.Build());
rootCommand.AddCommand(BenutzerCommandBuilder.Build());
rootCommand.AddCommand(SerieCommandBuilder.Build());
rootCommand.AddCommand(RollenCommandBuilder.Build());
rootCommand.AddCommand(VarianteCommandBuilder.Build());
rootCommand.AddCommand(UIDefinitionCommandBuilder.Build());
rootCommand.AddCommand(KonfigSatzCommandBuilder.Build());
rootCommand.AddCommand(WertelisteCommandBuilder.Build());
rootCommand.AddCommand(McpServerCommandBuilder.Build());
rootCommand.AddCommand(BelegCommandBuilder.Build());

return await rootCommand.InvokeAsync(effectiveArgs);
```

---

## Phase 6: MCP Integration Update

### File: `Mcp/McpToolRegistrar.cs`

#### Changes needed:

1. **Remove** (line 3):
   ```csharp
   using Cocona;
   ```

2. **Update `ScanCommandType` method** (lines 42-74):
   - Instead of scanning for `[Command]` attribute, scan for methods following naming convention or use a new custom attribute
   - Or scan the CommandBuilder classes for metadata
   
3. **Update `ExtractParameters` method** (lines 76-112):
   - Remove `[Option]` and `[Argument]` attribute reading
   - Use reflection on method parameters directly
   - Maintain parameter name mapping

#### Alternative Approach:
Create a `[CliCommand]` custom attribute to mark methods, preserving similar metadata extraction logic:

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class CliCommandAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; set; }
    
    public CliCommandAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class CliOptionAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class CliArgumentAttribute : Attribute
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

Then update command methods:
```csharp
[CliCommand("list")]
public async Task GetList(
    CommonParameters commonParams,
    [CliOption(Name = "jahr", Description = "Year to list")] int? jahr = null,
    [CliOption(Name = "includeArchive", Description = "Include archived")] bool includeArchive = true)
```

---

## Phase 7: Command Method Signature Updates

All command methods in 16 `*Commands.cs` files need minor updates:

1. **Remove** from each file:
   ```csharp
   using Cocona;
   ```

2. **Methods remain unchanged** - they can stay as:
   ```csharp
   public async Task GetList(CommonParameters commonParams, int? jahr = null, bool includeArchive = true)
   ```

3. **If using custom attributes** (see Phase 6 Alternative), add:
   ```csharp
   [CliCommand("list")]
   public async Task GetList(...)
   ```

---

## Phase 8: Testing & Validation

For each of the 16 command groups, verify:
- [ ] All subcommands work (`list`, `get`, `put`, `sample`, etc.)
- [ ] Options parse correctly with defaults
- [ ] Arguments are positional
- [ ] Global options (`--format`, `--filename`) work everywhere
- [ ] Help text displays correctly (`--help`)
- [ ] MCP tool registration still works
- [ ] Command hierarchy works (e.g., `idas vorgang list --help`)
- [ ] Auto-login fallback still functions

### Test Commands:
```bash
# Help
idas --help
idas vorgang --help
idas vorgang list --help

# Commands with options
idas vorgang list --jahr 2024 --include-archive false
idas vorgang get "guid-here"
idas benutzer login
idas kontakt list --format csv --filename output.csv

# MCP server
idas mcp
```

---

## Phase 9: Cleanup

- [ ] Remove all `using Cocona;` statements
- [ ] Delete any Cocona-specific configuration files
- [ ] Update README/documentation if it mentions Cocona
- [ ] Remove `ICommandParameterSet` usage
- [ ] Verify no Cocona namespaces remain

---

## Estimated Effort

| Phase | Task | Time |
|-------|------|------|
| 1 | Project file updates | 10 min |
| 2 | CommonParameters refactoring | 20 min |
| 3 | Command builders (16 files × 30 min) | 8 hours |
| 4 | Global options setup | 15 min |
| 5 | Program.cs rewrite | 30 min |
| 6 | MCP integration update | 2 hours |
| 7 | Remove Cocona usings | 15 min |
| 8 | Testing & validation | 2-3 hours |
| 9 | Cleanup | 15 min |
| **Total** | | **~13-14 hours** |

---

## Key Architectural Differences

| Feature | Cocona | System.CommandLine |
|---------|--------|-------------------|
| **Command definition** | Attributes (`[Command]`) | Explicit `Command` objects |
| **Options/Args** | Attributes (`[Option]`, `[Argument]`) | `Option<T>` and `Argument<T>` objects |
| **Parameter binding** | Automatic via attributes | `SetHandler()` with lambda |
| **Common parameters** | `ICommandParameterSet` | Manual construction in handler |
| **Discovery** | `AddCommands<T>()` | Manual command tree building |
| **Middleware** | Cocona middleware | `InvocationMiddleware` |
| **Help generation** | Automatic | Automatic |
| **Validation** | Attribute-based | Custom validators or handler logic |

---

## Migration Checklist

### Pre-Migration
- [ ] Create feature branch
- [ ] Backup current codebase
- [ ] Review all command implementations

### Migration Steps
- [ ] Phase 1: Update project file
- [ ] Phase 2: Refactor CommonParameters
- [ ] Phase 3: Create CommandBuilders
  - [ ] VorgangCommandBuilder
  - [ ] GSQLCommandBuilder
  - [ ] KontaktCommandBuilder
  - [ ] ArtikelCommandBuilder
  - [ ] AVCommandBuilder
  - [ ] LagerbestandCommandBuilder
  - [ ] LagerbuchungCommandBuilder
  - [ ] WarengruppeCommandBuilder
  - [ ] BenutzerCommandBuilder
  - [ ] SerieCommandBuilder
  - [ ] RollenCommandBuilder
  - [ ] VarianteCommandBuilder
  - [ ] UIDefinitionCommandBuilder
  - [ ] KonfigSatzCommandBuilder
  - [ ] WertelisteCommandBuilder
  - [ ] BelegCommandBuilder
  - [ ] McpServerCommandBuilder
- [ ] Phase 4: Setup GlobalOptions
- [ ] Phase 5: Rewrite Program.cs
- [ ] Phase 6: Update MCP integration
- [ ] Phase 7: Remove Cocona usings
- [ ] Phase 8: Test all commands
- [ ] Phase 9: Final cleanup

### Post-Migration
- [ ] Run full test suite
- [ ] Update documentation
- [ ] Merge to main branch
- [ ] Tag release

---

## Notes

- System.CommandLine is still in beta (2.0.0-beta4), but is stable for production use
- The naming convention binder package simplifies handler setup
- Consider creating a source generator in the future to auto-generate CommandBuilders from method signatures
- The MCP integration's reflection-based scanning will need the most careful attention
- All async method signatures can remain unchanged - only the calling/binding mechanism changes
