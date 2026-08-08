# Architettura delle Web Application

## 1. Scopo

Questo documento descrive responsabilità e confini architetturali delle Web Application di MultiPurposeServer.

La Web Application attualmente esistente è Portfolio.Web, sviluppata in PHP secondo il pattern MVC. Per le pagine non banali adotta una Page Architecture che separa ingresso HTTP, orchestrazione, stato della pagina e rendering.

La Page Architecture costituisce una scelta concreta per Portfolio.Web e può essere riutilizzata da future applicazioni MVC. Applicazioni realizzate con tecnologie o paradigmi differenti, per esempio frontend Angular o React, possono tradurre gli stessi confini nei costrutti idiomatici del framework scelto senza dover riprodurre artificialmente Controller e View server-side.

Il documento non prescrive framework, struttura dei file o convenzioni di codice. Ogni livello viene introdotto soltanto quando rappresenta una responsabilità reale.

---

## 2. Ruolo delle Web Application

Le Web Application sono client dei domini di MultiPurposeServer.

Consumano le API e i relativi Contracts pubblici e non accedono direttamente a Entity, DbContext, Repository, modelli interni dei Service o dettagli di persistenza.

Una Web Application può:

- ricevere richieste HTTP dal browser;
- gestire sessione, routing e navigazione;
- invocare le API del proprio dominio;
- comporre lo stato necessario alle pagine;
- utilizzare cache e persistenza locale con responsabilità esplicite;
- renderizzare l'interfaccia;
- coordinare il comportamento client-side.

La Web Application non è la fonte autorevole dei dati di dominio e non sostituisce i controlli di sicurezza del backend.

---

## 3. Page Architecture MVC

Portfolio.Web adotta il seguente flusso per le pagine che richiedono orchestrazione:

```text
Browser
    ↓
Controller
    ↓
Page Service
    ├── API Client
    ├── Routing Service
    ├── Cache
    ├── Local Persistence
    └── altri Application Service
    ↓
Esito applicativo + Page Model
    ↓
Controller
    ↓
View e Components
```

La Page Architecture non deve essere applicata meccanicamente. La struttura rimane proporzionata alla complessità effettiva del caso d'uso.

### 3.1 Pagina semplice

Una pagina può adottare un flusso diretto quando recupera un solo dato, non coordina più fonti, non gestisce cache o fallback e non richiede trasformazioni significative:

```text
Controller → API Client → DTO o Page Model → View
```

Il DTO può essere utilizzato direttamente soltanto quando rappresenta esattamente lo stato richiesto dalla View.

Anche nel flusso semplice, il Controller non costruisce manualmente richieste HTTP remote e non assorbe logica applicativa.

### 3.2 Pagina con orchestrazione

Il Page Service viene introdotto quando la pagina richiede composizione, trasformazione, routing applicativo, cache, fallback, coordinamento di più API o comportamento riutilizzato.

```text
Controller → Page Service → API Client e servizi specialistici
                         → Esito applicativo e Page Model
                         → View
```

Quando una pagina semplice acquisisce queste responsabilità, il flusso deve evolvere verso la Page Architecture completa.

---

## 4. Controller

Il Controller è l'adapter HTTP della Web Application.

È responsabile di:

- leggere route, query string e input della richiesta;
- verificare la correttezza formale dei dati necessari all'azione;
- invocare direttamente un API Client nei casi banali oppure un Page Service;
- tradurre gli esiti applicativi in risposte HTTP;
- selezionare la View;
- eseguire redirect quando richiesto dal flusso.

Il Controller non deve:

- contenere logica di dominio;
- implementare orchestrazioni complesse;
- costruire direttamente richieste HTTP remote;
- implementare cache, routing o persistenza;
- conoscere i dettagli interni dei componenti di rendering;
- duplicare comportamento appartenente al Page Service.

La semplicità del Controller deve derivare dalla semplicità reale del caso d'uso e non dalla compressione di responsabilità eterogenee nello stesso metodo.

---

## 5. Page Service

Il Page Service orchestra il caso d'uso necessario a costruire una pagina.

Può coordinare API Client, routing, cache, persistenza locale, servizi esterni, trasformazioni, fallback e composizione di più risultati.

Il Page Service:

- esprime il caso d'uso della pagina;
- governa l'ordine delle collaborazioni;
- costruisce il Page Model;
- restituisce un esito applicativo esplicito;
- rimane indipendente da HTML e dettagli di rendering;
- non restituisce direttamente una risposta HTTP.

Routing, cache e persistenza locale sono implementati da servizi specialistici. Il Page Service ne orchestra l'uso, ma non ne incorpora le responsabilità tecniche.

Un Page Service non deve diventare un contenitore generico di tutta la logica della Web Application.

---

