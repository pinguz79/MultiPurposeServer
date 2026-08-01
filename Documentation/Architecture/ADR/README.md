# Architecture Decision Records (ADR)

## Scopo

Questa cartella contiene gli **Architecture Decision Records (ADR)** di MultiPurposeServer.

Gli ADR documentano le decisioni architetturali significative che hanno influenzato l'evoluzione del progetto.

Ogni ADR descrive il contesto nel quale è nata una decisione, la soluzione adottata e le conseguenze che essa comporta.

Gli ADR costituiscono la memoria storica dell'architettura e permettono di comprendere **perché** determinate scelte siano state effettuate, anche molti anni dopo la loro introduzione.

---

## Relazione con la documentazione architetturale

Gli ADR non sostituiscono la documentazione architetturale.

I due tipi di documentazione hanno responsabilità differenti.

La documentazione architetturale descrive **come** MultiPurposeServer è organizzato.

Gli ADR descrivono invece **perché** sono state prese specifiche decisioni progettuali.

In particolare:

- `Architecture.md` descrive la visione complessiva del sistema.
- I documenti architetturali specializzati (`DomainArchitecture`, `InfrastructureArchitecture`, `SecurityArchitecture`, ecc.) descrivono i diversi sottosistemi.
- Gli ADR conservano la motivazione delle decisioni che hanno portato all'architettura attuale.

L'architettura rappresenta quindi lo stato corrente del progetto.

Gli ADR raccontano il percorso che ha portato a tale stato.

---

## Quando creare un ADR

Non ogni modifica richiede un Architecture Decision Records (ADR).

Un ADR dovrebbe essere creato quando una decisione:

- modifica la struttura architetturale del progetto;
- introduce un principio destinato a rimanere nel tempo;
- influenza più domini o più componenti;
- sostituisce una decisione architetturale precedente;
- potrebbe risultare difficile da comprendere in futuro senza conoscerne il contesto.

Le decisioni puramente implementative o temporanee non richiedono normalmente un ADR.

---

## Struttura di un ADR

Ogni ADR utilizza la seguente struttura.

```text
# ADR-000X – Titolo

## Stato

Accettato

## Contesto

...

## Decisione

...

## Conseguenze

### Positive

...

### Negative

...

## Vedi anche
```

La struttura deve rimanere il più possibile uniforme per tutti gli ADR del progetto.

---

## Convenzioni

Ogni ADR utilizza il seguente nome file:

```text
ADR-000X-titolo-breve.md
```

La numerazione è progressiva.

I numeri non devono essere riutilizzati, anche nel caso in cui un ADR venga successivamente superato.

---

## Stati

Ogni ADR può trovarsi in uno dei seguenti stati.

### Proposto

La decisione è ancora in discussione.

L'ADR documenta una possibile evoluzione architetturale.

### Accettato

La decisione è stata adottata ed entra a far parte dell'architettura del progetto.

### Superato

La decisione è stata sostituita da una successiva.

L'ADR non deve essere eliminato.

Deve invece indicare esplicitamente quale nuovo ADR lo sostituisce.

### Rifiutato

La proposta è stata valutata ma non adottata.

L'ADR viene conservato per documentare le alternative considerate.

---

## Evoluzione degli ADR

Gli ADR rappresentano documenti storici.

Una volta accettati non dovrebbero essere riscritti per riflettere modifiche successive dell'architettura.

Quando una decisione cambia è preferibile creare un nuovo ADR che descriva la nuova scelta, mantenendo il precedente nello stato **Superato**.

Questo permette di ricostruire l'evoluzione dell'architettura nel tempo.

---

## Ordine di lettura consigliato

Per comprendere MultiPurposeServer si suggerisce il seguente percorso.

1. `Architecture.md`
2. `DomainArchitecture.md`
3. `WebApplicationArchitecture.md`
4. `InfrastructureArchitecture.md`
5. `SecurityArchitecture.md`
6. `TestingArchitecture.md`
7. `SharedFramework.md`
8. `ArchitectureRoadmap.md`
9. Architecture Decision Records (ADR)

---

## ADR presenti

- ADR-0001 — I Service non dipendono dai Contracts
- ADR-0002 — Ogni dominio registra autonomamente le proprie dipendenze e possiede il proprio database
- ADR-0003 — Le Applications Web adottano una Page Architecture quando necessaria
- ADR-0004 — L'autenticazione del client è distinta dall'autenticazione dell'utente
- ADR-0005 — La normalizzazione e la validazione delle Request sono centralizzate nella pipeline MVC
- ADR-0006 — `IRequest` fornisce implementazioni predefinite per `Normalize()` e `Validate()`
- ADR-0007 — Le Request Bulk condividono una struttura e un contratto comuni
- ADR-0008 — La normalizzazione e la validazione dei Contracts sono dichiarative