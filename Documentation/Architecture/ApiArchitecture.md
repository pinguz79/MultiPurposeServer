# Architettura delle API

> **Stato: Stable 1.0 — autorevole.**

## 1. Scopo

Questo documento approfondisce protocollo pubblico, routing, Contracts ed evoluzione delle API dei domini di MultiPurposeServer.

Non descrive la business logic dei domini né le tecnologie interne dei client.

---

## 2. Routing e organizzazione dei Controller

Le route dei domini seguono la forma generale:

```text
<ServerBaseUrl>/<Domain>/<Surface>/<ControllerHierarchy>/<ActionOrResourceId>
```

`Domain` rende esplicito il proprietario funzionale dell'endpoint. `Surface` distingue normalmente `FrontEnd` e
`BackEnd`: la prima espone le operazioni destinate ai client ordinari del dominio, la seconda le operazioni
amministrative o di configurazione estesa. La gerarchia dei Controller e l'eventuale Action descrivono la risorsa
pubblica o il caso d'uso senza fare affidamento sulla co-ubicazione fisica di altri domini.

Il Controller è organizzato per risorsa pubblica restituita o manipolata, non per pagina del client e non
necessariamente per Entity persistita. Una pagina compone normalmente il proprio stato invocando più Controller.
Un endpoint aggregatore specifico per pagina viene introdotto soltanto quando emerge un'esigenza concreta di
composizione o di riduzione delle chiamate e possiede un contratto dedicato; non costituisce il contenitore
predefinito delle API FrontEnd.

Controller specialistici rimangono autonomi quando rappresentano una risorsa o una responsabilità diversa:

- `Routing` risolve un percorso e non restituisce una generica collezione di Album;
- `Media` restituisce rappresentazioni binarie e può possedere policy di accesso e caching differenti da `Foto`;
- `Bulk` espone contratti, strategie di atomicità e response per item distinti dagli endpoint puntuali;
- `Diagnostics` espone stato e comandi diagnostici, non Entity del dominio.

### 2.1 Convenzioni CRUD

Le operazioni CRUD puntuali adottano queste convenzioni:

- `GET <Controller>/List` restituisce esplicitamente una collezione;
- `GET <Controller>/{id}` restituisce un singolo elemento;
- `POST <Controller>` crea un elemento nella collection;
- `PATCH <Controller>/{id}` applica un aggiornamento parziale;
- `DELETE <Controller>/{id}` elimina l'elemento quando il dominio prevede la cancellazione.

`List` non viene sostituito dal plurale della risorsa. `GET /Conto/List` e `GET /Album/List` rendono uniforme la
forma della collection anche quando i nomi di dominio appartengono a lingue differenti. La creazione non aggiunge
Action come `Create` o `CreateNew`: il significato deriva da `POST` sulla collection.

Il verbo HTTP conserva una semantica coerente con l'operazione. Un aggiornamento che applica soltanto i campi
valorizzati della Request usa `PATCH`, non `PUT`; `PUT` rimane disponibile per la sostituzione completa o
l'impostazione integrale di una risorsa.

Le route annidate esprimono una navigazione o una relazione dal punto di vista della risorsa padre, per esempio:

```text
GET /Portfolio/FrontEnd/Album/{albumId}/Foto
```

Un endpoint apparentemente equivalente può rimanere autonomo quando esprime un caso d'uso differente. La ricerca
BackEnd di una lista di Foto per criteri del relativo Controller resta quindi:

```text
GET /Portfolio/BackEnd/Foto/List?albumId={albumId}
```

### 2.2 Controller Bulk e operazioni specialistiche

Gli endpoint Bulk rimangono fisicamente e semanticamente separati dagli endpoint puntuali:

```text
PATCH /Portfolio/BackEnd/Bulk/Album/Update
PATCH /Portfolio/BackEnd/Bulk/Foto/Update
```

Le Action Bulk possono restare esplicite perché identificano un caso d'uso con Request contenitore, options di
atomicità, strategia di valutazione e risultato per item. Anche le query specialistiche Bulk restano nella stessa
gerarchia quando appartengono a quella superficie operativa.

