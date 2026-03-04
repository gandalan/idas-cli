# Feature: Cocona → System.CommandLine Migration

## Overview
Migrate the idas-cli project from Cocona 2.2.0 to System.CommandLine 2.0.0-beta4 while maintaining exact functionality. This involves refactoring 17 command classes, updating the MCP integration, and rewriting the entry point.

## Success Criteria
- [ ] All Cocona dependencies removed
- [ ] System.CommandLine packages installed and configured
- [ ] All 17 command groups migrated with identical functionality
- [ ] MCP server integration updated and working
- [ ] Global options (--format, --filename) work across all commands
- [ ] All commands tested and produce identical output
- [ ] Build succeeds with no warnings
- [ ] Help text displays correctly for all commands

## Tasks

### Task-001: Project File Migration

**Priority**: High
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Remove `<PackageReference Include="Cocona" Version="2.2.0" />`
- [ ] Add `System.CommandLine` version `2.0.0-beta4.22272.1`
- [ ] Add `System.CommandLine.NamingConventionBinder` version `2.0.0-beta4.22272.1`
- [ ] Build succeeds after package swap

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet restore
dotnet build --nologo 2>&1 | head -20
```

---

### Task-002: GlobalOptions and CommonParameters Setup

**Priority**: High
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `GlobalOptions.cs` with `Option<string> Format` (default: "json") and `Option<string?> FileName`
- [ ] Update `CommonParameters` record in `Program.cs` to remove `ICommandParameterSet` inheritance and `[Option]` attributes
- [ ] Keep `CommonParameters` constructor parameters: `string Format = "json"`, `string? FileName = null`

**Files to Modify**:
- Create: `/home/phil/p/idas-cli/GlobalOptions.cs`
- Modify: `/home/phil/p/idas-cli/Program.cs` (lines 49-52)

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo 2>&1 | grep -i error || echo "Build successful"
```

---

### Task-003: VorgangCommands Migration

**Priority**: High
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Create `VorgangCommandBuilder.cs` with static `Build()` method
- [ ] Implement all 7 subcommands: list, get, put, sample, archive, archive-bulk, activate
- [ ] Each command uses `SetHandler()` with proper parameter mapping
- [ ] Options: --jahr, --include-archive, --include-others-data, --include-asp, --include-additional-properties
- [ ] Arguments: vorgang (Guid), file (string), vorgaenge (string for bulk)
- [ ] Remove `using Cocona;` from `VorgangCommands.cs`
- [ ] Remove `[Command]`, `[Option]`, `[Argument]` attributes from `VorgangCommands.cs`

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/VorgangCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/VorgangCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
echo "Test help:"
dotnet run -- vorgang --help 2>&1 | head -10
```

---

### Task-004: BenutzerCommands Migration

**Priority**: High
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `BenutzerCommandBuilder.cs`
- [ ] Implement 4 subcommands: login, logout, list, password-reset, change-password
- [ ] login: --timeout option (int, default 60)
- [ ] password-reset: email argument
- [ ] change-password: username, old-password, new-password arguments
- [ ] Remove Cocona attributes from `BenutzerCommands.cs`
- [ ] Remove `using Cocona;`

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/BenutzerCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/BenutzerCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- benutzer --help 2>&1 | head -10
```

---

### Task-005: KontaktCommands Migration

**Priority**: High
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `KontaktCommandBuilder.cs`
- [ ] Implement 4 subcommands: list, get, put, sample
- [ ] get: kontakt argument (Guid)
- [ ] put: file argument (string)
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/KontaktCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/KontaktCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- kontakt --help 2>&1 | head -10
```

---

### Task-006: BelegCommands Migration

**Priority**: High
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `BelegCommandBuilder.cs`
- [ ] Implement list command with options:
  - --jahr (int, default 0)
  - --format (string, default "json")
  - --separator (string, default ";")
  - --belegart (string?, optional)
  - --filename (string?, optional)
  - --include-archive (bool, default true)
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/BelegCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/BelegCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- beleg --help 2>&1 | head -10
```

---

### Task-007: ArtikelCommands Migration

