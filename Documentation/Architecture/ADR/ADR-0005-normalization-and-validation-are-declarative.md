# ADR-0005 — Normalizzazione e validazione dei Contracts sono dichiarative

## Stato

Accettato

## Ambito

Shared Framework

## Data della decisione

2026-08-06

## Origine

`ADR-ALPHA-0008`.

## Contesto

Le Request devono dichiarare vincoli di presenza, relazioni tra proprietà, normalizzazione dei valori e trattamento ricorsivo degli oggetti figli.

Implementare tali verifiche nei Controller o nei DTO concreti produrrebbe duplicazione, comportamenti incoerenti e accoppiamento tra Contracts e algoritmi infrastrutturali.

Era inoltre necessario separare la trasformazione in forma canonica dalla verifica di validità.

## Decisione

Le Request dichiarano normalizzazione e validazione canoniche tramite attributi applicati ai propri membri.

Gli attributi descrivono cosa applicare. I motori Shared definiscono come eseguire le regole, costruiscono piani riutilizzabili per tipo e applicano la normalizzazione prima della validazione.

La normalizzazione modifica la rappresentazione tecnica senza cambiare il significato, deve essere deterministica e, per quanto possibile, idempotente.

La validazione canonica comprende regole tecniche generiche. Le regole che richiedono semantica o contesto del dominio non vengono incorporate negli attributi canonici.

Ogni regola di validazione mantiene una semantica corretta anche quando viene invocata senza normalizzazione preventiva.

## Conseguenze

### Positive

- I vincoli sono visibili direttamente sul Contract.
- DTO e Controller non duplicano algoritmi.
- Normalizzazione e validazione rimangono responsabilità separate.
- I piani per tipo riducono il costo dell'analisi tramite reflection.
- I test dei motori e quelli di configurazione dei DTO possono restare distinti.

### Negative

- Il comportamento completo non è visibile nel solo Controller.
- Una configurazione errata può emergere durante la costruzione del piano.
- Nuovi attributi richiedono regola, wiring e test dedicati.
- Reflection, compilazione degli accessor e cache introducono complessità infrastrutturale.

## Alternative considerate

### Logica implementata nei DTO

Scartata perché mescolerebbe dichiarazione del contratto e algoritmi infrastrutturali.

### Validazione nei Controller

Scartata per duplicazione e incoerenza tra endpoint.

### Unificazione di normalizzazione e validazione

Scartata perché trasformazione e verifica hanno responsabilità e semantiche differenti.

## Riferimenti

- [Shared Framework](../SharedFramework.md)
- [ADR-0003](ADR-0003-request-processing-is-centralized-in-the-mvc-pipeline.md)
- [ADR-0004](ADR-0004-irequest-uses-default-interface-implementations.md)

