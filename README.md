# IDAS CLI

Kommandozeilen-Tool für den Zugriff auf IDAS/i3 ERP System.

## Installation

.NET 8 erforderlich. Release herunterladen oder selbst kompilieren:

```bash
dotnet build
```

## Erstmalige Einrichtung

1. `.env` Datei im Projektverzeichnis erstellen:

```
IDAS_APP_TOKEN=dein-app-token-guid
IDAS_ENV=dev
```

2. Einmalig einloggen (öffnet Browser für SSO):

```bash
idas benutzer login
```

Das erstellt eine `token` Datei mit dem Auth-Token. Diese Schritt ist nur nach Ablauf des Tokens (ca. 7 Tage) oder auf neuen Maschinen nötig.

## Verwendung

```bash
# Hilfe
idas --help

# Beispiele
idas vorgang list
idas vorgang get <guid>
idas kontakt list
idas benutzer list
```

## MCP Server

Für AI-Integration als MCP-Server starten:

```bash
idas mcp serve
```

**VS Code Konfiguration** (`.vscode/mcp.json`):
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

Der MCP-Server verwendet automatisch die `token` Datei im Arbeitsverzeichnis.