**Priority**: Medium
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `ArtikelCommandBuilder.cs`
- [ ] Implement all subcommands from original file
- [ ] Map options and arguments correctly
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/ArtikelCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/ArtikelCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- artikel --help 2>&1 | head -10
```

---

### Task-008: AVCommands Migration

**Priority**: Medium
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `AVCommandBuilder.cs`
- [ ] Implement all subcommands from original file
- [ ] Map options and arguments correctly
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/AVCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/AVCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- av --help 2>&1 | head -10
```

---

### Task-009: LagerbestandCommands Migration

**Priority**: Medium
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `LagerbestandCommandBuilder.cs`
- [ ] Implement all subcommands from original file
- [ ] Map options and arguments correctly
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/LagerbestandCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/LagerbestandCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- lagerbestand --help 2>&1 | head -10
```

---

### Task-010: LagerbuchungCommands Migration

**Priority**: Medium
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `LagerbuchungCommandBuilder.cs`
- [ ] Implement all subcommands from original file
- [ ] Map options and arguments correctly
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/LagerbuchungCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/LagerbuchungCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- lagerbuchung --help 2>&1 | head -10
```

---

### Task-011: WarengruppeCommands Migration

**Priority**: Medium
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `WarengruppeCommandBuilder.cs`
- [ ] Implement all subcommands from original file
- [ ] Map options and arguments correctly
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/WarengruppeCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/WarengruppeCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- warengruppe --help 2>&1 | head -10
```

---

### Task-012: SerieCommands and RollenCommands Migration

**Priority**: Medium
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Create `SerieCommandBuilder.cs`
- [ ] Create `RollenCommandBuilder.cs`
- [ ] Implement all subcommands from both files
- [ ] Remove Cocona attributes and usings from both files

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/SerieCommandBuilder.cs`
- Create: `/home/phil/p/idas-cli/CommandBuilders/RollenCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/SerieCommands.cs`
- Modify: `/home/phil/p/idas-cli/Commands/RollenCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- serie --help 2>&1 | head -5
dotnet run -- rollen --help 2>&1 | head -5
```

---

### Task-013: VarianteCommands and UIDefinitionCommands Migration

**Priority**: Medium
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Create `VarianteCommandBuilder.cs`
- [ ] Create `UIDefinitionCommandBuilder.cs`
- [ ] Implement all subcommands from both files
- [ ] Remove Cocona attributes and usings from both files

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/VarianteCommandBuilder.cs`
- Create: `/home/phil/p/idas-cli/CommandBuilders/UIDefinitionCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/VarianteCommands.cs`
- Modify: `/home/phil/p/idas-cli/Commands/UIDefinitionCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- variante --help 2>&1 | head -5
dotnet run -- uidefinition --help 2>&1 | head -5
```

---

### Task-014: KonfigSatzCommands and WertelisteCommands Migration

**Priority**: Medium
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Create `KonfigSatzCommandBuilder.cs`
- [ ] Create `WertelisteCommandBuilder.cs`
- [ ] Implement all subcommands from both files
- [ ] Remove Cocona attributes and usings from both files

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/KonfigSatzCommandBuilder.cs`
- Create: `/home/phil/p/idas-cli/CommandBuilders/WertelisteCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/KonfigSatzCommands.cs`
- Modify: `/home/phil/p/idas-cli/Commands/WertelisteCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- konfigsatz --help 2>&1 | head -5
dotnet run -- werteliste --help 2>&1 | head -5
```

---

### Task-015: gSQLCommands Migration

**Priority**: Medium
**Estimated Iterations**: 2-3

