# ADR-ALPHA-0003 — Le Applications Web adottano una Page Architecture quando necessario

## Stato

Accettato

---

## Contesto

Le Applications Web di MultiPurposeServer non si limitano a visualizzare dati provenienti dalle API.

Molte pagine richiedono infatti attività di orchestrazione, composizione dello stato della UI, gestione della navigazione, coordinamento di più chiamate HTTP e preparazione del modello di presentazione.

Concentrare tali responsabilità direttamente nei Controller o nelle View renderebbe il codice difficile da mantenere, testare ed evolvere.

Era quindi necessario introdurre un livello dedicato all'orchestrazione della pagina, mantenendo separata la logica di presentazione dalla logica applicativa e dalla comunicazione con il backend.

---

## Decisione

Quando una pagina presenta una logica di orchestrazione significativa, le Applications Web adottano una **Page Architecture**.

Una pagina può essere composta dai seguenti elementi:

- Controller;
- Page Service;
- Page Model;
- View;
- View Components, quando appropriato.

Le responsabilità vengono distribuite come segue.

### Controller

Il Controller:

- riceve la richiesta HTTP;
- delega la preparazione della pagina al Page Service;
- restituisce la View corretta.

Non deve contenere logica di orchestrazione.

### Page Service

Il Page Service rappresenta il punto di coordinamento della pagina.

È responsabile di:

- invocare uno o più API Client;
- coordinare eventuali servizi locali;
- costruire il modello della pagina;
- gestire la logica di presentazione non appartenente alla View.

### Page Model

Il Page Model rappresenta esclusivamente lo stato necessario alla renderizzazione della pagina.

Non contiene logica applicativa.

### View

La View ha il solo compito di renderizzare il Page Model.

Non deve contenere logica di business né effettuare chiamate ai servizi applicativi.

La Page Architecture viene adottata quando introduce un reale miglioramento nella separazione delle responsabilità.

Per pagine semplici è possibile utilizzare una struttura più leggera, evitando livelli non necessari.

---

## Conseguenze

### Vantaggi

- La logica di orchestrazione rimane separata dalla presentazione.
- I Controller risultano estremamente semplici.
- Le View contengono esclusivamente codice di rendering.
- Il Page Service è facilmente testabile.
- Le responsabilità della pagina risultano chiaramente distribuite.
- L'architettura può evolvere senza modificare la logica di business del backend.

### Costi

- Viene introdotto un ulteriore livello architetturale.
- Le pagine molto semplici potrebbero richiedere componenti aggiuntivi non sempre necessari.
- È necessario mantenere la disciplina nel rispettare le responsabilità dei diversi componenti.

---

## Vedi anche

- `Architecture.md`
- `WebApplicationArchitecture.md`
- `DomainArchitecture.md`
- `MpsPlaybook.md`
