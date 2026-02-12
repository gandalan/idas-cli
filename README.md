# IDAS CLI

Kommandozeilen-Tool für den Zugriff auf IDAS/i3 ERP System.

## Installation

Laden Sie das aktuelle Release herunter und entpacken Sie das Programm in einen beliebigen Ordner. 

Alternativ klonen Sie mit Git das Repository. Zum Build dieses Projektes ist .NET 8 erforderlich.

```bash
dotnet build
```

## Erstmalige Einrichtung

Die Einrichtung ist unabhängig davon, ob Sie den Build selbst erstellt oder heruntergeladen haben. 

1. `.env` Datei im Programmverzeichnis erstellen:

```
IDAS_APP_TOKEN=app-token-guid
IDAS_ENV=dev
```

Das AppToken ist individuell für jede IDAS-Drittanbieter-Einbindung. Sie erhalten das Token direkt von Gandalan.

**ACHTUNG**: Beachten Sie unbedingt die korrekte Umgebung (dev, stg oder prod) - sonst schlägt der Login in Schritt 2 fehlt, weil der Benutzer auf der entsprechenden Umgebung nicht gefunden werden kann! 

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

Für AI-Integration als MCP-Server starten (Sie müssen sich vorher eingeloggt haben, siehe oben):

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
