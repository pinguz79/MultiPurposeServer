# ADR-0009 — I Controller orchestrano le operazioni applicative

## Stato

Accettato

## Ambito

Architettura dei domini

## Data della decisione

2026-08-06

## Origine

`ADR-ALPHA-0010`, esteso durante il consolidamento di `DomainArchitecture.md`.

## Contesto

Molte architetture affidano a un Service l'intero caso d'uso. MPS espone invece Service focalizzati che devono poter essere combinati in operazioni singole, bulk, importazioni e altri flussi.

Il componente che interpreta il contratto HTTP deve poter scegliere quali capacità invocare, in quale ordine e quali effetti appartengono allo stesso esito complessivo.

L'appunto Alpha descriveva soprattutto il confine transazionale database. Un'operazione può però coinvolgere più risorse, come database, filesystem, pagamenti o servizi esterni.

## Decisione

Il Controller orchestra il caso d'uso esposto dall'API.

Può invocare uno o più Service, comporre i risultati e governare l'atomicità applicativa dell'operazione. I Service espongono capacità applicative focalizzate, applicano regole di business e coordinano le dipendenze necessarie alla singola capacità senza conoscere il contesto HTTP.

Il Controller non implementa business logic, non accede direttamente a Repository o `DbContext` e non modifica direttamente le Entity.

L'atomicità applicativa non coincide necessariamente con una transazione database. Quando le risorse coinvolte non possono condividere una transazione tecnica, l'orchestrazione utilizza strategie esplicite di compensazione, idempotenza, stato intermedio e riconciliazione.

Un coordinatore applicativo dedicato può essere estratto quando l'orchestrazione diventa complessa o deve essere riutilizzata fuori da HTTP; non costituisce un layer obbligatorio.

## Conseguenze

### Positive

- Le stesse capacità dei Service possono essere composte in flussi differenti.
- Il confine e la sequenza dell'operazione rimangono espliciti.
- Le operazioni bulk non richiedono Service duplicati per ogni scenario.
- L'atomicità può comprendere attori diversi dal database.

### Negative

- I Controller possono contenere più codice di orchestrazione.
- È necessaria disciplina per non introdurvi regole di business.
- Operazioni distribuite richiedono compensazione e gestione degli esiti incerti.
- Orchestrazioni complesse possono richiedere un componente dedicato.

## Alternative considerate

### Service orchestratore per ogni caso d'uso

Scartato come regola generale perché produrrebbe proliferazione e duplicazione tra operazioni singole, bulk e altri flussi.

### Controller limitato a una sola chiamata

Scartato perché nasconderebbe altrove una composizione che appartiene al contratto dell'operazione API.

### Atomicità limitata alla transazione database

Scartata perché non rappresenta casi d'uso che coinvolgono risorse eterogenee.

## Riferimenti

- [Domain Architecture](../DomainArchitecture.md)
- [ADR-0006](ADR-0006-bulk-requests-share-common-technical-contracts.md)
- [ADR-0007](ADR-0007-services-do-not-depend-on-contracts.md)
