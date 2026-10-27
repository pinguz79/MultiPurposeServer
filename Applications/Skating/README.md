# Skating System

Applicazione desktop autonoma Windows, WinForms su .NET 10, inclusa nella solution
principale `MultiPurposeServer.slnx` e nella sua build ordinaria. Non richiede il server MPS,
Finance o una connessione Internet a runtime.
Il ripristino delle dipendenze di sviluppo può richiedere la rete.

## Avvio e compilazione

Dalla root del repository, con SDK .NET 10 e workload desktop Windows:

```powershell
dotnet build Applications/Skating/Skating.Desktop/Skating.Desktop.csproj
dotnet run --project Applications/Skating/Skating.Desktop/Skating.Desktop.csproj
```

Publish autonomo, quando necessario, senza deploy Aruba:

```powershell
dotnet publish Applications/Skating/Skating.Desktop/Skating.Desktop.csproj -c Release
```

Il publish predefinito richiede .NET Desktop Runtime 10 sul PC destinatario.

## Struttura e funzionalità disponibili

- `Skating.Core`: manifestazione, anagrafiche e contratti di persistenza, senza riferimenti
  a UI, SQLite, Finance o server.
- `Skating.Persistence`: implementazione SQLite del contratto, con formato identificato e versionato.
- `Skating.Desktop`: WinForms e composizione delle dipendenze nell'entry point.
- `Tests/Skating/Skating.Tests`: test del modello e integrazione su archivi temporanei isolati.
- `Tests/Skating/Skating.Desktop.Tests`: test automatici dei controlli WinForms, senza finestre visibili.

Dal menu Manifestazione è possibile creare un nuovo archivio (Ctrl+N), scegliendo nome e file
`.skating`, oppure riaprirlo (Ctrl+O). L'archivio contiene identità, nome e anagrafiche.
Ogni operazione chiude la connessione; l'apertura avviene in sola lettura senza migrazioni implicite.
Un errore lascia invariata la manifestazione visualizzata.

Il menu Manifestazione conserva le ultime quattro manifestazioni aperte correttamente,
più recente in cima, riapribili con un clic. Nome e file sono visibili nel menu; il tooltip
riporta il percorso completo. Riaperture dello stesso percorso non creano duplicati.
L'elenco è una preferenza locale in
`%LOCALAPPDATA%/SkatingSystem/recent-manifestations.json`, esterna agli archivi e ai backup.
File recenti spostati o non disponibili producono il normale errore di apertura e restano
nell'elenco; usare Apri per individuarne il nuovo percorso. Aperture fallite o aggiornamenti
annullati non cambiano la cronologia. Preferenze mancanti o illeggibili non impediscono l'avvio.

La creazione non sovrascrive mai un file esistente, anche confermando la sostituzione nel dialog
di sistema. Un errore dopo la prenotazione del nome può lasciare un file incompleto, rifiutato
alla riapertura: scegliere un altro nome. Non si cancellano file automaticamente.
Archivi della vecchia demo, estranei o con versione non supportata vengono rifiutati senza modificarli.
Non viene cercato automaticamente alcun database sul PC.

### Anagrafiche

Dopo l'apertura usare **Anagrafiche -> Scuole / Atleti / Discipline / Categorie / Livelli**.
Ogni schermata mostra le voci salvate, permette inserimenti consecutivi e aggiorna subito l'elenco.
Sono disponibili inserimento, consultazione e modifica; l'eliminazione non è ancora prevista.
Selezionare una riga e premere **Modifica**, oppure fare doppio clic sulla riga.
I campi vengono precompilati: **Salva modifiche** aggiorna la stessa voce mantenendone
l'identificativo; **Annulla modifica** scarta le modifiche. La chiusura con dati non salvati
richiede conferma. Durante la modifica la selezione della griglia resta bloccata.

- Atleti: nome e cognome obbligatori, identità distinta per persona; omonimi ammessi.
  Nessun obbligo di scuola o coreografia per creare l'anagrafica.
- Discipline e categorie: nome obbligatorio.
- Scuole: nome obbligatorio e univoco, modificabile mantenendo le associazioni alle coreografie.
- Livelli: nome obbligatorio e flag **Percorso PRO**, indipendente dal nome.
- Spazi esterni rimossi; discipline, categorie e livelli non ammettono duplicati di nome
  nel proprio elenco, ignorando maiuscole/minuscole anche per lettere accentate.
- Categorie e livelli condivisi fra discipline. Nessuna matrice preventiva, categoria
  automatica in base all'età, gara generata o elenco di nomi precodificato.

Le scelte iniziali di campi e duplicati sono documentate in
[Anagrafiche del prototipo](../../Documentation/Skating/Anagrafiche.md).

### Archivi precedenti

Il formato corrente è v8. I file v1–v7 dei primi blocchi possono essere letti senza modifiche:
la GUI offre **Aggiorna con backup** (predefinito), **Continua senza backup** e **Annulla**.
Con backup, l'aggiornamento salva una copia SQLite consistente
`.before-v8-<identificativo>.bak` accanto all'originale, poi aggiunge le
tabelle in transazione senza alterare nome e identità. Se si rifiuta, resta aperta la manifestazione
precedente. Senza backup viene saltata solo la copia preventiva: validazione e transazione
restano attive. La scelta non viene ricordata per aperture successive e non elimina copie esistenti.
Se un backup richiesto fallisce non parte la migrazione; se la migrazione fallisce viene annullata.
Questa copia tecnica non sostituisce il futuro catalogo di backup con ripristino da GUI.

### Coreografie

