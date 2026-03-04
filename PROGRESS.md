# Progress Log: Cocona → System.CommandLine Migration

## Completed

- [x] Task-018: Runtime MCP Tool Registration
  - Created CliAttributes.cs with [CliCommand], [CliOption], [CliArgument] attributes
  - Created RuntimeMcpToolProvider.cs for runtime tool discovery via reflection
  - Created DynamicMcpToolHandler.cs for dynamic CLI command invocation
  - Updated McpToolRegistrar.cs to use new runtime discovery
  - Build verified - 0 errors, 0 warnings

- [x] Task-002: GlobalOptions and CommonParameters Setup
  - Created GlobalOptions.cs with Format and FileName System.CommandLine options
  - Simplified CommonParameters record (removed ICommandParameterSet and attributes)
  - Build verified with expected Cocona errors

- [x] Task-003: VorgangCommands Migration
  - Created VorgangCommandBuilder.cs with all 7 subcommands (list, get, put, sample, archive, archive-bulk, activate)
  - Updated VorgangCommands.cs to remove Cocona attributes
  - Build verified - no Vorgang-related errors

- [x] Task-004: BenutzerCommands Migration
  - Created BenutzerCommandBuilder.cs with 5 subcommands (login, logout, list, password-reset, change-password)
  - Updated BenutzerCommands.cs to remove Cocona attributes
  - Build verified - no Benutzer-related errors

- [x] Task-005: KontaktCommands Migration
  - Created KontaktCommandBuilder.cs with 4 subcommands (list, get, put, sample)
  - Updated KontaktCommands.cs to remove Cocona usings and attributes
  - Build verified - no Kontakt-related errors

- [x] Task-006: BelegCommands Migration
  - Created BelegCommandBuilder.cs with 'list' subcommand
  - BelegCommands has LOCAL --format and --filename options (not using GlobalOptions)
  - Updated BelegCommands.cs to remove Cocona usings and attributes
  - Build verified - no Beleg-related errors

- [x] Task-007: ArtikelCommands Migration
  - Created ArtikelCommandBuilder.cs with 3 subcommands (list, put, sample)
  - Updated ArtikelCommands.cs to remove Cocona usings and attributes
  - Build verified - no Artikel-related errors

- [x] Task-009: LagerbestandCommands Migration
  - Created LagerbestandCommandBuilder.cs with 'list' subcommand
  - Added --since option (DateTime?, optional)
  - Uses GlobalOptions for --format and --filename
  - Updated LagerbestandCommands.cs to remove Cocona usings and attributes
  - Build verified - no Lagerbestand-related errors

- [x] Task-010: LagerbuchungCommands Migration
  - LagerbuchungCommandBuilder.cs already exists with 3 subcommands (list, put, sample)
  - list: --from, --till options (DateTime)
  - put: file argument (string)
  - sample: no args
  - Updated LagerbuchungCommands.cs to remove Cocona usings and attributes
  - Build verified - no Lagerbuchung-related errors

- [x] Task-008: AVCommands Migration
  - Created AVCommandBuilder.cs with 2 subcommands: list, get
  - list: --since option (DateTime?, optional), uses CommonParameters
  - get: pos argument (string, AVPos-GUID or PCode), uses CommonParameters
  - Updated AVCommands.cs to remove Cocona usings and attributes
  - Build verified - no AV-related errors

- [x] Task-011: WarengruppeCommands Migration
  - Created WarengruppeCommandBuilder.cs with 'list' subcommand
  - list: no args, uses CommonParameters
  - Updated WarengruppeCommands.cs to remove Cocona usings and attributes
  - Build verified - no Warengruppe-related errors

- [x] Task-012: SerieCommands and RollenCommands Migration
  - Created SerieCommandBuilder.cs with 4 subcommands: list, get, put, sample
  - list: no args, uses CommonParameters
  - get: serie argument (Guid)
  - put: file argument (string)
  - sample: no args, uses CommonParameters
  - Created RollenCommandBuilder.cs with 1 subcommand: listrollen
  - Updated both Commands files to remove Cocona usings and attributes
  - Build verified - no Serie or Rollen-related errors

- [x] Task-014: KonfigSatzCommands and WertelisteCommands Migration
  - Created KonfigSatzCommandBuilder.cs with 2 subcommands: list, put
  - list: no args, uses CommonParameters
  - put: file argument (string)
  - Created WertelisteCommandBuilder.cs with 3 subcommands: list, get, put
  - list: --include-auto option (bool, default true), uses CommonParameters
  - get: guid argument (Guid), --include-auto option (bool, default true), uses CommonParameters
  - put: file argument (string)
  - Updated both Commands files to remove Cocona usings and attributes
  - Build verified - no KonfigSatz or Werteliste-related errors

- [x] Task-015: gSQLCommands Migration
  - Created GSQLCommandBuilder.cs with 3 subcommands: list, get, reset
  - list: no args, uses CommonParameters
  - get: beleg argument (Guid), uses CommonParameters (internally overrides Format to "gsql")
  - reset: since argument (DateTime), NO CommonParameters (just uses settings)
  - Updated gSQLCommands.cs to remove Cocona usings and attributes
  - Build verified - no gSQL-related errors

