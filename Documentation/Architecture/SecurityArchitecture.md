# Architettura della Sicurezza

## 1. Scopo del documento

Questo documento descrive l'architettura della sicurezza di MultiPurposeServer.

La sicurezza rappresenta una responsabilità trasversale dell'intero sistema e coinvolge domini, infrastruttura, host e applicazioni client.

L'obiettivo di questo documento è definire i principi architetturali che governano autenticazione, autorizzazione, protezione delle API, gestione dei segreti e responsabilità dei diversi componenti.

Le convenzioni di implementazione appartengono al `Documentation/Engineering/MpsPlaybook.md`.

Le decisioni architetturali specifiche vengono invece formalizzate tramite Architecture Decision Record (ADR).

---

## 2. Principi della sicurezza

La sicurezza non è un componente dell'architettura.

È una proprietà dell'intero sistema.

Ogni componente contribuisce alla sicurezza esclusivamente per la responsabilità che gli compete.

Di conseguenza:

- i domini applicano le regole di business;
- l'infrastruttura protegge i servizi comuni;
- l'host configura i meccanismi di sicurezza;
- le Applications guidano l'utente;
- il backend rimane sempre la fonte autorevole delle decisioni.

La sicurezza non deve mai dipendere esclusivamente dal comportamento del client.

### 2.1 Defense in Depth

MultiPurposeServer adotta una strategia di difesa multilivello.

Ogni livello riduce il rischio residuo senza assumere che il livello precedente sia sufficiente.

Ad esempio:

- HTTPS protegge il trasporto;
- Authentication identifica il chiamante;
- Authorization verifica i permessi;
- Validation protegge il dominio;
- Logging permette la diagnosi;
- Error Handling evita la divulgazione di dettagli interni.

Nessun singolo meccanismo è considerato sufficiente da solo.

### 2.2 Least Privilege

Ogni componente deve operare con il minimo insieme di privilegi necessario.

Questo principio si applica a:

- utenti;
- client;
- servizi;
- provider esterni;
- accesso ai file;
- database;
- configurazione.

L'assegnazione di privilegi più ampi del necessario aumenta inutilmente la superficie di attacco.

### 2.3 Trust Boundaries

L'architettura identifica chiaramente i confini di fiducia.

Ad esempio:

```text
Internet
    ↓
Web Application
    ↓
MultiPurposeServer
    ↓
Domains
    ↓
Infrastructure
    ↓
Database / Storage
```

Ogni attraversamento di un trust boundary richiede la validazione dei dati ricevuti.

I dati provenienti da un livello esterno non devono essere considerati attendibili fino alla loro verifica.

---

## 3. Threat Model

L'obiettivo della sicurezza non consiste nell'impedire genericamente gli attacchi, ma nel proteggere il sistema dalle minacce rilevanti per la sua architettura.

Il modello di sicurezza di MultiPurposeServer è progettato principalmente per mitigare:

- utilizzo di client non autorizzati;
- accesso di utenti non autorizzati;
- escalation di privilegi;
- divulgazione accidentale di informazioni;
- accesso non autorizzato ai file;
- manipolazione dei percorsi;
- utilizzo improprio delle API;
- esposizione di segreti applicativi;
- errori che rivelano dettagli interni dell'infrastruttura.

Questo elenco non rappresenta una classificazione completa delle minacce, ma descrive le principali categorie considerate durante la progettazione del sistema.

### 3.1 Sicurezza by Design

Le contromisure devono essere integrate nell'architettura.

Non devono essere aggiunte soltanto in fase di test o come correzione successiva.

Ogni nuova funzionalità dovrebbe essere progettata considerando fin dall'inizio:

- autenticazione;
- autorizzazione;
- validazione;
- gestione degli errori;
- logging;
- protezione dei dati.

---

## 4. Client Authentication

MultiPurposeServer distingue chiaramente l'identità del client dall'identità dell'utente.

La Client Authentication verifica quale applicazione sta utilizzando il server.

Ad esempio:

- Portfolio.Web;
- Portfolio.Mobile;
- future Desktop Applications;
- servizi esterni autorizzati.

