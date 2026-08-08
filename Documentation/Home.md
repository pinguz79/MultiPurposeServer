# MultiPurposeServer Documentation

## Scopo

Questa pagina è l'indice della documentazione di MultiPurposeServer.

Il percorso parte dall'identità della piattaforma e procede verso architettura, decisioni, pratiche di engineering, domini e pianificazione. I collegamenti sono distinti in base allo stato di consolidamento dei documenti.

---

## Bootstrap di una nuova sessione

Questa pagina appartiene al bootstrap ufficiale, ma il suo catalogo non sostituisce l'ordine definito dal [README della root](../README.md).

Per ricostruire il contesto di lavoro seguire, nell'ordine:

1. [Home](Home.md)
2. [Platform](Platform.md)
3. [Chat Recovery](ChatRecovery.md)
4. [Project Status](ProjectStatus.md)

Durante una procedura di recovery, completare questo percorso prima di approfondire i documenti specialistici elencati nelle sezioni successive. `ProjectStatus.md` stabilisce la milestone e l'attività correnti.

---

## Documentazione consolidata

### Piattaforma e architettura

1. [Platform](Platform.md) definisce identità, obiettivi e principi della piattaforma.
2. [Architecture](Architecture/Architecture.md) presenta la struttura generale e i principali confini del sistema.
3. [Shared Framework](Architecture/SharedFramework.md) descrive i servizi tecnici condivisi e le regole della loro evoluzione.
4. [Domain Architecture](Architecture/DomainArchitecture.md) definisce autonomia, composizione e responsabilità interne dei domini.
5. [Security Architecture](Architecture/SecurityArchitecture.md) definisce identità, autenticazione, autorizzazione e protezione delle risorse.
6. [Web Application Architecture](Architecture/WebApplicationArchitecture.md) descrive Portfolio.Web, MVC e Page Architecture.
7. [Testing Architecture](Architecture/TestingArchitecture.md) definisce livelli, responsabilità e confini della strategia di test.
8. [Architecture Decision Records](Architecture/ADR/README.md) raccoglie le motivazioni delle decisioni incontrate nei documenti architetturali.

Gli ADR si leggono dopo il documento specialistico pertinente e non sostituiscono la descrizione dell'architettura corrente.

### Engineering

- [Engineering](Engineering/README.md) introduce le pratiche di sviluppo.
- [MPS Playbook](Engineering/MpsPlaybook.md) definisce workflow, refactoring, documentazione e Definition of Done.
- [Code Review](Engineering/CodeReview.md) descrive il processo di revisione completa della solution.
- [Code Review Checklist](Engineering/CodeReviewChecklist.md) fornisce la checklist operativa.
- [Technical Debt](Engineering/TechnicalDebt.md) è il registro autorevole del debito tecnico noto.

### Dominio Portfolio

- [Portfolio Domain](Portfolio/Domain.md) definisce identità, linguaggio, concetti e invarianti funzionali del dominio.

Portfolio è attualmente l'unico dominio con una specifica consolidata. ModelBook, Skating e gli altri domini candidati rimangono direzioni future finché non vengono avviati e documentati.

### Roadmap e backlog

- [Visione](Roadmap/Vision.md) descrive le direzioni di lungo periodo.
- [Roadmap](Roadmap/Roadmap.md) organizza l'evoluzione in `Now`, `Next` e `Later`.
- [Backlog](Roadmap/Backlog.md) registra feature, bug e attività funzionali note.

`ProjectStatus.md` prevale sulla Roadmap quando occorre stabilire quale attività riprendere ora.

### Sicurezza operativa

- [Secret Risk Register](Security/SecretRiskRegister.md) registra le valutazioni dei segreti temporaneamente versionati.

---

## Documentazione operativa

- [Chat Recovery](ChatRecovery.md) definisce come ricostruire il contesto in una nuova sessione.
- [Project Status](ProjectStatus.md) rappresenta la fonte autorevole sullo stato corrente del progetto.

Questi documenti vengono aggiornati quando cambia la milestone, l'attività corrente o la procedura di bootstrap.

---

## Documentazione ancora Alpha

I seguenti documenti conservano materiale utile ma non ancora promosso a fonte autorevole:

- [Infrastructure Architecture](Architecture/InfrastructureArchitecture.md);
- [Architecture Roadmap](Architecture/ArchitectureRoadmap.md);
- [Glossary](Architecture/Glossary.md);
- [Portfolio.Web Roadmap](Portfolio/Portfolio.Web%20Roadmap.md);
- [Documentazione AI](AI/README.md);
- [Organizzazione della documentazione](README.md).

[Architecture Consolidation](Architecture/ArchitectureConsolidation.md) è un documento temporaneo di migrazione: conserva la destinazione dei concetti ancora da distribuire e verrà eliminato al termine del consolidamento.

Il contenuto Alpha può essere incompleto, incoerente o superato. Prima di usarlo come base per una decisione deve essere confrontato con il codice e con la documentazione consolidata.

---

## Regola di navigazione

Per comprendere un argomento:

1. partire dal documento più generale collegato da questa pagina;
2. seguire i riferimenti specialistici nell'ordine proposto dal documento;
3. leggere gli ADR collegati per comprenderne motivazioni e alternative;
4. verificare in `ProjectStatus.md` se il documento è coinvolto nell'attività corrente;
5. trattare esplicitamente come non autorevole ogni fonte ancora Alpha.
