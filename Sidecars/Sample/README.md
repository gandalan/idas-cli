# IDAS Sidecar Sample

Dieses Projekt ist ein Beispiel fuer eine IDAS CLI Sidecar-Erweiterung. Es demonstriert, wie ein externes Executable nach dem Muster `idas-<kommando>` aufgebaut werden kann.

## Ziel des Beispiels

Das Beispiel zeigt:

1. Ein Sidecar ist ein eigenstaendiges .NET-Konsolenprogramm
2. Das Sidecar verwendet deutsche Subcommands, passend zur IDAS CLI
3. Das Sidecar kann geerbte `IDAS_*`-Umgebungsvariablen, Token-Kontext via stdin und das aktuelle Arbeitsverzeichnis verwenden

Der Build erzeugt den Binary-Namen `idas-beispiel`. Ein Aufruf wie `idas beispiel diagnose` wird an genau dieses Executable delegiert.

## Enthaltene Commands

### `diagnose`

Zeigt Laufzeitinformationen an:

- Arbeitsverzeichnis
- Executable-Verzeichnis
- `IDAS_ENV`
- `IDAS_APPGUID`
- Vorhandensein einer `token`-Datei

Standardausgabe ist JSON. Mit `--text` erfolgt eine lesbare Textausgabe.

### `argumente`

Zeigt weitergereichte Argumente an. Das ist nuetzlich, um spaeteres Host-Forwarding zu pruefen.

Mit `--gross` werden die Werte in Grossbuchstaben ausgegeben.

### `hallo`

Ein minimales Smoke-Test-Kommando fuer die Sidecar-Ausfuehrung.

## Build

```bash
dotnet build Sidecars/Sample/IdasSidecarSample.csproj
```

## Publish

```bash
./Sidecars/Sample/publish.sh 0.1.0
```

Unter Windows:

```powershell
pwsh ./Sidecars/Sample/publish.ps1 -Version 0.1.0
```

Die Skripte erzeugen die Plattformen `win-x64` und `linux-x64` und legen diese Dateien in `dist` ab:

- `idas-beispiel.exe`
- `idas-beispiel`

## Direkte Ausfuehrung

```bash
dotnet run --project Sidecars/Sample/IdasSidecarSample.csproj -- diagnose
dotnet run --project Sidecars/Sample/IdasSidecarSample.csproj -- argumente foo bar baz
dotnet run --project Sidecars/Sample/IdasSidecarSample.csproj -- hallo Phil
```

## Beispiel fuer geerbte IDAS-Umgebung

```bash
IDAS_ENV=dev IDAS_APPGUID=11111111-1111-1111-1111-111111111111 \
dotnet run --project Sidecars/Sample/IdasSidecarSample.csproj -- diagnose --text
```

## Sidecar-Protokoll

Sidecars muessen folgendes Protokoll implementieren:

### Beschreibung

Wenn die Umgebungsvariable `IDAS_SIDECAR_DESCRIBE=1` gesetzt ist, muss das Sidecar eine JSON-Beschreibung ausgeben:

```json
{"Description": "Beschreibung des Sidecars"}
```

### Kontext

Wenn die Umgebungsvariable `IDAS_SIDECAR_CONTEXT_STDIN=1` gesetzt ist, wird der IDAS-Kontext als JSON ueber stdin uebergeben:

```json
{
  "AppGuid": "...",
  "Environment": "...",
  "TokenJson": "...",
  "WorkingDirectory": "...",
  "TimestampUtc": "..."
}
```

## Weitere Beispiele

Private Sidecars wie `idas-mandant` oder `idas-newsletter` koennen nach demselben Muster aufgebaut werden:

- Eigenstaendiges Executable
- Deutscher Command-Aufbau
- Sichtbarkeit in der normalen CLI ueber Host-Discovery
- Beschreibung ueber `IDAS_SIDECAR_DESCRIBE=1`
- IDAS-Kontext per stdin bei `IDAS_SIDECAR_CONTEXT_STDIN=1`
