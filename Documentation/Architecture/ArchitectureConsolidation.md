# Architecture Consolidation

## Stato del documento

**Documento temporaneo di migrazione — non autorevole.**

Questo documento conserva e classifica i concetti rimossi da `Architecture.md` durante il suo consolidamento come overview tecnologicamente neutrale.

Il suo scopo è rendere esplicita nella history Git la destinazione prevista dei contenuti e impedire che concetti ancora validi vengano persi durante la riorganizzazione.

Il documento deve essere progressivamente svuotato quando i contenuti vengono verificati e trasferiti nei documenti autorevoli. Al termine della migrazione deve essere eliminato.

---

## Regole di utilizzo

- Questo documento non fa parte del bootstrap ufficiale.
- I contenuti possono essere incompleti, ridondanti o obsoleti.
- La presenza di un concetto non equivale alla sua approvazione.
- Prima del trasferimento ogni contenuto deve essere confrontato con il codice e con le decisioni consolidate.
- Ogni voce deve essere rimossa dopo l'integrazione nella destinazione definitiva.
- Le decisioni scartate devono essere rimosse indicando nella commit la motivazione.

---

## Registro di migrazione

| Area | Destinazione prevista | Stato |
|---|---|---|
| Struttura del repository | Engineering o documentazione di repository | Da consolidare |
| Layer di un dominio | `DomainArchitecture.md` e futuri documenti specialistici | Overview consolidata; dettagli da distribuire |
| Controller, Contracts, Service e Repository | `DomainArchitecture.md` e futuri documenti specialistici | Overview consolidata; dettagli da distribuire |
| Struttura delle Applications Web | `WebApplicationArchitecture.md` | Da consolidare |
| Configurazione, logging, errori, media e cache | `InfrastructureArchitecture.md` | Da consolidare |
| Identità, autenticazione e autorizzazione | `SecurityArchitecture.md` | Da consolidare |
| Livelli e responsabilità dei test | `TestingArchitecture.md` | Da consolidare |
| Meccanismi tecnici condivisi | `SharedFramework.md` e futuri documenti specialistici | Overview consolidata; dettagli da distribuire |
| Regole di evoluzione e refactoring | `MpsPlaybook.md` | Da consolidare |
| Milestone tecniche | `ArchitectureRoadmap.md` | Da consolidare |
| Processo ADR | `ADR/README.md` | Da consolidare |
| Collaborazione con assistenti AI | `Documentation/AI` | Da consolidare |
| Organizzazione della documentazione | `Documentation/README.md` e `Home.md` | Da consolidare |

---

## 1. Struttura del repository

### Destinazione prevista

Engineering o documentazione dedicata alla struttura del repository.

### Contenuto da consolidare

La struttura del filesystem dovrebbe rendere riconoscibili le responsabilità principali:

| Area | Responsabilità |
|---|---|
| `Applications` | Client Web, Mobile o Desktop. |
| `Domains` | Moduli applicativi del server. |
| `Shared` | Componenti tecnici condivisi. |
| `Tests` | Progetti di test. |
| `tools` | Script e strumenti di sviluppo. |
| `Documentation` | Documentazione tecnica, architetturale e funzionale. |
| Host | Avvio e composizione finale della piattaforma. |

La struttura documentale e del repository deve crescere soltanto quando emerge una necessità reale. Non devono essere introdotte classificazioni vuote per anticipare esigenze future.

---

## 2. Layer e responsabilità interne dei domini

### Destinazione prevista

`DomainArchitecture.md`

### Contenuto da consolidare

I layer rappresentano responsabilità logiche e non implicano necessariamente assembly separati.

#### Contracts

I Contracts rappresentano il protocollo pubblico delle API e non il linguaggio interno della business logic.

#### Controllers

I Controller:

- ricevono request già normalizzate e validate dalla pipeline;
- interpretano il contratto HTTP;
- orchestrano le operazioni applicative;
- invocano i Service;
- trasformano i risultati in response;
- selezionano la risposta HTTP appropriata.

