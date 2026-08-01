# Architettura di MultiPurposeServer

## 1. Scopo del documento

Questo documento descrive la visione complessiva, la struttura e i confini architetturali di MultiPurposeServer.

Non rappresenta una raccolta astratta di best practice generiche. I principi e le decisioni qui descritti derivano dall'evoluzione reale del progetto, dai refactoring effettuati, dai test scritti e dalle esigenze emerse durante lo sviluppo dei primi moduli.

L'obiettivo è mantenere MultiPurposeServer semplice da comprendere, coerente tra moduli differenti, estendibile nel tempo, facilmente testabile, indipendente dai singoli protocolli o client e adatto a ospitare più domini funzionali senza creare accoppiamenti inutili.

Il documento deve evolvere insieme al progetto. Quando viene presa una decisione architetturale destinata ad avere valore anche in futuro, questa deve essere integrata nella documentazione architetturale oppure formalizzata tramite un Architecture Decision Record.

---

## 2. Relazione con la documentazione del progetto

Questo documento rappresenta il punto di ingresso della documentazione architetturale.

Descrive come è organizzato MultiPurposeServer nel suo insieme e assegna le principali responsabilità ai diversi sottosistemi. Gli approfondimenti appartengono invece ai documenti tematici che descrivono in dettaglio domini, Applications, infrastruttura, sicurezza, testing e framework condivisi.

Le pratiche utilizzate per progettare, implementare, testare, rifattorizzare e documentare il progetto sono definite nel `Documentation/Engineering/MpsPlaybook.md`.

I concetti e i comportamenti riutilizzabili tra più domini sono descritti in `SharedFramework.md`.

Le evoluzioni architetturali pianificate sono raccolte in `ArchitectureRoadmap.md`.

Le decisioni architetturali significative e durature vengono formalizzate tramite Architecture Decision Record.

Uno stesso argomento può essere trattato in documenti differenti quando cambia il punto di vista. Ogni documento deve tuttavia rimanere focalizzato sulla propria responsabilità ed evitare di duplicare lo stesso contenuto.

---

## 3. Visione di MultiPurposeServer

MultiPurposeServer non è un'applicazione monolitica dedicata a un solo scopo.

È un host e una piattaforma comune destinata a supportare più domini funzionali indipendenti, per esempio:

- Portfolio;
- ModelBook;
- Skating;
- moduli futuri ancora non definiti.

Ogni dominio deve poter evolvere in modo indipendente, riutilizzare l'infrastruttura comune, esporre le proprie API, possedere i propri Contracts, Service e Repository e mantenere separata la propria logica di business.

L'aggiunta di un nuovo dominio non deve richiedere modifiche invasive agli altri domini.

---

## 4. Struttura principale del repository

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

La documentazione architetturale è organizzata nella cartella:

```text
Documentation/
└── Architecture/
```

La struttura documentale deve crescere solo quando emerge una reale necessità organizzativa. Non devono essere introdotte cartelle, classificazioni o documenti vuoti per anticipare esigenze future.

---

## 5. Principi architetturali fondamentali

### 5.1 Separazione delle responsabilità

Ogni classe e ogni componente architetturale devono avere una responsabilità chiara e circoscritta.

Una classe non deve occuparsi contemporaneamente di accesso ai dati, logica di business, mapping HTTP, serializzazione, gestione dei file, sicurezza e presentazione.

L'obiettivo non è ottenere classi piccole a ogni costo, ma componenti semplici da comprendere, testare e modificare.

### 5.2 Il dominio applicativo non conosce il trasporto

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

### 5.3 I Repository non conoscono HTTP

I Repository si occupano esclusivamente di persistenza e recupero dei dati.

Non devono conoscere DTO, Controller, HTTP, Swagger, claims, authentication, response code o dettagli della UI.

### 5.4 I Controller orchestrano

I Controller devono limitarsi a:

- ricevere la richiesta;
- ricevere Request già normalizzate e validate dai componenti infrastrutturali;
- convertire i DTO in modelli applicativi;
- invocare i Service;
- convertire i risultati applicativi in DTO;
- restituire la risposta HTTP corretta.

La logica di business appartiene ai Service.

### 5.5 Le dipendenze puntano verso l'interno

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

## 6. Mappa architetturale del sistema

MultiPurposeServer è composto da sottosistemi con responsabilità distinte.

### 6.1 Domains

I Domains rappresentano i moduli funzionali del server.

Ogni dominio possiede i propri Contracts, Service, Repository, componenti di persistenza e configurazione. Deve poter evolvere indipendentemente dagli altri domini e registrare autonomamente la propria infrastruttura.

### 6.2 Applications

Le Applications sono client di MultiPurposeServer.

Consumano esclusivamente i Contracts pubblici e non devono accedere direttamente a Entity, DbContext, Repository o modelli interni dei Service.