Il client rappresenta l'applicazione.

Non rappresenta l'utente.

### 4.1 Responsabilità

La Client Authentication permette di:

- identificare il chiamante;
- limitare l'accesso alle API;
- applicare policy differenti;
- revocare singoli client;
- registrare l'attività del client.

Il possesso di credenziali valide identifica esclusivamente il software chiamante.

Non implica alcun diritto relativo all'utente finale.

### 4.2 Indipendenza dall'utente

Il modello architetturale separa completamente:

```text
Client Authentication

↓

"Chi sta chiamando il server?"
```

da

```text
User Authentication

↓

"Chi sta utilizzando l'applicazione?"
```

Le due identità possono esistere indipendentemente.

Un client può autenticarsi senza che esista un utente.

In futuro un utente potrà autenticarsi esclusivamente attraverso un client autorizzato.

### 4.3 Credenziali del client

Le credenziali del client devono:

- appartenere esclusivamente all'applicazione;
- essere archiviate in modo sicuro;
- poter essere revocate;
- poter essere ruotate periodicamente;
- non essere esposte agli utenti.

L'implementazione concreta del meccanismo di autenticazione potrà evolvere senza modificare il modello architetturale.

---

## 5. User Authentication

La User Authentication identifica la persona che utilizza l'applicazione.

MultiPurposeServer contiene attualmente un'implementazione sperimentale dell'autenticazione utente per `SampleApp`, comprendente l'integrazione con Google e l'emissione di token locali.

Tale implementazione non costituisce ancora un sottosistema completo di User Authentication: i token emessi non sono ancora integrati in uno schema di autenticazione utente applicato agli endpoint protetti.

L'architettura mantiene questa identità distinta dalla Client Authentication utilizzata dai domini e permette di completarne l'introduzione senza ridefinire il significato delle API key.

### 5.1 Responsabilità

L'autenticazione dell'utente consente di:

- identificare la persona;
- associare permessi;
- personalizzare il comportamento dell'applicazione;
- registrare le operazioni effettuate.

L'identità dell'utente rimane distinta da quella del client.

### 5.2 Evoluzione

Il sistema deve poter supportare differenti meccanismi di autenticazione, ad esempio:

- autenticazione locale;
- Identity Provider esterni;
- OAuth;
- OpenID Connect;
- Single Sign-On.

L'introduzione di nuovi provider non deve richiedere modifiche alla logica di business dei domini.

### 5.3 Separazione delle responsabilità

L'autenticazione determina esclusivamente l'identità.

Non stabilisce cosa l'utente possa fare.

Questa responsabilità appartiene all'autorizzazione.

---

## 6. Authorization

L'autorizzazione determina quali operazioni siano consentite a un'identità autenticata.

È una responsabilità esclusiva del backend.

Le Applications possono migliorare l'esperienza dell'utente nascondendo funzionalità non disponibili, ma tali controlli non devono mai sostituire le verifiche eseguite dal server.

### 6.1 Backend come fonte autorevole

Ogni richiesta deve essere verificata dal backend.

Il server decide sempre se un'operazione sia consentita.

Il client non deve poter aggirare tale verifica modificando il comportamento dell'interfaccia.

### 6.2 Livelli di autorizzazione

L'autorizzazione può essere applicata a differenti livelli.

Ad esempio:

- endpoint;
- dominio;
- singola risorsa;
- operazione;
- componente amministrativo.

Ogni livello verifica esclusivamente la responsabilità di propria competenza.

### 6.3 Separazione dalla logica di business

L'autorizzazione stabilisce se un'operazione possa essere eseguita.

La logica di business stabilisce come l'operazione debba essere eseguita.

Queste responsabilità devono rimanere distinte.

Ad esempio:

- l'autorizzazione verifica che un utente possa modificare un album;
- il dominio Portfolio stabilisce come l'album debba essere aggiornato.

L'esito di una verifica di autorizzazione non sostituisce le regole applicative del dominio.

---

## 7. Permission Model

L'autorizzazione viene espressa attraverso un modello basato sui permessi.

L'architettura non impone uno specifico modello di implementazione, ma distingue chiaramente i diversi livelli concettuali coinvolti.

