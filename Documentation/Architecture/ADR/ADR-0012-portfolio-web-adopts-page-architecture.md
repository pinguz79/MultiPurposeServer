# ADR-0012 — Portfolio.Web adotta la Page Architecture

## Stato

Accettato

## Ambito

Application: Portfolio.Web

## Data della decisione

2026-08-08

## Origine

`ADR-ALPHA-0003`, corretto e precisato durante il consolidamento di `WebApplicationArchitecture.md`.

## Contesto

Portfolio.Web è una Web Application PHP basata sul pattern MVC e non si limita a mostrare dati provenienti da Portfolio.Api.

Le pagine non banali possono richiedere composizione di più risultati, routing applicativo, cache, fallback e costruzione di uno stato specifico per la presentazione. Collocare queste responsabilità nei Controller o nelle View renderebbe confusi i confini, difficile la verifica separata e costosa l'evoluzione delle pagine.

Applicare invece gli stessi livelli a ogni pagina, comprese quelle meramente pass-through, introdurrebbe componenti cerimoniali privi di responsabilità reale.

## Decisione

Portfolio.Web adotta la Page Architecture per le pagine che possiedono una reale responsabilità di orchestrazione.

Il flusso completo separa:

- Controller, come adapter HTTP;
- Page Service, come orchestratore del caso d'uso della pagina;
- API Client e servizi specialistici, come adapter verso API, routing, cache e persistenza;
- Page Model, come stato completo necessario al rendering;
- View e Components, come livello di presentazione.

Il Page Service restituisce un esito applicativo distinto dal Page Model e dalla risposta HTTP. Il Controller traduce tale esito in View, redirect o risposta di errore.

Una pagina realmente semplice può usare il flusso diretto `Controller → API Client → DTO o Page Model → View`. Il Page Service e un Page Model dedicato vengono introdotti non appena emergono composizione, trasformazioni, cache, routing, fallback o altre responsabilità applicative.

La decisione riguarda Portfolio.Web e può essere riutilizzata da future Web Application MVC. Non impone Controller, Page Service, Page Model e View ad applicazioni fondate su paradigmi differenti, che devono comunque preservare la separazione fra trasporto, orchestrazione, stato della UI e rendering.

## Conseguenze

### Positive

- Controller e View conservano responsabilità circoscritte.
- L'orchestrazione delle pagine è esplicita e verificabile separatamente.
- API, routing, cache e persistenza rimangono incapsulati dietro servizi dedicati.
- Le pagine semplici non richiedono livelli privi di comportamento.
- Future tecnologie Web possono adottare strutture idiomatiche senza perdere i confini architetturali.

### Negative

- Le pagine articolate richiedono più componenti e mapping.
- La soglia fra flusso semplice e Page Service richiede giudizio progettuale.
- Senza disciplina, il Page Service può accumulare responsabilità appartenenti a servizi specialistici.
- Pagine simili possono temporaneamente avere strutture differenti durante la loro evoluzione.

## Alternative considerate

### Orchestrazione nei Controller

Scartata perché mescola adattamento HTTP, composizione applicativa e costruzione dello stato della pagina.

### Orchestrazione nelle View

Scartata perché rende la presentazione dipendente da servizi, trasporto e logica applicativa.

### Page Architecture completa obbligatoria per ogni pagina

Scartata perché introdurrebbe Page Service e Page Model cerimoniali anche nei flussi banali.

### Page Architecture obbligatoria per ogni tecnologia Web

Scartata perché framework non MVC possono assegnare le stesse responsabilità a costrutti differenti.

## Riferimenti

- [Web Application Architecture](../WebApplicationArchitecture.md)
- [Domain Architecture](../DomainArchitecture.md)
- [Testing Architecture](../TestingArchitecture.md)
- [Security Architecture](../SecurityArchitecture.md)

