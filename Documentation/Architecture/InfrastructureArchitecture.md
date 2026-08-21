# Architettura dell'Infrastruttura

> **Stato: Alpha 0 — non autorevole.** Il contenuto deve essere verificato e consolidato prima della promozione.

## 1. Scopo del documento

Questo documento descrive l'architettura dei componenti infrastrutturali di MultiPurposeServer.

L'infrastruttura comprende tutti quei servizi trasversali che supportano l'esecuzione del sistema senza appartenere a un dominio funzionale specifico.

Il suo obiettivo è fornire capacità comuni ai diversi domini mantenendoli indipendenti dalla tecnologia utilizzata per implementarle.

Questo documento descrive la struttura, le responsabilità e i confini dell'infrastruttura.

Le convenzioni di implementazione e le pratiche di sviluppo sono invece definite nel `Documentation/Engineering/MpsPlaybook.md`.

---

## 2. Ruolo dell'infrastruttura

L'infrastruttura rappresenta l'insieme dei servizi tecnici condivisi utilizzati dall'intera piattaforma.

A differenza dei domini applicativi, l'infrastruttura non implementa logica di business.

La sua responsabilità consiste nel fornire servizi comuni come:

- hosting;
- configurazione;
- logging;
- gestione degli errori;
- caching;
- media management;
- documentazione delle API;
- servizi di supporto all'esecuzione.

I domini utilizzano questi servizi senza conoscerne necessariamente l'implementazione.

L'infrastruttura deve quindi essere progettata per essere:

- indipendente dal business;
- riutilizzabile;
- facilmente sostituibile;
- facilmente testabile;
- estendibile nel tempo.

### 2.1 Business e infrastruttura

L'infrastruttura non deve contenere regole di business.

Le decisioni funzionali appartengono ai domini.

L'infrastruttura fornisce gli strumenti necessari affinché tali decisioni possano essere eseguite.

Ad esempio:

- il Media Service salva un file;
- il dominio Portfolio decide quale file salvare.

- il sistema di caching memorizza un risultato;
- il dominio decide quando quel risultato rappresenta un'informazione valida.

- il sistema di logging registra un evento;
- il dominio decide quali eventi sono significativi.

### 2.2 Dipendenze

L'infrastruttura può dipendere da:

- Shared;
- librerie esterne;
- framework utilizzati dal progetto.

I domini possono utilizzare l'infrastruttura.

L'infrastruttura non deve invece dipendere da un dominio specifico.

Quando emerge una dipendenza verso un dominio significa generalmente che una responsabilità è stata collocata nel livello sbagliato.

### 2.3 Isolamento per dominio

Ogni dominio configura autonomamente le istanze dei servizi tecnici che utilizza, anche quando più domini adottano lo stesso provider o la stessa implementazione.

La configurazione può essere duplicata intenzionalmente per preservare autonomia ed estraibilità. I servizi configurabili o stateful rimangono isolati per dominio; servizi realmente stateless possono essere condivisi fisicamente quando non introducono accoppiamento.

Il provider di logging può essere comune al processo, mentre categorie, livelli e destinazioni possono essere separati per dominio. I componenti Shared conservano il contesto necessario ad attribuire correttamente gli eventi.

Le responsabilità strettamente legate al processo rimangono comuni all'host e non contengono decisioni applicative.

---

## 3. Host MultiPurposeServer

Il progetto `MultiPurposeServer` rappresenta l'host dell'applicazione.

Costituisce il punto di composizione dell'intero sistema.

La sua responsabilità consiste nell'assemblare i diversi moduli che costituiscono la piattaforma senza contenerne la logica funzionale.

L'host è responsabile di:

- creare l'applicazione ASP.NET Core;
- registrare middleware comuni;
- configurare i servizi condivisi;
- registrare i domini;
- inizializzare l'infrastruttura;
- configurare autenticazione e autorizzazione;
- configurare OpenAPI e la relativa interfaccia interattiva;
- esporre gli endpoint HTTP;
- avviare l'applicazione.

L'host non implementa logica di business.

### 3.1 Composition Root

L'host rappresenta il Composition Root dell'intera applicazione.

Ogni dominio registra autonomamente le proprie dipendenze.

L'host si limita a richiamarne il punto di ingresso.