Non contengono business logic e non accedono direttamente alla persistenza.

#### Services

I Service contengono la logica applicativa e non dipendono dai Contracts HTTP.

Possono ricevere singoli valori o Business Model costruiti dal Controller. Recuperano le entità tramite Repository, applicano le regole e preparano le entità da persistere.

#### Repositories

I Repository si occupano esclusivamente di recupero e persistenza dei dati.

Non conoscono DTO, Controller, HTTP, documentazione API, identità del client, response code o presentazione.

Possono restituire entità del Data Layer ai Service. Il Service introduce un Business Model soltanto quando esiste una reale divergenza semantica.

#### Data

Il Data Layer contiene i componenti di persistenza di basso livello e le entità persistite.

Business Entity e Data Entity possono coincidere quando rappresentano lo stesso concetto senza introdurre dettagli infrastrutturali indesiderati. Devono divergere quando cambiano responsabilità, lifecycle o vincoli.

#### Direzione logica

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Data Layer
```

La collocazione nello stesso progetto non autorizza scorciatoie tra layer.

### Dettagli destinati a documenti specialistici

`DomainArchitecture.md` costituisce il riferimento comune dei domini. I seguenti contenuti rimossi dall'overview devono essere verificati e distribuiti nei livelli appropriati:

| Area | Destinazione indicativa |
|---|---|
| Struttura fisica di progetti e cartelle | Engineering o documentazione del repository. |
| API concrete di Controller, Service e Repository | Documentazione implementativa dei domini. |
| Interfacce, Dependency Injection e generic repository | Pattern implementativi e Testing Architecture. |
| Entity Framework, migration e lazy loading | Persistence Architecture, ADR o documentazione del dominio Portfolio. |
| Options e gestione tecnica della configurazione | Infrastructure Architecture. |
| Naming di DTO, modelli ed Entity | MPS Playbook. |
| Checklist di creazione di un dominio | Engineering. |
| Strategia e livelli dei test | Testing Architecture. |

### Decisioni emerse durante il consolidamento

- `Domain.Api` è il modulo server componibile del dominio e non necessariamente un host eseguibile.
- MPS ospita attualmente i domini; un futuro `Domain.WebApi` può sostituirlo in una solution dedicata.
- Controller, Service, Repository e Data rappresentano layer logici e possono convivere nello stesso progetto.
- Il Controller orchestra l'operazione API; il Service espone capacità applicative focalizzate e non coincide necessariamente con l'intero caso d'uso.
- I Service non dipendono dai Contracts e ricevono normalmente valori estratti dalla Request.
- Il Business Model è opzionale e viene introdotto soltanto quando diverge realmente dal Data Model.
- Quando esiste, il Business Model può dipendere dal Data Model per il mapping; la dipendenza inversa non è ammessa.
- Il Repository può restituire Entity EF al Service.
- Il Response DTO può effettuare il mapping da Data Model o Business Model.
- `Domain.Contracts` è l'implementazione server-side; i client condividono il wire contract OpenAPI, non necessariamente l'assembly.
- Ogni dominio possiede database o schema logico, migration e ciclo evolutivo dei dati indipendenti.
- Più domini possono condividere server DB e provider, ma non dati, foreign key o transazioni.
- Il Controller governa l'atomicità applicativa dell'operazione anche quando questa comprende più risorse tecniche.
- Per risorse non transazionali l'atomicità può richiedere compensazione, idempotenza, stato intermedio e riconciliazione.
- Interfacce, mockabilità e genericità di Service e Repository non sono vincoli di questa overview e appartengono ai livelli tecnici dedicati.
- Gli ADR nati da Portfolio non diventano automaticamente regole universali dei domini futuri.

### ADR da riallineare

- ADR-0001 prescrive sempre un Application Model: deve consentire il passaggio diretto di valori e rendere il Business Model opzionale.
- ADR-0009 è sostanzialmente confermato; deve essere valutata la generalizzazione da Portfolio a tutti i domini.
- ADR-0010 conferma l'orchestrazione nel Controller; deve estendere il concetto di transazione a quello più generale di atomicità applicativa.

---

## 3. API pubbliche e Contracts

### Destinazione prevista

`DomainArchitecture.md` oppure un futuro documento specialistico dedicato all'architettura delle API.

### Contenuto emerso durante il consolidamento

- Le route seguono la forma generale `<ServerBaseUrl>/api/<Domain>/<ControllerHierarchy>/<Action>`.
- Le action CRUD sono esplicite, per esempio `Get`, `Create`, `Update` e `Delete`.
- Il verbo HTTP mantiene una semantica coerente con l'action.
- Le operazioni `Update` sono normalmente parziali: `null` nella request significa proprietà non modificata.
- Il reset esplicito di una proprietà nullable avviene tramite un'operazione dedicata.
- Il Controller spacchetta la request e orchestra le chiamate al Service.
- Le response DTO server-side possono essere costruite da Data Entity o Business Entity.
- Le request DTO devono essere costruibili dal deserializzatore.
- I DTO possono utilizzare esclusivamente primary constructor.
- La specifica OpenAPI costituisce la descrizione autorevole del protocollo pubblico.
- Server e client possono avere implementazioni differenti dei modelli del wire contract.
- Una breaking change è completa soltanto quando tutti i client interessati sono aggiornati.
- Server e client interessati vengono normalmente rilasciati insieme.
- Il versionamento parallelo viene introdotto soltanto quando emerge una reale necessità.

---

## 4. Applications

### Destinazione prevista

`WebApplicationArchitecture.md` e futura documentazione delle altre tipologie di Application.

### Contenuto da consolidare

Le Applications consumano le API pubbliche senza accedere a entità persistite, Repository, modelli interni o dettagli di implementazione del server.

Una Web Application può separare responsabilità come:

- Controller;
- Page Service;
- Page Model;
- View;
- Components;
- API Client;
- routing;
- JavaScript;
- CSS.

Ogni Application è responsabile dell'interazione con l'utente, dello stato di presentazione, del routing applicativo e delle eventuali cache locali.

Un'Application può consumare più domini configurandoli come servizi indipendenti. Non deve dedurne la co-ubicazione dalla radice degli URL.

---

## 5. Infrastruttura

### Destinazione prevista

`InfrastructureArchitecture.md`

### Contenuto da consolidare

L'infrastruttura comprende capacità tecniche come:

- configurazione;
- logging;
- gestione degli errori;
- persistenza;
- filesystem e media;
- caching;
- documentazione delle API;
- integrazione con provider esterni.

Ogni dominio configura autonomamente le istanze dei servizi tecnici utilizzati, anche quando più domini adottano la stessa implementazione.

Il provider di logging può essere comune al processo, mentre categorie, livelli e destinazioni possono essere separati per dominio. I componenti Shared devono conservare il contesto del dominio per attribuire correttamente i log.

Le responsabilità legate al processo rimangono comuni all'host e non contengono decisioni applicative.

---

## 6. Sicurezza

### Destinazione prevista

`SecurityArchitecture.md`

### Contenuto da consolidare

- Ogni dominio possiede account, ruoli, permessi e configurazione di sicurezza.
- Uno stesso essere umano registrato in più domini possiede account distinti e non correlati automaticamente.
- L'account utente è unico all'interno del dominio e può essere utilizzato da più Applications.
- Identità del client e identità dell'utente sono distinte.
- L'autorizzazione valuta sia le capacità del client sia i permessi dell'utente.
- Un client non amministrativo non può accedere alle API amministrative anche se utilizzato da un amministratore.
- Un client amministrativo non attribuisce privilegi a un utente che non li possiede.
- Il backend rimane la fonte autorevole di autenticazione e autorizzazione.

### Punto aperto

Il meccanismo di autenticazione dei public client, che non possono conservare un segreto permanente, deve essere deciso durante il consolidamento della Security Architecture.

---

## 7. Testing

### Destinazione prevista

`TestingArchitecture.md`

### Contenuto da consolidare

La suite riflette le responsabilità dei componenti produttivi e verifica ogni contratto nel livello appropriato.

I livelli attualmente individuati comprendono:

- Unit Test;
- Framework Test;
- Contract Test;
- Integration Test.

I test non devono duplicare inutilmente comportamenti già verificati in un altro livello.

---

## 8. Shared Framework

### Destinazione prevista

`SharedFramework.md`

### Contenuto da consolidare

Lo Shared Framework raccoglie concetti, pipeline, astrazioni e comportamenti tecnici che hanno dimostrato di essere indipendenti dal contesto applicativo di origine.

Un'interfaccia condivisa è giustificata quando esiste un consumatore programmatico reale. Può rappresentare comportamento, dati o una capacità di classificazione.

Entità analoghe appartenenti a domini differenti rimangono separate finché non emerge un'astrazione semanticamente stabile e realmente consumata.

### Dettagli destinati a documenti specialistici

`SharedFramework.md` costituisce l'overview autorevole dei confini e del modello evolutivo. I seguenti dettagli, rimossi dall'overview durante il consolidamento, dovranno confluire in documenti specialistici dedicati:

| Area | Contenuti |
|---|---|
| Request Contracts | API concrete di `IRequest`, contratti opzionali di capacità e convenzioni dei DTO. |
| Request Pipeline | Sequenza di esecuzione, integrazione MVC, gestione delle eccezioni e mapping HTTP. |
| Normalizzazione | Attributi disponibili, regole, ricorsione, costruzione dei piani e cache. |
| Validazione | Attributi disponibili, aggregazione degli errori, ricorsione, piani e futura estensione business. |
| Bulk Operations | Strategie, atomicità, ordinamento, chiavi, dipendenze tra item e formato delle response. |
| Testing Architecture | Separazione tra test dei motori, della pipeline e della configurazione dei Contracts. |

La scelta di esporre `Normalize()` e `Validate()` su `IRequest` rimane approvata e documentata dall'ADR-0006. Le implementazioni predefinite fungono da façade semantica verso i motori Shared: la pipeline invoca la Request senza dipendere direttamente dai motori, mentre le Request concrete non implementano gli algoritmi.

### Decisioni emerse durante il consolidamento

- Lo Shared Framework è composto da servizi con confini logici separati, anche quando convivono nello stesso progetto e nella stessa DLL.
- Namespace e alberatura di cartelle devono preservare la futura estraibilità di ciascun servizio in una DLL dedicata.
- Le dipendenze tra servizi Shared sono ammesse se esplicite, unidirezionali, acicliche e rivolte esclusivamente alla superficie pubblica del servizio consumato.
- La superficie pubblica non implica necessariamente una `interface` C#.
- Non è ancora obbligatorio il pattern interfaccia più implementazione per ogni servizio.
- Le dipendenze di terze parti sono normalmente nascoste, salvo che vengano adottate deliberatamente come parte del contratto tecnico.
- Il dominio compone i servizi Shared che utilizza; l'host continua a conoscere soltanto i punti di ingresso pubblici del dominio.
- I servizi configurabili o stateful sono isolati per dominio; i servizi stateless possono essere fisicamente condivisi.
- Lo Shared Framework definisce meccanismi e tassonomie tecniche; semantica e codici applicativi appartengono ai domini.
- La normalizzazione precede la validazione ed è una trasformazione tecnica deterministica e possibilmente idempotente.
- La validazione canonica è attuale; la validazione business estensibile è pianificata ma i relativi contratti non sono ancora progettati.
- Una normalizzazione custom di dominio è considerata possibile, ma non pianificata.
- Capacità opzionali come ordinabilità e chiavi non devono appesantire il contratto base di `IRequest`.

### Punti aperti

- Definire il contratto opzionale con cui una Request espone la propria chiave.
- Valutare i nomi degli attributi candidati `[Id]` e `[Key]` ed evitare ambiguità con le convenzioni .NET esistenti.
- Decidere se chiave logica e vincolo di unicità debbano rimanere concetti distinti.
- Per ora non supportare più chiavi logiche alternative per lo stesso tipo di Request.
- Progettare in futuro il contratto di validazione business senza introdurre ora `IValidatable` o validatori astratti prematuri.
- Verificare durante il consolidamento degli ADR che ADR-0006 espliciti la leggibilità semantica e il disaccoppiamento della pipeline dai motori come motivazioni della scelta.

### Bulk Operations

- Ogni chiamata API ordinaria è atomica.
- Le bulk request espongono strategie indipendenti di persistenza e gestione degli errori.
- La persistenza può essere `AllOrNothing` oppure per singolo item.
- La valutazione può interrompersi al primo item fallito oppure proseguire su tutti gli item.
- Ogni singolo item rimane atomico.
- La request contenitore valida option, struttura della lista e unicità prima di qualsiasi persistenza.
- La presenza di duplicati invalida sempre l'intera bulk request.
- Le chiavi logiche o fisiche identificano gli item nella response.
- Gli item possono dichiarare una capacità intrinseca di ordinamento senza esporne la semantica al framework.
- Il framework conosce che un item è ordinabile; il DTO definisce come ordinarlo.
- Relazioni autoreferenziali sono ammesse, mentre auto-riferimenti e cicli tra istanze non sono validi.
- Il chiamante fornisce normalmente gli item in un ordine logicamente valido.
- Una response bulk elaborata secondo la strategia richiesta restituisce `200 OK` anche in presenza di fallimenti individuali ammessi dalla strategia.
- Il body distingue esito aggregato ed esiti individuali `Succeeded`, `Failed` e `NotProcessed`.
- Gli errori globali della bulk request producono una risposta di errore e nessuna persistenza.

---

## 9. Evoluzione e pratiche di engineering

### Destinazioni previste

- `MpsPlaybook.md`
- `ArchitectureRoadmap.md`

### Contenuto da consolidare

- La documentazione e l'architettura evolvono insieme al progetto.
- Nuove cartelle, layer, progetti e astrazioni vengono introdotti soltanto quando rappresentano responsabilità reali.
- La duplicazione può essere accettata mentre il problema non è ancora sufficientemente compreso.
- La separazione in DLL dei layer interni dei domini rimane un possibile refactoring futuro, non una decisione né un TODO attuale.
- Le milestone tecniche appartengono alla roadmap e non all'overview architetturale.

---

## 10. Architecture Decision Records

### Destinazione prevista

`ADR/README.md`

### Contenuto da consolidare

Gli ADR preservano contesto, decisione, motivazioni, conseguenze e alternative delle scelte architetturali significative e durature.

Non sostituiscono la descrizione dell'architettura corrente.

---

## 11. Collaborazione con assistenti AI

### Destinazione prevista

`Documentation/AI`

### Contenuto da consolidare

Gli assistenti AI supportano progettazione, refactoring, implementazione e documentazione, ma non costituiscono una fonte autorevole dell'architettura.

Le decisioni consolidate devono essere registrate nella documentazione appropriata o negli ADR.

---

## 12. Organizzazione della documentazione

### Destinazioni previste

- `Documentation/README.md`
- `Documentation/Home.md`
- `Documentation/Engineering/MpsPlaybook.md`

### Contenuto da consolidare

- La documentazione è organizzata per responsabilità e livelli di profondità progressivi.
- Ogni concetto possiede una fonte autorevole.
- Gli altri documenti vi fanno riferimento senza duplicarne il contenuto.
- `Architecture.md` rappresenta l'interfaccia architetturale generale.
- I documenti specialistici descrivono l'implementazione dei singoli sottosistemi.
- Gli ADR spiegano le motivazioni delle decisioni significative.
- Engineering definisce come il progetto viene sviluppato.