**Acceptance Criteria**:
- [ ] Create `GSQLCommandBuilder.cs`
- [ ] Implement all subcommands from original file
- [ ] Map options and arguments correctly
- [ ] Remove Cocona attributes and using

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/GSQLCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/gSQLCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- gsql --help 2>&1 | head -10
```

---

### Task-016: McpServerCommand Migration (No Code Generation)

**Priority**: High
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Create `McpServerCommandBuilder.cs`
- [ ] Implement ONLY the `serve` command (NO `generate-tools` command)
- [ ] Remove `generate-tools` command and all source generation code
- [ ] Remove Cocona attributes and using from `McpServerCommand.cs`

**Files**:
- Create: `/home/phil/p/idas-cli/CommandBuilders/McpServerCommandBuilder.cs`
- Modify: `/home/phil/p/idas-cli/Commands/McpServerCommand.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- mcp --help 2>&1 | head -10
# Should only show 'serve' command, NOT 'generate-tools'
```

---

### Task-017: Program.cs Rewrite

**Priority**: High
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Replace Cocona app builder with System.CommandLine RootCommand
- [ ] Add GlobalOptions.Format and GlobalOptions.FileName as global options
- [ ] Add all 17 command builders to root command
- [ ] Use `return await rootCommand.InvokeAsync(effectiveArgs);`
- [ ] Remove `using Cocona;`
- [ ] Keep configuration loading and auto-login logic intact

**Files**:
- Modify: `/home/phil/p/idas-cli/Program.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo
dotnet run -- --help 2>&1 | head -20
```

---

### Task-018: Runtime MCP Tool Registration (No Source Generation)

**Priority**: High
**Estimated Iterations**: 5-6

**Acceptance Criteria**:
- [ ] Create custom `[CliCommand]`, `[CliOption]`, `[CliArgument]` attributes in `CliAttributes.cs`
- [ ] Create `RuntimeMcpToolProvider.cs` that discovers tools at runtime using reflection
- [ ] Create `DynamicMcpToolHandler.cs` that invokes CLI commands dynamically via System.CommandLine
- [ ] Update `McpToolRegistrar.cs` to use new runtime discovery instead of Cocona
- [ ] Tools are discovered at server startup by scanning for `[CliCommand]` attributes on methods in `*Commands` classes
- [ ] Each discovered tool creates an MCP tool that invokes the corresponding CLI command
- [ ] Parameter mapping works for options, arguments, and optional parameters
- [ ] Remove `using Cocona;` from `McpToolRegistrar.cs`

**Runtime Discovery Pattern**:
```csharp
// At server startup:
1. Scan assembly for classes inheriting from CommandsBase
2. Find methods with [CliCommand] attribute
3. Build MCP tool metadata from method signatures
4. Create dynamic handlers that invoke CLI commands via rootCommand.InvokeAsync()
```

**Files**:
- Create: `/home/phil/p/idas-cli/CliAttributes.cs`
- Create: `/home/phil/p/idas-cli/Mcp/RuntimeMcpToolProvider.cs`
- Create: `/home/phil/p/idas-cli/Mcp/DynamicMcpToolHandler.cs`
- Modify: `/home/phil/p/idas-cli/Mcp/McpToolRegistrar.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo 2>&1 | grep -i error || echo "Build successful"
```

---

### Task-018b: Add CliCommand Attributes to Command Methods

**Priority**: High
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Add `[CliCommand("list", Description = "...")]` to all list methods in *Commands.cs files
- [ ] Add `[CliCommand("get", Description = "...")]` to all get methods
- [ ] Add `[CliCommand("put", Description = "...")]` to all put methods
- [ ] Add `[CliCommand("sample", Description = "...")]` to all sample methods
- [ ] Add `[CliCommand("archive", Description = "...")]` etc. for other commands
- [ ] Add `[CliOption(Description = "...")]` to parameters where description is needed
- [ ] Add `[CliArgument(Description = "...")]` to argument parameters where needed
- [ ] Do NOT add attributes to internal/private helper methods

**Files to Modify**:
- `/home/phil/p/idas-cli/Commands/VorgangCommands.cs`
- `/home/phil/p/idas-cli/Commands/BenutzerCommands.cs`
- `/home/phil/p/idas-cli/Commands/KontaktCommands.cs`
- `/home/phil/p/idas-cli/Commands/BelegCommands.cs`
- `/home/phil/p/idas-cli/Commands/ArtikelCommands.cs`
- `/home/phil/p/idas-cli/Commands/AVCommands.cs`
- `/home/phil/p/idas-cli/Commands/LagerbestandCommands.cs`
- `/home/phil/p/idas-cli/Commands/LagerbuchungCommands.cs`
- `/home/phil/p/idas-cli/Commands/WarengruppeCommands.cs`
- `/home/phil/p/idas-cli/Commands/SerieCommands.cs`
- `/home/phil/p/idas-cli/Commands/RollenCommands.cs`
- `/home/phil/p/idas-cli/Commands/VarianteCommands.cs`
- `/home/phil/p/idas-cli/Commands/UIDefinitionCommands.cs`
- `/home/phil/p/idas-cli/Commands/KonfigSatzCommands.cs`
- `/home/phil/p/idas-cli/Commands/WertelisteCommands.cs`
- `/home/phil/p/idas-cli/Commands/gSQLCommands.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo 2>&1 | grep -i error || echo "Build successful"
# Verify attributes compile correctly
grep -r "\[CliCommand" Commands/ | wc -l
# Should show ~50+ CliCommand attributes
```

---

### Task-018c: Remove Source Generation Files

**Priority**: High
**Estimated Iterations**: 1-2

**Acceptance Criteria**:
- [ ] Delete `/home/phil/p/idas-cli/Mcp/McpToolSourceGenerator.cs`
- [ ] Delete `/home/phil/p/idas-cli/Mcp/AutoGeneratedMcpTools.cs`
- [ ] Remove any references to `McpToolSourceGenerator` in other files
- [ ] Update `McpServerCommand.cs` to remove generate-tools command entirely
- [ ] Build succeeds without source generation files

**Files to Delete**:
- `/home/phil/p/idas-cli/Mcp/McpToolSourceGenerator.cs`
- `/home/phil/p/idas-cli/Mcp/AutoGeneratedMcpTools.cs`

**Verification**:
```bash
cd /home/phil/p/idas-cli
dotnet build --nologo 2>&1 | grep -i error || echo "Build successful"
# Verify files are gone
ls Mcp/ | grep -E "SourceGenerator|AutoGenerated" || echo "Source generation files removed"
```

---

### Task-019: Final Verification and Testing

**Priority**: High
**Estimated Iterations**: 3-4

**Acceptance Criteria**:
- [ ] Build succeeds with no errors
- [ ] All help commands work: `idas --help`, `idas vorgang --help`, `idas vorgang list --help`
- [ ] Verify no `using Cocona;` statements remain (except possibly in comments)
- [ ] Verify no Cocona package references remain
- [ ] Test at least 3 different command groups manually

**Verification**:
```bash
cd /home/phil/p/idas-cli

