# Architettura delle Web Applications

## 1. Scopo del documento

Questo documento descrive l'architettura delle applicazioni Web collocate nella cartella `Applications` di MultiPurposeServer.

Definisce come separare orchestrazione HTTP, logica applicativa, accesso alle API, stato della pagina, rendering, componenti JavaScript e CSS.

L'obiettivo è mantenere le applicazioni Web semplici da comprendere, testare ed evolvere, evitando che Controller, View o componenti di presentazione assumano responsabilità che appartengono ad altri livelli.

Il documento non prescrive l'uso meccanico di tutti i componenti descritti. Ogni livello deve essere introdotto soltanto quando rappresenta una responsabilità reale.

---

## 2. Ruolo delle Web Applications

Le Web Applications sono client di MultiPurposeServer.

Consumano i Contracts pubblici esposti dalle API e non devono accedere direttamente a:

- Entity;
- DbContext;
- Repository;
- modelli interni dei Service;
- dettagli di persistenza dei domini.

Una Web Application è responsabile di:

- ricevere richieste HTTP dal browser;
- invocare le API di MultiPurposeServer;
- comporre lo stato necessario alle pagine;
- gestire routing e cache applicative;
- renderizzare l'interfaccia;
- coordinare il comportamento client-side.

La sicurezza non deve essere delegata esclusivamente alla Web Application.

L'interfaccia può nascondere funzionalità non disponibili, ma il backend deve sempre verificare autenticazione, autorizzazione e permessi.

---

## 3. Flusso architetturale

Per le pagine con logica non banale si adotta il seguente flusso:

```text
Browser
    ↓
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

Il Page Service può utilizzare:

```text
Page Service
    ↓
API Client
    ↓
MultiPurposeServer
```

e coordinare componenti applicativi come:

```text
Routing Cache
Response Cache
Local Persistence
External Services
```

Il flusso completo può quindi essere rappresentato così:

```text
Browser
    ↓
Controller
    ↓
Page Service
    ├── API Client
    ├── Routing Cache
    ├── Response Cache
    ├── Local Persistence
    └── Application Services
    ↓
Page Model
    ↓
View
    ↓
