# ADR-0008 — I Response DTO mappano i modelli interni

## Stato

Accettato

## Ambito

Architettura dei domini

## Data della decisione

2026-08-06

## Origine

`ADR-ALPHA-0009`, generalizzato durante il consolidamento di `DomainArchitecture.md`.

## Contesto

L'API deve trasformare il modello interno restituito dai Service nel Response DTO pubblico. Vietare qualsiasi dipendenza dai modelli interni richiederebbe mapper dedicati prevalentemente meccanici; eseguire il mapping nei Service li renderebbe dipendenti dai Contracts.

L'appunto originario riguardava Portfolio e soltanto le Entity del Data Model. L'architettura dei domini prevede ora anche Business Model opzionali.

## Decisione

Il Response DTO server-side è responsabile della traduzione del modello interno nella rappresentazione pubblica.

Può utilizzare nel proprio primary constructor una Entity del Data Model oppure un Business Model. La dipendenza ammessa è unidirezionale:

```text
Contracts → Business Model → Data Model
Contracts ────────────────→ Data Model
```

Business Model e Data Model non dipendono dai Contracts.

Questa decisione autorizza esclusivamente la lettura del modello ricevuto e la proiezione dei campi pubblici. Il DTO non accede a `DbContext`, Repository, query, persistenza o logica applicativa.

Le Entity non vengono serializzate direttamente come risposta pubblica.

## Conseguenze

### Positive

- Il mapping rimane vicino al contratto che lo espone.
- Non servono mapper dedicati puramente meccanici.
- I Service restano indipendenti dai Contracts.
- Il DTO seleziona esplicitamente i campi pubblici.

### Negative

- Il progetto Contracts server-side può dipendere dai progetti contenenti i modelli interni.
- Una separazione fisica del Business Model deve preservare un grafo delle dipendenze aciclico.
- Il mapping deve avvenire mentre i dati necessari sono ancora disponibili.

## Alternative considerate

### Mapper dedicati

Scartati come requisito generale perché introdurrebbero classi prive di valore applicativo.

### Mapping nei Service

Scartato perché renderebbe i Service dipendenti dal protocollo pubblico.

### Mapping nei modelli interni

Scartato perché introdurrebbe una dipendenza inversa verso Contracts.

## Riferimenti

- [Domain Architecture](../DomainArchitecture.md)
- [ADR-0007](ADR-0007-services-do-not-depend-on-contracts.md)