## 6. Esiti applicativi

L'esito dell'orchestrazione è distinto dal Page Model e dal protocollo HTTP.

Esempi semantici:

```text
Success(PageModel)
NotFound
Forbidden
TemporaryFailure
Partial(PageModel, informazioni di fallback)
```

Il Page Service produce l'esito applicativo. Il Controller decide se renderizzare una View, restituire un errore HTTP o eseguire un redirect.

La View riceve uno stato già coerente e non interpreta errori di trasporto o status restituiti direttamente dalle API.

La forma tecnica degli esiti appartiene alla progettazione implementativa di Portfolio.Web e non è prescritta da questo documento.

---

## 7. Page Model

Il Page Model rappresenta lo stato completo necessario al rendering di una pagina. È un modello della Web Application e non coincide necessariamente con un DTO HTTP.

Un Page Model:

- dipende soltanto dai bisogni della pagina;
- rimane indipendente dal markup;
- non espone Entity o modelli interni del server;
- contiene dati derivati, navigazione e stato di presentazione quando necessari;
- riduce la logica richiesta alla View;
- è costruibile e verificabile separatamente.

Un Page Model dedicato è necessario quando la pagina compone più DTO, dati derivati, informazioni di navigazione, paginazione, cache o stato specifico della presentazione.

I componenti ricevono il modello più piccolo sufficiente alla propria responsabilità; non ricevono automaticamente l'intero Page Model.

---

## 8. View e Components

La View renderizza il Page Model e compone i Components.

Può applicare trasformazioni semplici e puramente presentazionali, come formattare un valore, scegliere una classe CSS o mostrare un fallback già previsto dal modello.

La View non deve:

- invocare API o servizi applicativi;
- gestire cache o persistenza;
- ricostruire logica di orchestrazione;
- effettuare controlli di autorizzazione sostitutivi del backend;
- interpretare dettagli tecnici degli errori remoti.

Un Component rappresenta una porzione riutilizzabile della presentazione, possiede una responsabilità precisa e riceve soltanto lo stato necessario.

Un componente contenitore può coordinare componenti di rendering più piccoli, ma non può diventare un Page Service nascosto nella View.

---

## 9. Accesso alle API

Ogni integrazione HTTP è incapsulata in un API Client o in un adapter dedicato.

Un API Client è responsabile di:

- costruire la richiesta HTTP;
- applicare l'identità del client e, quando necessario, quella dell'utente;
- serializzare e deserializzare i Contracts;
- gestire timeout e cancellazione;
- tradurre errori di trasporto e status remoti in risultati comprensibili alla Web Application;
- conservare le informazioni necessarie alla diagnosi.

Non compone pagine e non implementa logica di presentazione.

Portfolio.Web consuma esclusivamente i Contracts pubblici di Portfolio.Api. Un'eventuale API di un altro dominio viene trattata come servizio esterno e configurata separatamente, anche quando condivide lo stesso host fisico.

---

## 10. Routing applicativo

Il routing pubblico può differire dagli identificativi interni delle API. La risoluzione tra path, risorsa e contesto di navigazione appartiene a un Routing Service dedicato.

```text
Public Path
    ↓
Routing Service
    ↓
Resource Identifier + Navigation Context
    ↓
Page Service o API Client
```

Il routing non viene ricostruito indipendentemente in più Controller o View.

### 10.1 Path canonici e navigation path

Occorre distinguere:

- path tecnicamente equivalenti, alias o vecchi slug, che possono essere rediretti a un path canonico;
- navigation path di dominio intenzionalmente distinti, che rimangono validi e preservano il contesto di arrivo.

Nel dominio Portfolio, uno stesso Album fisico può essere raggiunto attraverso più percorsi virtuali. Il Routing Service restituisce quindi anche il contesto necessario a costruire breadcrumb e navigazione. Questi percorsi non vengono automaticamente rediretti al path fisico canonico.

Il Routing Service segnala l'eventuale canonicalizzazione; il Controller esegue il redirect.

---

## 11. Cache e persistenza locale

Le Web Application possono utilizzare meccanismi distinti per:

- cache delle risposte API;
- cache di routing;
- persistenza locale applicativa;
- cache dei file multimediali.

Ogni meccanismo definisce fonte autorevole, chiavi, popolamento, scadenza, invalidazione e comportamento in caso di dato assente o non valido.

Il Page Service coordina l'uso delle cache attraverso servizi dedicati. L'invalidazione non viene distribuita fra Controller, View e componenti JavaScript.

### 11.1 Confini di accesso

Una cache non può riutilizzare indiscriminatamente contenuti protetti fra utenti o client differenti.

Chiavi, durata e invalidazione includono il contesto di accesso quando il dato non è pubblico. Un dato assente, scaduto o non valido provoca il recupero dalla fonte autorevole e non abilita un fallback meno protetto.

