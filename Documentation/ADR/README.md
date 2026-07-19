# Architecture Decision Records

Questa cartella contiene gli Architecture Decision Record di MultiPurposeServer.

Gli ADR documentano decisioni tecniche strutturali già adottate o esplicitamente superate.

## Convenzione

```text
ADR-NNNN-titolo-breve.md
```

## Stati

- Proposta
- Accettata
- Superata
- Rifiutata

Quando una decisione viene sostituita, il vecchio ADR non viene eliminato: viene marcato come `Superata` e indica l'ADR che lo sostituisce.

## ADR presenti

- ADR-0001 — I Service non dipendono dai Contracts
- ADR-0002 — Ogni dominio registra le proprie dipendenze e possiede il proprio database
- ADR-0003 — Le applicazioni Web adottano una Page Architecture solo quando necessaria
- ADR-0004 — L'autenticazione del client è distinta dall'autenticazione dell'utente
