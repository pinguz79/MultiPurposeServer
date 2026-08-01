# Shared Framework di MultiPurposeServer

## 1. Scopo del documento

Questo documento descrive il framework condiviso di MultiPurposeServer.

Per framework condiviso si intende un insieme di contratti, componenti e comportamenti riutilizzabili tra domini differenti, privi di logica di business specifica e sufficientemente generali da rappresentare concetti comuni dell'intera piattaforma.

L'obiettivo non è costruire una libreria generica indipendente dal progetto, ma far emergere naturalmente le astrazioni condivise durante l'evoluzione dei domini.

---

# 2. Filosofia

Il framework condiviso nasce dai domini.

Non vengono introdotte astrazioni perché potrebbero essere utili in futuro.

Ogni componente condiviso deve soddisfare almeno i seguenti requisiti:

- essere utilizzato o chiaramente riutilizzabile da più domini;
- non contenere logica specifica di un dominio;
- ridurre concretamente duplicazioni;
- rappresentare un concetto stabile.

L'evoluzione preferita è:

```text
Dominio
        ↓
Secondo dominio
        ↓
Concetto condiviso
        ↓
Shared
```

e non:

```text
Shared
        ↓
Adattamento forzato dei domini
```

## Dipendenze da framework di terze parti

Il framework condiviso deve rimanere indipendente dai singoli domini applicativi, ma non necessariamente dai framework e dalle librerie di terze parti.

Un componente Shared può dipendere da una tecnologia esterna quando:

- rappresenta un comportamento realmente riutilizzabile tra più domini;
- non introduce dipendenze verso un dominio specifico;
- la tecnologia esterna è parte esplicita della responsabilità del componente;
- la dipendenza rimane circoscritta e visibile;
- il componente conserva un significato stabile all'interno della piattaforma.

Ad esempio, `MultiPurposeServer.Shared.Utils` può contenere utility cross-domain basate su Entity Framework Core, come la paginazione asincrona di `IQueryable<T>`.

La dipendenza:

```text
MultiPurposeServer.Shared.Utils
        ↓
Microsoft.EntityFrameworkCore
```

è quindi architetturalmente ammessa.

Non sarebbe invece ammessa una dipendenza come:

```text
MultiPurposeServer.Shared.Utils
        ↓
Portfolio.Data
```

perché renderebbe il framework condiviso dipendente da uno specifico dominio applicativo.

Il criterio determinante non è quindi l'indipendenza da qualsiasi framework esterno, ma l'indipendenza dai domini applicativi e la reale riutilizzabilità cross-domain del componente.

---

# 3. Shared.Contracts

I contratti condivisi rappresentano concetti architetturali comuni.

Non appartengono ad alcun dominio specifico.

Attualmente comprendono, tra gli altri:

- IRequest
- IBulk<TItem>
- BulkRequest<TItem>
- BulkOptions

---

# 4. IRequest

## Responsabilità

`IRequest` rappresenta un **contratto di richiesta** che partecipa alla pipeline condivisa di MultiPurposeServer.

Implementare `IRequest` non rappresenta una capacità del dominio né introduce logica di business. Significa invece che la Request aderisce alle convenzioni del framework condiviso e può essere elaborata automaticamente dall'infrastruttura comune.

Ogni Request che implementa `IRequest` partecipa allo stesso ciclo di vita:

```text
Request
    ↓
Normalize()
    ↓
Validate()
    ↓
Controller
    ↓
Application Service
```

Questo consente ai Controller e agli Application Service di ricevere richieste già normalizzate e validate, eliminando codice ripetitivo e garantendo un comportamento uniforme tra tutti i domini.

---

## Operazioni comuni

Per partecipare alla pipeline condivisa ogni Request espone due operazioni fondamentali:

```csharp
void Normalize();
void Validate();
```

Le implementazioni fornite dal framework permettono di evitare duplicazioni nei singoli DTO e garantiscono un comportamento coerente tra tutte le API.

Le due operazioni rappresentano responsabilità infrastrutturali comuni e non devono contenere logica di business.

---

## Normalizzazione