Un modello tipico può essere rappresentato come:

```text
User
    ↓
Role
    ↓
Permission
    ↓
Resource
    ↓
Operation
```

Ogni livello rappresenta una responsabilità distinta.

### 7.1 Ruoli

I ruoli rappresentano un modo conveniente per raggruppare permessi.

Non devono contenere logica di business.

Il loro unico scopo consiste nel semplificare l'assegnazione dei permessi agli utenti.

Ad esempio:

- Administrator
- Moderator
- Editor
- Photographer
- Model

L'architettura non dipende dai nomi o dal numero dei ruoli.

### 7.2 Permessi

I permessi rappresentano operazioni autorizzabili.

Devono essere espressi utilizzando nomi che descrivano chiaramente l'azione consentita.

Ad esempio:

- Album.Read
- Album.Create
- Album.Update
- Album.Delete

I permessi rappresentano capacità.

Non identificano utenti né ruoli.

### 7.3 Risorse

Una risorsa rappresenta un elemento del dominio sul quale può essere eseguita un'operazione.

Esempi:

- Album
- Photo
- Gallery
- User
- Competition

L'autorizzazione può dipendere sia dal tipo della risorsa sia dalla sua istanza.

### 7.4 Autorizzazione granulare

L'architettura deve consentire controlli differenti a seconda del contesto.

Ad esempio:

- modificare qualsiasi album;
- modificare soltanto i propri album;
- visualizzare album pubblici;
- amministrare contenuti di altri utenti.

Le regole applicative rimangono responsabilità dei domini.

---

## 8. Sicurezza delle API

Le API rappresentano il principale punto di ingresso del sistema.

Devono quindi essere considerate il primo confine di sicurezza dell'architettura.

### 8.1 HTTPS

Le comunicazioni devono utilizzare protocolli sicuri.

Le API non devono essere esposte tramite connessioni non protette quando trattano dati sensibili o credenziali.

### 8.2 Validazione

Ogni Request deve essere validata.

La validazione comprende:

- formato;
- valori obbligatori;
- lunghezza;
- coerenza sintattica;
- regole dichiarative.

La validazione protegge il dominio da dati non corretti.

Non sostituisce le regole di business.

### 8.3 CORS

Le policy CORS appartengono all'infrastruttura.

Devono essere configurate esplicitamente.

Non devono essere utilizzate configurazioni permissive salvo durante attività di sviluppo controllate.

### 8.4 Rate Limiting

L'architettura deve poter limitare il numero di richieste provenienti da client o utenti.

Il Rate Limiting rappresenta una misura di protezione dell'infrastruttura.

Non costituisce un meccanismo di autorizzazione.

### 8.5 Swagger

Swagger deve documentare correttamente:

- schemi di autenticazione;
- endpoint protetti;
- codici 401;
- codici 403;
- permessi richiesti quando applicabili.

La documentazione delle API costituisce parte integrante della sicurezza.

### 8.6 Accesso anonimo ai contenuti multimediali

Gli endpoint del `MediaController` sono accessibili in modo anonimo.

Questa scelta è intenzionale e dipende dal meccanismo con cui le immagini vengono utilizzate dalle applicazioni web.

Le API di consultazione di album e fotografie richiedono la Client Authentication tramite policy `FrontEnd` o `BackEnd`.

I relativi Response DTO espongono gli URL delle varianti multimediali disponibili, ad esempio:

- thumbnail;
- cover;
- immagine completa.

Tali URL vengono utilizzati direttamente negli elementi HTML:

```html
<img src="...">
```

Il caricamento dell'immagine viene quindi eseguito direttamente dal browser e non attraverso il client REST autenticato di `Portfolio.Web`.

La richiesta generata dall'elemento HTML non include automaticamente la API key utilizzata per ottenere i DTO.

Il `MediaController` deve pertanto consentire l'accesso anonimo alle immagini referenziate dai Contracts.

Il flusso previsto è:

```text
Portfolio.Web
    ↓ REST + API key FrontEnd
API Album / Foto
    ↓
Response DTO con URL multimediali
    ↓
Elemento HTML <img>
    ↓ richiesta diretta del browser
MediaController anonimo
    ↓
Thumbnail / Cover / Full Image
```