Ad esempio:

```csharp
builder.Services.AddPortfolio(configuration);
builder.Services.AddModelBook(configuration);
builder.Services.AddSkating(configuration);
```

L'host non deve conoscere quali Service, Repository o componenti interni vengano registrati.

### 3.2 Middleware

I middleware appartengono all'infrastruttura.

Sono responsabili di aspetti trasversali come:

- gestione delle eccezioni;
- autenticazione;
- autorizzazione;
- logging;
- CORS;
- compressione;
- caching HTTP;
- diagnostica;
- routing.

Ogni middleware deve avere una responsabilità chiara.

Non devono essere creati middleware "generalisti" contenenti comportamenti eterogenei.

### 3.3 Bootstrap

La fase di bootstrap comprende tutte le operazioni necessarie prima che il sistema inizi a gestire richieste.

Può includere:

- validazione della configurazione;
- inizializzazione dei servizi infrastrutturali;
- verifica delle dipendenze;
- registrazione dei provider;
- inizializzazione di componenti condivisi.

Le inizializzazioni specifiche dei singoli domini devono rimanere all'interno dei rispettivi domini.

---

## 4. Configurazione

La configurazione deve essere rappresentata tramite classi fortemente tipizzate.

Ogni modulo è responsabile delle proprie Options.

Esempi:

- PortfolioMediaOptions
- PortfolioAuthenticationOptions
- MediaStorageOptions
- CacheOptions

Le Options rappresentano esclusivamente configurazione.

Non devono contenere logica applicativa.

### 4.1 Binding

La configurazione deve essere caricata tramite il sistema Options di ASP.NET Core.

Le classi di configurazione devono essere registrate tramite Dependency Injection.

Devono poter essere validate durante l'avvio dell'applicazione.

### 4.2 Nessuno stato globale

La configurazione non deve essere conservata in campi statici.

Ogni componente deve ricevere esclusivamente la configurazione di cui necessita tramite Dependency Injection.

Questo principio rende il codice:

- testabile;
- indipendente dall'ambiente;
- facilmente sostituibile.

### 4.3 Segreti

Password, API Key, Client Secret e credenziali non devono essere inseriti nel repository.

L'infrastruttura deve poter utilizzare i meccanismi messi a disposizione dall'ambiente di esecuzione per la gestione dei segreti.

---

## 5. Logging

Il logging costituisce uno strumento diagnostico dell'infrastruttura.

Il suo scopo è permettere la comprensione del comportamento del sistema durante l'esecuzione.

Il logging non rappresenta un meccanismo di controllo del flusso.

La policy autorevole e i dettagli implementativi sono definiti in [Logging Architecture](LoggingArchitecture.md). Questa sezione conserva soltanto i principi infrastrutturali di alto livello.

### 5.1 Responsabilità

Ogni componente registra gli eventi significativi che gestisce o assorbe nella propria responsabilità. Le eccezioni propagate non devono essere registrate ripetutamente lungo la catena delle chiamate.

Ad esempio:

- l'host registra l'avvio del sistema;
- un Repository registra un errore di persistenza soltanto quando lo assorbe, applica un fallback o ne gestisce il recupero;
- un servizio media registra operazioni sui file;
- un middleware registra eccezioni non gestite.

### 5.2 Livelli

Il livello di logging deve riflettere la gravità dell'evento.

Indicativamente:

- Trace
- Debug
- Information
- Warning
- Error
- Critical

La scelta del livello deve essere coerente in tutto il progetto e deve distinguere la gravità tecnica dalla normale semantica applicativa.

### 5.3 Structured Logging

Quando possibile il logging deve utilizzare proprietà strutturate anziché costruire messaggi tramite concatenazione di stringhe.

Ad esempio:

```csharp
_logger.LogInformation(
    "Album {AlbumId} created by {UserId}",
    albumId,
    userId);
```

Il logging strutturato facilita ricerca, aggregazione e analisi.

### 5.4 Informazioni sensibili

Non devono essere registrati:

- password;
- token;
- API Key;
- dati personali non necessari;
- segreti applicativi.

I log devono essere sufficienti alla diagnosi senza compromettere la sicurezza del sistema.

---

## 6. Error Handling

La gestione degli errori rappresenta una responsabilità infrastrutturale.