La normalizzazione porta una Request in uno stato canonico prima che venga elaborata dall'applicazione.

Può, ad esempio:

- eliminare spazi iniziali e finali;
- convertire stringhe vuote in `null`;
- normalizzare ricorsivamente gli oggetti figli;
- uniformare la rappresentazione dei dati.

La normalizzazione può modificare la rappresentazione dei dati, ma non il loro significato.

---

## Validazione

La validazione verifica che la Request rappresenti un contratto semanticamente valido.

Il suo scopo è garantire che i dati ricevuti rispettino i requisiti richiesti dall'applicazione prima di raggiungere la logica di business.

La validazione:

- non modifica mai il contenuto della Request;
- verifica esclusivamente la correttezza del contratto;
- genera una `ValidationException` in presenza di errori.

Le regole di validazione devono rimanere indipendenti dal dominio applicativo e descrivere esclusivamente la validità della Request come contratto.

---

# 5. Convenzioni dei Request Contract

Tutti i Request Contract che partecipano al framework condiviso seguono un insieme di convenzioni comuni.

Le Request descrivono il contratto. Il framework ne gestisce il ciclo di vita.

Lo scopo di tali convenzioni è garantire un modello di programmazione uniforme tra tutti i domini, ridurre il codice ripetitivo e permettere all'infrastruttura condivisa di operare automaticamente senza conoscere il dominio applicativo.

Le convenzioni descritte in questo capitolo costituiscono il modello di programmazione raccomandato per tutte le API che utilizzano il framework condiviso.

---

## I DTO rappresentano contratti

I DTO non rappresentano oggetti del dominio, ma contratti tra il client e l'applicazione.

Il loro compito è descrivere i dati necessari all'esecuzione di un caso d'uso.

Per questo motivo devono rimanere semplici contenitori di dati e non devono contenere logica di business.

In particolare:

- descrivono esclusivamente il contratto della Request;
- non contengono logica di persistenza;
- non contengono logica di business;
- espongono solamente i dati necessari all'operazione richiesta.

---

## Partecipazione alla Request Pipeline

Implementare `IRequest` significa aderire alle convenzioni della Request Pipeline condivisa.

Non rappresenta una capacità del dominio né identifica uno specifico tipo di operazione.

Il framework utilizza tale contratto per applicare automaticamente le operazioni comuni previste dal ciclo di vita della Request.

Di conseguenza Controller e Application Service possono assumere che ogni Request sia già stata normalizzata e validata.

---

## Configurazione dichiarativa

La normalizzazione e la validazione vengono configurate in modo dichiarativo attraverso attributi.

Le Request descrivono **che cosa** deve essere eseguito.

Il framework decide **come**, **quando** e **in quale ordine** tali operazioni vengono applicate.

Questo approccio permette di:

- mantenere i DTO privi di codice ripetitivo;
- centralizzare il comportamento del framework;
- estendere il framework senza modificare i singoli domini;
- rendere il comportamento facilmente verificabile tramite test.

---

## Normalizzazione e validazione sono responsabilità infrastrutturali

La normalizzazione e la validazione costituiscono responsabilità del framework condiviso.

Non devono contenere logica di business né dipendere dal dominio applicativo.

Le regole implementate dal framework devono descrivere esclusivamente la correttezza del contratto della Request.

Ogni verifica che richieda la conoscenza del dominio deve essere eseguita dall'Application Service.

---

## Le Request Bulk seguono le stesse convenzioni

Una Request Bulk non introduce un nuovo modello di programmazione.

Rappresenta semplicemente una collezione di Request indipendenti che partecipano allo stesso ciclo di vita.

Ogni elemento viene quindi:

- normalizzato;
- validato;
- elaborato indipendentemente dagli altri elementi.

L'infrastruttura Bulk è responsabile esclusivamente dell'orchestrazione dell'operazione collettiva.

La correttezza di ciascun elemento rimane responsabilità della singola Request.

---

# 6. Operazioni Bulk

Molte API elaborano collezioni di elementi anziché singole Request.

Per evitare duplicazioni e mantenere un modello di programmazione uniforme, il framework condiviso estende le convenzioni dei Request Contract alle operazioni Bulk.

