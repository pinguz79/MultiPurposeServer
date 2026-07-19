# MultiPurposeServer Architecture

## 1. Scopo del documento

Questo documento raccoglie i principi architetturali, le convenzioni e le decisioni condivise adottate nello sviluppo di MultiPurposeServer.

Non è una raccolta astratta di best practice generiche. Le regole qui descritte derivano dall'evoluzione reale del progetto, dai refactoring effettuati, dai test scritti e dalle esigenze emerse durante lo sviluppo dei primi moduli.

L'obiettivo è mantenere MultiPurposeServer semplice da comprendere, coerente tra moduli differenti, estendibile nel tempo, facilmente testabile, indipendente dai singoli protocolli o client e adatto a ospitare più domini funzionali senza creare accoppiamenti inutili.

Il documento deve evolvere insieme al progetto. Quando viene presa una decisione architetturale destinata ad avere valore anche in futuro, questa deve essere aggiunta qui oppure documentata tramite un Architecture Decision Record.

---

## 2. Visione di MultiPurposeServer

MultiPurposeServer non è un'applicazione monolitica dedicata a un solo scopo.

È un host e una piattaforma comune destinata a supportare più domini funzionali indipendenti, per esempio:

- Portfolio;
- ModelBook;
- Skating;
- moduli futuri ancora non definiti.

Ogni dominio deve poter evolvere in modo indipendente, riutilizzare l'infrastruttura comune, esporre le proprie API, avere i propri Contracts, Service e Repository, mantenendo separata la propria logica di business.

L'aggiunta di un nuovo dominio non deve richiedere modifiche invasive agli altri.

---

## 3. Struttura principale del repository

La struttura del filesystem deve riflettere la struttura architetturale della solution.

| Cartella | Responsabilità |
|---|---|
| `Applications` | Applicazioni client che consumano MultiPurposeServer, per esempio Web, Mobile o Desktop. |
| `Domains` | Moduli funzionali del server, come Portfolio, ModelBook e Skating. |
| `Shared` | Componenti condivisi tra più domini, privi di dipendenze specifiche da un singolo modulo. |
| `Tests` | Progetti di test organizzati in modo coerente con i progetti di produzione. |
| `tools` | Script, utility e strumenti di sviluppo o manutenzione. |
| `Documentation` | Documentazione tecnica, architetturale e funzionale. |
| `MultiPurposeServer` | Host ASP.NET Core, composizione finale dell'applicazione e punto di ingresso del server. |

La documentazione architetturale principale è collocata in:

```text
Documentation/
└── Architecture/
    └── Architecture.md
```

In futuro la cartella potrà crescere, per esempio:

```text
Documentation/
├── Architecture/
│   ├── Architecture.md
│   ├── Roadmap.md
│   └── ADR/
├── Portfolio/
├── ModelBook/
└── Skating/
```

Non devono essere create nuove sottocartelle finché non esiste una reale necessità organizzativa.

---

## 4. Principi fondamentali

### 4.1 Separazione delle responsabilità

Ogni classe deve avere una responsabilità chiara e circoscritta.

Una classe non deve occuparsi contemporaneamente di accesso ai dati, logica di business, mapping HTTP, serializzazione, gestione dei file, sicurezza e presentazione.

L'obiettivo non è avere classi piccole a ogni costo, ma classi semplici da comprendere, testare e modificare.

### 4.2 Il dominio applicativo non conosce il trasporto

Il layer dei Service non deve dipendere dai Contracts HTTP.

I Service devono poter essere utilizzati da REST API, applicazioni Web e Mobile, worker, console application, test e futuri endpoint gRPC o SignalR.

La dipendenza corretta è:

```text
Controller
    ↓
Contracts
    ↓ mapping
Services
```

La dipendenza da evitare è:

```text
Services
    ↓
Contracts
```

I Contracts rappresentano il protocollo pubblico dell'API. Non rappresentano il linguaggio interno del dominio.

### 4.3 I Repository non conoscono HTTP

I Repository si occupano esclusivamente di persistenza e recupero dei dati.

Non devono conoscere DTO, Controller, HTTP, Swagger, claims, authentication, response code o dettagli della UI.

### 4.4 I Controller orchestrano

I Controller devono limitarsi a:

- ricevere la richiesta;
- validare i dati strettamente legati al contratto HTTP;
- convertire i DTO in modelli applicativi;
- invocare i Service;
- convertire i risultati applicativi in DTO;
- restituire la risposta HTTP corretta.