Dal menu **Coreografie** si apre l'elenco con **Nuova coreografia** e **Modifica**
(anche doppio clic). Preparare prima scuole, atleti e cataloghi dal menu Anagrafiche.
La scheda contiene nome, scuola, disciplina, categoria, livello, composizione, durata
in minuti/secondi e selezione multipla degli atleti. Assolo richiede un atleta, Duo due,
Gruppo almeno tre; la composizione è distinta dalla disciplina.
Il campo filtro cerca per nome e/o cognome, anche parziali e in ordine inverso,
conservando le selezioni fuori filtro. **Aggiungi atleta** apre l'anagrafica atleti
e aggiorna l'elenco al ritorno senza perdere i dati della coreografia.

Gli atleti possono partecipare a più coreografie, anche per scuole diverse. Sono indicati
conteggio delle coreografie e scuole già rappresentate; gli omonimi hanno un suffisso
identificativo nella selezione. Il riepilogo distingue partecipanti unici e intera anagrafica.
Stessi atleti nella stessa combinazione disciplina/categoria/livello: ammessi con nomi diversi.
Il salvataggio di dati e componenti è atomico; annullare non scrive nulla.
Non è ancora disponibile l'eliminazione delle coreografie. Dopo la predisposizione della
scheda voti sono protetti i dati strutturali, mantenendo modificabili nome e durata.

Prova consigliata: creare due scuole e tre atleti, inserire un duo, modificarne un componente,
riaprire l'archivio e verificare; poi iscrivere gli stessi atleti con un nome diverso e una
scuola diversa. I dettagli sono in [Scuole e coreografie](../../Documentation/Skating/ScuoleECoreografie.md).

### Gare, giudici e criteri

In **Anagrafiche -> Giudici** si inseriscono e rinominano i giudici.
**Anagrafiche -> Criteri di valutazione** gestisce l'elenco comune: inserimento, modifica
ed eliminazione con conferma. Inizialmente: Tecnica, Interpretazione / Musicalità, Show.

**Gare** mostra le combinazioni disciplina/categoria/livello effettivamente iscritte.
Il raggruppamento si aggiorna salvando le coreografie; nessuna gara vuota dal prodotto
dei cataloghi. **Partecipanti e giuria** (anche doppio clic) mostra le coreografie e consente
di selezionare i giudici. **Salva giuria** richiede almeno tre giudici e conserva una
selezione esplicita; **Usa tutti i giudici** ripristina il default, che include anche
giudici aggiunti in seguito. Una giuria personalizzata non cambia aggiungendo giudici.

La vista segnala giurie incomplete, assenza di criteri e più categorie usate nel percorso
PRO. Non accorpa automaticamente categorie PRO diverse. L'avvio dal menu Scaletta e voti
blocca criteri e inserimento di nuovi giudici. L'assenza si gestisce dall'anagrafica giudici.
Dettagli e prove in [Gare e giurie](../../Documentation/Skating/GareEGiurie.md).

### Scaletta e votazioni

**Scaletta e voti** permette preparazione, assegnazione dei blocchi, conferma e avvio.
La scheda del blocco selezionato consente inserimento diretto, salvataggio automatico dei
voti validi, gestione delle assenze, conferma e riapertura delle esibizioni. Preferenze
di Invio e colore sono modificabili nella scheda e persistono in
`%LOCALAPPDATA%/SkatingSystem/recent-manifestations.scores.json`.
Prima di confermare viene conservata una copia `.before-confirm-*.bak`.
Dettagli e prova: [Scaletta e voti](../../Documentation/Skating/ScalettaEVoti.md).

La classifica della singola gara è disponibile da **Gare → Classifica...**, con parità,
chiusura/riapertura ed esiti reversibili. Vedere [Classifiche](../../Documentation/Skating/Classifiche.md).

Il menu **Premi intermedi...** aggrega i vincitori non PRO per disciplina o categoria.
Prima dell'uso impostare `IntermediateAwardAggregation` in `skating-settings.json` accanto
all'eseguibile a `Discipline` oppure `Category`, poi riavviare. Il valore distribuito è `null`:
nessun criterio regolamentare scelto implicitamente. Dettagli in
[Premi intermedi](../../Documentation/Skating/PremiIntermedi.md).

Il menu **Assolute** offre percorsi separati non PRO (vincitori dei premi intermedi) e PRO
(vincitori delle gare PRO, senza aggregatore intermedio). Convalida esplicita, parità e
accesso alla gara originale per le correzioni: [Premi assoluti](../../Documentation/Skating/PremiAssoluti.md).

Non sono ancora implementati orari e pause della scaletta, configurazione generale,
catalogo/ripristino dei backup e notifiche.
Usare esclusivamente dati dimostrativi. Il database di servizio separato sarà introdotto insieme
a backup/notifiche, senza inserire quelle informazioni nell'archivio manifestazione.

Verifiche mirate dalla root:

```powershell
dotnet test Tests/Skating/Skating.Tests/Skating.Tests.csproj
dotnet test Tests/Skating/Skating.Desktop.Tests/Skating.Desktop.Tests.csproj
```

Dipendenze runtime: Microsoft.Data.Sqlite 10.0.10 e SQLitePCLRaw.bundle_e_sqlite3 2.1.14.
L'override del bundle evita la dipendenza nativa vulnerabile 2.1.11; non modifica i pacchetti MPS.

La condivisione della solution non implica dipendenze applicative. UI, logica e persistenza
manterranno confini che consentano un futuro backend amministrativo pre-manifestazione;
il funzionamento durante la gara deve restare offline. Nessuna API o sincronizzazione
è introdotta in questo blocco.

Specifiche autorevoli: [Documentation/Skating](../../Documentation/Skating/README.md).