# Build check
dotnet build --nologo 2>&1 | grep -E "(error|warning|Build)" | head -20

# Check for remaining Cocona references
grep -r "using Cocona" --include="*.cs" . || echo "No Cocona usings found"
grep -r "Cocona" --include="*.csproj" . || echo "No Cocona in project files"

# Help verification
dotnet run -- --help 2>&1 | head -30
dotnet run -- vorgang list --help 2>&1 | head -20
dotnet run -- benutzer --help 2>&1 | head -10
```

## Technical Constraints

- **Language**: C# 12 / .NET 8.0
- **CLI Framework**: System.CommandLine 2.0.0-beta4.22272.1
- **Binding**: System.CommandLine.NamingConventionBinder 2.0.0-beta4.22272.1
- **Build Tool**: dotnet CLI
- **Testing**: Manual CLI verification

## Architecture Notes

### Pattern: CommandBuilder

Each command group gets a static builder class:

```csharp
public static class VorgangCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("vorgang", "Manage Vorgänge");
        
        var listCmd = new Command("list", "List all Vorgänge");
        var jahrOption = new Option<int?>("--jahr", "Year to list");
        listCmd.AddOption(jahrOption);
        
        listCmd.SetHandler(async (format, filename, jahr) => {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.GetList(commonParams, jahr);
        }, GlobalOptions.Format, GlobalOptions.FileName, jahrOption);
        
        cmd.AddCommand(listCmd);
        return cmd;
    }
}
```

### Global Options Pattern

```csharp
public static class GlobalOptions
{
    public static Option<string> Format = new("--format", () => "json", "Output format");
    public static Option<string?> FileName = new("--filename", "Dump output to file");
}
```

### Custom Attributes for MCP

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class CliCommandAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; set; }
    public CliCommandAttribute(string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Parameter)]
public class CliOptionAttribute : Attribute
{
    public string? Description { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class CliArgumentAttribute : Attribute
{
    public string? Description { get; set; }
}
```

### Runtime MCP Tool Discovery Architecture (NO Source Generation)