Components
```

Il pattern non deve essere applicato meccanicamente.

Una pagina semplice, che recupera dati da un solo Service senza una reale orchestrazione, può essere gestita direttamente dal Controller.

Il Page Service deve essere introdotto quando esiste una responsabilità applicativa concreta.

---

## 4. Controller

Il Controller rappresenta il punto di ingresso HTTP della Web Application.

Le sue responsabilità comprendono:

- leggere route, query string e parametri della richiesta;
- verificare la correttezza formale dei dati necessari all'azione;
- invocare il Page Service quando richiesto;
- tradurre gli esiti applicativi in risposte HTTP;
- selezionare la View da renderizzare;
- eseguire redirect quando il flusso applicativo lo richiede.

Il Controller non deve:

- contenere logica di business;
- comporre direttamente strutture di pagina complesse;
- gestire direttamente cache;
- accedere direttamente alle API remote quando è necessaria orchestrazione;
- contenere logica di persistenza;
- conoscere dettagli del rendering dei componenti;
- duplicare comportamento già appartenente a un Page Service.

### 4.1 Pagine semplici

Il Controller può gestire direttamente una pagina quando:

- utilizza un solo Service;
- non coordina più fonti dati;
- non gestisce cache o routing applicativo;
- non compone uno stato complesso;
- non contiene logica destinata al riuso.

La semplicità del flusso deve essere reale, non ottenuta comprimendo nel Controller responsabilità che appartengono ad altri componenti.

### 4.2 Traduzione degli esiti

Il Controller traduce gli esiti applicativi nel protocollo HTTP.

Esempi:

- risorsa disponibile → `200`;
- risorsa non trovata → `404`;
- richiesta non valida → `400`;
- accesso non autorizzato → `401` o `403`;
- redirect verso un path canonico → risposta di redirect;
- errore non previsto → gestione infrastrutturale appropriata.

Il Page Service non deve restituire direttamente `IActionResult`.

---

## 5. Page Service

Il Page Service costruisce lo stato completo necessario alla pagina.

Rappresenta il livello di orchestrazione applicativa della Web Application.

Può coordinare:

- API Client;
- cache delle risposte;
- cache di routing;
- persistenza locale;
- servizi esterni;
- trasformazioni e normalizzazioni dei dati;
- composizione di più risultati;
- fallback applicativi;
- risoluzione di identificativi e path.

Il Page Service non deve conoscere HTML, markup o dettagli del rendering.

### 5.1 Quando introdurlo

Un Page Service è appropriato quando la pagina richiede:

- dati provenienti da più API o Service;
- composizione di più modelli;
- gestione coordinata di cache;
- risoluzione di routing applicativo;
- fallback o strategie di recupero;
- trasformazioni non banali;
- comportamento riutilizzato da più azioni;
- logica che renderebbe il Controller difficile da leggere o testare.

### 5.2 Responsabilità

Un Page Service deve:

- esprimere chiaramente il caso d'uso della pagina;
- restituire un risultato applicativo esplicito;
- mantenere separate orchestrazione e presentazione;
- rendere testabile la composizione dello stato;
- evitare dipendenze dal protocollo HTTP quando non necessarie.

### 5.3 Confini

Un Page Service non deve:

- restituire HTML;
- selezionare View;
- accedere direttamente al DOM;
- contenere codice JavaScript;
- conoscere dettagli CSS;
- restituire `IActionResult`;
- assumere responsabilità di sicurezza appartenenti al backend.

---

## 6. Page Model

Il Page Model rappresenta lo stato completo necessario al rendering di una pagina.

È un modello applicativo della Web Application e non un DTO HTTP.

Deve contenere esclusivamente le informazioni necessarie alla View e ai componenti che la compongono.

### 6.1 Caratteristiche

Un Page Model deve:

- essere indipendente dal markup HTML;
- essere indipendente dal trasporto HTTP;
- evitare dipendenze da Entity e modelli interni del server;
- rappresentare in modo esplicito lo stato della pagina;
- ridurre la logica necessaria nella View;
- essere facilmente costruibile e verificabile nei test.

### 6.2 DTO e Page Model

Un DTO ricevuto dalle API può essere utilizzato direttamente soltanto quando rappresenta già esattamente lo stato richiesto dalla pagina.

Quando la pagina necessita di:

- più DTO;
- dati derivati;
- informazioni di navigazione;
- stato di selezione;
- paginazione;
- dati provenienti da cache;
- informazioni specifiche della presentazione;

deve essere introdotto un Page Model dedicato.

### 6.3 Stato completo e modelli parziali

Il Page Model rappresenta lo stato completo della pagina.

I singoli componenti devono invece ricevere il modello più piccolo sufficiente alla propria responsabilità.

---

## 7. View

La View riceve un Page Model e si occupa esclusivamente della presentazione.

Può:

- costruire il layout della pagina;
- comporre i componenti;
- applicare semplici trasformazioni legate alla presentazione;
- produrre markup accessibile;
- esporre al client-side i dati strettamente necessari.

La View non deve:

- invocare direttamente API remote;
- gestire cache;
- ricostruire logica applicativa;
- eseguire query di persistenza;
- duplicare trasformazioni già appartenenti al Page Service;
- assumere responsabilità di autorizzazione del backend.

### 7.1 Logica di presentazione

Sono ammesse nella View operazioni semplici come:

- scegliere una classe CSS in base allo stato;
- mostrare o nascondere un elemento;
- formattare un valore;
- selezionare un testo di fallback;
- comporre componenti.

Quando la logica richiede ramificazioni complesse, accesso a più fonti dati o conoscenza del caso d'uso, deve essere spostata nel Page Service o in un componente applicativo dedicato.

---

## 8. Components

I componenti rappresentano porzioni riutilizzabili della View.

Ogni componente deve avere una responsabilità precisa e ricevere il modello più piccolo sufficiente al proprio lavoro.

Esempi:

- `AlbumCard` riceve un singolo `Album`;
- `AlbumGrid` riceve una collezione di `Album`;
- `PhotoBrowser` può ricevere l'intero `AlbumPage` quando necessita dello stato completo della pagina.

La dimensione del modello deve essere guidata dalle responsabilità del componente.

Devono essere evitati entrambi gli estremi:

- passare l'intero Page Model a componenti che utilizzano un solo valore;
- frammentare artificialmente il modello in molti oggetti privi di una responsabilità reale.

### 8.1 Componenti presentazionali

Un componente puramente presentazionale:

- riceve dati già pronti;
- non invoca Service;
- non gestisce persistenza;
- non conosce routing applicativo;
- non ricostruisce logica di business;
- produce markup relativo alla propria responsabilità.

### 8.2 Composizione

I componenti possono essere composti tra loro quando la responsabilità rimane chiara.

Un componente contenitore può coordinare componenti più piccoli, ma non deve diventare un secondo Page Service nascosto nella View.

---

## 9. Accesso alle API

L'accesso a MultiPurposeServer deve essere incapsulato in API Client o Service dedicati.

Controller, View e componenti non devono costruire direttamente richieste HTTP remote quando esiste un livello applicativo appropriato.

Gli API Client sono responsabili di:

- costruire la richiesta HTTP;
- applicare autenticazione del client;
- serializzare e deserializzare i Contracts;
- gestire timeout e cancellazione;
- tradurre gli esiti del trasporto in risultati comprensibili all'applicazione;
- preservare i dettagli necessari alla diagnosi degli errori.

Gli API Client non devono contenere logica di composizione della pagina.

### 9.1 Contracts pubblici

Le Applications consumano esclusivamente Contracts pubblici.

Non devono dipendere da:

- Entity;
- DbContext;
- Repository;
- `Services.Models`;
- classi interne dei domini;
- dettagli di implementazione delle API.

### 9.2 Errori remoti

Gli errori restituiti dalle API devono essere tradotti in esiti applicativi comprensibili.

Il Page Service decide come tali esiti influenzano lo stato della pagina.

Il Controller decide come tradurli nella risposta HTTP della Web Application.

---

## 10. Routing applicativo

Il routing pubblico della Web Application può differire dagli identificativi interni utilizzati dalle API.

Quando il client utilizza path leggibili o stabili, la risoluzione tra path e identificativi deve appartenere a un componente applicativo dedicato.

Esempio:

```text
Public Path
    ↓
