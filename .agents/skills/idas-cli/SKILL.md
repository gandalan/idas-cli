---
name: idas-cli
description: Execute IDAS/i3 CLI commands to retrieve and manage data from the IDAS ERP system. Use when the user wants to query IDAS data, work with i3 system data, manage Vorgänge, Kontakte, Artikel, Lager, Benutzer, or any other IDAS entity. Triggers on mentions of "IDAS", "i3", "idas-cli", or when working with ERP data from this system.
---

# IDAS CLI Skill

Execute IDAS/i3 CLI commands to interact with the IDAS ERP system.

## Overview

This skill provides access to the idas-cli command-line tool, which interfaces with the IDAS/i3 ERP system. The CLI can be invoked either by using a compiled executable (if available in `dist/`) or by using `dotnet run` as a fallback.

## Quick Start

### Direct Execution

Execute idas commands directly by first checking if the compiled executable exists, otherwise use `dotnet run`:

**Linux/macOS:**
```bash
# Check if executable exists and run it, otherwise use dotnet
if [ -f "dist/idas" ]; then
    ./dist/idas [command] [options]
else
    dotnet run -- [command] [options]
fi
```

**Windows:**
```powershell
# Check if executable exists and run it, otherwise use dotnet
if (Test-Path "dist/idas.exe") {
    .\dist\idas.exe [command] [options]
} else {
    dotnet run -- [command] [options]
}
```

### First-Time Setup: Check Authentication

**Before querying data, ensure authentication is configured:**

1. **Check if .env file exists** (recommended):
   ```bash
   ls -la .env
   ```
   If `.env` exists with credentials, you're ready to query data.

2. **If no .env file, login manually**:
   ```bash
   ./dist/idas benutzer login --user <username> --password <password> --appguid <guid> --env <environment>
   # OR using dotnet run:
   dotnet run -- benutzer login --user <username> --password <password> --appguid <guid> --env <environment>
   ```

### Examples

```bash
# Get help
./dist/idas --help                    # If executable exists
dotnet run -- --help                  # Otherwise

# List all Vorgänge (requires login first)
./dist/idas vorgang list              # If executable exists
dotnet run -- vorgang list            # Otherwise

# Get a specific Vorgang
./dist/idas vorgang get <guid>        # If executable exists
dotnet run -- vorgang get <guid>     # Otherwise

# List contacts
./dist/idas kontakt list              # If executable exists
dotnet run -- kontakt list            # Otherwise

# Get sample data
./dist/idas vorgang sample            # If executable exists
dotnet run -- vorgang sample          # Otherwise
```

## Authentication

**Important:** Before executing any data query command in a new session, authentication is required.

### Automatic Authentication Check

When using this skill, it automatically checks for authentication in this order:

1. **Environment Variables (.env file):** First checks if `.env` file exists in the project root with required credentials
2. **Session Cache:** Checks if a previous login is still valid in the current session
3. **Manual Login:** If neither exists, prompts for manual login via `benutzer login`

### Required Credentials

The IDAS CLI requires these authentication parameters (via .env or command line):
- `IDAS_USER` or `--user` - User name
- `IDAS_PASSWORD` or `--password` - Password
- `IDAS_APP_TOKEN` or `--appguid` - AppGuid/App Token (provided by Gandalan)
- `IDAS_ENV` or `--env` - Environment: `dev`, `staging`, or `produktiv`

### Method 1: .env File (Recommended)

Create a `.env` file in the project root (see `.env.sample` for template):

```bash
IDAS_USER=you@your-provider.de
IDAS_PASSWORD=Y0urP@ssw0rd
IDAS_APP_TOKEN=your-app-token-guid
IDAS_ENV=dev
```

**Important:** When using `dotnet run`, the CLI looks for `.env` in the current working directory. Make sure to run commands from the project root directory where `.env` is located:

```bash
# Navigate to project root first
cd /path/to/idas-cli

# Then run commands
dotnet run -- vorgang list --jahr 2022
```

