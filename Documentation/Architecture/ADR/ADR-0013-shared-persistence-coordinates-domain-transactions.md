# ADR-0013 — Shared Persistence coordina le transazioni del dominio

## Stato

Accettato

## Ambito

Shared Framework e Architettura dei domini

## Data della decisione

2026-08-21

## Contesto

Portfolio ha introdotto un lifecycle applicativo composto da `Operation`, transazione di persistenza e checkpoint. Il Controller apre un'Operation soltanto quando il caso d'uso richiede atomicità fra più azioni; i Repository salvano immediatamente quando non esiste una transazione e differiscono il completamento quando partecipano a un'Operation.

La prima implementazione conservava però lo stato transazionale nel singolo Repository che aveva aperto la transazione. Il modello risultava sufficiente per operazioni concentrate su un solo Repository, ma non rappresentava esplicitamente un caso d'uso coordinato fra Repository differenti.

L'avvio di Finance costituisce un secondo consumatore reale. Le sue operazioni possono coinvolgere contemporaneamente Pianificazioni, Movimenti, Conti e correlazioni, mantenendo un unico confine atomico. Il comportamento non appartiene alla semantica di Portfolio o Finance ed è quindi una responsabilità tecnica trasversale ormai sufficientemente compresa per essere promossa in Shared.

## Decisione

Il lifecycle generico di Operation, transazione e checkpoint viene estratto nel progetto autonomo `MultiPurposeServer.Shared.Persistence`.

L'adapter riutilizzabile per Entity Framework Core viene isolato nel progetto `MultiPurposeServer.Shared.Persistence.EntityFramework`. In questo modo il lifecycle principale resta provider-independent, mentre i domini che adottano EF condividono anche l'implementazione tecnica senza duplicarla.

Shared Persistence definisce contratti e comportamento provider-independent per:

- completamento esplicito e rollback automatico delle Operation;
- commit e rollback delle transazioni di persistenza;
- checkpoint applicativi supportati da checkpoint di persistenza;
- rifiuto di utilizzi successivi al dispose;
- idempotenza del completamento.

`IPersistenceCoordinator` rappresenta il coordinatore scoped comune. `EntityFrameworkPersistenceCoordinator<TContext>` ne fornisce l'implementazione generica per i domini basati su EF e:

- possiede la transazione del `DbContext` del dominio;
- espone a tutti i Repository dello scope se una transazione è attiva;
- applica commit, rollback e checkpoint usando Entity Framework Core;
- impedisce l'apertura di transazioni o Operation annidate.

Ogni dominio registra il coordinatore generico usando il proprio tipo di `DbContext`. Tutti i Repository coinvolti nell'operazione condividono lo stesso `DbContext` e lo stesso coordinatore. Un Repository salva immediatamente quando nessuna transazione è attiva; durante un'Operation differisce il salvataggio al completamento coordinato.

Il Controller rimane responsabile dell'orchestrazione e decide se aprire l'Operation. I Service e i Repository partecipano al confine esistente senza aprire autonomamente Operation annidate.

`BaseRepository<TEntity>` non viene promosso in Shared. Continua a contenere scelte specifiche del dominio e potrà essere separato in una base comune soltanto quando implementazioni concrete ulteriori ne dimostreranno il confine stabile.

## Conseguenze

### Positive

- Portfolio e Finance condividono lo stesso lifecycle senza dipendere l'uno dall'altro.
- Una singola transazione coordina Repository differenti dello stesso dominio.
- Lo stato transazionale non è duplicato nei singoli Repository.
- Commit, rollback e checkpoint possono essere testati indipendentemente da Entity Framework Core.
- I domini basati su Entity Framework riutilizzano lo stesso adapter generico variando soltanto il tipo di `DbContext`.
- L'estrazione applica il criterio del secondo consumatore reale definito per Shared.

### Negative

- Ogni dominio deve registrare il coordinatore scoped appropriato al provider adottato.
- L'adapter Entity Framework introduce un secondo progetto Shared con dipendenza esplicita da EF Core.
- Tutti i partecipanti devono ricevere la stessa istanza di `DbContext` e coordinatore.
- Un errore nella configurazione della dependency injection può compromettere il confine transazionale.
- Le operazioni che coinvolgono risorse non transazionali richiedono comunque compensazione o riconciliazione.

## Alternative considerate

### Duplicare il meccanismo in Finance

Scartato perché Finance costituisce il secondo consumatore reale e il lifecycle non contiene semantica di dominio.

### Lasciare la transazione nel Repository che la apre

Scartato perché gli altri Repository dello stesso caso d'uso non condividerebbero esplicitamente stato e regole di completamento.

### Estrarre anche un BaseRepository generico

Rinviato. L'attuale implementazione contiene assunzioni specifiche di Portfolio e non esiste ancora evidenza sufficiente per stabilire una base comune corretta.

### Consentire Operation annidate

Scartato perché renderebbe ambiguo quale livello possiede commit e rollback. Una sola Operation attiva definisce un confine atomico esplicito.

## Riferimenti

- [Shared Framework](../SharedFramework.md)
- [Domain Architecture](../DomainArchitecture.md)
- [ADR-0002 — Shared nasce da responsabilità tecniche concrete](ADR-0002-shared-emerges-from-concrete-technical-responsibilities.md)
- [ADR-0009 — I Controller orchestrano le operazioni applicative](ADR-0009-controllers-orchestrate-application-operations.md)
