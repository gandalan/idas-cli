---
name: idas-umsatz-mcp
description: Calculate total revenue (Warenwert) from AB-Belege in IDAS/i3 ERP system for a specific year using MCP Server. Uses the MCP tool `beleg_list` to retrieve all AB-Belege including archived ones and sums up the Warenwert. Use when user asks for revenue calculation, Umsatz, yearly sales, AB-Beleg totals, or financial summaries from IDAS via MCP.
---

# IDAS Umsatz Berechnung (MCP Server)

Berechnet den Gesamtumsatz (Warenwert) aus AB-Belegen für ein bestimmtes Jahr aus dem IDAS/i3 ERP-System über den MCP Server.

## Overview

Dieser Skill nutzt das MCP Tool `beleg_list`, um alle AB-Belege abzurufen und den Gesamtumsatz zu berechnen:

1. **AB-Belege abrufen** - Nutzt `beleg_list` mit Filter auf AB-Belege
2. **JSON parsen** - Extrahiert Warenwert und Bruttobetrag
3. **Summieren** - Addiert alle Werte für das Gesamtergebnis

## Usage

### Einfache Umsatzberechnung

```python
# Alle AB-Belege eines Jahres als JSON
beleg_list(jahr=2025, belegart="AB", format="json")
```

### Beispiel-Workflow

**Schritt 1: AB-Belege abrufen**
```python
# Alle AB-Belege für 2025 abrufen
belege = beleg_list(jahr=2025, belegart="AB", format="json", includeArchive=true)
```

**Schritt 2: Warenwerte summieren**
```python
# Warenwerte aller AB-Belege summieren
gesamt_warenwert = sum(b.Warenwert for b in belege)
gesamt_brutto = sum(b.EndbetragBrutto for b in belege)
```

### Examples

```python
# Umsatz 2025 berechnen
belege = beleg_list(jahr=2025, belegart="AB", format="json")

# Umsatz 2024 berechnen
belege = beleg_list(jahr=2024, belegart="AB", format="json")

# Alle Jahre (jahr=0 oder weglassen)
belege = beleg_list(belegart="AB", format="json")

# Als CSV exportieren
beleg_list(jahr=2025, belegart="AB", format="csv", separator=";")
```

## What It Calculates

Das `beleg_list` Tool liefert folgende Daten pro AB-Beleg:

- **VorgangsNummer** - Eindeutige Vorgangs-ID
- **Kundenname** - Name des Kunden
- **KundenNummer** - Kundennummer
- **BelegArt** - Immer "AB" bei Filter
- **BelegNummer** - Belegnummer
- **BelegJahr** - Jahr
- **BelegDatum** - Datum (ISO 8601 Format)
- **AnzahlPositionen** - Anzahl Positionen
- **Warenwert** - Netto-Warenwert
- **RabatteAufschlaege** - Summe aller Rabatte/Aufschläge
- **Transportkosten** - Transportkosten
- **Mehrwertsteuer** - MwSt.-Betrag
- **EndbetragBrutto** - Gesamtbetrag inkl. MwSt.
- **GesamtbetragNetto** - Alternativer Netto-Wert
- **IstArchiviert** - Ob der Vorgang archiviert ist

## Output Format

### JSON (Standard)

```json
[
  {
    "VorgangsNummer": 1000,
    "Kundenname": "Test GmbH",
    "KundenNummer": "0000000001",
    "BelegArt": "AB",
    "BelegNummer": 1000,
    "BelegJahr": 2025,
    "BelegDatum": "2025-02-04T07:27:35.2733333Z",
    "AnzahlPositionen": 1,
    "Warenwert": 96560.00,
    "RabatteAufschlaege": 4828.00,
    "Transportkosten": 9173.20,
    "Mehrwertsteuer": 19171.99,
    "EndbetragBrutto": 120077.19,
    "GesamtbetragNetto": 0,
    "VorgangGuid": "...",
    "BelegGuid": "...",
    "IstArchiviert": false
  }
]
```

### CSV (deutsches Format)

```csv
VorgangsNummer;Kundenname;KundenNummer;BelegArt;BelegNummer;BelegJahr;BelegDatum;AnzahlPositionen;Warenwert;RabatteAufschlaege;Transportkosten;Mehrwertsteuer;EndbetragBrutto;GesamtbetragNetto;VorgangGuid;BelegGuid;IstArchiviert
1000;Test GmbH;0000000001;AB;1000;2025;04.02.2025;1;96.560,00;4.828,00;9.173,20;19.171,99;120.077,19;0,00;...;...;Nein
```

