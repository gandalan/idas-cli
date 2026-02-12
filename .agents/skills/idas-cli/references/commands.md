# IDAS CLI Commands Reference

Complete reference for all idas CLI commands and their parameters.

## Overview

The idas CLI is a command-line interface for interacting with the IDAS/i3 system. It provides commands for managing:
- Vorgänge (transactions/processes)
- Kontakte (contacts)
- Artikel (articles/products)
- Lager (inventory)
- Benutzer (users)
- And more

## Global Options

All commands support these global options:

- `--format <String>` - Output format (default: json)
- `--filename <String>` - Dump output to file
- `-h, --help` - Show help message
- `--version` - Show version

## Commands

### vorgang

Manage Vorgänge (transactions/processes).

**Subcommands:**
- `list` - List all Vorgänge
- `get` - Get a specific Vorgang by GUID
- `put` - Create or update a Vorgang
- `sample` - Create a sample VorgangDTO
- `archive` - Archive a single Vorgang
- `archive-bulk` - Archive multiple Vorgänge at once
- `activate` - Activate a Vorgang

**vorgang list:**
```
idas vorgang list [options]
```

Options:
- `--jahr <Int32>` - Year to list (0 = all years)
- `--include-archive=<true|false>` - Include archived Vorgänge (default: True)
- `--include-others-data=<true|false>` - Include data from other users (default: True)
- `--include-asp=<true|false>` - Include application specific properties (default: True)
- `--include-additional-properties=<true|false>` - Include additional properties (default: True)

**vorgang get:**
```
idas vorgang get <vorgang> [options]
```

Arguments:
- `vorgang` - Vorgang-GUID (required)

**vorgang put:**
```
idas vorgang put [options]
```

**vorgang sample:**
```
idas vorgang sample [options]
```

**vorgang archive:**
```
idas vorgang archive [options]
```

**vorgang archive-bulk:**
```
idas vorgang archive-bulk [options]
```

**vorgang activate:**
```
idas vorgang activate [options]
```

---

### gsql

Execute gSQL queries.

**Subcommands:**
- `list` - List available queries
- `get` - Execute a specific query
- `reset` - Reset query cache/state

**gsql list:**
```
idas gsql list [options]
```

**gsql get:**
```
idas gsql get [options]
```

**gsql reset:**
```
idas gsql reset [options]
```

---

### kontakt

Manage Kontakte (contacts).

**Subcommands:**
- `list` - List all contacts
- `get` - Get a specific contact
- `put` - Create or update a contact
- `sample` - Create a sample KontaktDTO

**kontakt list:**
```
idas kontakt list [options]
```

**kontakt get:**
```
idas kontakt get [options]
```

**kontakt put:**
```
idas kontakt put [options]
```

**kontakt sample:**
```
idas kontakt sample [options]
```

---

### artikel

Manage Artikel (articles/products).

**Subcommands:**
- `list` - List all articles
- `put` - Create or update an article
- `sample` - Create a sample KatalogArtikelDTO

**artikel list:**
```
idas artikel list [options]
```

**artikel put:**
```
idas artikel put [options]
```

**artikel sample:**
```
idas artikel sample [options]
```

---

### av

Manage AV (likely Auftragsverwaltung - order management).

**Subcommands:**
- `list` - List AV entries
- `get` - Get a specific AV entry

**av list:**
```
idas av list [options]
```

**av get:**
```
idas av get [options]
```

---

### lagerbestand

Manage Lagerbestand (inventory levels).

**Subcommands:**
- `list` - Get the inventory list

**lagerbestand list:**
```
idas lagerbestand list [options]
```

---

### lagerbuchung

Manage Lagerbuchung (inventory bookings).

**Subcommands:**
- `list` - Get the booking list
- `put` - Book inventory
- `sample` - Create a sample LagerbuchungDTO

**lagerbuchung list:**
```
idas lagerbuchung list [options]
```

**lagerbuchung put:**
```
idas lagerbuchung put [options]
```

**lagerbuchung sample:**
```
idas lagerbuchung sample [options]
```

---

### warengruppe

