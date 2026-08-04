# Architettura dei Domini

## 1. Scopo del documento

Questo documento descrive l'architettura interna dei domini di MultiPurposeServer.

Definisce come un dominio deve organizzare il proprio protocollo pubblico, la logica applicativa, l'accesso ai dati, la persistenza e la registrazione delle dipendenze.

L'obiettivo è permettere a ciascun dominio di evolvere in modo indipendente, mantenendo responsabilità chiare e riducendo l'accoppiamento con l'host, con le Applications e con gli altri domini.

Il documento non prescrive una struttura identica per ogni modulo. Ogni progetto, cartella o astrazione deve esistere soltanto quando rappresenta una responsabilità reale.

---

## 2. Ruolo di un dominio

Un dominio rappresenta un modulo funzionale indipendente di MultiPurposeServer.

Esempi attuali o previsti includono:

- Portfolio;
- ModelBook;
- Skating;
- moduli futuri ancora non definiti.

Ogni dominio è responsabile della propria logica applicativa, dei propri Contracts, della persistenza, delle dipendenze e dell'esposizione delle API.

Un dominio deve poter:

- evolvere senza richiedere modifiche invasive agli altri domini;
- riutilizzare i componenti realmente condivisi;
- mantenere separata la propria logica di business;
- registrare autonomamente la propria infrastruttura;
- esporre API indipendenti;
- possedere il proprio DbContext e le proprie migration quando utilizza una persistenza relazionale.

L'host compone i domini, ma non ne conosce i dettagli interni.

---

## 3. Struttura di un dominio

Un dominio può essere organizzato attraverso progetti simili ai seguenti:

```text
Domains/
└── Portfolio/
    ├── Portfolio.Api
    ├── Portfolio.Contracts
    ├── Portfolio.Data
    ├── Portfolio.Constants
    └── altri progetti specifici del dominio
```

Questa struttura rappresenta un possibile punto di partenza, non un template da replicare meccanicamente.

Ogni progetto deve essere introdotto soltanto quando:

- esiste una responsabilità distinta;
- la separazione riduce un accoppiamento concreto;
- il numero di classi rende difficile la navigazione;
- la responsabilità richiede dipendenze differenti;
- il riuso interno al dominio giustifica un confine più esplicito.

Non devono essere create cartelle vuote, tassonomie preventive o progetti privi di una responsabilità reale.

---

## 4. Dipendenze e confini

Le dipendenze devono puntare verso i componenti che esprimono responsabilità più interne.

Il flusso generale è:

```text
Controller
    ↓
Contracts
    ↓ mapping
Services / Services.Models
    ↓
Repositories
    ↓
Data.Models / Database
```

I layer interni non devono dipendere dai dettagli dei layer esterni.

In particolare:

- i Service non dipendono dai Contracts HTTP;
- i Repository non dipendono da Controller, DTO o HTTP;
- le Entity non vengono utilizzate come contratti pubblici;
- i Contracts non contengono logica di persistenza;
- l'host non dipende dai dettagli implementativi del dominio;
- le Applications non accedono direttamente a Entity, DbContext, Repository o modelli interni dei Service.

Le dipendenze circolari tra progetti o layer non sono ammesse.

---

## 5. Progetto `*.Api`

Il progetto `*.Api` rappresenta il punto di composizione del dominio all'interno del server.

Può contenere:

- Controller;
- Authentication specifica del dominio;
- configurazione Swagger;
- extension per Dependency Injection;
- Service applicativi;
- Repository;
- modelli applicativi interni;
- Options;
- componenti infrastrutturali specifici del dominio.

La presenza di questi elementi nello stesso progetto non elimina la necessità di mantenerne separate le responsabilità.

Quando il dominio cresce, componenti differenti possono essere estratti in progetti autonomi soltanto se la separazione produce un beneficio architetturale concreto.

---

## 6. Contracts

Il progetto `*.Contracts` contiene il protocollo pubblico esposto dal dominio.

Comprende normalmente:

- Request DTO;
- Response DTO;
- DTO per operazioni Bulk;
- oggetti condivisi con i client;
- contratti pubblici necessari alle Applications.

I Contracts descrivono i dati scambiati tra il client e l'applicazione.

Non rappresentano il linguaggio interno del dominio.

### 6.1 Responsabilità