Routing Service / Routing Cache
    ↓
Resource Identifier
    ↓
API Client
```

Il routing non deve essere ricostruito indipendentemente in più Controller o View.

### 10.1 Path canonici

Quando più path identificano la stessa risorsa, l'applicazione dovrebbe individuare un path canonico.

La decisione di eseguire un redirect appartiene al Controller, mentre la risoluzione del path appartiene al Page Service o al servizio di routing.

### 10.2 Cache di routing

La cache di routing deve rimanere distinta dalle cache delle risposte API.

Queste cache hanno responsabilità differenti:

- la cache di routing associa path e identificativi;
- la cache delle risposte conserva dati recuperati dalle API;
- la cache dei file multimediali appartiene al sistema media.

---

## 11. Cache e persistenza locale

Le Web Applications possono utilizzare cache o persistenza locale per ridurre le chiamate remote e mantenere informazioni applicative.

Ogni meccanismo deve avere una responsabilità esplicita.

Devono essere distinte almeno:

- cache delle risposte API;
- cache di routing;
- persistenza locale applicativa;
- cache dei file multimediali, quando presente.

### 11.1 Regole generali

Una cache deve definire chiaramente:

- chi la popola;
- chi la legge;
- quando scade;
- come viene invalidata;
- quale comportamento adottare in caso di dato assente o non valido;
- quale sia la fonte autorevole.

La cache non deve diventare la fonte primaria del dato se questo appartiene a MultiPurposeServer.

### 11.2 Invalidazione

Le operazioni di invalidazione devono essere esplicite e testabili.

Non devono essere sparse tra Controller, View e componenti JavaScript.

---

## 12. Componenti JavaScript

I componenti JavaScript devono mantenere responsabilità ben definite.

Ogni componente viene inizializzato a partire da un elemento root del DOM e incapsula il proprio stato e comportamento.

Le query sul DOM devono essere limitate, quando possibile, al sottoalbero del componente.

### 12.1 Inizializzazione

L'elemento root rappresenta il confine del componente.

Il componente deve:

- cercare i propri elementi all'interno del root;
- registrare i propri event handler;
- mantenere locale il proprio stato;
- evitare dipendenze da selettori globali non necessari;
- poter essere inizializzato più volte su istanze differenti della stessa struttura.

### 12.2 Stato

Lo stato client-side deve essere il più piccolo possibile.

Quando lo stato rappresenta dati autorevoli del dominio, deve essere recuperato o confermato dal backend.

Il JavaScript non deve diventare una seconda implementazione indipendente della logica applicativa.

### 12.3 Classi base e astrazioni

Le classi base comuni devono essere introdotte soltanto quando esiste un comportamento realmente condiviso.

Una struttura simile non è sufficiente a giustificare un'astrazione.

> **Shared is Earned, not Planned.**

### 12.4 Accessibilità e comportamento progressivo

I componenti JavaScript devono preservare, quando possibile:

- semantica HTML;
- navigazione da tastiera;
- focus visibile;
- messaggi comprensibili;
- comportamento progressivo in assenza di JavaScript.

---

## 13. CSS e design token

Le Web Applications devono distinguere chiaramente:

- layout della pagina;
- componenti;
- design token globali;
- design token specifici del componente.

### 13.1 Design token globali

I valori condivisi tra più pagine o componenti devono essere rappresentati tramite CSS Custom Properties definite a livello applicativo, normalmente in `:root`.

Esempi:

```css
:root {
    --page-max-width: 1200px;
    --spacing-medium: 1rem;
    --border-radius-card: 0.5rem;
}
```

Un token globale deve rappresentare un concetto realmente condiviso.

### 13.2 Token specifici

I token utilizzati da un solo componente devono rimanere locali al componente.

Non devono essere promossi a livello globale soltanto per comodità.

### 13.3 Separazione delle responsabilità

Il CSS deve mantenere distinti:

- regole di layout;
- aspetto dei componenti;
- stato interattivo;
- adattamento responsive;
- valori condivisi.

La struttura delle classi deve favorire il riuso senza creare dipendenze implicite tra componenti non correlati.

### 13.4 Responsive design

Il comportamento responsive deve essere progettato a livello di layout e componente.

Non deve dipendere da correzioni occasionali sparse nel foglio di stile.

---

## 14. Sicurezza

La Web Application partecipa alla sicurezza del sistema, ma non ne rappresenta la fonte autorevole.

Può:

- autenticare il client applicativo verso MultiPurposeServer;
- gestire la sessione dell'utente quando prevista;
- nascondere funzioni non disponibili;
- guidare l'utente verso flussi consentiti.

Non può sostituire i controlli del backend.

MultiPurposeServer deve verificare sempre:

- identità del client;
- identità dell'utente, quando introdotta;
- autorizzazione;
- permessi;
- validità della Request.

I segreti utilizzati dalla Web Application non devono essere esposti al browser quando appartengono al server-side.

---

## 15. Error handling

Gli errori devono essere gestiti dal livello che ne possiede la responsabilità.

Esempi:

- l'API Client traduce gli errori di trasporto;
- il Page Service interpreta gli esiti applicativi;
- il Controller seleziona la risposta HTTP;
- la View presenta un messaggio già determinato;
- il JavaScript gestisce errori relativi all'interazione client-side.

I dettagli tecnici non devono essere mostrati indiscriminatamente all'utente.

Gli errori di sistema devono essere registrati attraverso l'infrastruttura di logging appropriata.

### 15.1 Stati di errore della pagina

Il Page Model può rappresentare esplicitamente stati come:

- contenuto disponibile;
- contenuto non trovato;
- accesso negato;
- errore temporaneo;
- dati parziali;
- fallback disponibile.

La View deve limitarsi a renderizzare lo stato ricevuto.

---

## 16. Testing

L'architettura della Web Application deve rendere testabili separatamente:

- Controller;
- Page Service;
- API Client;
- servizi di routing;
- cache;
- trasformazioni;
- componenti JavaScript con comportamento significativo.

### 16.1 Controller Test

I test dei Controller verificano:

- traduzione degli input HTTP;
- invocazione del Page Service;
- selezione della View;
- redirect;
- codici di risposta;
- mapping degli esiti applicativi.

Non devono duplicare la logica già verificata nei test del Page Service.

### 16.2 Page Service Test

I test dei Page Service verificano:

- orchestrazione;
- composizione dei dati;
- uso delle cache;
- fallback;
- trasformazione degli esiti;
- costruzione del Page Model.

### 16.3 API Client Test

Gli API Client devono essere verificati rispetto a:

- serializzazione;
- deserializzazione;
- autenticazione del client;
- traduzione degli status code;
- timeout e cancellazione;
- errori di trasporto.

### 16.4 View e Components

Le View puramente dichiarative non richiedono necessariamente test dedicati.

La logica significativa deve essere estratta in componenti testabili anziché verificata attraverso test fragili legati al markup.

I dettagli generali della strategia e dell'organizzazione della suite sono descritti in `TestingArchitecture.md` e nel `Documentation/Engineering/MpsPlaybook.md`.

---

## 17. Evoluzione dell'architettura Web

L'architettura deve evolvere in modo incrementale.

Nuovi componenti, Page Service, cache o astrazioni devono essere introdotti soltanto quando:

- esiste una responsabilità reale;
- riducono un accoppiamento concreto;
- migliorano la testabilità;
- eliminano duplicazioni ormai comprese;
- semplificano l'evoluzione della pagina o dell'applicazione.

Non si deve applicare l'intero pattern a ogni pagina per uniformità formale.

La struttura deve rimanere proporzionata alla complessità effettiva del caso d'uso.

---

## 18. Checklist per una nuova pagina

Prima di considerare definita l'architettura di una nuova pagina, verificare che:

- il Controller contenga soltanto orchestrazione HTTP;
- il Page Service sia stato introdotto solo se esiste una reale composizione applicativa;
- il Page Model rappresenti lo stato completo necessario alla View;
- i componenti ricevano il modello più piccolo sufficiente;
- la View non contenga logica applicativa;
- l'accesso alle API sia incapsulato;
- routing e cache abbiano responsabilità distinte;
- il JavaScript sia limitato al sottoalbero del componente;
- i design token globali rappresentino concetti realmente condivisi;
- la sicurezza non sia delegata esclusivamente al frontend;
- i componenti con logica significativa siano testabili;
- non siano state introdotte astrazioni premature.

---

## 19. Vedi anche

- `Architecture.md`
- `DomainArchitecture.md`
- `InfrastructureArchitecture.md`
- `SecurityArchitecture.md`
- `TestingArchitecture.md`
- `SharedFramework.md`
- `Documentation/Engineering/MpsPlaybook.md`
- `Architecture Decision Records (ADR)`