The CLI automatically loads these on startup - no manual login required.

**Check if .env exists:**
```bash
if [ -f ".env" ]; then
    echo ".env file found - credentials will be loaded automatically"
else
    echo "No .env file found - manual login required"
fi
```

### Method 2: Manual Login

If no .env file exists, use the login command:

```bash
./dist/idas benutzer login --user <username> --password <password> --appguid <guid> --env <environment>
dotnet run -- benutzer login --user <username> --password <password> --appguid <guid> --env <environment>
```

### Session Management

- Credentials from .env are loaded automatically on each command
- Manual login credentials are cached for the duration of the session
- No need to re-authenticate for subsequent commands in the same session
- If authentication expires, the CLI will prompt for re-authentication

## Command Categories

### Data Retrieval Commands

**Vorgang (Transactions):**
- `vorgang list` - List all Vorgänge with filtering options
- `vorgang get <guid>` - Get specific Vorgang by GUID
- `vorgang sample` - Generate sample VorgangDTO

**Kontakt (Contacts):**
- `kontakt list` - List all contacts
- `kontakt get` - Get specific contact
- `kontakt sample` - Generate sample KontaktDTO

**Artikel (Articles/Products):**
- `artikel list` - List all articles
- `artikel sample` - Generate sample KatalogArtikelDTO

**Lager (Inventory):**
- `lagerbestand list` - Get inventory levels
- `lagerbuchung list` - Get inventory bookings
- `lagerbuchung sample` - Generate sample LagerbuchungDTO

**Warengruppe (Product Groups):**
- `warengruppe list` - Get all product groups with their products

### User Management Commands

**Benutzer (Users):**
- `benutzer login` - User login
- `benutzer list` - List own users
- `benutzer password-reset` - Reset password by email
- `benutzer change-password` - Change current user password

**Rollen (Roles):**
- `rollen` - List all roles

### System Configuration Commands

**Serien (Series):**
- `serie list` - List all series
- `serie sample` - Generate sample SerieDTO

**Varianten (Variants):**
- `variante list` - Get all variants
- `variante guids` - Get variant GUIDs

**UI Definitions:**
- `uidefinition list` - Get all UI definitions

**Configuration:**
- `konfigsatz list` - Get all configuration sets
- `werteliste list` - Get all value lists

### Query Commands

**gSQL (Graph SQL):**
- `gsql list` - List available queries
- `gsql get` - Execute a specific query
- `gsql reset` - Reset query cache

### Archive Commands

**Vorgang Archiving:**
- `vorgang archive` - Archive a single Vorgang
- `vorgang archive-bulk` - Archive multiple Vorgänge
- `vorgang activate` - Activate a Vorgang

### Document Commands

**Beleg (Documents):**
- `beleg list` - List documents with filtering options

### MCP Server Commands

**Model Context Protocol:**
- `mcp serve` - Start MCP server
- `mcp generate-tools` - Generate MCP tool source code

## Global Options

All commands support:
- `--format <String>` - Output format (default: json)
- `--filename <String>` - Dump output to file
- `-h, --help` - Show help

## Special Options

**Vorgang list filtering:**
- `--jahr <Int32>` - Filter by year (0 = all)
- `--include-archive=<true|false>` - Include archived (default: true)
- `--include-others-data=<true|false>` - Include others' data (default: true)
- `--include-asp=<true|false>` - Include ASP properties (default: true)
- `--include-additional-properties=<true|false>` - Include additional properties (default: true)

**Beleg list filtering:**
- `--jahr <Int32>` - Filter by year (0 = all)
- `--belegart <String>` - Filter by document type (e.g., AB, Angebot, Rechnung)
- `--include-archive=<true|false>` - Include archived (default: true)
- `--format <String>` - Output format: json or csv (default: json)
- `--separator <String>` - CSV separator (default: ;)

## Working with Data

### Getting JSON Output

Most commands output JSON by default:
```bash
./dist/idas vorgang list > vorgaenge.json
dotnet run -- vorgang list > vorgaenge.json
```