### 6.3 Shared

`Shared` contiene esclusivamente componenti realmente riutilizzabili tra più domini e privi di dipendenze specifiche dal modulo che li ha originati.

Non deve essere utilizzato come contenitore generico per codice privo di una collocazione chiara.

### 6.4 Host MultiPurposeServer

Il progetto `MultiPurposeServer` rappresenta l'host ASP.NET Core e il composition root dell'applicazione.

È responsabile dell'avvio, della composizione dei moduli, dei middleware comuni, della configurazione, del logging e dell'esposizione finale degli endpoint.

Non contiene logica di business specifica dei singoli domini.

### 6.5 Infrastructure

L'infrastruttura comprende i componenti trasversali che supportano l'esecuzione del sistema, tra cui media, filesystem, caching, configurazione, logging, gestione degli errori e integrazione Swagger.

Questi componenti devono rimanere separati dalla logica di business.

### 6.6 Security

La sicurezza distingue l'identità del client applicativo dall'identità dell'utente finale.

Authentication e Authorization devono essere applicate dal backend e non delegate esclusivamente alle Applications.

### 6.7 Tests

La suite di test è organizzata secondo le responsabilità architetturali dei componenti produttivi.

Ogni livello verifica il proprio contratto senza duplicare il comportamento già coperto da altri livelli.

---

## 7. Architettura dei domini

I domini rappresentano moduli funzionali indipendenti di MultiPurposeServer.

Ogni dominio è responsabile della propria logica applicativa, dei propri Contracts, della persistenza, delle dipendenze e dell'esposizione delle API.

Un dominio deve poter:

- evolvere senza richiedere modifiche invasive agli altri domini;
- mantenere separata la propria logica di business;
- registrare autonomamente la propria infrastruttura;
- possedere il proprio DbContext e le proprie migration;
- riutilizzare esclusivamente componenti realmente condivisi.

L'host compone i domini, ma non ne conosce i dettagli interni.

La struttura interna, le dipendenze consentite e le responsabilità di Contracts, Controller, Service, Repository e persistenza sono descritte in:

- `DomainArchitecture.md`

---

## 8. Architettura delle Applications

Le Applications rappresentano i client di MultiPurposeServer.

Consumano esclusivamente i Contracts pubblici esposti dai domini e non devono accedere direttamente a Entity, DbContext, Repository o modelli interni dei Service.

Ogni Application è responsabile di:

- orchestrare l'interazione con l'utente;
- comporre lo stato necessario alla presentazione;
- coordinare il routing applicativo;
- gestire cache e persistenza locale quando necessario;
- renderizzare l'interfaccia utente.

La struttura interna di una Web Application separa responsabilità come:

- Controller;
- Page Service;
- Page Model;
- View;
- Components;
- API Client;
- Routing;
- JavaScript;
- CSS.

L'obiettivo è mantenere separati orchestrazione HTTP, logica applicativa e presentazione.

L'architettura completa delle Web Applications è descritta in:

- `WebApplicationArchitecture.md`

---

## 9. Architettura dell'infrastruttura

L'infrastruttura raccoglie i servizi tecnici trasversali utilizzati dalla piattaforma.

Il suo scopo consiste nel fornire capacità comuni ai domini senza introdurre dipendenze dalla logica di business.

Comprende, tra gli altri:

- Host MultiPurposeServer;
- configurazione;
- logging;
- gestione degli errori;
- media management;
- caching;
- documentazione delle API;
- servizi infrastrutturali condivisi.

I domini utilizzano questi servizi senza conoscerne necessariamente l'implementazione.

L'architettura completa dell'infrastruttura è descritta in:

- `InfrastructureArchitecture.md`

---

## 10. Architettura della sicurezza

La sicurezza rappresenta una responsabilità trasversale dell'intero sistema.

MultiPurposeServer distingue chiaramente:

- autenticazione del client;
- autenticazione dell'utente;
- autorizzazione;
- permessi.

Le Applications possono migliorare l'esperienza utente, ma il backend rimane sempre la fonte autorevole delle decisioni di sicurezza.

L'architettura della sicurezza descrive inoltre:

- il modello di autenticazione;
- il modello dei permessi;
- la protezione delle API;
- la gestione dei segreti;
- la sicurezza dell'infrastruttura.

Per i dettagli fare riferimento a:

- `SecurityArchitecture.md`

---

## 11. Architettura del testing

La suite di test costituisce parte integrante dell'architettura di MultiPurposeServer.

La sua organizzazione riflette la struttura della solution e garantisce che ogni responsabilità venga verificata nel livello più appropriato.

La strategia di testing distingue differenti livelli di verifica, tra cui:

- Unit Test;
- Framework Test;
- Contract Test;
- Integration Test.

Ogni livello verifica responsabilità differenti ed evita duplicazioni inutili all'interno della suite.

