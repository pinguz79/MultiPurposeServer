# AI

Questa cartella raccoglie la documentazione dedicata alla collaborazione con gli strumenti di Intelligenza Artificiale utilizzati durante lo sviluppo di MultiPurposeServer.

Il suo scopo non è descrivere l'architettura del progetto, ma definire un modo di lavorare coerente tra sviluppatore e assistenti AI.

La documentazione è organizzata su due livelli.

## MpsPlaybook

Il **MpsPlaybook** rappresenta il documento di riferimento comune per tutti gli assistenti AI.

Definisce i principi e le pratiche di ingegneria che devono essere seguiti indipendentemente dallo strumento utilizzato.

Comprende, tra gli altri:

- filosofia di sviluppo;
- linee guida architetturali;
- linee guida di implementazione;
- strategia di testing;
- strategia di refactoring;
- strategia della documentazione;
- modalità di collaborazione.

Ogni assistente AI dovrebbe seguire queste regole indipendentemente dal modello utilizzato.

## Documenti specifici

Ogni assistente AI può richiedere istruzioni aggiuntive legate alle proprie caratteristiche o ai propri limiti operativi.

Per questo motivo possono essere presenti documenti dedicati, ad esempio:

- ChatGPT
- GitHub Copilot
- Claude
- Gemini

Questi documenti devono contenere esclusivamente istruzioni specifiche dello strumento.

Tutte le regole di carattere generale appartengono al **MpsPlaybook** e non devono essere duplicate.

---

## Relazione con la documentazione del progetto

La documentazione contenuta in questa cartella non sostituisce la documentazione tecnica del progetto.

Questa cartella non costituisce una fonte autorevole dell'architettura di MultiPurposeServer.

Le decisioni architetturali appartengono alla documentazione presente nella cartella Architecture.

Le decisioni architetturali rimangono documentate in:

- Architecture
- SharedFramework
- ArchitectureRoadmap
- Architecture Decision Records (ADR)

Gli assistenti AI non costituiscono la fonte autorevole dell'architettura di MultiPurposeServer.

Il loro compito è applicare in modo coerente le regole e i principi definiti dalla documentazione del progetto.