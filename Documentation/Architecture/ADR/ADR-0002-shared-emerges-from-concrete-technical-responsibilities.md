# ADR-0002 — Shared nasce da responsabilità tecniche concrete

## Stato

Accettato

## Ambito

Shared Framework

## Data della decisione

2026-08-06

## Origine

Consolidamento di `Architecture.md` e `SharedFramework.md`.

## Contesto

Domini differenti possono utilizzare meccanismi tecnici simili. Anticipare ogni possibile somiglianza nello Shared Framework produrrebbe però astrazioni preventive, costringerebbe i domini ad adattarsi a modelli non ancora compresi e trasformerebbe Shared in un contenitore generico.

D'altra parte, alcune responsabilità strutturali della piattaforma devono poter nascere direttamente come capacità condivise anche prima dell'esistenza di più domini implementati.

## Decisione

Un componente entra nello Shared Framework quando risponde a un'esigenza concreta, ha natura esclusivamente tecnica, rappresenta una responsabilità stabile della piattaforma e non dipende dalla semantica di un dominio.

Una capacità strutturale della piattaforma può nascere direttamente in Shared. Quando invece una soluzione nasce da un dominio, normalmente rimane al suo interno finché un secondo utilizzatore reale non ne dimostra la generalità.

Il secondo utilizzatore è una protezione contro astrazioni premature, non una regola assoluta. La sola riusabilità ipotetica non è sufficiente.

I servizi Shared preservano confini logici ed estraibilità anche quando convivono nello stesso progetto e nella stessa DLL. Possono dipendere tra loro soltanto attraverso superfici pubbliche, con dipendenze esplicite, unidirezionali e acicliche.

## Conseguenze

### Positive

- Shared rimane tecnico e indipendente dal linguaggio dei domini.
- Le astrazioni sono guidate da responsabilità comprese.
- La duplicazione temporanea può fornire evidenza prima dell'estrazione.
- I servizi Shared possono essere separati fisicamente in futuro.

### Negative

- Alcuni meccanismi simili possono rimanere duplicati per un periodo.
- L'estrazione può richiedere un refactoring successivo.
- Il confine tra responsabilità strutturale e soluzione specifica richiede giudizio architetturale.

## Alternative considerate

### Richiedere sempre due domini consumatori

Scartato perché impedirebbe la creazione diretta di capacità strutturali della piattaforma.

### Promuovere componenti chiaramente riutilizzabili

Scartato come criterio sufficiente perché la riusabilità soltanto prevista favorisce astrazioni premature.

### Imporre interfaccia più implementazione a ogni servizio

Non adottato. Il pattern potrà diventare una convenzione solo se emergerà stabilmente dalle implementazioni reali.

## Riferimenti

- [Architecture](../Architecture.md)
- [Shared Framework](../SharedFramework.md)