Ogni livello del sistema gestisce gli errori di propria competenza.

L'infrastruttura coordina la gestione degli errori non previsti.

### 6.1 Errori previsti

Gli errori funzionali appartengono ai domini.

Esempi:

- album inesistente;
- file non trovato;
- autorizzazione negata;
- validazione fallita.

Questi casi devono essere rappresentati tramite risultati applicativi espliciti.

### 6.2 Eccezioni

Le eccezioni rappresentano condizioni anomale.

Non devono costituire il normale meccanismo di controllo del flusso.

L'infrastruttura deve:

- intercettarle;
- registrarle;
- produrre una risposta coerente;
- evitare l'esposizione di dettagli interni.

### 6.3 Middleware globale

Le eccezioni non gestite devono essere intercettate da un middleware dedicato.

Il middleware è responsabile di:

- logging;
- traduzione della risposta HTTP;
- eventuale correlation id;
- eventuale diagnostica.

I Controller non devono duplicare questo comportamento.

---

## 7. Media Management

Il sistema Media rappresenta il servizio infrastrutturale responsabile della gestione dei contenuti binari.

Il suo compito consiste nell'astrarre la memorizzazione dei file rispetto ai domini.

I domini descrivono cosa salvare.

Il Media System decide come salvarlo.

### 7.1 Responsabilità

Il sistema Media può essere responsabile di:

- salvataggio dei file;
- recupero dei file;
- cancellazione;
- generazione delle miniature;
- validazione dei percorsi;
- gestione dei provider di storage;
- metadati dei file.

### 7.2 Provider

L'accesso allo storage deve essere astratto tramite provider.

Questo permette di sostituire l'implementazione senza modificare i domini.

Esempi futuri:

- File System locale;
- Azure Blob Storage;
- Amazon S3;
- altri provider.

I domini non devono conoscere il provider utilizzato.

### 7.3 Sicurezza dei percorsi

I path costruiti a partire da dati esterni devono essere validati.

Non deve essere possibile uscire dalla directory prevista mediante path traversal.

La costruzione dei percorsi appartiene all'infrastruttura.

I domini devono utilizzare identificatori logici anziché concatenare manualmente path del filesystem.

---

## 8. Caching

Il caching rappresenta una responsabilità infrastrutturale il cui scopo è ridurre il costo di accesso ai dati senza modificarne il significato.

La cache non costituisce la fonte autorevole dell'informazione.

Il dato autorevole appartiene sempre al sistema che ne è proprietario.

### 8.1 Responsabilità

Il sistema di caching può essere utilizzato per:

- ridurre il numero di richieste ripetitive;
- diminuire il carico sul database;
- migliorare i tempi di risposta;
- memorizzare risultati costosi da calcolare;
- supportare il routing applicativo;
- ottimizzare il recupero dei contenuti multimediali.

Ogni cache deve avere una responsabilità chiaramente identificata.

### 8.2 Tipologie di cache

MultiPurposeServer può utilizzare differenti livelli di cache.

Ad esempio:

- Memory Cache;
- Distributed Cache;
- Routing Cache;
- Response Cache;
- Media Cache.

Ogni livello possiede responsabilità differenti.

Non devono essere utilizzati indistintamente.

### 8.3 Invalidazione

Ogni cache deve definire esplicitamente:

- chi inserisce il dato;
- chi lo legge;
- quando scade;
- come viene invalidato;
- quale sia la fonte autorevole.

L'invalidazione deve essere prevedibile e facilmente verificabile.

### 8.4 Cache e domini

I domini non devono conoscere l'implementazione della cache.

Essi richiedono un'informazione.

L'infrastruttura decide se tale informazione possa essere recuperata dalla cache oppure dalla sorgente primaria.

---

## 9. OpenAPI e documentazione interattiva delle API

La specifica OpenAPI rappresenta il contratto pubblico delle API; Scalar ne fornisce la consultazione interattiva.

La sua responsabilità consiste nel descrivere il comportamento del server.

Non implementa logica applicativa.

### 9.1 Documentazione

Ogni endpoint pubblico dovrebbe descrivere:

- scopo;
- parametri;
- Request;
- Response;
- codici HTTP;
- requisiti di autenticazione;
- eventuali permessi richiesti.

