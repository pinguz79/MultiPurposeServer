# Pacchetto Skating per il cliente

La procedura è locale e non pubblica su server, non invia email e non modifica Finance.
Richiede Windows, PowerShell 7, Git e SDK .NET 10. Il ripristino dei pacchetti/runtime può
richiedere rete; l'applicazione confezionata funziona offline su Windows x64 compatibile.

## Comando

```powershell
./Applications/Skating/Publish-Client.ps1 -Version 0.1.0-preview.1
```

Opzionalmente `-Aggregation Discipline` oppure `-Aggregation Category` preimposta il criterio
nel solo pacchetto. Il default `None` lascia `null`: il cliente lo sceglie dalla configurazione
e riavvia. Non vengono modificate configurazioni sorgenti o già distribuite.

## Sequenza

1. Esecuzione dei test Core/persistenza e desktop in Release; errore interrompe la procedura.
2. Publish `win-x64`, self-contained, cartella completa; niente trimming o single-file.
3. Copia delle istruzioni e configurazione; verifica dei file indispensabili, incluso SQLite.
4. Rifiuto di archivi, backup e simboli di debug nel pacchetto.
5. Identificazione versione, commit, modifiche locali e data in `versione.json`.
6. Inventario completo dei file runtime con dimensioni e SHA256 in `contenuto-runtime.json`.
7. ZIP e impronta SHA256 esterna sotto `artifacts/Skating`, ignorato da Git.

Ogni esecuzione usa una directory nuova con timestamp e identificativo; non cancella nulla.
In caso di errore resta l'output diagnostico parziale, non da distribuire. Le modifiche non
committate sono ammesse per collaudo ma esplicitamente indicate nei metadati e nell'output.

## Consegna e prova

Estrarre lo ZIP in una cartella nuova e verificare l'avvio di `App/Skating.Desktop.exe`;
provare la configurazione, salvare, chiudere e riaprire. Per verificare davvero l'autonomia
dal runtime installato occorre anche un PC/VM compatibile senza .NET preinstallato.
La verifica sul PC di sviluppo non sostituisce questo collaudo.

Inviare lo ZIP completo, mai il solo EXE. Se la dimensione o i filtri di posta impediscono
l'allegato, inviare un link al medesimo file senza aggirare le protezioni. Il pacchetto non
è firmato digitalmente e può generare avvisi di reputazione: verificare provenienza e hash.
Non disabilitare le protezioni del sistema.

L'app conserva preferenze in LocalAppData e archivi nei percorsi scelti dall'utente; non
sono inclusi nel pacchetto. Salvare gli archivi fuori dalla cartella applicativa. Per gli
aggiornamenti usare una cartella nuova e reimpostare la configurazione. Il criterio delle
manifestazioni avviate resta protetto. Nessun installer o aggiornamento automatico.

## Dipendenze

Il publish include il runtime .NET/Windows Desktop, Microsoft.Data.Sqlite e SQLitePCLRaw,
compresa la libreria nativa `e_sqlite3.dll`. L'inventario è generato dall'output effettivo,
non dalle date dei file. Conservare integralmente `App`, incluse cartelle delle risorse
localizzate. Non introdotte nuove versioni di pacchetti NuGet: cambia la modalità di distribuzione.
Non ci sono artefatti MPS/Aruba da distribuire. Il prototipo resta destinato a dati dimostrativi.