Questa eccezione riguarda esclusivamente la distribuzione dei contenuti multimediali.

Non rende anonime:

- le API che espongono la struttura degli album;
- le API che espongono i metadati delle fotografie;
- le operazioni amministrative;
- le operazioni di modifica dei contenuti;
- gli endpoint protetti dalle policy `FrontEnd` o `BackEnd`.

Swagger deve rispettare questa configurazione e non deve associare requisiti di autenticazione agli endpoint decorati con `[AllowAnonymous]`, anche quando il Controller eredita una policy da una classe base.

---

## 9. Gestione dei segreti

Le credenziali costituiscono informazioni sensibili.

La loro gestione appartiene all'infrastruttura.

### 9.1 Segreti applicativi

Rientrano in questa categoria, ad esempio:

- API Key;
- Client Secret;
- Signing Key;
- Password;
- Token;
- stringhe di connessione sensibili.

Questi valori non devono essere archiviati nel repository.

### 9.2 Rotazione

L'architettura deve consentire la rotazione delle credenziali senza richiedere modifiche al codice applicativo.

La sostituzione di un segreto deve rappresentare un'operazione amministrativa.

### 9.3 Configurazione

I componenti applicativi devono ricevere i segreti esclusivamente tramite i meccanismi di configurazione previsti dall'infrastruttura.

Non devono leggerli direttamente da file o variabili statiche.

---

## 10. Sicurezza dell'infrastruttura

La sicurezza riguarda anche i servizi infrastrutturali.

### 10.1 Logging

I log non devono contenere:

- password;
- token;
- API Key;
- dati personali non necessari;
- informazioni che facilitino un attacco.

Devono invece contenere informazioni sufficienti alla diagnosi dei problemi.

### 10.2 File System

I percorsi costruiti utilizzando dati provenienti dall'esterno devono essere validati.

L'infrastruttura deve impedire:

- Path Traversal;
- accessi fuori dalla root prevista;
- manipolazioni del filesystem.

I domini devono utilizzare identificatori logici.

La costruzione dei percorsi appartiene al sistema Media.

### 10.3 Error Handling

Le eccezioni non devono esporre:

- stack trace;
- percorsi del filesystem;
- dettagli del database;
- informazioni di configurazione;
- implementazioni interne.

I dettagli diagnostici appartengono ai log.

Non alle risposte HTTP.

### 10.4 Configurazione

Le configurazioni devono essere validate durante l'avvio dell'applicazione.

Una configurazione non valida deve impedire l'avvio del sistema piuttosto che produrre comportamenti imprevedibili.

---

## 11. Evoluzione del modello di sicurezza

L'architettura della sicurezza deve poter evolvere senza modificare i domini.

Nuovi meccanismi di autenticazione, provider esterni o modelli di autorizzazione devono essere introdotti come componenti infrastrutturali.

I domini devono continuare a ragionare esclusivamente in termini di identità autorizzata e permessi disponibili.

La sicurezza deve evolvere come un servizio condiviso.

Non come una responsabilità distribuita tra i domini.

> **La sicurezza non è un componente dell'architettura. È una proprietà dell'intero sistema.**

---

## 12. Checklist

Prima di introdurre una nuova funzionalità verificare che:

- distingua chiaramente autenticazione e autorizzazione;
- non deleghi la sicurezza al frontend;
- utilizzi il principio del minimo privilegio;
- validi tutti gli input provenienti dall'esterno;
- non esponga dettagli interni tramite errori;
- non registri informazioni sensibili nei log;
- protegga correttamente file e risorse;
- utilizzi configurazioni sicure;
- mantenga separate logica di business e controlli di sicurezza;
- possa evolvere senza modificare i domini.

---

## 13. Vedi anche

- `Architecture.md`
- `DomainArchitecture.md`
- `InfrastructureArchitecture.md`
- `WebApplicationArchitecture.md`
- `TestingArchitecture.md`
- `SharedFramework.md`
- `Documentation/Engineering/MpsPlaybook.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`