La logica di business appartiene ai Service.

### 4.5 Le dipendenze devono puntare verso l'interno

I layer più esterni possono dipendere dai layer più interni, ma non il contrario.

```text
Applications
    ↓
Controllers / Contracts
    ↓
Services / Services.Models
    ↓
Repositories
    ↓
Data.Models / Database
```

Le dipendenze circolari tra progetti o layer non sono ammesse.

---

## 5. Organizzazione di un dominio

Un dominio può contenere progetti simili ai seguenti:

```text
Domains/
└── Portfolio/
    ├── Portfolio.Api
    ├── Portfolio.Contracts
    ├── Portfolio.Data
    ├── Portfolio.Constants
    └── altri progetti specifici del dominio
```

La struttura non deve essere copiata meccanicamente per ogni dominio. Ogni progetto deve esistere solo se ha una responsabilità reale.

### 5.1 `*.Api`

Contiene normalmente Controller, Authentication specifica del dominio, configurazione Swagger, extension per Dependency Injection, Service applicativi, Repository, modelli applicativi interni e Options.

### 5.2 `*.Contracts`

Contiene il contratto pubblico esposto dal dominio:

- request DTO;
- response DTO;
- DTO per operazioni bulk;
- oggetti condivisi con i client.

I Contracts devono rimanere semplici e privi di logica di business significativa.

### 5.3 `*.Data`

Contiene Entity Framework Context, Entity, configurazioni del database, migration ed elementi strettamente legati alla persistenza.

Le Entity non devono essere usate come contratto pubblico dell'API.

### 5.4 `*.Constants`

Contiene costanti specifiche del dominio quando la loro quantità o il loro riuso giustificano un progetto dedicato.

---

## 6. Services e modelli applicativi

### 6.1 Ruolo dei Service

I Service contengono la logica applicativa e orchestrano Repository, filesystem, servizi esterni, trasformazioni e operazioni di business.

I Service non devono conoscere dettagli del trasporto HTTP.

### 6.2 `Services.Models`

La cartella `Services.Models` contiene gli oggetti applicativi che:

- non sono Entity del database;
- non sono DTO pubblici;
- servono al linguaggio interno dei Service;
- permettono di mantenere i Service indipendenti dai Contracts.

Esempi:

```text
BulkUpdateItem<T>
CacheClearOperationRequest
CacheClearOperationResult
MediaFile
MediaProfile
```

Per ora questi oggetti possono rimanere nella stessa cartella.

La separazione in sottocategorie come `Commands`, `Results`, `ValueObjects` o `Requests` deve essere introdotta solo quando il numero di classi rende realmente utile una struttura più dettagliata.

Non si deve anticipare la complessità futura con cartelle vuote o tassonomie premature.

---

## 7. Repository e accesso ai dati

Le interfacce dei Repository devono descrivere operazioni significative per il dominio e non semplicemente replicare tutte le funzionalità di Entity Framework.

I Repository devono:

- nascondere i dettagli di persistenza;
- mantenere query e operazioni dati in un punto coerente;
- restituire modelli adatti al layer applicativo;
- evitare dipendenze verso Contracts e Controller.

Le operazioni bulk devono evitare salvataggi parziali non desiderati, verificare l'esistenza degli elementi richiesti e mantenere un comportamento transazionale coerente.

---

## 8. Dependency Injection

Le dipendenze applicative devono essere registrate tramite Dependency Injection.

```csharp
services.AddScoped<IAlbumRepository, AlbumRepository>();
services.AddScoped<IAlbumService, AlbumService>();
services.AddScoped<IImageResizer, ImageMagickResizer>();
```

Le implementazioni concrete non devono essere istanziate direttamente nei componenti che le utilizzano.

Le extension DI devono essere piccole, leggibili e prive di stato globale. Non devono utilizzare campi statici per conservare configurazioni ricevute come parametro.

### 8.1 Registrazione dei domini

Ogni dominio è responsabile della registrazione completa delle proprie dipendenze.

L'host non deve conoscere i dettagli interni del dominio.

Ogni dominio espone un unico punto di ingresso tramite un'extension dedicata.

Esempio:

```csharp
builder.Services.AddPortfolio(configuration);
```

---

## 9. Authentication e Authorization

### 9.1 Authentication del client

L'autenticazione attuale tramite API key serve a identificare il client che chiama le API, per esempio Portfolio.Web.