I Contracts devono:

- definire esclusivamente il contratto pubblico;
- essere serializzabili;
- non contenere logica di business;
- mantenere le Request indipendenti da Entity, DbContext e componenti di persistenza;
- consentire ai Response DTO di dipendere dalle Entity esclusivamente per tradurre il modello interno nel contratto pubblico, secondo quanto definito in ADR-0009;
- non accedere da alcun Contract a DbContext, Repository, query o logica di persistenza;
- essere riutilizzabili dal client.

### Mapping dei Response DTO

I Response DTO sono responsabili della traduzione del modello interno nella rappresentazione pubblica dell'API.

Questa responsabilità giustifica la dipendenza unidirezionale dei Response DTO dalle Entity del dominio.

Per i dettagli della decisione architetturale fare riferimento a:

- ADR-0009 — Response DTO map domain entities

### 6.2 Indipendenza dei Service

I Service non devono ricevere o restituire direttamente DTO pubblici quando ciò li rende dipendenti dal trasporto.

Il mapping tra Contracts e modelli applicativi appartiene al confine dell'API.

La dipendenza corretta è:

```text
Controller
    ↓
Request DTO
    ↓ mapping
Application Model
    ↓
Service
```

### 6.3 Request Contract

Le Request che partecipano alla pipeline condivisa implementano `IRequest`.

Normalizzazione, validazione e operazioni Bulk appartengono allo Shared Framework e sono descritte in `SharedFramework.md`.

I test dei Contracts verificano la corretta configurazione dichiarativa del DTO, non duplicano il comportamento già coperto dai test del framework.

---

## 7. Controller

I Controller rappresentano il punto di ingresso HTTP del dominio.

Devono limitarsi a orchestrare la richiesta.

Le loro responsabilità comprendono:

- ricevere route, query string e body;
- ricevere Request già normalizzate e validate dalla pipeline;
- convertire i DTO in modelli applicativi;
- invocare i Service;
- convertire i risultati applicativi in DTO;
- tradurre gli esiti applicativi nella risposta HTTP corretta.

I Controller non devono contenere logica di business, accesso diretto al database o gestione diretta della persistenza.

La logica non banale deve appartenere ai Service o ai componenti infrastrutturali appropriati.

---

## 8. Service applicativi

I Service contengono la logica applicativa del dominio.

Orchestrano:

- Repository;
- filesystem;
- servizi esterni;
- trasformazioni;
- operazioni di business;
- componenti infrastrutturali specifici del dominio.

I Service non devono conoscere dettagli del trasporto HTTP.

Devono poter essere utilizzati da Controller, worker, console application, test e futuri endpoint senza dipendere dal protocollo che li invoca.

### 8.1 Responsabilità

Un Service deve:

- esprimere casi d'uso significativi per il dominio;
- coordinare le dipendenze necessarie all'operazione;
- mantenere separata la logica applicativa dalla persistenza;
- restituire risultati adatti al layer applicativo;
- evitare dipendenze da DTO pubblici quando non necessarie;
- rendere espliciti gli esiti previsti dell'operazione.

### 8.2 Confini

Un Service non deve:

- restituire direttamente `IActionResult`;
- conoscere status code HTTP;
- accedere direttamente al Model Binding;
- dipendere da Controller;
- utilizzare Entity come contratto pubblico;
- incorporare dettagli di rendering o presentazione.

---

## 9. `Services.Models`

La cartella `Services.Models` contiene i modelli applicativi interni al dominio.

Questi oggetti:

- non sono Entity del database;
- non sono DTO pubblici;
- descrivono il linguaggio interno dei Service;
- mantengono i Service indipendenti dai Contracts;
- rappresentano richieste, risultati o valori utilizzati nei casi d'uso.

Esempi:

```text
BulkUpdateItem<T>
CacheClearOperationRequest
CacheClearOperationResult
MediaFile
MediaProfile
```

Per quanto possibile, i modelli applicativi devono avere nomi che ne rendano evidente il ruolo.

La separazione in sottocategorie come `Commands`, `Results`, `ValueObjects` o `Requests` deve essere introdotta soltanto quando il numero di classi rende realmente utile una classificazione più dettagliata.

Non si deve anticipare la complessità futura con cartelle vuote o tassonomie premature.

---

