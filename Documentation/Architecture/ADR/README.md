# Architecture Decision Records

## 1. Scopo

Gli Architecture Decision Record descrivono perché MultiPurposeServer ha adottato decisioni architetturali significative, durature o non ovvie.

La documentazione architetturale descrive lo stato corrente della piattaforma. Gli ADR ne conservano contesto, alternative e conseguenze senza sostituirla.

---

## 2. Quando creare un ADR

Un ADR è appropriato quando una decisione:

- modifica un confine o una responsabilità architetturale;
- introduce un principio destinato a durare;
- influenza più componenti o domini;
- adotta o rifiuta un'alternativa con conseguenze rilevanti;
- sostituisce una decisione precedente;
- risulterebbe difficile da comprendere in futuro senza conoscerne il contesto.

Modifiche locali, convenzioni di codice, dettagli temporanei e scelte facilmente reversibili appartengono normalmente alla documentazione implementativa o al Playbook.

Non ogni principio architetturale richiede un ADR: il record deve preservare una decisione e le sue motivazioni, non duplicare l'intera architettura.

---

## 3. Ambito

Ogni ADR dichiara esplicitamente il proprio ambito, per esempio:

- `Piattaforma`;
- `Shared Framework`;
- `Architettura dei domini`;
- `Dominio: Portfolio`;
- `Application: Portfolio.Web`;
- `Infrastruttura`;
- `Sicurezza`.

Una decisione nata analizzando un solo dominio non diventa automaticamente una regola di piattaforma. L'ambito deve riflettere i consumatori e i vincoli realmente considerati.

---

## 4. Struttura

Ogni ADR ufficiale utilizza la seguente struttura minima:

```markdown
# ADR-000X — Titolo

## Stato

Accettato

## Ambito

Piattaforma

## Data della decisione

YYYY-MM-DD

## Contesto

...

## Decisione

...

## Conseguenze

### Positive

...

### Negative

...

## Alternative considerate

...

## Riferimenti

...
```

Se l'ADR deriva da appunti precedenti può includere una sezione `Origine`. Un ADR superato deve includere `Superato da`; il nuovo ADR indica a sua volta quale decisione sostituisce.

---

## 5. Stati

### Proposto

La decisione è in discussione e non fa ancora parte dell'architettura.

### Accettato

La decisione è stata adottata nell'ambito dichiarato.

### Superato

La decisione è stata sostituita da un ADR successivo. Il documento viene conservato e indica il record che lo supera.

### Rifiutato

La proposta è stata valutata e non adottata. Viene conservata quando documentarne il rifiuto evita di ripetere la stessa analisi.

---

## 6. Numerazione e nomi

Gli ADR ufficiali utilizzano nomi nel formato:

```text
ADR-000X-titolo-breve.md
```

La numerazione è progressiva, stabile e non viene riutilizzata. Dopo la prima pubblicazione ufficiale, un nuovo ADR riceve sempre il numero successivo anche quando riguarda un concetto collocato prima nell'ordine di lettura.

L'indice può proporre un ordine tematico differente dalla numerazione.

---

## 7. Evoluzione

Un ADR accettato non viene riscritto per rappresentare una decisione sostanzialmente diversa.

- Correzioni editoriali, link e chiarimenti che non alterano la decisione sono ammessi.
- Un cambiamento sostanziale richiede un nuovo ADR.
- Il precedente passa allo stato `Superato` e rimane nel repository.
- Gli ADR non vengono rinumerati per adattarsi a evoluzioni successive della documentazione.

---

## 8. Reset del catalogo Alpha

Il primo catalogo ADR è nato durante una ricostruzione non ordinata dell'architettura e conteneva appunti con granularità, scope e livello di astrazione incoerenti.

Prima della prima pubblicazione stabile è stato avviato un unico reset editoriale. Gli appunti originari sono identificati dal prefisso:

```text
ADR-ALPHA-000X
```

Il numero Alpha conserva la corrispondenza con il vecchio nome del file, ma non appartiene alla numerazione ufficiale.

Gli appunti Alpha:

- non sono ADR ufficiali;
- non costituiscono una fonte architetturale autorevole;
- vengono elencati in `ArchitectureConsolidation.md` finché non sono consolidati;
- possono essere riscritti, fusi, separati o eliminati;
- non possono essere creati dopo il completamento del reset.

Questa procedura è un'eccezione irripetibile dovuta alla natura Alpha del catalogo originario e non costituisce una regola generale di gestione degli ADR.

---

## 9. Ordine di lettura

Gli ADR si leggono dopo il documento architetturale che descrive il relativo sottosistema. Il percorso parte dal bootstrap e procede dal generale al particolare:

1. `README.md` della root;
2. documenti di bootstrap collegati;
3. `Architecture.md`;
4. documento specialistico pertinente;
5. ADR collegati da quel documento.

Gli ADR non fanno parte del bootstrap minimo, ma approfondiscono le motivazioni delle scelte incontrate durante la lettura.

---

## 10. ADR ufficiali

### Piattaforma

- [ADR-0001 — I domini sono autonomi e ricomponibili](ADR-0001-domains-are-autonomous-and-recomposable.md)

### Shared Framework

- [ADR-0002 — Shared nasce da responsabilità tecniche concrete](ADR-0002-shared-emerges-from-concrete-technical-responsibilities.md)
- [ADR-0003 — L'elaborazione delle Request è centralizzata nella pipeline MVC](ADR-0003-request-processing-is-centralized-in-the-mvc-pipeline.md)
- [ADR-0004 — `IRequest` espone `Normalize()` e `Validate()` tramite implementazioni predefinite](ADR-0004-irequest-uses-default-interface-implementations.md)
- [ADR-0005 — Normalizzazione e validazione dei Contracts sono dichiarative](ADR-0005-normalization-and-validation-are-declarative.md)
- [ADR-0006 — Le Request Bulk condividono contratti tecnici comuni](ADR-0006-bulk-requests-share-common-technical-contracts.md)

### Architettura dei domini

- [ADR-0007 — I Service non dipendono dai Contracts](ADR-0007-services-do-not-depend-on-contracts.md)
- [ADR-0008 — I Response DTO mappano i modelli interni](ADR-0008-response-dtos-map-internal-models.md)
- [ADR-0009 — I Controller orchestrano le operazioni applicative](ADR-0009-controllers-orchestrate-application-operations.md)

### Sicurezza

- [ADR-0010 — Client e utente sono identità distinte](ADR-0010-client-and-user-identities-are-distinct.md)
- [ADR-0011 — I segreti versionati temporaneamente richiedono rischio basso](ADR-0011-temporary-versioned-secrets-require-low-risk.md)

---

## Riferimenti

- [Architecture](../Architecture.md)
- [Architecture Consolidation](../ArchitectureConsolidation.md)
- [Shared Framework](../SharedFramework.md)
- [Domain Architecture](../DomainArchitecture.md)
- [MPS Playbook](../../Engineering/MpsPlaybook.md)
