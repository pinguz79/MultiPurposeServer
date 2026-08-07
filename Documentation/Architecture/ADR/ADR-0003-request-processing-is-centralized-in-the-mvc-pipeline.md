# ADR-0003 — L'elaborazione delle Request è centralizzata nella pipeline MVC

## Stato

Accettato

## Ambito

Shared Framework

## Data della decisione

2026-08-06

## Origine

`ADR-ALPHA-0005`.

## Contesto

Le Request devono essere normalizzate e validate prima della logica applicativa. L'invocazione manuale nei singoli Controller produceva duplicazione e consentiva omissioni, inversioni dell'ordine e comportamenti incoerenti tra endpoint.

Normalizzazione e validazione sono responsabilità infrastrutturali comuni e non appartengono all'orchestrazione del Controller.

## Decisione

La pipeline MVC individua le Request che implementano `IRequest` ed esegue automaticamente:

1. `Normalize()`;
2. `Validate()`;
3. invocazione del Controller, soltanto in assenza di errori.

La normalizzazione precede sempre la validazione. I Controller ricevono Request già elaborate e non invocano manualmente tali operazioni.

Gli errori di validazione vengono tradotti uniformemente in risposte HTTP dai componenti infrastrutturali dedicati.

## Conseguenze

### Positive

- L'ordine delle operazioni è garantito centralmente.
- I Controller non duplicano codice infrastrutturale.
- Una Request non valida non raggiunge la logica applicativa.
- Il comportamento può essere verificato indipendentemente dai Controller.

### Negative

- Una parte del comportamento dell'endpoint non è visibile nel solo Controller.
- I test che invocano direttamente il Controller non attraversano la pipeline.
- L'integrazione MVC richiede test e gestione uniforme degli errori.

## Alternative considerate

### Invocazione manuale nei Controller

Scartata per duplicazione e rischio di comportamento incoerente.

### Invocazione nei Service

Scartata perché accoppierebbe i Service ai Contracts e farebbe arrivare dati non elaborati oltre il confine HTTP.

## Riferimenti

- [Shared Framework](../SharedFramework.md)
- [ADR-0004](ADR-0004-irequest-uses-default-interface-implementations.md)
- [ADR-0005](ADR-0005-normalization-and-validation-are-declarative.md)
