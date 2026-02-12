---
name: idas-umsatz
description: Calculate total revenue (Warenwert) from AB-Belege in IDAS/i3 ERP system for a specific year. Uses the integrated 'beleg list' command to retrieve all AB-Belege including archived ones and sums up the Warenwert. Use when user asks for revenue calculation, Umsatz, yearly sales, AB-Beleg totals, or financial summaries from IDAS.
---

# IDAS Umsatz Berechnung

Berechnet den Gesamtumsatz (Warenwert) aus AB-Belegen für ein bestimmtes Jahr aus dem IDAS/i3 ERP-System.

## Overview

Dieser Skill nutzt das integrierte `beleg list` Kommando des idas-cli, um alle AB-Belege abzurufen und den Gesamtumsatz zu berechnen:

1. **AB-Belege abrufen** - Nutzt `idas beleg list` mit Filter auf AB-Belege
2. **CSV/JSON parsen** - Extrahiert Warenwert und Bruttobetrag
3. **Summieren** - Addiert alle Werte für das Gesamtergebnis

## Usage

### Einfache Umsatzberechnung

Verwende das integrierte `beleg list` Kommando:

```bash
# Alle AB-Belege eines Jahres als CSV
idas beleg list --jahr 2025 --belegart AB --format csv

# Als Datei speichern
idas beleg list --jahr 2025 --belegart AB --format csv --filename umsatz_2025.csv

# Als JSON für weitere Verarbeitung
idas beleg list --jahr 2025 --belegart AB --format json
```

### Umsatz berechnen und anzeigen

**CSV-Format (direkt lesbar):**
```bash
idas beleg list --jahr 2025 --belegart AB --format csv
```

**Mit anderem Trennzeichen (z.B. Komma für internationales Excel):**
```bash
idas beleg list --jahr 2025 --belegart AB --format csv --separator ","
```

### Examples

```bash
# Umsatz 2025 berechnen
idas beleg list --jahr 2025 --belegart AB --format csv

# Umsatz 2024 als JSON
idas beleg list --jahr 2024 --belegart AB --format json

# Alle Jahre (kein Jahr angegeben)
as beleg list --belegart AB --format csv

# In Datei exportieren
idas beleg list --jahr 2025 --belegart AB --format csv --filename umsatz_2025.csv
```

## What It Calculates

Das `beleg list` Kommando liefert folgende Daten pro AB-Beleg:

- **VorgangsNummer** - Eindeutige Vorgangs-ID
- **Kundenname** - Name des Kunden
- **KundenNummer** - Kundennummer
- **BelegArt** - Immer "AB" bei Filter
- **BelegNummer** - Belegnummer
- **BelegJahr** - Jahr
- **BelegDatum** - Datum (DD.MM.YYYY)
- **AnzahlPositionen** - Anzahl Positionen
- **Warenwert** - Netto-Warenwert
- **RabatteAufschlaege** - Summe aller Rabatte/Aufschläge
- **Transportkosten** - Transportkosten
- **Mehrwertsteuer** - MwSt.-Betrag
- **EndbetragBrutto** - Gesamtbetrag inkl. MwSt.
- **GesamtbetragNetto** - Alternativer Netto-Wert
- **IstArchiviert** - Ja/Nein

## Output Format

### CSV (Standard für Excel)

```csv
VorgangsNummer;Kundenname;KundenNummer;BelegArt;BelegNummer;BelegJahr;BelegDatum;AnzahlPositionen;Warenwert;RabatteAufschlaege;Transportkosten;Mehrwertsteuer;EndbetragBrutto;GesamtbetragNetto;VorgangGuid;BelegGuid;IstArchiviert
1000;Test GmbH;0000000001;AB;1000;2025;04.02.2025;1;96.560,00;4.828,00;9.173,20;19.171,99;120.077,19;0,00;...;...;Nein
1001;Test GmbH;0000000001;AB;1001;2025;06.02.2025;2;16.240,95;812,05;1.542,89;3.224,64;20.196,43;0,00;...;...;Nein
...
```

**Formatierung:**
- Trennzeichen: Semikolon `;` (deutsches Excel)
- Dezimal: Komma `,`
- Datum: DD.MM.YYYY
- Zahlen: Mit Tausender-Punkt (z.B. 96.560,00)

### JSON (für Programmierung)

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

## How It Works

### Algorithmus

1. **`idas beleg list` aufrufen**:
   ```bash
   idas beleg list --jahr <year> --belegart AB --format csv
   ```
   - Lädt alle AB-Belege für das Jahr
   - Archivierte Vorgänge sind standardmäßig inkludiert
   - Alle Belege eines Vorgangs werden aufgelistet

2. **CSV parsen** (optional):
   - Header-Zeile überspringen
   - Jede Zeile = Ein AB-Beleg
   - Warenwert aus Spalte 9 extrahieren
   - EndbetragBrutto aus Spalte 13 extrahieren

3. **Summieren**:
   - Alle Warenwerte addieren = Gesamt-Netto
   - Alle EndbetragBrutto addieren = Gesamt-Brutto

### Vollständiger Workflow

```bash
# 1. CSV exportieren
idas beleg list --jahr 2025 --belegart AB --format csv --filename belege_2025.csv

# 2. In Excel öffnen und analysieren
# oder: Mit Shell-Befehlen summieren (Linux/macOS)
tail -n +2 belege_2025.csv | awk -F';' '{sum+=$9} END {printf "Warenwert: %.2f EUR\n", sum}'

# 3. Alternative: JSON für Programmierung
idas beleg list --jahr 2025 --belegart AB --format json > belege_2025.json
```

## Prerequisites

- IDAS CLI muss kompiliert sein (`dotnet build`)
- `.env` Datei mit Zugangsdaten oder manueller Login
- Für CSV-Export: Keine zusätzlichen Tools nötig

## Authentication

Der Skill verwendet die gleiche Authentifizierung wie das idas-cli:

**Option 1: .env Datei** (empfohlen)
```bash
IDAS_USER=your_username
IDAS_PASSWORD=your_password
IDAS_APP_TOKEN=your_token
IDAS_ENV=dev
```

**Option 2: Manueller Login vorher**
```bash
idas benutzer login --user <user> --password <pass> --appguid <guid> --env <env>
```

## When to Use

Use this skill when:
- User asks for yearly revenue calculation (Jahresumsatz)
- User wants to know total sales from AB-Belege
- Financial reporting or analysis is needed
- Comparing revenue between years
- Checking archived vs active Vorgänge revenue
- Exporting data to Excel for further analysis

## Command Options

| Option | Default | Beschreibung |
|--------|---------|--------------|
| `--jahr` | 0 (alle) | Jahr filtern |
| `--format` | json | csv oder json |
| `--separator` | ; | CSV-Trennzeichen |
| `--belegart` | - | Filter z.B. "AB" |
| `--filename` | - | In Datei speichern |
| `--include-archive` | true | Archivierte inkludieren |

## Error Handling

- **"Please provide user and password"**: Login erforderlich, .env prüfen
- **"Keine Vorgänge gefunden"**: Jahr prüfen oder Daten existieren nicht
- **Leere CSV**: Keine AB-Belege für das Jahr vorhanden

## Notes

- Ein Vorgang kann mehrere Belege haben (Angebot → AB → Rechnung)
- Das `beleg list` Kommando listet ALLE Belege auf
- Archivierte Vorgänge werden standardmäßig inkludiert
- Der Warenwert ist netto (vor MwSt.)
- CSV ist direkt in Excel importierbar (deutsches Format)
- JSON eignet sich für automatisierte Verarbeitung