## 10. Repository

I Repository rappresentano il confine tra la logica applicativa e la persistenza.

Le loro interfacce devono descrivere operazioni significative per il dominio, non replicare meccanicamente tutte le funzionalità di Entity Framework.

I Repository devono:

- nascondere i dettagli di persistenza;
- mantenere query e operazioni dati in un punto coerente;
- restituire modelli adatti al layer applicativo;
- evitare dipendenze verso Contracts, Controller e HTTP;
- rendere esplicite le operazioni transazionali necessarie.

### 10.1 Operazioni Bulk

Le operazioni Bulk devono:

- evitare salvataggi parziali non desiderati;
- verificare l'esistenza degli elementi richiesti;
- mantenere un comportamento transazionale coerente;
- restituire esiti comprensibili al layer applicativo.

La strategia di gestione degli errori collettivi appartiene al contratto dell'operazione Bulk e non deve introdurre dipendenze HTTP nel Repository.

### 10.2 Strategia di caricamento delle navigation property

Il dominio Portfolio utilizza intenzionalmente EF Core Lazy Loading Proxies.

La scelta consente ai Repository di mantenere query focalizzate senza dover dichiarare sistematicamente `Include` per ogni navigation property utilizzata dai livelli successivi.

Le navigation property necessarie al lazy loading devono essere dichiarate `virtual` e le Entity devono rimanere compatibili con la generazione dei proxy EF Core.

Il mapping verso i Response DTO deve avvenire mentre il `DbContext` dello scope applicativo è ancora attivo.

Il lazy loading può generare query aggiuntive e possibili scenari N+1. Tali casi devono essere ottimizzati mediante eager loading o proiezioni soltanto quando emergano esigenze concrete di prestazioni o misurazioni che ne dimostrino la necessità.


---

## 11. Persistenza e progetto `*.Data`

Il progetto `*.Data` contiene gli elementi strettamente legati alla persistenza.

Comprende normalmente:

- DbContext;
- Entity;
- configurazioni Entity Framework;
- migration;
- mapping verso il database;
- componenti specifici del provider utilizzato.

Le Entity rappresentano lo stato persistito e non devono essere esposte come contratto pubblico dell'API.

Ogni dominio è responsabile del proprio DbContext e delle proprie migration.

L'host non mantiene un database condiviso tra i domini.

Qualora in futuro emergano dati realmente comuni, come utenti, autorizzazioni, configurazioni globali o audit, la loro responsabilità dovrà essere definita centralmente e non assegnata arbitrariamente a un dominio esistente.

---

## 12. Progetto `*.Constants`

Un progetto `*.Constants` può essere introdotto quando la quantità o il riuso delle costanti specifiche del dominio giustificano una responsabilità autonoma.

Non deve essere creato automaticamente per ogni dominio.

Le costanti realmente condivise tra più domini possono essere promosse in `Shared` soltanto quando mantengono lo stesso significato fuori dal dominio originario.

---

## 13. Dependency Injection

Le dipendenze applicative devono essere registrate tramite Dependency Injection.

```csharp
services.AddScoped<IAlbumRepository, AlbumRepository>();
services.AddScoped<IAlbumService, AlbumService>();
services.AddScoped<IImageResizer, ImageMagickResizer>();
```

Le implementazioni concrete non devono essere istanziate direttamente nei componenti che le utilizzano.

Le extension DI devono essere:

- piccole;
- leggibili;
- prive di stato globale;
- focalizzate sulla composizione;
- indipendenti da campi statici utilizzati per conservare configurazioni.

### 13.1 Registrazione del dominio

Ogni dominio è responsabile della registrazione completa delle proprie dipendenze.

L'host deve conoscere soltanto il punto di ingresso pubblico del dominio.

Esempio:

```csharp
builder.Services.AddPortfolio(configuration);
```

L'extension del dominio deve incapsulare i dettagli relativi a:

- Service;
- Repository;
- Options;
- DbContext;
- Authentication specifica;
- componenti infrastrutturali interni;
- eventuali servizi di inizializzazione.

### 13.2 Inizializzazione

Quando un dominio richiede operazioni asincrone di inizializzazione, deve esporre un punto di ingresso esplicito e mantenere i dettagli al proprio interno.

