# ADR-0009 — Response DTO map domain entities

## Stato

Accettato

---

## Contesto

I Service restituiscono oggetti del dominio (`Portfolio.Data`).

L'API deve trasformare tali oggetti nei Response DTO definiti in `Portfolio.Contracts`.

Durante la progettazione è emerso il dubbio se i Contracts possano dipendere dalle Entity del dominio oppure se il mapping debba essere delegato ad un layer separato.

Una regola architetturale generica suggerirebbe di evitare qualsiasi dipendenza tra Contracts e Data, ma tale soluzione introdurrebbe un layer aggiuntivo senza apportare benefici concreti all'architettura di MultiPurposeServer.

---

## Decisione

I Response DTO appartenenti a `Portfolio.Contracts` sono responsabili della traduzione del modello interno nella rappresentazione pubblica dell'API.

Per questo motivo possono dipendere dalle Entity esposte da `Portfolio.Data`.

La dipendenza ammessa è quindi:

```text
Portfolio.Contracts
        ↓
Portfolio.Data
```

La direzione opposta non è ammessa.

```text
Portfolio.Data
        ✕
Portfolio.Contracts
```

---

## Vincoli

Questa decisione autorizza esclusivamente il mapping delle Entity.

Non sono invece consentiti:

- accesso a `DbContext`;
- accesso ai Repository;
- query Entity Framework;
- logica di persistenza;
- logica applicativa;
- dipendenze da ASP.NET Core.

Il Response DTO deve limitarsi a leggere il modello di dominio ricevuto e costruire il contratto pubblico.

---

## Motivazione

Questa scelta mantiene il mapping vicino al contratto che lo espone.

I benefici sono:

- eliminazione di mapper dedicati puramente meccanici;
- riduzione del codice boilerplate;
- maggiore coesione dei Response DTO;
- responsabilità chiaramente definita.

La dipendenza è considerata accettabile perché:

- è unidirezionale;
- non introduce dipendenze inverse;
- non espone dettagli della persistenza;
- non aumenta l'accoppiamento tra i domini.

---

## Conseguenze

I Response DTO possono esporre costruttori o factory che ricevono Entity del dominio.

I Service continuano a restituire il modello di dominio.

Le Entity rimangono completamente indipendenti dai Contracts.

Le Request DTO continuano invece a non conoscere le Entity.

---

## Alternative considerate

### Mapper dedicati

Creare un layer separato di mapper.

Scartato perché avrebbe introdotto un elevato numero di classi prive di reale logica.

### Mapping nei Service

Far costruire i Response DTO direttamente dai Service.

Scartato perché avrebbe spostato nei Service una responsabilità appartenente ai Contracts.

### Mapping nelle Entity

Consentire alle Entity di costruire direttamente i Response DTO.

Scartato perché avrebbe introdotto una dipendenza inversa:

```text
Portfolio.Data
        ↓
Portfolio.Contracts
```

in contrasto con l'architettura del progetto.

---

## Riferimenti

- SharedFramework.md
- CodeReviewChecklist.md