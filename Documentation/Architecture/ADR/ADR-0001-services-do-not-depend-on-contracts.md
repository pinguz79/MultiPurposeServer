# ADR-0001 — I Service non dipendono dai Contracts

## Stato

Accettato

---

## Contesto

MultiPurposeServer espone funzionalità tramite API REST, ma i Service devono poter essere riutilizzati anche da applicazioni Web, Mobile, Desktop, worker, console application o futuri endpoint gRPC e SignalR.

I Contracts descrivono il protocollo pubblico dell'API e possono evolvere per esigenze di trasporto, serializzazione o compatibilità con i client.

Se i Service dipendessero direttamente dai Contracts, la logica applicativa diventerebbe accoppiata al protocollo di comunicazione.

---

## Decisione

I Service non devono dipendere dai Contracts.

I Controller sono responsabili di:

- ricevere le Request già normalizzate e validate dalla pipeline;
- convertire i DTO pubblici in modelli applicativi;
- invocare i Service;
- convertire i risultati applicativi nei DTO di risposta.

I modelli interni utilizzati dai Service appartengono al layer applicativo, ad esempio in `Services.Models`.

Il flusso corretto è:

```text
Controller
    ↓
Contracts
    ↓ mapping
Services
```

La seguente dipendenza non è ammessa:

```text
Services
    ↓
Contracts
```

I Contracts rappresentano il contratto pubblico delle API.

I Service rappresentano il modello applicativo interno.

Le due responsabilità devono rimanere indipendenti.

---

## Conseguenze

### Vantaggi

- I Service rimangono indipendenti dal protocollo HTTP.
- La logica applicativa è riutilizzabile da client e trasporti differenti.
- I Contracts possono evolvere senza propagare modifiche nel dominio applicativo.
- I test dei Service risultano più semplici e focalizzati sul comportamento.

### Costi

- È necessario introdurre un mapping tra DTO e modelli applicativi.
- Alcuni tipi concettualmente simili possono esistere sia nei Contracts sia nel layer applicativo.

---

## Vedi anche

- `Architecture.md`
- `DomainArchitecture.md`
- `SharedFramework.md`
- `MpsPlaybook.md`