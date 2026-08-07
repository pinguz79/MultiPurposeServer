# Registro del Debito Tecnico

## 1. Scopo

Questo documento è la fonte autorevole del debito tecnico noto di MultiPurposeServer.

Il Playbook definisce criteri, priorità e processo di gestione. `ProjectStatus.md` mostra soltanto le voci appartenenti al livello di priorità attivo più alto e riepiloga numericamente le altre.

Gli identificatori `TD-XXXX` sono stabili e non vengono riutilizzati.

---

## 2. Riepilogo

| Priorità | Voci attive |
|---|---:|
| Critica | 0 |
| Alta | 2 |
| Media | 2 |
| Bassa | 3 |

---

## 3. Voci attive

### TD-0001 — Integration Test della pipeline MVC

- **Area:** Shared Framework / Portfolio.Api
- **Stato:** Aperto
- **Priorità:** Alta
- **Registrato:** 2026-08-07
- **Origine:** code review generale e `ArchitectureRoadmap.md`

La pipeline MVC non dispone ancora della suite di Integration Test prevista per verificare congiuntamente Model Binding, normalizzazione, validazione, filtro delle eccezioni e mancata invocazione dei Service in caso di Request non valida.

- **Impatto:** una regressione nell'integrazione tra componenti può non essere rilevata dai test unitari dei singoli motori.
- **Costi/benefici:** costo circoscritto e beneficio elevato su una responsabilità tecnica centrale.
- **Urgenza strategica:** direttamente collegato al consolidamento della Testing Architecture e al completamento della milestone della pipeline MVC.
- **Workaround:** test unitari e Framework Test coprono i componenti isolati, ma non l'interazione HTTP completa.
- **Condizione di revisione:** avvio del consolidamento della Testing Architecture o modifica della Request Pipeline.

### TD-0002 — Gestione centralizzata di `KeyNotFoundException`

- **Area:** Pipeline MVC / Controller
- **Stato:** Aperto
- **Priorità:** Alta
- **Registrato:** 2026-08-07
- **Origine:** code review generale e `ArchitectureRoadmap.md`

Alcuni Controller traducono ancora localmente `KeyNotFoundException` in una risposta HTTP invece di delegare la responsabilità a un componente centralizzato della pipeline.

- **Impatto:** duplicazione, possibili risposte incoerenti e Controller meno focalizzati sull'orchestrazione.
- **Costi/benefici:** intervento presumibilmente contenuto con beneficio trasversale e rimozione di test collocati al livello errato.
- **Urgenza strategica:** completa il confine della pipeline MVC e si integra naturalmente con TD-0001.
- **Workaround:** gestione locale tramite `try/catch` nei Controller interessati.
- **Condizione di revisione:** implementazione di TD-0001 o modifica della gestione centralizzata degli errori.

### TD-0003 — Logging policy dei Controller

- **Area:** Logging / Controller
- **Stato:** Aperto
- **Priorità:** Media
- **Registrato:** 2026-08-07
- **Origine:** code review generale e `ArchitectureRoadmap.md`

Non è ancora definito quali eventi debbano essere registrati dai Controller, quali appartengano a middleware o Service e quale granularità debbano avere le categorie di logging.

- **Impatto:** osservabilità e coerenza non uniformi; rischio di log duplicati o poco significativi.
- **Costi/benefici:** la decisione può semplificare i Controller e migliorare la diagnosi, ma richiede consolidamento dell'infrastruttura di logging.
- **Urgenza strategica:** nessuna feature imminente dipende attualmente dalla policy.
- **Workaround:** logging locale esistente e gestione caso per caso.
- **Condizione di revisione:** consolidamento di `InfrastructureArchitecture.md` o introduzione di nuovi Controller.

### TD-0004 — Documentazione XML delle API pubbliche

- **Area:** API pubbliche / Documentazione
- **Stato:** Aperto
- **Priorità:** Media
- **Registrato:** 2026-08-07
- **Origine:** code review generale e `ArchitectureRoadmap.md`

La generazione della documentazione XML è disabilitata perché tipi e membri pubblici non sono ancora documentati in modo completo.

- **Impatto:** minore qualità della documentazione tecnica e del supporto agli strumenti che consumano i metadati pubblici.
- **Costi/benefici:** beneficio ampio ma intervento esteso su numerosi tipi e membri.
- **Urgenza strategica:** subordinato al consolidamento dell'architettura e delle convenzioni; può aumentare con lo sviluppo sistematico dei client OpenAPI.
- **Workaround:** documentazione Markdown e descrizione OpenAPI attuale.
- **Condizione di revisione:** completamento del consolidamento documentale o avvio della generazione sistematica dei client.

### TD-0005 — Conversione dei namespace residui

- **Area:** Convenzioni C#
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-07
- **Origine:** code review generale

Alcuni file non seguono ancora uniformemente la convenzione corrente dei namespace a blocco.

- **Impatto:** incoerenza stilistica senza conseguenze funzionali.
- **Costi/benefici:** costo basso ma beneficio prevalentemente editoriale.
- **Urgenza strategica:** nessuna; può essere affrontato quando i file interessati vengono modificati.
- **Workaround:** nessuno necessario.
- **Condizione di revisione:** consolidamento delle convenzioni C# o interventi nelle aree interessate.

### TD-0006 — Uniformazione della formattazione interna

- **Area:** Convenzioni C#
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-07
- **Origine:** code review generale

Permangono differenze non funzionali nella formattazione e nell'organizzazione interna di alcuni file sorgente.

- **Impatto:** leggibilità e uniformità leggermente ridotte.
- **Costi/benefici:** intervento semplice ma beneficio limitato; una riscrittura meccanica estesa produrrebbe rumore nella history.
- **Urgenza strategica:** nessuna; preferibile una correzione opportunistica nelle aree già modificate.
- **Workaround:** seguire lo stile circostante.
- **Condizione di revisione:** consolidamento delle convenzioni C# o introduzione di formattazione automatizzata.

### TD-0007 — Allineamento dei nomi dei Contract Configuration Test

- **Area:** Testing / Contracts
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-07
- **Origine:** consolidamento di `TestingArchitecture.md`

Alcuni progetti e namespace, come `Portfolio.ContractsTests`, utilizzano ancora la precedente denominazione generica dei Contract Test. La Testing Architecture consolidata identifica questa responsabilità come Contract Configuration Test per distinguerla dai consumer/provider contract test.

- **Impatto:** la struttura fisica non riflette pienamente la tassonomia documentata e può rendere ambiguo lo scopo della suite.
- **Costi/benefici:** refactoring circoscritto con beneficio prevalentemente semantico; richiede comunque l'aggiornamento coordinato di solution, riferimenti e documentazione.
- **Urgenza strategica:** nessuna; la divergenza non modifica il comportamento dei test.
- **Workaround:** interpretare gli attuali progetti `ContractsTests` come suite di Contract Configuration Test.
- **Condizione di revisione:** interventi sui progetti interessati o consolidamento delle convenzioni implementative di testing.

---

## 4. Voci risolte

Nessuna voce risolta.

Le voci risolte conservano identificatore, data ed esito. Se il registro diventerà troppo esteso potranno essere trasferite in un archivio senza riutilizzarne gli ID.

---

## Riferimenti

- [MPS Playbook](MpsPlaybook.md)
- [Project Status](../ProjectStatus.md)
- [Architecture Roadmap](../Architecture/ArchitectureRoadmap.md)
- [Code Review](CodeReview.md)