Una Request Bulk non rappresenta un modello differente rispetto ad una Request singola.

Rappresenta semplicemente una collezione di Request indipendenti che partecipano allo stesso ciclo di vita definito dal framework condiviso.

---

## IBulk<TItem>

`IBulk<TItem>` rappresenta il contratto comune di tutte le operazioni Bulk.

Espone:

- `BulkOptions`;
- `IReadOnlyCollection<TItem>`.

Ogni elemento della collezione deve implementare `IRequest` e viene trattato dal framework come una normale Request.

---

## BulkRequest<TItem>

`BulkRequest<TItem>` rappresenta l'implementazione condivisa delle Request Bulk senza conoscere il dominio applicativo.

Il suo scopo è raccogliere il comportamento comune evitando duplicazioni tra i diversi domini applicativi.

L'infrastruttura Bulk è responsabile esclusivamente dell'orchestrazione dell'operazione collettiva.

La normalizzazione, la validazione e la correttezza del singolo elemento rimangono responsabilità della relativa Request.

---

## BulkOptions

`BulkOptions` raccoglie le opzioni comuni delle operazioni Bulk.

Attualmente comprende:

- `ErrorStrategy`.

In futuro potranno essere introdotte ulteriori opzioni realmente condivise tra più domini.

Le opzioni Bulk descrivono esclusivamente il comportamento dell'elaborazione collettiva e non devono influenzare la validità delle singole Request.

---

# 7. Request Pipeline

La Request Pipeline costituisce il meccanismo attraverso il quale il framework condiviso applica automaticamente il ciclo di vita definito dai Request Contract.

Il suo scopo è garantire che tutte le Request vengano elaborate secondo lo stesso flusso, indipendentemente dal dominio applicativo che le utilizza.

Ogni Request che implementa `IRequest` viene elaborata automaticamente secondo la seguente sequenza:

```text
Authentication
        ↓
Authorization
        ↓
Model Binding
        ↓
Normalize
        ↓
Validate
        ↓
Endpoint
        ↓
Application Service
```

L'ordine delle operazioni costituisce parte integrante del framework condiviso.

In particolare:

- la normalizzazione viene sempre eseguita prima della validazione;
- la validazione viene sempre completata prima dell'esecuzione del Controller;
- il Controller riceve sempre una Request già normalizzata e validata;
- gli Application Service possono assumere che il contratto della Request sia già stato verificato.

Questo approccio consente di:

- eliminare codice ripetitivo dai Controller;
- centralizzare il comportamento infrastrutturale;
- garantire un comportamento uniforme tra tutti i domini;
- mantenere separata la logica di business dalle responsabilità infrastrutturali.

La Request Pipeline rappresenta quindi il punto di incontro tra i Request Contract e i framework di normalizzazione e validazione descritti nei capitoli successivi.

---

# 8. Framework di Normalizzazione

## Responsabilità

Il Framework di Normalizzazione è responsabile della preparazione delle Request prima della loro validazione e della successiva elaborazione da parte dell'applicazione.

Il suo scopo è trasformare ogni Request in una rappresentazione canonica, eliminando differenze di formato che non modificano il significato dei dati.

La normalizzazione costituisce una responsabilità infrastrutturale comune a tutti i domini.

---

## Componenti

Attualmente il framework comprende:

- `NormalizeAttribute`
- `NormalizeChildrenAttribute`

Nuovi attributi possono essere introdotti qualora rappresentino comportamenti realmente condivisi tra più domini.

---

## Funzionamento

La normalizzazione viene eseguita automaticamente dalla Request Pipeline.

Ogni Request descrive dichiarativamente le operazioni da applicare attraverso gli attributi presenti sui propri membri.

Il framework interpreta tali informazioni ed esegue automaticamente le operazioni previste.

Le Request descrivono **che cosa** deve essere normalizzato.

Il framework decide **come** eseguire la normalizzazione.

---

## Ottimizzazioni

La reflection viene utilizzata esclusivamente durante la costruzione del piano di normalizzazione.

Le informazioni ottenute vengono compilate e memorizzate nella cache.

