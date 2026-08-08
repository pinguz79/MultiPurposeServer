# Architettura di MultiPurposeServer

## 1. Scopo

Questo documento rappresenta il punto di ingresso dell'architettura di MultiPurposeServer.

Descrive i componenti principali della piattaforma, le loro responsabilità, i confini che li separano e le dipendenze consentite. Non descrive tecnologie, pattern implementativi o convenzioni di codice: tali dettagli appartengono ai documenti specialistici e agli Architecture Decision Record.

---

## 2. Relazione con la piattaforma

[Platform](../Platform.md) definisce l'identità di MultiPurposeServer, i suoi obiettivi e i domini applicativi previsti.

Questo documento parte da quella visione e descrive come la piattaforma preserva l'autonomia dei domini pur ospitandoli in un unico sistema.

---

## 3. Contesto architetturale

MultiPurposeServer è composto da un host, più domini applicativi indipendenti, un framework tecnico condiviso e un insieme di Applications che consumano le API pubbliche dei domini.

```text
Applications
    │
    │ API pubbliche
    ▼
MultiPurposeServer Host
├── Domain A
├── Domain B
├── Domain C
└── Shared Framework
```

L'host compone la piattaforma, ma non costituisce un dominio applicativo.

Lo Shared Framework fornisce meccanismi tecnici, ma non costituisce un dominio e non coordina i domini ospitati.

---

## 4. Principi architetturali

### 4.1 Host unico

I domini sono ospitati in un unico host per ridurre costi infrastrutturali e complessità operativa.

La condivisione dell'host è una scelta di deployment e non implica condivisione di logica applicativa, dati o ciclo evolutivo.

### 4.2 Autonomia dei domini

Ogni dominio possiede autonomamente:

- API pubbliche;
- logica applicativa;
- persistenza;
- configurazione;
- dipendenze;
- regole di sicurezza;
- ciclo evolutivo.

Un dominio non deve richiedere modifiche invasive agli altri domini per poter evolvere.

### 4.3 Estraibilità per ricomposizione

L'estraibilità è un test architetturale utilizzato per verificare la qualità dei confini, non un obiettivo immediato di deployment indipendente.

Un dominio deve poter essere ospitato in un nuovo sistema ricomponendo i suoi moduli, la configurazione e i servizi tecnici necessari, senza modificare la propria logica applicativa.

### 4.4 Assenza di logica applicativa nell'host

Tutta la funzionalità applicativa appartiene a un dominio.

L'host è responsabile esclusivamente dell'avvio, della composizione dei moduli e delle responsabilità tecniche legate al processo. Non contiene business logic e non diventa il contenitore delle funzionalità prive di una collocazione chiara.

### 4.5 Indipendenza tra domini

I domini non condividono implementazioni, entità persistite, database o transazioni.

Un dominio non accede ai componenti interni di un altro dominio. Quando eccezionalmente consuma una sua API pubblica, lo tratta come un servizio esterno, senza privilegi derivanti dalla presenza nello stesso host.

### 4.6 Opacità del deployment

Applications e domini conoscono gli indirizzi pubblici dei servizi utilizzati, ma non assumono che servizi differenti condividano host, processo, configurazione o infrastruttura.

La co-ubicazione di più domini non deve essere dedotta dalla struttura degli URL né utilizzata per creare dipendenze implicite.

### 4.7 Condivisione tecnica

Lo Shared Framework contiene esclusivamente meccanismi tecnici realmente riutilizzabili e indipendenti dal linguaggio applicativo dei domini.

La somiglianza tra entità o strutture appartenenti a domini differenti non giustifica da sola una condivisione.

> **Shared is Earned, not Planned.**

### 4.8 Configurazione indipendente dei servizi tecnici

Lo Shared Framework può fornire contratti e implementazioni comuni, ma ogni dominio seleziona e configura autonomamente le istanze dei servizi tecnici che utilizza.

La duplicazione della configurazione è preferibile all'introduzione di un accoppiamento tra domini.

Le responsabilità intrinsecamente legate al processo rimangono comuni all'host senza assumere significato applicativo.

### 4.9 Confini logici e fisici

I layer rappresentano confini di responsabilità.

La separazione in cartelle, progetti, assembly o pacchetti viene introdotta soltanto quando rafforza un confine reale. La coesistenza di più layer nello stesso progetto non autorizza dipendenze contrarie all'architettura.