La documentazione deve essere considerata parte integrante dell'API.

### 9.2 Contratti

OpenAPI documenta esclusivamente i Contracts pubblici.

Entity, modelli interni dei Service e componenti infrastrutturali non devono essere esposti.

### 9.3 Sicurezza

OpenAPI deve rappresentare correttamente:

- gli schemi di autenticazione;
- le policy;
- gli endpoint protetti;
- le possibili risposte `401`;
- le possibili risposte `403`.

---

## 10. Servizi infrastrutturali

L'infrastruttura può ospitare servizi condivisi che non appartengono a uno specifico dominio.

Esempi includono:

- Background Service;
- Hosted Service;
- Scheduler;
- Queue Processor;
- servizi di manutenzione;
- servizi di sincronizzazione;
- provider condivisi.

Questi componenti devono mantenere responsabilità ben definite.

### 10.1 Hosted Service

Un Hosted Service rappresenta un processo eseguito dal server indipendentemente dalle richieste HTTP.

Può essere utilizzato per:

- manutenzione;
- sincronizzazione;
- pulizia delle cache;
- aggiornamenti periodici;
- elaborazioni pianificate.

La logica di business continua ad appartenere ai domini.

### 10.2 Provider

I provider costituiscono adattatori verso sistemi esterni.

Ad esempio:

- servizi cloud;
- storage;
- SMTP;
- provider di autenticazione;
- servizi REST esterni.

Il resto dell'applicazione non deve dipendere direttamente dalla tecnologia utilizzata.

---

## 11. Dipendenze e confini

L'infrastruttura occupa un livello trasversale dell'architettura.

Può essere utilizzata da:

- Domains;
- Applications;
- Host.

Non deve invece dipendere da un dominio specifico.

### 11.1 Direzione delle dipendenze

La direzione corretta delle dipendenze è:

```text
Applications
        ↓

Domains
        ↓

Infrastructure
        ↓

Framework / Provider esterni
```

L'infrastruttura non deve conoscere il significato funzionale dei dati che gestisce.

### 11.2 Shared

Quando un componente infrastrutturale diventa realmente condiviso e indipendente dal contesto che lo ha originato, può essere promosso nello Shared Framework.

`MultiPurposeServer.Shared.Persistence` applica questo principio al lifecycle provider-independent di Operation, transazioni e checkpoint e non dipende da un provider concreto. `MultiPurposeServer.Shared.Persistence.EntityFramework` contiene invece l'adapter generico `EntityFrameworkPersistenceCoordinator<TContext>`: ogni dominio EF lo registra con il proprio `DbContext` e condivide così lo stato transazionale fra i Repository coinvolti senza duplicarne l'implementazione.

Come per il resto del progetto:

> **Shared is Earned, not Planned.**

---

## 12. Evoluzione dell'infrastruttura

L'infrastruttura deve evolvere insieme al progetto.

Nuovi servizi devono essere introdotti soltanto quando emerge una responsabilità tecnica chiaramente distinta.

L'infrastruttura non deve trasformarsi in un contenitore generico di utility.

Ogni nuovo componente dovrebbe rispondere ad almeno una delle seguenti esigenze:

- ridurre duplicazioni infrastrutturali;
- semplificare i domini;
- migliorare la testabilità;
- isolare tecnologie esterne;
- centralizzare responsabilità trasversali.

L'infrastruttura deve rendere più semplice lo sviluppo del business.

Non deve sostituirsi ad esso.

---

## 13. Checklist

Prima di introdurre un nuovo componente infrastrutturale verificare che:

- rappresenti una responsabilità tecnica reale;
- non contenga logica di business;
- possa essere utilizzato da più domini;
- non introduca dipendenze verso domini specifici;
- sia registrato tramite Dependency Injection;
- sia facilmente sostituibile;
- sia facilmente testabile;
- non utilizzi stato globale non necessario;
- mantenga separata configurazione e implementazione;
- documenti chiaramente il proprio ruolo architetturale.

---

## 14. Vedi anche

- `Architecture.md`
- `DomainArchitecture.md`
- `WebApplicationArchitecture.md`
- `SecurityArchitecture.md`
- `TestingArchitecture.md`
- `SharedFramework.md`
- `Documentation/Engineering/MpsPlaybook.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`