I comandi che non corrispondono naturalmente a un CRUD non vengono forzati in una forma artificiale. Usano un
verbo HTTP coerente e un'Action esplicita, per esempio:

```text
POST /Portfolio/BackEnd/Cache/Invalidate
```

### 2.3 Route di riferimento

Le seguenti route costituiscono esempi autorevoli della convenzione:

```text
GET    /Portfolio/FrontEnd/Album/List
GET    /Portfolio/FrontEnd/Album/{albumId}/Foto
GET    /Portfolio/FrontEnd/Routing/Album?path={path}
GET    /Portfolio/FrontEnd/Media/Cover/{photoId}
GET    /Portfolio/FrontEnd/Media/EditorialCover/{photoId}
GET    /Portfolio/FrontEnd/Media/Thumbnail/{photoId}
GET    /Portfolio/FrontEnd/Media/Image/{photoId}

GET    /Portfolio/BackEnd/Album/List?id={parentId}
GET    /Portfolio/BackEnd/Album/{albumId}
POST   /Portfolio/BackEnd/Album
PATCH  /Portfolio/BackEnd/Album/{albumId}
DELETE /Portfolio/BackEnd/Album/{albumId}

GET    /Portfolio/BackEnd/Foto/List?albumId={albumId}
GET    /Portfolio/BackEnd/Foto/{photoId}
PATCH  /Portfolio/BackEnd/Foto/{photoId}

GET    /Portfolio/BackEnd/Bulk/Album/MissingDescriptions
GET    /Portfolio/BackEnd/Bulk/Album/Match
PATCH  /Portfolio/BackEnd/Bulk/Album/Update
GET    /Portfolio/BackEnd/Bulk/Foto/MissingDescriptions
PATCH  /Portfolio/BackEnd/Bulk/Foto/Update

POST   /Portfolio/BackEnd/Cache/Invalidate
GET    /Portfolio/BackEnd/Diagnostics/Logging
PUT    /Portfolio/BackEnd/Diagnostics/Logging
DELETE /Portfolio/BackEnd/Diagnostics/Logging

GET    /Finance/FrontEnd/Conto/List
GET    /Finance/FrontEnd/Conto/{contoId}
POST   /Finance/BackEnd/Conto
GET    /Finance/BackEnd/Conto/{contoId}
PATCH  /Finance/BackEnd/Conto/{contoId}
```

I metodi dei Controller mantengono nomi semanticamente espliciti e coerenti con la forma della risposta e con
l'operazione esposta. La convenzione puntuale sui nomi dei metodi viene consolidata insieme alle altre regole di
implementazione dei Controller e non viene dedotta automaticamente dal solo segmento di route.

Gli endpoint che restituiscono dati espongono `Task<IActionResult>` e costruiscono esplicitamente la risposta
HTTP (`Ok`, `CreatedAtAction`, `NotFound` e analoghi). Anche una collezione viene materializzata, trasformata nei
relativi Response DTO e restituita con `Ok`; il tipo CLR della collezione non sostituisce il contratto HTTP.

---

## 3. Contracts pubblici

I Contracts rappresentano il protocollo pubblico dell'API e non il modello interno del dominio.

Comprendono Request e Response DTO serializzabili. Non espongono Entity, DbContext, Repository o modelli interni non previsti dal protocollo.

La specifica OpenAPI costituisce la descrizione autorevole del wire contract. `Domain.Contracts` contiene l'implementazione server-side; i client possono implementare gli stessi modelli con tecnologie e forme differenti.

### 3.1 Implementazioni server e client

Lato server, i DTO usano primary constructor. Le Request adottano normalmente record deserializzabili; i
Response DTO che traducono un modello interno sono classi con proprieta pubbliche `get; set;`, inizializzate dal
primary constructor che riceve quel modello. Non vengono introdotti costruttori alternativi come convenzione
concorrente.