**Formatierung:**
- Trennzeichen: Semikolon `;` (deutsches Excel)
- Dezimal: Komma `,`
- Datum: DD.MM.YYYY
- Zahlen: Mit Tausender-Punkt (z.B. 96.560,00)

## How It Works

### Algorithmus

1. **`beleg_list` aufrufen**:
   ```python
   belege = beleg_list(
       jahr=<year>,
       belegart="AB",
       format="json",
       includeArchive=true
   )
   ```
   - Lädt alle AB-Belege für das Jahr
   - Archivierte Vorgänge sind standardmäßig inkludiert
   - Alle Belege eines Vorgangs werden aufgelistet

2. **JSON parsen**:
   - Array von BelegListDTO Objekten
   - Warenwert aus `Warenwert` Feld extrahieren
   - EndbetragBrutto aus `EndbetragBrutto` Feld extrahieren

3. **Summieren**:
   - Alle Warenwerte addieren = Gesamt-Netto
   - Alle EndbetragBrutto addieren = Gesamt-Brutto

### Vollständiger Workflow

```python
# 1. AB-Belege laden
belege = beleg_list(jahr=2025, belegart="AB", format="json", includeArchive=true)

# 2. Summen berechnen
gesamt_warenwert = sum(b.Warenwert for b in belege)
gesamt_brutto = sum(b.EndbetragBrutto for b in belege)
anzahl_belege = len(belege)

# 3. Ergebnis ausgeben
print(f"Gesamtumsatz 2025:")
print(f"  Warenwert (Netto): {gesamt_warenwert:,.2f} EUR")
print(f"  Endbetrag (Brutto): {gesamt_brutto:,.2f} EUR")
print(f"  Anzahl AB-Belege: {anzahl_belege}")
```

## Prerequisites

- IDAS MCP Server muss verfügbar sein
- MCP Tool `beleg_list` muss registriert sein
- Authentifizierung über MCP Server

## Authentication

Der Skill verwendet die MCP Server Authentifizierung:

- Keine manuelle Anmeldung erforderlich
- Authentifizierung erfolgt automatisch über den MCP Server
- Zugangsdaten werden in der MCP Konfiguration verwaltet

## MCP Tools Used

| Tool | Zweck |
|------|-------|
| `beleg_list` | Liste aller Belege mit Filter-Optionen |

### Tool Parameters

**beleg_list:**
- `jahr` (int?, default: 0) - Jahr filtern (0 = alle Jahre)
- `belegart` (string?, default: null) - Belegart filtern (z.B. "AB", "Angebot", "Rechnung")
- `format` (string, default: "json") - Output format: "json" oder "csv"
- `separator` (string, default: ";") - CSV-Trennzeichen
- `filename` (string?, default: null) - Output in Datei speichern
- `includeArchive` (bool?, default: true) - Archivierte Vorgänge inkludieren

## When to Use

Use this skill when:
- User asks for yearly revenue calculation (Jahresumsatz)
- User wants to know total sales from AB-Belege
- Financial reporting or analysis is needed
- Comparing revenue between years
- Checking archived vs active Vorgänge revenue
- Working in MCP environment (not CLI)

## Error Handling

- **"MCP Server not available"**: Verbindung zum MCP Server prüfen
- **"Keine Vorgänge gefunden"**: Jahr prüfen oder Daten existieren nicht
- **Leere Ergebnis-Liste**: Keine AB-Belege für das Jahr vorhanden

## Notes

- Ein Vorgang kann mehrere Belege haben (Angebot → AB → Rechnung)
- Das `beleg_list` Tool listet ALLE Belege auf
- Archivierte Vorgänge werden standardmäßig inkludiert
- Der Warenwert ist netto (vor MwSt.)
- CSV ist direkt in Excel importierbar (deutsches Format)
- JSON eignet sich für automatisierte Verarbeitung
- Dieser Skill ist das MCP-Pendant zum CLI-Skill `idas-umsatz`

## Differences to idas-umsatz (CLI)

| Aspekt | idas-umsatz (CLI) | idas-umsatz-mcp (MCP) |
|--------|-------------------|----------------------|
| **Zugriff** | Via `dotnet run -- beleg list` | Via MCP Tool `beleg_list` |
| **Authentifizierung** | .env Datei oder manuell | MCP Server |
| **Datenformat** | CSV/JSON Output | Direkte JSON Objekte oder CSV |
| **Performance** | Ein Kommando | Ein MCP Call |
| **Umgebung** | CLI/Terminal | MCP/IDE Integration |
| **Tool Name** | `idas beleg list` | `beleg_list` |