Manage Warengruppen (product groups).

**Subcommands:**
- `list` - Get the list of product groups, including all their products

**warengruppe list:**
```
idas warengruppe list [options]
```

---

### benutzer

Manage Benutzer (users).

**Subcommands:**
- `login` - User login
- `list` - Get the list of own users
- `password-reset` - Reset password for a user by email
- `change-password` - Change password for the current user

**benutzer login:**
```
idas benutzer login [options]
```

**benutzer list:**
```
idas benutzer list [options]
```

**benutzer password-reset:**
```
idas benutzer password-reset [options]
```

**benutzer change-password:**
```
idas benutzer change-password [options]
```

---

### serie

Manage Serien (series).

**Subcommands:**
- `list` - List all series
- `get` - Get a specific series
- `put` - Create or update a series
- `sample` - Create a sample SerieDTO

**serie list:**
```
idas serie list [options]
```

**serie get:**
```
idas serie get [options]
```

**serie put:**
```
idas serie put [options]
```

**serie sample:**
```
idas serie sample [options]
```

---

### rollen

Manage Rollen (roles).

**Usage:**
```
idas rollen [options]
```

Options:
- `--format <String>` - Output format (default: json)
- `--filename <String>` - Dump output to file

---

### variante

Manage Varianten (variants).

**Subcommands:**
- `list` - Get all variants
- `get` - Get a specific variant
- `put` - Create or update a variant
- `guids` - Get variant GUIDs

**variante list:**
```
idas variante list [options]
```

**variante get:**
```
idas variante get [options]
```

**variante put:**
```
idas variante put [options]
```

**variante guids:**
```
idas variante guids [options]
```

---

### uidefinition

Manage UI definitions.

**Subcommands:**
- `list` - Get all UI definitions
- `get` - Get a specific UI definition
- `put` - Create or update a UI definition

**uidefinition list:**
```
idas uidefinition list [options]
```

**uidefinition get:**
```
idas uidefinition get [options]
```

**uidefinition put:**
```
idas uidefinition put [options]
```

---

### konfigsatz

Manage Konfigsätze (configuration sets).

**Subcommands:**
- `list` - Get all configuration sets
- `put` - Create or update a configuration set

**konfigsatz list:**
```
idas konfigsatz list [options]
```

**konfigsatz put:**
```
idas konfigsatz put [options]
```

---

### werteliste

Manage Wertelisten (value lists).

**Subcommands:**
- `list` - Get all value lists
- `get` - Get a specific value list
- `put` - Create or update a value list

**werteliste list:**
```
idas werteliste list [options]
```

**werteliste get:**
```
idas werteliste get [options]
```

**werteliste put:**
```
idas werteliste put [options]
```

---

### mcp

Model Context Protocol server commands.

**Subcommands:**
- `serve` - Start the MCP server with dynamically registered Cocona commands
- `generate-tools` - Generate MCP tool source code from Cocona commands

**mcp serve:**
```
idas mcp serve [options]
```

**mcp generate-tools:**
```
idas mcp generate-tools [options]
```

---

## Common Patterns

### Listing Data

Most entities support listing:
```bash
idas <entity> list
idas <entity> list --format json
idas <entity> list --filename output.json
```

### Getting Specific Items

Get by GUID or ID:
```bash
idas vorgang get <guid>
idas kontakt get <guid>
idas gsql get <query-name>
```

### Creating/Updating Items

Use `put` commands (typically read from stdin or file):
```bash
idas vorgang put < vorgang.json
idas kontakt put < kontakt.json
```

### Getting Sample Data

Generate sample DTOs:
```bash
idas vorgang sample
idas kontakt sample
idas artikel sample
```

## Environment Variables

The idas CLI reads configuration from environment variables (typically via a `.env` file):

- Required for API authentication and endpoint configuration
- See `.env.sample` in the project root for available variables

## Output Formats

- `json` (default) - JSON formatted output
- Other formats may be supported depending on the command

## Error Handling

- Commands return non-zero exit codes on failure
- Error messages are printed to stderr
- Use `--help` on any command for detailed usage information
