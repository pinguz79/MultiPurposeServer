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
| Alta | 0 |
| Media | 2 |
| Bassa | 3 |

---

## 3. Voci attive

### TD-0003 — Logging policy dei Controller

- **Area:** Logging / Controller
- **Stato:** Aperto
- **Priorità:** Media
- **Milestone:** Consolidamento delle API pubbliche e dell'osservabilità
- **Registrato:** 2026-08-07
- **Origine:** code review generale e `ArchitectureRoadmap.md`

La policy è stata consolidata in `LoggingArchitecture.md`: i Controller non producono log ordinari, la pipeline registra le eccezioni non gestite una sola volta e i componenti registrano gli errori che assorbono o gestiscono. Il debito resta aperto fino all'applicazione completa della policy.

- **Impatto:** osservabilità e coerenza non uniformi; rischio di log duplicati o poco significativi.
- **Costi/benefici:** la decisione può semplificare i Controller e migliorare la diagnosi, ma richiede consolidamento dell'infrastruttura di logging.
- **Urgenza strategica:** nessuna feature imminente dipende attualmente dalla policy.
- **Workaround:** logging locale esistente e gestione caso per caso.
- **Condizione di chiusura:** introduzione di `MultiPurposeServer.Shared.Logging`, applicazione della policy e rimozione delle dipendenze inutilizzate dai Controller.

### TD-0004 — Documentazione XML delle API pubbliche

- **Area:** API pubbliche / Documentazione
- **Stato:** Aperto
- **Priorità:** Media
- **Milestone:** Consolidamento delle API pubbliche e dell'osservabilità
- **Registrato:** 2026-08-07
- **Origine:** code review generale e `ArchitectureRoadmap.md`

La generazione della documentazione XML è disabilitata perché tipi e membri pubblici non sono ancora documentati in modo completo.

- **Impatto:** minore qualità della documentazione tecnica e del supporto agli strumenti che consumano i metadati pubblici.
- **Costi/benefici:** beneficio ampio ma intervento esteso su numerosi tipi e membri.
- **Urgenza strategica:** subordinato al consolidamento dell'architettura e delle convenzioni; può aumentare con lo sviluppo sistematico dei client OpenAPI.
- **Workaround:** documentazione Markdown e descrizione OpenAPI attuale.
- **Condizione di revisione:** completamento del consolidamento documentale o avvio della generazione sistematica dei client.

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

### TD-0008 — Separazione e rotazione dei segreti versionati

- **Area:** Sicurezza / Deployment
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-07
- **Origine:** consolidamento di `SecurityArchitecture.md` e ADR-0011

Alcuni segreti sono temporaneamente presenti nella configurazione versionata e nella history del repository. Le categorie correnti sono state valutate con rischio residuo basso o molto basso e l'esposizione rimane una deviazione consapevole dallo stato architetturale obiettivo.

- **Impatto:** possibile esposizione di credenziali con danno, recovery e probabilità attualmente valutati come limitati.
- **Costi/benefici:** la migrazione richiede distribuzione sicura della configurazione, aggiornamento degli host, disaster recovery separato e rotazione dei valori; il beneficio corrente non è ancora proporzionato al costo operativo.
- **Urgenza strategica:** bassa nel contesto attuale; aumenta al verificarsi delle condizioni definite dall'ADR-0011.
- **Workaround:** repository privato, accessi limitati, divieto di logging, possibilità di rotazione e registro delle valutazioni.
- **Condizione di revisione:** maturazione del deployment oppure variazione di esposizione, impatto, recovery, probabilità, collaborazione o criticità dei dati.

### TD-0009 — Centralizzazione degli invarianti strutturali degli Album

- **Area:** Portfolio / Album
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-08
- **Origine:** consolidamento di `Documentation/Portfolio/Domain.md`

I flussi correnti impediscono gran parte delle configurazioni invalide, ma gli invarianti che vietano Photo nelle Gallery e la compresenza di Photo e sottoalbum non sono espressi e verificati centralmente per ogni possibile percorso di scrittura o riconciliazione.

- **Impatto:** un futuro flusso di import o upload potrebbe introdurre una struttura incoerente e rendere inattendibile la classificazione `AlbumKind`.
- **Costi/benefici:** intervento circoscritto con beneficio crescente al moltiplicarsi dei flussi di scrittura; può richiedere validazione applicativa e controlli di consistenza sui dati esistenti.
- **Urgenza strategica:** bassa con le API correnti; deve essere rivalutata prima di introdurre upload, nuove API di creazione Photo o ulteriori importazioni.
- **Workaround:** `CreateAlbum` e la sincronizzazione applicano controlli locali sui casi attualmente gestiti.
- **Condizione di revisione:** progettazione di upload/import oppure modifica dei flussi di creazione di Album e Photo.

---

## 4. Voci risolte

### TD-0001 — Integration Test della pipeline MVC

- **Risolto:** 2026-08-13
- **Esito:** introdotto un progetto dedicato di Integration Test HTTP in memoria che verifica binding, normalizzazione, validazione ricorsiva, unicità dei payload bulk, mancata invocazione dei Service e traduzione delle eccezioni.

### TD-0002 — Gestione centralizzata di `KeyNotFoundException`

- **Risolto:** 2026-08-13
- **Esito:** introdotto un exception filter globale per la traduzione in `404 Not Found`; rimossi i `try/catch` duplicati dagli endpoint puntuali e mantenuta la gestione locale dei bulk che produce warning per item.

### TD-0005 — Conversione dei namespace residui

- **Risolto:** 2026-08-13
- **Esito:** tutti i sorgenti applicativi C# usano namespace block-scoped coerenti con progetto e gerarchia delle cartelle; i file generati restano esclusi dalla regola.

### TD-0006 — Uniformazione della formattazione interna

- **Risolto:** 2026-08-13
- **Esito:** convenzioni strutturali e di formattazione consolidate, applicate e verificate su server, client, Shared Framework, test e Portfolio.Web; enforcement deterministico disponibile in `Tools/CodeStyle`.

Le voci risolte conservano identificatore, data ed esito. Se il registro diventerà troppo esteso potranno essere trasferite in un archivio senza riutilizzarne gli ID.

---

## Riferimenti

- [MPS Playbook](MpsPlaybook.md)
- [Project Status](../ProjectStatus.md)
- [Architecture Roadmap](../Architecture/ArchitectureRoadmap.md)
- [Code Review](CodeReview.md)