---

## 5. Componenti principali

### 5.1 Host

L'host costituisce il punto di composizione della piattaforma.

È responsabile delle attività tecniche comuni al processo e attiva i domini senza conoscerne la struttura interna.

### 5.2 Domains

I Domains rappresentano i sistemi applicativi ospitati dalla piattaforma.

Ogni dominio racchiude il proprio protocollo pubblico, la propria logica applicativa, la propria persistenza e le proprie dipendenze.

### 5.3 Applications

Le Applications sono consumatori delle API pubbliche.

Un'Application può utilizzare uno o più domini senza creare una relazione architetturale tra essi. Ogni servizio viene configurato e consumato come dipendenza autonoma.

### 5.4 Shared Framework

Lo Shared Framework raccoglie capacità tecniche configurabili, estendibili e sostituibili.

I domini utilizzano selettivamente tali capacità senza essere coordinati dal framework e senza trasferirvi logica applicativa.

### 5.5 Responsabilità trasversali

Infrastruttura, sicurezza e testing attraversano l'intera piattaforma, ma mantengono responsabilità e documentazione dedicate.

---

## 6. Confini e dipendenze

Il flusso generale di una richiesta attraversa confini espliciti:

```text
Application
    │
    │ protocollo pubblico
    ▼
Domain API
    ▼
Business Logic
    ▼
Persistence
```

Valgono le seguenti regole:

- le Applications non conoscono l'implementazione interna dei domini;
- la descrizione pubblica del protocollo è indipendente dai modelli interni;
- la logica applicativa non dipende dal protocollo di trasporto;
- la persistenza non conosce protocollo, Applications o presentazione;
- l'host non conosce i componenti interni dei domini;
- un dominio non dipende dai componenti interni di un altro dominio;
- lo Shared Framework non dipende dalla business logic dei domini;
- le dipendenze circolari tra componenti non sono ammesse.

Le dipendenze devono seguire le responsabilità logiche anche quando più componenti sono collocati nello stesso assembly.

---

## 7. Composizione della piattaforma

Ogni dominio espone punti di composizione che consentono all'host di:

- registrare configurazione, dipendenze e servizi;
- contribuire alla pipeline e alle API pubbliche.

L'host utilizza tali punti di ingresso senza conoscere i dettagli interni del dominio.

L'ordine relativo di registrazione e attivazione dei domini non ne modifica il comportamento. L'host mantiene invece il controllo dell'ordine delle responsabilità tecniche comuni al processo.

Le forme concrete dei punti di composizione appartengono alla documentazione specialistica.

---

## 8. Evoluzione

L'architettura evolve incrementalmente a partire da esigenze osservate nel progetto.

Nuovi domini, Applications e servizi tecnici devono poter essere aggiunti preservando:

- autonomia dei domini;
- chiarezza dei confini;
- direzione delle dipendenze;
- testabilità;
- estraibilità per ricomposizione;
- coerenza della documentazione.

Nuove astrazioni, layer o separazioni fisiche vengono introdotti soltanto quando risolvono una responsabilità o un accoppiamento concreto.

Le decisioni significative e durature vengono formalizzate tramite Architecture Decision Record.

---

## 9. Approfondimenti

La documentazione prosegue dal generale al particolare.

### Architettura specialistica

- [Shared Framework](SharedFramework.md)
- [Request Processing](RequestProcessing.md)
- [Bulk Operations](BulkOperations.md)
- [Domain Architecture](DomainArchitecture.md)
- [API Architecture](ApiArchitecture.md)
- [Security Architecture](SecurityArchitecture.md)
- [Web Application Architecture](WebApplicationArchitecture.md)
- [Testing Architecture](TestingArchitecture.md)
- [Glossary](Glossary.md)

### Decisioni ed evoluzione

- [Architecture Decision Records](ADR/README.md)
- [ADR-0001 — I domini sono autonomi e ricomponibili](ADR/ADR-0001-domains-are-autonomous-and-recomposable.md)

### Materiale Alpha

- [Infrastructure Architecture](InfrastructureArchitecture.md)
- [Architecture Roadmap](ArchitectureRoadmap.md)

Questi documenti non sono ancora fonti autorevoli e devono essere confrontati con l'architettura consolidata e con il codice.

### Processo di sviluppo

- [MPS Playbook](../Engineering/MpsPlaybook.md)