L'esecuzione della normalizzazione utilizza quindi esclusivamente informazioni già preparate, evitando ulteriori operazioni di reflection durante l'elaborazione delle Request.

---

# 9. Framework di Validazione

## Responsabilità

Il Framework di Validazione è responsabile della verifica della correttezza dei Request Contract.

Il suo scopo è garantire che ogni Request soddisfi i requisiti richiesti dal contratto prima di raggiungere la logica di business.

La validazione costituisce una responsabilità infrastrutturale comune a tutti i domini.

---

## Componenti

Attualmente il framework comprende:

- `RequiredAttribute`
- `RequiredAtLeastOneAttribute`
- `RequiredAtLeastOneTrueAttribute`
- `ValidateChildrenAttribute`

Nuovi attributi possono essere introdotti esclusivamente quando rappresentano regole realmente condivise tra più domini.

---

## Funzionamento

La validazione viene eseguita automaticamente dalla Request Pipeline dopo il completamento della normalizzazione.

Ogni Request descrive dichiarativamente le proprie regole di validazione attraverso gli attributi presenti sui propri membri.

Il framework interpreta tali informazioni ed esegue automaticamente le verifiche previste.

Le Request descrivono **che cosa** deve essere validato.

Il framework decide **come** eseguire la validazione.

In presenza di errori viene generata una `ValidationException` che interrompe il normale flusso della Request.

---

## Ottimizzazioni

Come per il Framework di Normalizzazione, la reflection viene utilizzata esclusivamente durante la costruzione del piano di validazione.

Le informazioni raccolte vengono compilate e memorizzate nella cache.

L'esecuzione della validazione utilizza quindi esclusivamente informazioni già preparate, riducendo il costo computazionale durante l'elaborazione delle Request.

---

# 10. Integrazione con ASP.NET Core

La pipeline MVC costituisce il punto di ingresso comune delle API.

Le principali responsabilità sono:

- normalizzare automaticamente le Request;
- validare automaticamente le Request;
- interrompere l'esecuzione del Controller in caso di errori;
- convertire automaticamente le ValidationException in risposte HTTP coerenti.

Questo permette di mantenere i Controller privi di codice ripetitivo.

---

# 11. Testing

Il framework condiviso viene testato su due livelli.

## Framework

I test del framework verificano il comportamento di:

- normalizzazione;
- validazione;
- pipeline;
- eccezioni;
- contratti condivisi.

---

## Contracts

I test dei Contracts non verificano il comportamento del framework.

Verificano esclusivamente che i DTO siano configurati correttamente.

Non verificano il comportamento del framework, che è già coperto dai test dedicati ai componenti condivisi.

Ad esempio:

- presenza degli attributi;
- coerenza Parent/Children;
- configurazione delle operazioni Bulk.

---

# 12. Evoluzione

Nuovi componenti condivisi devono emergere naturalmente durante lo sviluppo.

L'introduzione di un framework condiviso deve essere giustificata dalla presenza di un reale comportamento comune e non dall'anticipazione di esigenze future.

Quando un concetto risulta ancora fortemente legato a un dominio specifico, deve rimanere all'interno del dominio stesso.

Solo quando almeno due domini condividono realmente lo stesso comportamento si valuta il suo spostamento in Shared.

---

# 13. Principi

L'evoluzione del framework condiviso segue alcuni principi fondamentali.

- I domini guidano l'evoluzione di Shared.
- Le responsabilità devono essere chiaramente separate.
- La reflection deve essere limitata alla fase di inizializzazione.
- I Controller devono rimanere privi di codice ripetitivo.
- La validazione deve essere dichiarativa.
- La normalizzazione deve essere automatica.
- I test devono descrivere il comportamento e proteggere i refactoring.
- Le astrazioni premature devono essere evitate.
- Le dipendenze da framework di terze parti sono ammesse quando supportano comportamenti cross-domain e non introducono dipendenze verso domini specifici.
- Ogni nuovo componente deve poter essere riutilizzato da domini futuri senza conoscere quelli esistenti.

---

## See also

- Architecture.md
- ArchitectureRoadmap.md
- EngineeringPlaybook.md
- ADR