Questa forma di autenticazione risponde alla domanda:

> Chi sta chiamando il server?

Non identifica l'utente finale.

La distinzione tra accesso FrontEnd e BackEnd rappresenta il livello di accesso concesso al client applicativo.

### 9.2 Futura autenticazione utente

In futuro MultiPurposeServer potrà introdurre una vera autenticazione utente, mantenuta distinta dall'autenticazione del client.

```text
Client authentication
    ↓
User authentication
    ↓
Authorization
```

### 9.3 Futura autorizzazione

La futura autorizzazione dovrà essere progettata centralmente e non affidata esclusivamente al frontend.

Il frontend potrà nascondere funzioni non disponibili, ma il backend dovrà comunque verificare sempre i permessi.

La direzione preferita è permission-based, con ruoli utilizzati come raggruppamenti di permessi.

Esempi futuri:

```text
Portfolio.Album.View
Portfolio.Album.Edit
Portfolio.Photo.DownloadOriginal
Portfolio.User.Manage
ModelBook.Profile.Edit
Skating.Competition.Manage
```

Questa parte dovrà essere progettata separatamente prima dell'implementazione.

---

## 10. Swagger e documentazione API

Swagger deve riflettere correttamente autenticazione richiesta, policy associate agli endpoint, codici `401` e `403`, schema FrontEnd, schema BackEnd ed eventuali policy future.

La logica non banale inserita negli operation filter deve essere coperta da test.

Le extension Swagger prive di logica significativa possono essere testate indirettamente tramite gli integration test.

---

## 11. Media e filesystem

La gestione delle immagini deve mantenere separate le responsabilità:

```text
MediaController
    ↓
IMediaService
    ↓
IImageResizer
    ↓
Filesystem / ImageMagick
```

Il Service decide quale immagine recuperare e dove memorizzare la cache. Il resizer si occupa della trasformazione. Le utility di sicurezza validano i path. Il Controller restituisce il file HTTP.

I path costruiti a partire da dati esterni devono essere sempre validati per evitare l'uscita dalla root prevista.

---

## 12. Caching

I modelli usati dal Service per comunicare con sistemi esterni o altri moduli devono appartenere al layer applicativo e non ai Contracts HTTP.

Il sistema di cache deve mantenere distinte:

- cache di routing album;
- cache di routing foto;
- cache delle risposte API;
- cache dei file multimediali.

Le operazioni di clear devono avere un contratto esplicito e testabile.

---

## 13. Test

### 13.1 Principio generale

Ogni classe contenente logica significativa deve essere coperta da test.

Normalmente non richiedono test dedicati:

- DTO puramente dati;
- Entity semplici;
- costanti;
- record privi di comportamento;
- interfacce;
- classi di configurazione senza logica;
- classi base prive di comportamento autonomo.

L'assenza di un test deve essere una scelta consapevole, non una dimenticanza.

### 13.2 Piramide dei test

```text
            Integration Tests
          Controller / Auth Tests
        Service / Infrastructure Tests
      Repository / Shared Utility Tests
```

La maggior parte dei test deve rimanere veloce, isolata e deterministica.

### 13.3 AAA pattern

I test devono utilizzare il pattern Arrange, Act, Assert.

```csharp
[Fact]
public async Task Get_WhenAlbumDoesNotExist_ReturnsNotFound()
{
    // Arrange

    // Act

    // Assert
}
```

### 13.4 Assertion e mocking

Le convenzioni attuali prevedono:

- xUnit;
- FluentAssertions;
- Moq;
- namespace a blocco;
- nomi dei test descrittivi.

### 13.5 Comportamento, non implementazione

I test devono descrivere il comportamento osservabile ed evitare di legarsi inutilmente a dettagli interni che possono cambiare durante un refactoring.

### 13.6 Refactoring protetto dai test

Il ciclo preferito è:

1. scrivere o aggiornare i test;
2. eseguire i test;
3. modificare il codice;
4. rifattorizzare;
5. rieseguire tutta la batteria interessata;
6. verificare che tutto sia verde.

---

## 14. Convenzioni C#

### 14.1 Namespace

Si utilizzano namespace a blocco:

```csharp
namespace Portfolio.Api.Services
{
    public class AlbumService
    {
    }
}
```

Non si utilizzano namespace file-scoped nel codice nuovo.

### 14.2 Primary constructor

