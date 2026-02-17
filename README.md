# Weather Station Monitor

## Popis

Worker Service v .NET 8, která v pravidelných intervalech (1h) stahuje XML data z meteostanice. Aplikace data transformuje do formátu JSON a ukládá je na SQL Server pomocí Entity Framework Core. Robustnost je zajištěna ošetřením chyb při nedostupnosti stanice.

## Nastroje

- .NET 8 SDK
- Entity Framework Core
- LINQ to XML
- Serilog (Sinks FIle)

## Jak spustit

1. Ověřte Connection String v appsettings.json. (default nastavena LocalDB)
2. V Package Manager Console spusťte Update-Database.
3. Spusťte projekt (F5).
4. Kontrola: Logy se průběžně zapisují do textových souborů ve složce /logs
