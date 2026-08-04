# ADR-0010 — Controllers orchestrate application operations

## Stato

Accepted

---

## Contesto

Molte architetture attribuiscono ai Service applicativi la responsabilità di orchestrare l'intero caso d'uso, compresi:

- apertura della transazione;
- invocazione di più operazioni;
- commit o rollback;
- composizione dei risultati.

Durante la code review è emerso che MultiPurposeServer adotta invece un modello differente.

I Service espongono operazioni applicative elementari e focalizzate.

I Controller sono responsabili dell'orchestrazione del caso d'uso e possono combinare più operazioni applicative all'interno della stessa richiesta HTTP.

Questa scelta è stata inizialmente interpretata come una possibile violazione architetturale, ma si è rivelata intenzionale e vantaggiosa, in particolare per le operazioni Bulk.

---

## Decisione

L'orchestrazione del caso d'uso appartiene ai Controller.

I Controller possono:

- aprire una `IApplicationOperation`;
- invocare uno o più Service;
- decidere quali operazioni appartengono allo stesso confine transazionale;
- completare o annullare l'operazione.

I Service devono invece esporre operazioni applicative elementari, prive di responsabilità di orchestrazione.

Schema generale:

```text
HTTP Request
        ↓
Controller
        ↓
BeginOperation()
        ↓
Service A
        ↓
Service B
        ↓
Service C
        ↓
Complete()
        ↓
HTTP Response
```

---

## Motivazioni

### 1. Supporto naturale alle operazioni Bulk

Le API Bulk possono eseguire centinaia o migliaia di aggiornamenti.

Consentire al Controller di definire il confine transazionale permette di utilizzare una singola operazione applicativa invece di aprire una transazione per ogni elemento.

---

### 2. Composizione libera dei casi d'uso

Uno stesso Service può essere riutilizzato in contesti differenti.

Ad esempio:

- aggiornamento singolo;
- aggiornamento Bulk;
- importazioni;
- sincronizzazioni;
- workflow futuri.

Il Service non deve conoscere il contesto nel quale viene utilizzato.

---

### 3. Service semplici

Ogni metodo del Service mantiene una responsabilità estremamente focalizzata.

Esempi:

```text
UpdateName()
UpdateDescription()
CreateAlbum()
UpdatePhotoDescription()
```

Il Service non contiene logica di orchestrazione.

---

### 4. Confine transazionale esplicito

L'apertura della `IApplicationOperation` rende evidente quali operazioni appartengono allo stesso caso d'uso.

Il confine della transazione non è nascosto all'interno dei Service.

---

## Conseguenze

### Positive

- nessuna duplicazione dei Service per scenari singoli e Bulk;
- massima riusabilità delle operazioni applicative;
- un solo confine transazionale quando necessario;
- migliore controllo delle performance nelle operazioni massive;
- Controller espliciti riguardo al flusso applicativo.

### Negative

I Controller contengono una quantità maggiore di codice rispetto ad architetture che demandano completamente l'orchestrazione ai Service.

Questo è considerato accettabile purché i Controller non introducano:

- regole di business;
- invarianti del dominio;
- logica di validazione;
- logica di normalizzazione.

Il loro ruolo rimane esclusivamente quello di coordinare le operazioni applicative.

---

## Vincoli

I Controller possono:

- aprire e completare una `IApplicationOperation`;
- coordinare più Service;
- gestire la composizione dei risultati;
- definire il confine transazionale del caso d'uso.

I Controller non devono:

- implementare regole di business;
- accedere direttamente ai Repository;
- modificare direttamente le Entity;
- duplicare normalizzazione o validazione;
- contenere logica di persistenza.

I Service devono:

- implementare operazioni applicative elementari e focalizzate;
- essere riutilizzabili in contesti differenti;
- non conoscere HTTP;
- non conoscere il Controller chiamante;
- delegare la persistenza ai Repository.

---

## Alternative considerate

### Service orchestratori

Ogni caso d'uso viene implementato da un metodo dedicato del Service.

Esempio:

```text
UpdateAlbum(...)
BulkUpdateAlbums(...)
ImportAlbums(...)
SynchronizeAlbums(...)
```

#### Vantaggi

- Controller estremamente sottili.
- Orchestrazione centralizzata.

#### Svantaggi

- duplicazione della logica tra casi d'uso simili;
- proliferazione dei metodi applicativi;
- minore riusabilità delle operazioni elementari;
- difficoltà nel riutilizzare le stesse operazioni in scenari Bulk;
- minore controllo esplicito del confine transazionale.

Questa alternativa è stata scartata.

---

## Relazioni

Questo ADR integra e completa:

- ADR-0001 — Domain boundaries
- ADR-0005 — Request processing is centralized in the MVC pipeline
- ADR-0007 — Bulk requests share a common contract and base type

definendo esplicitamente la responsabilità di orchestrazione tra Controller e Service.

---

## Note

La presenza di `BeginOperation()` nei Controller è una conseguenza diretta di questa decisione architetturale e **non costituisce un anti-pattern**.

Durante le future code review non deve essere considerata una violazione, purché siano rispettati i vincoli definiti in questo ADR:

- il Controller orchestra;
- il Service implementa operazioni applicative elementari;
- le regole di business rimangono nel livello di dominio;
- il Controller non accede direttamente ai Repository.