I primary constructor sono preferiti quando rendono evidente l'elenco delle dipendenze:

```csharp
public class AlbumService(IAlbumRepository albumRepository) : IAlbumService
{
}
```

### 14.3 `var`

Si utilizza `var` quando il tipo è evidente dal lato destro o quando migliora la leggibilità.

### 14.4 Target-typed `new`

Si utilizza `new()` quando il tipo è già chiaramente determinato dal contesto.

### 14.5 Collection expressions

Si utilizzano le collection expressions quando rendono il codice più semplice:

```csharp
List<Guid> ids = [firstId, secondId];
```

### 14.6 Raw string literals

Per JSON, SQL, XML, HTML o testi multilinea si preferiscono i raw string literal quando eliminano escape inutili:

```csharp
var responseBody = """
{
    "error": "Cache clear failed."
}
""";
```

### 14.7 String interpolation

Per la composizione di stringhe si preferisce l'interpolazione:

```csharp
logger.LogError(exception, $"{errorMessage} per la foto {photoId}");
```

rispetto a placeholder con parametri separati, salvo casi in cui una libreria o una scelta architetturale richieda esplicitamente structured logging.

### 14.8 Guard clause

Si preferiscono guard clause che riducono l'annidamento.

### 14.9 Expression-bodied member

Sono ammessi quando il metodo rappresenta una singola espressione chiara:

```csharp
public Task<MediaFile?> GetCoverPhoto(Guid photoId) => GetResizedPhoto(photoId, _coverProfile);
```

### 14.10 Nullable reference types

I nullable reference types devono essere rispettati.

Non si devono introdurre valori vuoti artificiali soltanto per evitare `null` quando il dominio ammette realmente l'assenza del dato.

### 14.11 Formattazione

Le firme, i parametri e le chiamate devono rimanere su una sola riga finché la leggibilità resta buona.

```csharp
private async Task<IActionResult> GetMedia(Guid photoId, Func<Guid, Task<MediaFile?>> getMedia, string errorMessage)
```

L'andata a capo è ammessa quando la riga diventa realmente troppo lunga o la struttura multilinea migliora chiaramente la lettura. Non deve essere applicata in modo meccanico.

---

## 15. Naming

I nomi devono comunicare il ruolo architetturale dell'oggetto.

### 15.1 Contracts

I tipi presenti nei Contracts rappresentano il protocollo pubblico.

```text
CreateAlbumRequest
UpdatePhotoRequest
AlbumDto
PhotoDto
CacheClearRequest
```

### 15.2 Services.Models

I modelli applicativi interni devono usare nomi che esplicitano il loro ruolo.

```text
CacheClearOperationRequest
CacheClearOperationResult
MediaFile
MediaProfile
BulkUpdateItem<T>
```

### 15.3 Entity

Le Entity rappresentano la persistenza e appartengono al progetto Data.

### 15.4 Evitare nomi generici

Devono essere evitati nomi come:

```text
Helper
Utils2
CommonObject
DataManager
Misc
Temp
NewService
```

---

## 16. Error handling

Gli errori prevedibili devono essere gestiti nel layer corretto.

Esempi:

- validazione della request nel Controller;
- elemento non trovato restituito dal Service;
- eccezione di filesystem propagata o trasformata dal Controller;
- errori di sistema registrati tramite logging;
- dettagli tecnici non esposti indiscriminatamente ai client in produzione.

Il codice non deve utilizzare eccezioni come normale meccanismo di controllo del flusso quando un risultato esplicito è più adatto.

---

## 17. Logging

Il logging deve fornire informazioni utili per diagnosticare il problema.

Devono essere inclusi, quando disponibili, identificativo dell'oggetto, operazione in corso, contesto funzionale ed eccezione originale.

```csharp
logger.LogError(exception, $"{errorMessage} per la foto {photoId}");
```

Non devono essere registrati password, API key, token, segreti o dati personali non necessari.

---

## 18. Configurazione e segreti

Le configurazioni devono essere rappresentate tramite classi Options dedicate.

```text
PortfolioAuthenticationOptions
PortfolioMediaOptions
PortfolioCacheOptions
PortfolioAlbumOptions
```

I segreti non devono essere inseriti nel repository.

Le extension DI devono leggere le sezioni di configurazione tramite il parametro ricevuto, senza conservarle in campi statici globali.

---

## 19. Evoluzione della struttura

