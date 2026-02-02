# `idas-cli` Utility

## Quick start

Check out and compile with .NET 8 installed, or download one of the releases.

### Authentication/Login

At least once you need to login to IDAS. After a successful login, a file named `token` will be created in your working directory containing the `AuthToken`. After the token is expired, you will need to login again.

```
idas benutzer login --user xxx --password yyy --appguid 123 --env dev
```

You can save a lot of time and hassle if you configure a `.env` file, a sample is included. That reduces the login command to 

```
idas benutzer login
```

## Usage

```
idas --help
```

lists all available commands, and --help after a command will list the command's arguments and options:

```
idas vorgang --help
idas vorgang get --help
```

## MCP Server Mode

The `idas` CLI can run as a Model Context Protocol (MCP) server, allowing AI assistants and other MCP clients to interact with IDAS programmatically.

### Starting the MCP Server

```
idas mcp serve
```

This starts the MCP server using stdio transport, which is the standard way for MCP clients to communicate with the server.

### Configuring in Claude Desktop / VS Code

Add the following to your MCP client configuration:

**Claude Desktop** (`claude_desktop_config.json`):
```json
{
  "mcpServers": {
    "idas": {
      "command": "idas",
      "args": ["mcp", "serve"]
    }
  }
}
```

**VS Code** (`.vscode/mcp.json` or user settings):
```json
{
  "servers": {
    "idas": {
      "type": "stdio",
      "command": "idas",
      "args": ["mcp", "serve"]
    }
  }
}
```

### Prerequisites

Before using the MCP server, ensure you have authenticated with IDAS:

```
idas benutzer login --user xxx --password yyy --appguid 123 --env dev
```

The MCP server will use the token stored in the `token` file in the working directory.

### Available Tools

The MCP server exposes all IDAS CLI commands as tools. Use your MCP client to discover available tools, which include:
- `vorgang_list`, `vorgang_get`, `vorgang_put` - Manage Vorgänge
- `kontakt_list`, `kontakt_get`, `kontakt_put` - Manage Kontakte
- `artikel_list`, `artikel_put` - Manage Artikel
- `benutzer_login`, `benutzer_list` - User management
- And many more...