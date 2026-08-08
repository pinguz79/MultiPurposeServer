# Architettura delle API

> **Stato: Stable 1.0 — autorevole.**

## 1. Scopo

Questo documento approfondisce protocollo pubblico, routing, Contracts ed evoluzione delle API dei domini di MultiPurposeServer.

Non descrive la business logic dei domini né le tecnologie interne dei client.

---

## 2. Routing

Le route dei domini seguono la forma generale:

```text
<ServerBaseUrl>/api/<Domain>/<ControllerHierarchy>/<Action>
```

Il segmento Domain rende esplicito il proprietario funzionale dell'endpoint. Controller hierarchy e Action descrivono la risorsa o il caso d'uso senza fare affidamento sulla co-ubicazione fisica di altri domini.

Le operazioni CRUD usano normalmente Action esplicite come `Get`, `Create`, `Update` e `Delete`. Il verbo HTTP conserva una semantica coerente con l'Action.

---

## 3. Contracts pubblici

I Contracts rappresentano il protocollo pubblico dell'API e non il modello interno del dominio.

Comprendono Request e Response DTO serializzabili. Non espongono Entity, DbContext, Repository o modelli interni non previsti dal protocollo.

La specifica OpenAPI costituisce la descrizione autorevole del wire contract. `Domain.Contracts` contiene l'implementazione server-side; i client possono implementare gli stessi modelli con tecnologie e forme differenti.

### 3.1 Implementazioni server e client

Lato server, i DTO usano primary constructor. Request e Response adottano forme compatibili rispettivamente con deserializzazione e mapping senza introdurre costruttori alternativi come convenzione concorrente.

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