La cache non concede autorizzazioni e non diventa fonte primaria del dato di dominio.

---

## 12. Componenti client-side

I componenti JavaScript mantengono confini, stato e comportamento locali. Il loro stato deve essere il più piccolo possibile e non deve diventare un'implementazione concorrente della logica autorevole del backend.

Quando possibile, i componenti preservano semantica HTML, accessibilità, navigazione da tastiera e comportamento progressivo.

Le astrazioni condivise vengono introdotte soltanto quando emerge un comportamento realmente comune. Somiglianze superficiali non giustificano una gerarchia preventiva.

Inizializzazione, selettori, struttura dei file e librerie adottate appartengono alle convenzioni implementative di ciascuna Application.

---

## 13. Stili e design token

L'architettura degli stili distingue layout, componenti, stati interattivi, comportamento responsive e design token.

Un design token globale rappresenta un concetto effettivamente condiviso. Valori utilizzati da un solo componente rimangono locali finché non emerge un riuso concreto.

Naming, organizzazione dei fogli di stile, posizione dei token e framework CSS appartengono alla documentazione implementativa della singola Application.

---

## 14. Sicurezza

Portfolio.Web gestisce la propria sessione e presenta soltanto le funzioni disponibili, ma Portfolio.Api rimane autorevole per autenticazione del client, autenticazione dell'utente, autorizzazione e accesso ai dati.

Nascondere una funzione nell'interfaccia non costituisce una misura di sicurezza sufficiente.

Le cache rispettano il contesto di accesso e i segreti server-side non vengono esposti al browser. Le regole generali sono definite in [Security Architecture](SecurityArchitecture.md).

---

## 15. Testing

I confini della Web Application devono rendere verificabili separatamente:

- Controller;
- Page Service;
- API Client;
- Routing Service;
- cache e persistenza locale;
- trasformazioni;
- componenti client-side con comportamento significativo.

Il test di un livello verifica le sue responsabilità e la collaborazione con il livello successivo, senza ripetere i test interni del collaboratore.

Le View puramente dichiarative non richiedono necessariamente test dedicati. La logica significativa viene estratta in componenti verificabili invece di essere coperta con test fragili legati al markup.

Strategia generale, livelli e convenzioni di scrittura sono definiti in [Testing Architecture](TestingArchitecture.md) e nel [MPS Playbook](../Engineering/MpsPlaybook.md).

---

## 16. Evoluzione

Portfolio.Web adotta Page Architecture e MVC come scelte correnti.

Nuove Web Application MVC possono riutilizzare lo stesso modello. Applicazioni basate su altri paradigmi devono preservare almeno la separazione fra:

- trasporto e accesso alle API;
- orchestrazione applicativa;
- stato della pagina o della UI;
- rendering e interazione;
- sicurezza autorevole del backend.

Non sono obbligate a introdurre componenti denominati Controller, Page Service, Page Model o View quando la tecnologia scelta assegna tali responsabilità in modo differente.

Nuovi livelli e astrazioni vengono introdotti soltanto quando riducono un accoppiamento concreto, rendono esplicita una responsabilità o migliorano l'evoluzione e la verificabilità del sistema.

---

## 17. Checklist architetturale

Prima di considerare definita una nuova pagina MVC, verificare che:

- il Controller si limiti all'adattamento HTTP e all'orchestrazione banale;
- il Page Service sia introdotto quando esiste composizione applicativa reale;
- il Page Model rappresenti lo stato completo richiesto dalla View;
- gli esiti applicativi siano distinti dalle risposte HTTP;
- i Components ricevano il modello più piccolo sufficiente;
- la View non contenga logica applicativa;
- l'accesso alle API sia incapsulato;
- routing, cache e persistenza abbiano responsabilità distinte;
- i navigation path validi conservino il proprio contesto;
- le cache rispettino i confini di autorizzazione;
- JavaScript e CSS non introducano stato o astrazioni globali non giustificate;
- la sicurezza non sia delegata al frontend;
- i componenti con logica significativa siano verificabili separatamente.

---

## 18. Riferimenti

- [Architecture](Architecture.md)
- [Domain Architecture](DomainArchitecture.md)
- [Security Architecture](SecurityArchitecture.md)
- [Testing Architecture](TestingArchitecture.md)
- [Shared Framework](SharedFramework.md)
- [Portfolio Domain](../Portfolio/Domain.md)
- [MPS Playbook](../Engineering/MpsPlaybook.md)
- [Architecture Decision Records](ADR/README.md)
- [ADR-0012 — Portfolio.Web adotta la Page Architecture](ADR/ADR-0012-portfolio-web-adopts-page-architecture.md)