Lato client, i modelli possono adottare costrutti più adatti al linguaggio, al framework e alla serializzazione utilizzati. La condivisione di un assembly non è richiesta: deve essere condiviso il significato del wire contract.

---

## 4. Request DTO

Il Controller riceve Request già elaborate dalla Request Pipeline Shared e ne traduce i dati nei parametri richiesti dai Service.

Le Request dichiarano dati e regole tecniche applicabili, ma non implementano algoritmi condivisi di normalizzazione, validazione o persistenza.

### 4.1 Update parziale

Un'operazione `Update` usa normalmente un DTO contenente tutti i campi modificabili come proprietà nullable. Almeno una proprietà deve essere valorizzata.

Per l'Update:

- proprietà valorizzata significa sostituire il valore corrente;
- proprietà `null` significa non modificare il valore corrente;
- `null` non viene usato per richiedere l'azzeramento di un valore.

Quando `null` possiede un significato valido nel modello persistito, il ritorno a tale stato avviene tramite un'operazione `Reset` esplicita o un contratto equivalente non ambiguo.

I campi persistiti sono normalmente non null. Un valore mancante può rappresentare uno stato tecnicamente valido ma incompleto quando il dominio lo consente esplicitamente.

---

## 5. Response DTO

Il Response DTO traduce Data Model o Business Model nella rappresentazione pubblica.

Può omettere campi del modello interno e non deve serializzare direttamente una Entity come contratto implicito. Mapping e forma pubblica appartengono al Contract server-side.

Il mapping è implementato nel Response DTO, per esempio `new ContoDto(conto)`. Il Controller non introduce
funzioni `Map`, `MapDto` o equivalenti per copiare i campi del modello nel contratto.

Gli errori pubblici distinguono almeno:

- errori globali della Request;
- errori di validazione del singolo item;
- violazioni di persistenza;
- dipendenze o risorse mancanti;
- item non processati quando la strategia lo consente.

La tassonomia tecnica comune può essere fornita dallo Shared Framework; codici e significato applicativo appartengono al dominio.

---

## 6. Controller e protocollo

Il Controller:

- interpreta route, query string e body;
- orchestra Service e altri collaboratori;
- costruisce Response DTO;
- governa l'atomicità applicativa dell'operazione;
- traduce esiti applicativi in risposte HTTP.

Le dipendenze ricevute dal Controller hanno nomi semantici (`albumService`, `contoService`), non nomi generici
come `service`. I metodi HTTP mantengono il verbo CRUD quando rappresentano l'azione esposta (`Get`, `Create`,
`Update`); Service e Repository descrivono invece esplicitamente la capacità di dominio (`CreateAlbum`,
`CreateConto`, `GetById`, `GetAlbums`, `GetConti`).

Una singola operazione ordinaria è atomica. Le operazioni bulk applicano invece la strategia dichiarata nel contratto e sono approfondite in [Bulk Operations](BulkOperations.md).

---

## 7. Evoluzione del contratto

Una breaking change inizia quando viene modificato il DTO o il comportamento pubblico lato server ed è considerata conclusa soltanto quando tutti i client interessati sono stati aggiornati.

Server e client vengono normalmente rilasciati insieme. Deroghe, compatibilità parallela e versionamento multiplo vengono introdotti caso per caso soltanto quando emerge una necessità reale.

La firma del codice non è l'unica fonte di compatibilità: serializzazione, semantica dei campi, status HTTP e regole di validazione fanno parte del contratto osservabile.

---

## 8. Riferimenti

- [Architecture](Architecture.md)
- [Domain Architecture](DomainArchitecture.md)
- [Shared Framework](SharedFramework.md)
- [Request Processing](RequestProcessing.md)
- [Bulk Operations](BulkOperations.md)
- [Security Architecture](SecurityArchitecture.md)
- [ADR-0008 — I Response DTO mappano i modelli interni](ADR/ADR-0008-response-dtos-map-internal-models.md)
- [ADR-0009 — I Controller orchestrano le operazioni applicative](ADR/ADR-0009-controllers-orchestrate-application-operations.md)