### Getting Sample Data

To understand the data structure:
```bash
./dist/idas vorgang sample
dotnet run -- vorgang sample

./dist/idas kontakt sample
dotnet run -- kontakt sample

./dist/idas artikel sample
dotnet run -- artikel sample
```

### Filtering Results

For Vorgänge, use the filtering options:
```bash
./dist/idas vorgang list --jahr 2024 --include-archive=false
dotnet run -- vorgang list --jahr 2024 --include-archive=false
```

## Execution Methods

Two ways to execute commands:

1. **Compiled Executable:** If `dist/idas` (Linux/macOS) or `dist/idas.exe` (Windows) exists:
   ```bash
   ./dist/idas [command]
   ```

2. **dotnet run:** Fallback when executable not available (requires .NET SDK):
   ```bash
   dotnet run -- [command]
   ```

## Environment Configuration

The CLI requires environment variables for API access (typically via `.env` file):
- Located in project root
- See `.env.sample` for available variables
- Must be configured before running commands

## Error Handling

- Commands return non-zero exit codes on failure
- Error messages go to stderr
- Use `--help` on any command for detailed usage information

### Common Issues

**"Please provide user and password" error:**
This error occurs when the CLI cannot find authentication credentials. Solutions:

1. **Check .env file location:** Ensure `.env` is in the current working directory when running `dotnet run`
   ```bash
   # Wrong - running from different directory
   cd /some/other/path && dotnet run --project /path/to/idas-cli -- vorgang list
   
   # Correct - running from project root
   cd /path/to/idas-cli && dotnet run -- vorgang list
   ```

2. **Use compiled executable:** The executable automatically finds `.env` in the executable directory
   ```bash
   ./dist/idas vorgang list  # Works from any directory
   ```

3. **Pass credentials manually:** Use command-line arguments
   ```bash
   dotnet run -- benutzer login --user <user> --password <pass> --appguid <guid> --env <env>
   ```

4. **Set environment variables:** Export variables in your shell
   ```bash
   export IDAS_USER=your_user
   export IDAS_PASSWORD=your_password
   export IDAS_APP_TOKEN=your_token
   export IDAS_ENV=dev
   dotnet run -- vorgang list
   ```

## Resources

- **Complete Command Reference:** See [references/commands.md](references/commands.md) for all commands and parameters

## Examples

### List all Vorgänge from 2024

```bash
# Using executable
./dist/idas vorgang list --jahr 2024

# Using dotnet run
dotnet run -- vorgang list --jahr 2024
```

### List all AB-Belege from 2024

```bash
# Using executable
./dist/idas beleg list --jahr 2024 --belegart AB

# Using dotnet run
dotnet run -- beleg list --jahr 2024 --belegart AB
```

### Get a specific contact

```bash
./dist/idas kontakt get <contact-guid>
dotnet run -- kontakt get <contact-guid>
```

### Generate sample article data

```bash
./dist/idas artikel sample
dotnet run -- artikel sample
```

### List all product groups

```bash
./dist/idas warengruppe list
dotnet run -- warengruppe list
```

### Check inventory levels

```bash
./dist/idas lagerbestand list
dotnet run -- lagerbestand list
```

### Save output to file

```bash
./dist/idas kontakt list --filename contacts.json
dotnet run -- kontakt list --filename contacts.json
```

## When to Use

Use this skill when:
- User asks about IDAS data or i3 system data
- Need to query Vorgänge, Kontakte, Artikel, Lager, or other IDAS entities
- Working with ERP data from the IDAS system
- User mentions specific IDAS commands or workflows
- Need sample data structures from IDAS
- Archiving or managing Vorgänge

## Integration Notes

The idas-cli is built with .NET/C# and uses Cocona for command-line parsing. It provides structured JSON output suitable for programmatic consumption by LLMs and other tools. The tool requires either a compiled executable in `dist/` or the .NET SDK for `dotnet run`.