```csharp
// 1. At MCP server startup, scan for CliCommand attributes:
var toolMethods = assembly.GetTypes()
    .Where(t => t.IsSubclassOf(typeof(CommandsBase)))
    .SelectMany(t => t.GetMethods())
    .Where(m => m.GetCustomAttribute<CliCommandAttribute>() != null);

// 2. For each method, create an MCP tool:
foreach (var method in toolMethods)
{
    var attr = method.GetCustomAttribute<CliCommandAttribute>();
    var toolName = $"{GetSubCommandName(method.DeclaringType)}_{attr.Name}";
    
    // Create dynamic handler that invokes CLI command
    var handler = new DynamicMcpToolHandler(toolName, method);
    RegisterMcpTool(toolName, attr.Description, handler);
}

// 3. Dynamic invocation via System.CommandLine:
public class DynamicMcpToolHandler
{
    public async Task<object> InvokeAsync(Dictionary<string, object?> parameters)
    {
        // Build command line arguments from parameters
        var args = BuildArgsFromParameters(parameters);
        
        // Invoke rootCommand directly
        var result = await _rootCommand.InvokeAsync(args);
        
        // Capture and return output
        return GetOutputFromInvocation();
    }
}
```

### Key Difference from Old Approach

| Aspect | Old (Source Generation) | New (Runtime Discovery) |
|--------|------------------------|-------------------------|
| Tool discovery | Build-time code generation | Runtime reflection |
| Files generated | AutoGeneratedMcpTools.cs | None |
| Commands available | Fixed at compile time | Discovered dynamically at server start |
| Performance | Slightly faster (pre-generated) | Slight startup overhead (reflection) |
| Flexibility | Requires rebuild for changes | Adapts to code changes automatically |

## Command Groups Reference

| Command File | Builder File | Subcommands |
|--------------|--------------|-------------|
| VorgangCommands.cs | VorgangCommandBuilder.cs | list, get, put, sample, archive, archive-bulk, activate |
| BenutzerCommands.cs | BenutzerCommandBuilder.cs | login, logout, list, password-reset, change-password |
| KontaktCommands.cs | KontaktCommandBuilder.cs | list, get, put, sample |
| ArtikelCommands.cs | ArtikelCommandBuilder.cs | (varies) |
| AVCommands.cs | AVCommandBuilder.cs | (varies) |
| LagerbestandCommands.cs | LagerbestandCommandBuilder.cs | (varies) |
| LagerbuchungCommands.cs | LagerbuchungCommandBuilder.cs | (varies) |
| WarengruppeCommands.cs | WarengruppeCommandBuilder.cs | (varies) |
| SerieCommands.cs | SerieCommandBuilder.cs | (varies) |
| RollenCommands.cs | RollenCommandBuilder.cs | (varies) |
| VarianteCommands.cs | VarianteCommandBuilder.cs | (varies) |
| UIDefinitionCommands.cs | UIDefinitionCommandBuilder.cs | (varies) |
| KonfigSatzCommands.cs | KonfigSatzCommandBuilder.cs | (varies) |
| WertelisteCommands.cs | WertelisteCommandBuilder.cs | (varies) |
| gSQLCommands.cs | GSQLCommandBuilder.cs | (varies) |
| BelegCommands.cs | BelegCommandBuilder.cs | list |
| McpServerCommand.cs | McpServerCommandBuilder.cs | serve |

## Out of Scope

- Source generators for auto-building CommandBuilders (future enhancement)
- Source generators for MCP tools (REMOVED - using runtime discovery instead)
- Middleware pipeline customization
- Custom validators (use handler logic)
- Tab completion enhancements
- Performance optimizations
- Caching of reflected MCP tool metadata (could be added later)

## Notes for RalphExecutor

1. **File Structure**: Create all new files in `/home/phil/p/idas-cli/CommandBuilders/`
2. **Incremental Builds**: Run `dotnet build` after each task to catch errors early
3. **Testing**: Use `dotnet run -- [command] --help` to verify command structure
4. **MCP Integration**: 
   - Tasks 018, 018b, 018c replace the old source generation approach
   - Tools are discovered at RUNTIME using reflection and `[CliCommand]` attributes
   - NO code generation - `McpToolSourceGenerator.cs` and `AutoGeneratedMcpTools.cs` will be DELETED
   - Task-018b is CRITICAL: must add `[CliCommand]` attributes to ALL command methods that need MCP exposure
5. **BelegCommands**: Has local CommonParameters replacement (own --format, --filename) - do not break this
6. **Task Order**: Task-018b (add attributes) should be done BEFORE Task-018c (remove source generation) so MCP keeps working throughout the migration