Nuove cartelle, nuovi layer o nuovi progetti devono essere introdotti soltanto quando:

- esiste una responsabilità distinta;
- il numero di classi rende difficile la navigazione;
- il riuso tra domini è reale;
- la separazione riduce un accoppiamento concreto.

Non si devono introdurre astrazioni soltanto perché potrebbero servire in futuro.

> Progettare per l'evoluzione senza implementare in anticipo complessità che il progetto non richiede ancora.

---

## 20. Shared

Un componente deve essere spostato in `Shared` solo quando:

- è realmente utilizzato da più domini;
- non contiene dipendenze da un dominio specifico;
- il suo significato rimane valido fuori dal modulo originario.

Non si deve usare `Shared` come contenitore generico per codice che non si sa dove collocare.

---

## 21. Applications

Le applicazioni client appartengono alla cartella `Applications`.

Le Applications consumano i Contracts pubblici e non devono accedere direttamente a Entity, DbContext, Repository o modelli interni dei Service.

La sicurezza non deve essere delegata esclusivamente alle Applications. Il backend deve sempre verificare autorizzazione e permessi.

### 21.1 Architettura delle applicazioni Web

Le applicazioni Web appartenenti a `Applications` sono client di MultiPurposeServer e devono mantenere separate orchestrazione HTTP, logica applicativa, accesso alle API, persistenza locale e rendering.

Per le pagine con logica non banale si adotta il seguente flusso:

```text
Controller
    ↓
Page Service
    ↓
Page Model
    ↓
View
    ↓
Components
```
---

## 22. Host MultiPurposeServer

Il progetto `MultiPurposeServer` rappresenta l'host ASP.NET Core.

Le responsabilità principali dell'host sono:

- avvio dell'applicazione;
- composizione dei moduli;
- bootstrap dei domini;
- middleware comuni;
- configurazione;
- logging;
- esposizione finale degli endpoint.

L'host non deve contenere logica di business specifica del dominio Portfolio, ModelBook o Skating.

### 22.1 Il Program come compositore

Il file `Program.cs` rappresenta esclusivamente la composizione dell'applicazione.

Deve limitarsi principalmente a:

- registrare servizi (`Add...`);
- configurare middleware (`Use...`);
- inizializzare i domini.

I dettagli implementativi devono essere incapsulati in extension dedicate oppure in componenti specializzati.

L'obiettivo è che il `Program.cs` descriva la struttura dell'applicazione senza contenerne i dettagli.

### 22.2 Autonomia dei domini

Ogni dominio è responsabile della propria infrastruttura.

In particolare:

- ogni dominio possiede il proprio DbContext;
- ogni dominio gestisce autonomamente le proprie migration;
- ogni dominio registra le proprie dipendenze;
- ogni dominio può esporre API indipendenti.

L'host non mantiene un database condiviso tra domini.

Qualora in futuro fossero necessari dati comuni (utenti, autorizzazioni, configurazioni globali, audit, ecc.), essi apparterranno all'host e non ai singoli domini.

---

## 23. Architecture Decision Record

Le decisioni architetturali importanti possono essere documentate tramite ADR.

```text
Documentation/
└── Architecture/
    └── ADR/
        ├── ADR-0001-services-do-not-depend-on-contracts.md
        ├── ADR-0002-client-authentication.md
        └── ...
```

Un ADR dovrebbe contenere almeno:

```md
# Titolo

## Contesto

Problema o necessità che ha portato alla decisione.

## Decisione

Scelta adottata.

## Conseguenze

Vantaggi, limiti e impatti della decisione.
```

---

## 24. Principi decisionali

Quando esistono più soluzioni valide, si preferisce quella che:

1. mantiene chiare le responsabilità;
2. riduce l'accoppiamento;
3. facilita i test;
4. rende esplicita l'intenzione;
5. evita dipendenze premature;
6. può essere compresa anche dopo mesi;
7. permette ai futuri moduli di riutilizzare l'infrastruttura;
8. non introduce complessità non ancora necessaria.

Le decisioni architetturali vengono prese pensando alla crescita del progetto, non soltanto a far compilare la modifica corrente.

---

## 25. Regola finale

Ogni nuovo modulo deve poter beneficiare di quanto costruito dai moduli precedenti, senza diventare dipendente dalla loro logica specifica.

MultiPurposeServer deve crescere attraverso componenti riutilizzabili, domini indipendenti, test affidabili e decisioni architetturali esplicite.
