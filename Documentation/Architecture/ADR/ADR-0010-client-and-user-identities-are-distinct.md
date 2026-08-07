# ADR-0010 — Client e utente sono identità distinte

## Stato

Accettato

## Ambito

Sicurezza

## Data della decisione

2026-08-07

## Origine

`ADR-ALPHA-0004`, corretto ed esteso durante il consolidamento di `SecurityArchitecture.md`.

## Contesto

MPS supporta Applications differenti, come Web, Mobile, Desktop e componenti amministrativi. L'applicazione che invoca un'API e la persona che la utilizza rappresentano soggetti differenti e possono possedere capacità indipendenti.

La decisione originaria assumeva tuttavia che ogni client potesse custodire una credenziale. Codice eseguito nel browser e applicazioni distribuite agli utenti non possono mantenere riservato un segreto statico incorporato e non devono essere considerati autenticati soltanto perché lo presentano.

## Decisione

Il contesto del client e l'identità dell'utente sono dimensioni logicamente distinte del Security Context.

L'autorizzazione considera sia le capacità riconosciute al client sia i permessi dell'eventuale account utente. Un client non amministrativo non accede alle API amministrative anche quando è utilizzato da un amministratore; un client amministrativo non attribuisce privilegi a un utente che non li possiede.

La distinzione logica non impone due credenziali fisiche. Client e utente possono essere rappresentati da credenziali separate, da un unico access token, da una sessione o da altri meccanismi coerenti con il protocollo adottato.

I client vengono distinti in:

- **confidential client**, capace di custodire una credenziale e quindi autenticabile con un meccanismo adeguato;
- **public client**, incapace di mantenere segreto un valore incorporato.

Per un public client, un identificatore o una chiave statica può fornire contesto ma non prova forte dell'identità del software. Le policy non gli attribuiscono garanzie superiori a quelle offerte dal meccanismo concretamente adottato.

L'eventuale accesso forte di un public client ad API amministrative richiede una decisione specifica basata su un caso reale.

## Conseguenze

### Positive

- Client e account possono essere revocati o limitati indipendentemente.
- Le policy rappresentano esplicitamente entrambe le dimensioni.
- L'introduzione della User Authentication non modifica il significato del contesto client.
- Il modello non fonda la sicurezza dei public client su segreti estraibili.

### Negative

- Configurazione, policy e test risultano più articolati.
- Alcune API richiedono la composizione di più evidenze di sicurezza.
- Le garanzie disponibili differiscono fra confidential e public client.
- Le applicazioni amministrative pubblicamente distribuibili possono richiedere infrastruttura aggiuntiva.

## Alternative considerate

### Una sola identità per richiesta

Scartata perché confonderebbe capacità dell'applicazione e permessi della persona.

### API key statica per ogni tipo di client

Scartata come prova universale dell'identità perché un public client non può mantenerla riservata.

### Autorizzazione basata soltanto sull'utente

Scartata perché un client ordinario potrebbe invocare API non previste per la propria superficie.

## Riferimenti

- [Security Architecture](../SecurityArchitecture.md)
- [OAuth 2.0 — RFC 6749](https://www.rfc-editor.org/rfc/rfc6749)
- [OAuth 2.0 for Native Apps — RFC 8252](https://www.rfc-editor.org/rfc/rfc8252)
- [OAuth 2.0 Security Best Current Practice — RFC 9700](https://www.rfc-editor.org/rfc/rfc9700)
