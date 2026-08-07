# ADR-0007 — I Service non dipendono dai Contracts

## Stato

Accettato

## Ambito

Architettura dei domini

## Data della decisione

2026-08-06

## Origine

`ADR-ALPHA-0001`, corretto durante il consolidamento di `DomainArchitecture.md`.

## Contesto

I Contracts descrivono il protocollo pubblico e possono cambiare per esigenze di trasporto, serializzazione o compatibilità con i client. I Service rappresentano invece capacità applicative del dominio e devono poter essere invocati da endpoint o orchestrazioni differenti.

L'appunto Alpha prescriveva sempre un Application Model intermedio. L'analisi di domini futuri ha mostrato che tale modello sarebbe spesso puramente meccanico e non deve diventare un passaggio obbligatorio.

## Decisione

I Service non dipendono dai Contracts pubblici.

Il Controller riceve la Request e passa al Service i valori richiesti dall'operazione. Può costruire un Business Model quando esiste una divergenza semantica reale rispetto al Data Model, ma non introduce un modello intermedio per convenzione.

Il Business Model è opzionale e può dipendere dal Data Model per effettuarne il mapping. Quando non serve, il Service può utilizzare direttamente le Entity restituite dai Repository.

Il flusso di uscita segue lo stesso confine: il Service restituisce Data Model o Business Model e il Response DTO costruisce il contratto pubblico.

## Conseguenze

### Positive

- I Service rimangono indipendenti dal protocollo HTTP.
- Le operazioni applicative possono essere riutilizzate in orchestrazioni differenti.
- I Contracts possono evolvere senza diventare il linguaggio interno del dominio.
- Non vengono creati modelli applicativi privi di semantica propria.

### Negative

- Il Controller deve tradurre Request e risultati attraverso il confine API.
- In presenza di divergenza semantica possono coesistere Contract, Business Model e Data Model.
- La scelta di introdurre un Business Model richiede valutazione caso per caso.

## Alternative considerate

### Passare i DTO direttamente ai Service

Scartato perché accoppierebbe la logica applicativa al protocollo pubblico.

### Introdurre sempre un Application Model

Scartato come regola generale perché produrrebbe mapping e tipi ridondanti quando Data Model e modello applicativo coincidono.

## Riferimenti

- [Domain Architecture](../DomainArchitecture.md)
- [ADR-0008](ADR-0008-response-dtos-map-internal-models.md)

