# ADR-0001 — I Service non dipendono dai Contracts

## Stato

Accettata

## Contesto

MultiPurposeServer espone funzionalità tramite API REST, ma i Service devono poter essere riutilizzati anche da applicazioni Web, Mobile, Desktop, worker, console application o futuri endpoint gRPC e SignalR.

I Contracts descrivono il protocollo pubblico dell'API e possono cambiare per esigenze di trasporto, serializzazione o compatibilità con i client. Se i Service dipendessero direttamente dai Contracts, la logica applicativa diventerebbe accoppiata al trasporto HTTP.

## Decisione

I Service non devono dipendere dai Contracts.

I Controller sono responsabili di:

- ricevere e validare i DTO pubblici;
- convertirli in modelli applicativi;
- invocare i Service;
- convertire i risultati applicativi in DTO di risposta.

I modelli interni necessari ai Service appartengono al layer applicativo, per esempio in `Services.Models`.

```text
Controller
    ↓
Contracts
    ↓ mapping
Services
```

La dipendenza seguente non è ammessa:

```text
Services
    ↓
Contracts
```

## Conseguenze

### Vantaggi

- I Service rimangono indipendenti dal protocollo HTTP.
- La logica applicativa è riutilizzabile da client e trasporti differenti.
- I Contracts possono evolvere senza propagare modifiche nel dominio applicativo.
- I test dei Service risultano più semplici e focalizzati sul comportamento.

### Costi

- È necessario introdurre mapping tra DTO e modelli applicativi.
- Alcuni tipi simili possono esistere sia nei Contracts sia nel layer applicativo.