Il file `Program.cs` dell'host deve rimanere un compositore leggibile e non contenere logica di inizializzazione specifica del dominio.

---

## 14. Configurazione del dominio

Le configurazioni devono essere rappresentate tramite classi Options dedicate.

Esempi:

```text
PortfolioAuthenticationOptions
PortfolioMediaOptions
PortfolioCacheOptions
PortfolioAlbumOptions
```

Le extension di registrazione devono leggere la configurazione tramite il parametro ricevuto.

Non devono conservarla in campi statici globali.

I segreti non devono essere inseriti nel repository.

Le responsabilità generali relative a configurazione, segreti e infrastruttura sono approfondite in `InfrastructureArchitecture.md` e `SecurityArchitecture.md`.

---

## 15. Error handling tra i layer

Gli errori prevedibili devono essere gestiti dal layer che ne possiede la responsabilità.

Esempi:

- la Request Pipeline gestisce normalizzazione e validazione del contratto;
- il Service rappresenta gli esiti applicativi, come un elemento non trovato;
- il Repository segnala errori di persistenza senza tradurli in HTTP;
- il Controller traduce gli esiti applicativi nella risposta HTTP;
- le eccezioni di sistema vengono registrate e gestite dai componenti infrastrutturali appropriati.

I dettagli tecnici non devono essere esposti indiscriminatamente ai client.

Le eccezioni non devono essere utilizzate come normale meccanismo di controllo del flusso quando un risultato esplicito rappresenta meglio un esito previsto.

---

## 16. Naming architetturale

I nomi devono comunicare il ruolo architetturale dell'oggetto.

### 16.1 Contracts

I tipi pubblici devono rendere evidente il loro ruolo nel protocollo:

```text
CreateAlbumRequest
UpdatePhotoRequest
AlbumDto
PhotoDto
CacheClearRequest
```

### 16.2 Modelli applicativi

I modelli interni devono rendere evidente il loro utilizzo nel layer applicativo:

```text
CacheClearOperationRequest
CacheClearOperationResult
MediaFile
MediaProfile
BulkUpdateItem<T>
```

### 16.3 Entity

Le Entity rappresentano la persistenza e appartengono al progetto Data.

Non devono essere confuse con DTO o modelli applicativi.

### 16.4 Nomi generici

Devono essere evitati nomi che non comunicano una responsabilità chiara, per esempio:

```text
Helper
Utils2
CommonObject
DataManager
Misc
Temp
NewService
```

Le convenzioni generali di naming e implementazione sono definite nel `Documentation/Engineering/MpsPlaybook.md`.

---

## 17. Evoluzione di un dominio

La struttura di un dominio deve evolvere in modo incrementale.

Nuove cartelle, nuovi progetti o nuove astrazioni devono essere introdotti soltanto quando:

- esiste una responsabilità distinta;
- la navigazione è diventata realmente complessa;
- il riuso è concreto;
- la separazione riduce un accoppiamento;
- le dipendenze richiedono un confine più chiaro.

Non si devono introdurre astrazioni per anticipare esigenze future.

Un comportamento può essere promosso nello Shared Framework soltanto dopo aver dimostrato di essere stabile, generico e realmente utilizzabile da più domini.

> **Shared is Earned, not Planned.**

---

## 18. Checklist per un nuovo dominio

Prima di considerare definita l'architettura iniziale di un nuovo dominio, verificare che:

- il dominio rappresenti una responsabilità funzionale autonoma;
- i Contracts descrivano esclusivamente il protocollo pubblico;
- i Service non dipendano dal trasporto HTTP;
- i Repository non conoscano Controller o DTO;
- le Entity non siano esposte come contratti pubblici;
- il dominio possieda un unico punto di registrazione;
- l'host non conosca i dettagli interni del dominio;
- la struttura non contenga progetti o cartelle premature;
- i componenti condivisi siano stati promossi in `Shared` soltanto dopo un riuso reale;
- i test riflettano le responsabilità architetturali dei componenti.

---

## 19. Vedi anche

- `Architecture.md`
- `SharedFramework.md`
- `InfrastructureArchitecture.md`
- `SecurityArchitecture.md`
- `TestingArchitecture.md`
- `ArchitectureRoadmap.md`
- `Documentation/Engineering/MpsPlaybook.md`
- `Architecture Decision Records (ADR)`