- [x] Task-016: McpServerCommand Migration
  - Created McpServerCommandBuilder.cs with 'serve' subcommand
  - serve: no args, no CommonParameters
  - Sets CommandsBase.IsSilentMode = true
  - Calls McpToolRegistrar.ScanAndRegisterTools(verbose: false)
  - Creates Host with McpServer, WithStdioServerTransport
  - Updated McpServerCommand.cs to remove Cocona using and [Command] attributes
  - Removed GenerateTools() method entirely (including #if DEBUG block)
  - Build verified - no McpServer-related errors

- [x] Task-017: Program.cs Rewrite
  - Replaced `using Cocona;` with `using System.CommandLine;`
  - Replaced CoconaApp builder with System.CommandLine RootCommand
  - Added all 17 command builders (Vorgang, GSQL, Kontakt, Artikel, AV, Lagerbestand, Lagerbuchung, Warengruppe, Benutzer, Serie, Rollen, Variante, UIDefinition, KonfigSatz, Werteliste, McpServer, Beleg)
  - Added global options (--format, --filename) via GlobalOptions
  - Fixed return type to `return await rootCommand.InvokeAsync(effectiveArgs)` returning int
  - Kept configuration loading code (FirstRunManager, env vars, auto-login)
  - Kept CommonParameters record at the end
  - Build verified - 0 errors, 0 warnings

## Current Iteration

- Iteration: 10
- Working on: Task-018: Runtime MCP Tool Registration
- Started: 2026-03-04
- Completed: 2026-03-04

## Task Status

| Task | Description | Status | Iterations |
|------|-------------|--------|------------|
| Task-001 | Project File Migration | ⏳ Pending | - |
| Task-002 | GlobalOptions and CommonParameters Setup | ✅ Complete | 1 |
| Task-003 | VorgangCommands Migration | ✅ Complete | 1 |
| Task-004 | BenutzerCommands Migration | ✅ Complete | 1 |
| Task-005 | KontaktCommands Migration | ✅ Complete | 1 |
| Task-006 | BelegCommands Migration | ✅ Complete | 1 |
| Task-007 | ArtikelCommands Migration | ✅ Complete | 1 |
| Task-008 | AVCommands Migration | ✅ Complete | 1 |
| Task-009 | LagerbestandCommands Migration | ✅ Complete | 1 |
| Task-010 | LagerbuchungCommands Migration | ✅ Complete | 1 |
| Task-011 | WarengruppeCommands Migration | ✅ Complete | 1 |
| Task-012 | SerieCommands and RollenCommands Migration | ✅ Complete | 1 |
| Task-013 | VarianteCommands and UIDefinitionCommands Migration | ⏳ Pending | - |
| Task-014 | KonfigSatzCommands and WertelisteCommands Migration | ✅ Complete | 1 |
| Task-015 | gSQLCommands Migration | ✅ Complete | 1 |
| Task-016 | McpServerCommand Migration (no generate-tools) | ✅ Complete | 1 |
| Task-017 | Program.cs Rewrite | ✅ Complete | 1 |
| Task-018 | Runtime MCP Tool Registration | ✅ Complete | 1 |
| Task-018b | Add CliCommand Attributes to All Methods | 🔄 In Progress | - |
| Task-018c | Remove Source Generation Files | ⏳ Pending | - |
| Task-019 | Final Verification and Testing | ⏳ Pending | - |

## Task Status

| Task | Description | Status | Iterations |
|------|-------------|--------|------------|
| Task-001 | Project File Migration | ⏳ Pending | - |
| Task-002 | GlobalOptions and CommonParameters Setup | ✅ Complete | 1 |
| Task-003 | VorgangCommands Migration | ✅ Complete | 1 |
| Task-004 | BenutzerCommands Migration | ✅ Complete | 1 |
| Task-005 | KontaktCommands Migration | ✅ Complete | 1 |
| Task-006 | BelegCommands Migration | ✅ Complete | 1 |
| Task-007 | ArtikelCommands Migration | ✅ Complete | 1 |
| Task-008 | AVCommands Migration | ✅ Complete | 1 |
| Task-009 | LagerbestandCommands Migration | ✅ Complete | 1 |
| Task-010 | LagerbuchungCommands Migration | ✅ Complete | 1 |
| Task-011 | WarengruppeCommands Migration | ✅ Complete | 1 |
| Task-012 | SerieCommands and RollenCommands Migration | ✅ Complete | 1 |
| Task-013 | VarianteCommands and UIDefinitionCommands Migration | ⏳ Pending | - |
| Task-014 | KonfigSatzCommands and WertelisteCommands Migration | ✅ Complete | 1 |
| Task-015 | gSQLCommands Migration | ✅ Complete | 1 |
| Task-016 | McpServerCommand Migration (no generate-tools) | ✅ Complete | 1 |
| Task-017 | Program.cs Rewrite | ✅ Complete | 1 |
| Task-018 | Runtime MCP Tool Registration (NEW) | ⏳ Pending | - |
| Task-018b | Add CliCommand Attributes to All Methods (NEW) | ⏳ Pending | - |
| Task-018c | Remove Source Generation Files (NEW) | ⏳ Pending | - |
| Task-019 | Final Verification and Testing | ⏳ Pending | - |

## Blockers

- None

## Notes

- Ralph loop initialized
- PRD created: 2026-03-04
- Estimated total iterations: 45-55 (increased due to runtime MCP work)
- Key changes from original plan:
  - REMOVED: Source generation for MCP tools (McpToolSourceGenerator.cs, AutoGeneratedMcpTools.cs)
  - ADDED: Runtime reflection-based MCP tool discovery
  - ADDED: [CliCommand], [CliOption], [CliArgument] attributes on all command methods
  - MCP tools are discovered at server startup, not at build time
