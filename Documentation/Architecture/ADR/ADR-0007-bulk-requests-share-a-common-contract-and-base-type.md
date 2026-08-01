# ADR-0007 — Le Request Bulk condividono una struttura e un contratto comuni

## Stato

Accettato

---

## Contesto

Diversi endpoint di MultiPurposeServer devono eseguire la stessa operazione su più elementi contemporaneamente.

Esempi tipici includono:

- eliminazione di più elementi;
- aggiornamenti multipli;
- operazioni di attivazione o disattivazione;
- modifiche massive.

In assenza di un modello condiviso, ogni dominio rischierebbe di definire proprie Request Bulk con strutture, proprietà e comportamenti differenti.

Questo introdurrebbe incoerenze tra le API, duplicazione di codice e una maggiore complessità sia lato server sia lato client.

Poiché le operazioni Bulk rappresentano un concetto trasversale ai domini, è opportuno definirne un contratto comune all'interno dello Shared Framework.

---

## Decisione

Le operazioni Bulk adottano una struttura condivisa basata su un contratto comune.

Le Request Bulk ereditano da una base comune oppure implementano un'interfaccia condivisa che definisce il comportamento previsto dal framework.

Il contratto comune stabilisce:

- la rappresentazione della collezione degli elementi interessati;
- il comportamento di normalizzazione;
- il comportamento di validazione;
- le convenzioni utilizzate dalla pipeline condivisa.

Ogni dominio rimane responsabile esclusivamente della logica applicativa dell'operazione Bulk.

Il funzionamento comune appartiene invece allo Shared Framework.

---

## Conseguenze

### Vantaggi

- Tutte le API Bulk presentano una struttura coerente.
- I client possono interagire con operazioni massive seguendo convenzioni uniformi.
- Normalizzazione e validazione vengono centralizzate.
- La duplicazione di codice tra domini viene ridotta.
- Nuove operazioni Bulk possono essere introdotte rapidamente adottando il contratto condiviso.
- I test relativi al comportamento comune vengono concentrati nello Shared Framework.

### Costi

- Le Request Bulk devono rispettare il contratto definito dal framework condiviso.
- Eventuali esigenze particolari devono essere valutate per evitare di compromettere la coerenza del modello comune.
- L'evoluzione del contratto Bulk richiede particolare attenzione alla compatibilità con tutti i domini che lo utilizzano.

---

## Vedi anche

- `Architecture.md`
- `SharedFramework.md`
- `DomainArchitecture.md`
- `TestingArchitecture.md`
- `MpsPlaybook.md`