L'architettura completa del sistema di testing è descritta in:

- `TestingArchitecture.md`

Le convenzioni di scrittura dei test e le pratiche di sviluppo sono invece definite nel `Documentation/Engineering/MpsPlaybook.md`.

---

## 12. Shared Framework

`Shared` contiene esclusivamente componenti realmente riutilizzabili tra più domini.

Lo Shared Framework raccoglie concetti, pipeline, astrazioni e comportamenti infrastrutturali che hanno dimostrato di essere indipendenti dal contesto applicativo che li ha originati.

La promozione di un componente nello Shared Framework avviene soltanto quando il suo significato rimane stabile e riutilizzabile in contesti differenti.

> **Shared is Earned, not Planned.**

L'architettura e l'evoluzione dello Shared Framework sono descritte in:

- `SharedFramework.md`

---

## 13. Evoluzione dell'architettura

L'architettura di MultiPurposeServer è progettata per evolvere nel tempo.

Nuovi domini, nuove Applications e nuovi servizi infrastrutturali devono poter essere introdotti senza modificare la struttura fondamentale del sistema.

Ogni evoluzione dovrebbe preservare:

- la separazione delle responsabilità;
- l'indipendenza dei domini;
- la chiarezza dei confini architetturali;
- la direzione delle dipendenze;
- la testabilità;
- la coerenza della documentazione.

Nuove cartelle, nuovi layer, nuovi progetti o nuove astrazioni devono essere introdotti soltanto quando rappresentano una responsabilità reale e riducono un accoppiamento concreto.

Non si deve anticipare la complessità futura implementando strutture che il progetto non richiede ancora.

Le evoluzioni previste sono raccolte in `ArchitectureRoadmap.md`.

---

## 14. Architecture Decision Records

Le decisioni architetturali significative e durature vengono documentate tramite Architecture Decision Record.

Gli ADR preservano il contesto e le motivazioni delle principali scelte progettuali, permettendo di comprenderne l'origine anche quando il sistema evolve.

Ogni ADR dovrebbe descrivere almeno:

- il contesto;
- il problema o la necessità;
- la decisione adottata;
- le conseguenze;
- eventuali alternative considerate.

Gli ADR costituiscono la memoria storica delle principali decisioni architetturali del progetto.

---

## 15. Collaborazione con strumenti di Intelligenza Artificiale

MultiPurposeServer utilizza strumenti di Intelligenza Artificiale come supporto alle attività di sviluppo.

Le modalità operative, le responsabilità e le istruzioni specifiche degli assistenti AI sono documentate nella cartella:

- `Documentation/AI/`

Gli strumenti AI non rappresentano una fonte autorevole per l'architettura del progetto.

Le decisioni definitive rimangono responsabilità dello sviluppatore e devono essere consolidate nella documentazione tecnica o negli ADR.

---

## 16. Relazione con il MpsPlaybook

La documentazione architetturale descrive come è organizzato MultiPurposeServer e assegna le responsabilità ai diversi componenti del sistema.

Il `Documentation/Engineering/MpsPlaybook.md` descrive invece come il progetto viene sviluppato.

In particolare:

- `Architecture.md` definisce la visione complessiva e i confini del sistema;
- i documenti architetturali specializzati descrivono i singoli sottosistemi;
- `SharedFramework.md` descrive il funzionamento dei componenti comuni;
- `Documentation/Engineering/MpsPlaybook.md` definisce i principi e le pratiche di ingegneria.

Questi documenti sono complementari e devono evolvere in modo coerente.

Qualora una convenzione operativa risulti in contrasto con una decisione architetturale, la documentazione architetturale ha la precedenza.

---

## 17. Organizzazione della documentazione

La documentazione tecnica di MultiPurposeServer è organizzata secondo responsabilità.

`Architecture.md` rappresenta il punto di ingresso della documentazione architetturale.

Gli approfondimenti appartengono ai documenti specializzati:

- `DomainArchitecture.md`;
- `WebApplicationArchitecture.md`;
- `InfrastructureArchitecture.md`;
- `SecurityArchitecture.md`;
- `TestingArchitecture.md`;
- `SharedFramework.md`.

Ogni documento costituisce la fonte autorevole del proprio argomento.

Uno stesso tema può essere osservato da prospettive differenti, ma lo stesso contenuto non deve essere duplicato in più documenti.

Quando appropriato, i documenti si richiamano mediante la sezione `Vedi anche`.

---

## 18. Vedi anche

### Architettura

- `DomainArchitecture.md`
- `WebApplicationArchitecture.md`
- `InfrastructureArchitecture.md`
- `SecurityArchitecture.md`
- `TestingArchitecture.md`
- `SharedFramework.md`

### Evoluzione

- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`

### Processo di sviluppo

- `Documentation/Engineering/MpsPlaybook.md`