# Roadmap Architetturale di MultiPurposeServer

> **Stato: Alpha 0 — non autorevole.** Il contenuto deve essere verificato e consolidato prima della promozione.

## 1. Scopo del documento

Questo documento descrive lo stato di avanzamento dell'architettura di MultiPurposeServer.

A differenza della Roadmap del progetto, che descrive la visione di lungo periodo e l'evoluzione funzionale della piattaforma, questo documento segue esclusivamente l'evoluzione tecnica del framework condiviso e dell'architettura applicativa.

Le decisioni già adottate vengono documentate negli Architecture Decision Records (ADR), mentre i concetti emergenti dei singoli domini appartengono alla rispettiva documentazione di dominio.

Le attività presenti in questo documento rappresentano milestone architetturali e non attività operative.

Ogni milestone può trovarsi in uno dei seguenti stati:

- ✅ Completed
- 🚧 In Progress
- 📋 Planned

---

# 2. Shared Contracts

Status: ✅ Completed

## IRequest

Responsabilità comuni di tutte le Request.

### Completed

- [x] Introduzione del contratto condiviso
- [x] Default interface implementation
- [x] Normalize()
- [x] Validate()

---

## IBulk<TItem>

Contratto comune per tutte le richieste Bulk.

### Completed

- [x] Introduzione del contratto
- [x] Espone BulkOptions
- [x] Espone IReadOnlyCollection<TItem>

---

## BulkRequest<TItem>

Implementazione comune delle richieste Bulk.

### Completed

- [x] Introduzione della classe base
- [x] BulkOptions comuni
- [x] Items comuni
- [x] Vincolo where TItem : IRequest
- [x] Eliminazione del codice duplicato nelle BulkRequest

---

## BulkOptions

### Completed

- [x] Estrazione sotto Shared.Contracts
- [x] ErrorStrategy comune

---

# 3. Normalization Framework

Status: ✅ Completed

## Engine

### Completed

- [x] Reflection based engine
- [x] Cached NormalizationPlan
- [x] Getter compilati
- [x] Eliminazione della reflection durante l'esecuzione

---

## Attributes

### Completed

- [x] NormalizeAttribute
- [x] NormalizeChildrenAttribute

---

## Tests

### Completed

- [x] Unit test del motore
- [x] Test dei contratti DTO
- [x] Test di coerenza Parent/Children
- [x] Test di regressione IRequest.Normalize()

---

# 4. Validation Framework

Status: ✅ Completed

## Engine

### Completed

- [x] Reflection based engine
- [x] Cached ValidationPlan
- [x] Getter compilati
- [x] Eliminazione della reflection durante l'esecuzione

---

## Validation Attributes

### Completed

- [x] RequiredAttribute
- [x] RequiredAtLeastOneAttribute
- [x] RequiredAtLeastOneTrueAttribute
- [x] ValidateChildrenAttribute

---

## Validation Rules

### Completed

- [x] Required
- [x] RequiredAtLeastOne
- [x] RequiredAtLeastOneTrue
- [x] ValidateChildren

---

## Tests

### Completed

- [x] Unit test del motore
- [x] Test dei contratti DTO
- [x] Test di regressione IRequest.Validate()

---

# 5. MVC Request Pipeline

Status: 🚧 In Progress

## Completed

### Request Pipeline

- [x] RequestNormalizationValidationFilter
- [x] Normalizzazione automatica
- [x] Validazione automatica

### Exception Handling

- [x] ValidationExceptionFilter
- [x] Conversione automatica ValidationException → HTTP 400

### Controllers

- [x] Rimozione della validazione manuale dai controller puntuali

---

## Remaining

- [ ] Test di integrazione della pipeline MVC
- [ ] Refactoring dei controller Bulk
- [ ] Eliminazione delle validazioni manuali residue
- [ ] Gestione centralizzata delle eccezioni applicative, per esempio KeyNotFoundException

---

# 6. Portfolio

Status: 🚧 In Progress

## Backend APIs

### Completed

- [x] Album API
- [x] Photo API
- [x] Cache API

### Remaining

- [ ] Bulk Album API
- [ ] Bulk Photo API

---

# 7. Authentication

Status: 🚧 In Progress

### Completed

- [x] API Key Authentication
- [x] FrontEnd / BackEnd Authorization Policies
- [x] Bypass autenticazione in ambiente Development

### Remaining

- [ ] Valutazione autenticazione Client/Desktop
- [ ] Revisione integrazione con altri domini

---

# 8. Technical Debt

## Validation

- [ ] Valutare la fattorizzazione tra RequiredAtLeastOne e RequiredAtLeastOneTrue.

## Bulk

- [ ] Revisionare completamente la pipeline Bulk.

## Organizzazione interna dei file sorgente

Definire una convenzione uniforme per l'organizzazione interna dei file sorgente C#.

### Obiettivi

- [x] Definire un ordinamento coerente dei membri all'interno delle classi.
- [x] Raggruppare i membri per responsabilità.
- [x] Introdurre sezioni `#region` significative quando migliorano realmente la navigazione del codice.
- [x] Separare chiaramente i metodi di test dai metodi di supporto.
- [x] Separare helper, fixture, factory e dati di test in tipi e sezioni coerenti, vietando i tipi annidati.
- [x] Applicare la convenzione in modo uniforme a tutta la solution.
- [x] Evitare di introdurre formattazioni e ritorni a capo non necessari durante refactoring non stilistici.

### Stato

- **Completato il 2026-08-13**

> **Nota**
>
> Prima di formalizzare la convenzione nel `MpsPlaybook.md`, essa dovrà essere applicata e verificata su un numero significativo di classi, in particolare nei progetti di test, così da validarne l'efficacia e l'usabilità.

## Uniformazione dei namespace C#

Convertire tutti i namespace ancora dichiarati con sintassi file-scoped alla convenzione block-scoped adottata dalla solution.

### Obiettivi

- [x] Convertire tutti i namespace file-scoped in namespace block-scoped.
- [x] Mantenere invariati i namespace dichiarati.
- [x] Non modificare la collocazione dei file.
- [x] Non applicare refactoring o riformattazioni non correlate.
- [x] Verificare che i namespace continuino a corrispondere al progetto e alla struttura delle cartelle.
- [x] Eseguire build e test completi dopo la conversione.
- [x] Verificare che non rimangano dichiarazioni `namespace ...;` nei file sorgente applicativi.

### Perimetro iniziale

La code review ha individuato 32 file con namespace file-scoped distribuiti tra:

- host `MultiPurposeServer`;
- `Portfolio.Api`;
- `Portfolio.Contracts`;
- `Portfolio.Data`;
- `MultiPurposeServer.Shared.Utils`;
- `SampleApp.Mobile`;
- progetti di test Shared.

### Stato

- **Pianificato**
- Rilievo emerso durante la code review della struttura della solution.
- Non bloccante per la prosecuzione della code review.

---

## Integration Test della pipeline MVC

Introdurre una suite di Integration Test dedicata alla pipeline HTTP di Portfolio.Api.

Gli Integration Test dovranno verificare il comportamento completo:

```text
HTTP Request
    ↓
Model Binding
    ↓
Normalizzazione
    ↓
Validazione
    ↓
Controller
    ↓
Exception Filter
    ↓
HTTP Response
```

### Obiettivi

- [x] Verificare che le Request non valide vengano respinte prima dell'esecuzione del Controller.
- [x] Verificare che la normalizzazione venga applicata prima della validazione e dell'invocazione dei Service.
- [x] Verificare la validazione ricorsiva delle Request Bulk.
- [x] Verificare la conversione di ValidationException in una risposta HTTP 400 Bad Request.
- [x] Verificare che i Service non vengano invocati quando la Request non supera la validazione.
- [x] Evitare di duplicare nei test unitari dei Controller comportamenti appartenenti alla pipeline MVC.

### Specifiche derivate dai test unitari rimossi

#### Album

- [x] `Create_WhenNameIsMissing_ReturnsBadRequest`
- [x] `Create_WhenNameContainsOuterSpaces_PassesNormalizedNameToApplication`
- [x] `Update_WhenNoFieldsAreSpecified_ReturnsBadRequest`
- [x] `Update_WhenFieldsContainOuterSpaces_PassesNormalizedValuesToApplication`

#### Foto

- [x] `Update_WhenDescriptionIsMissing_ReturnsBadRequest`
- [x] `Update_WhenDescriptionContainsOuterSpaces_PassesNormalizedDescriptionToApplication`

#### Cache

- [x] `ClearCache_WhenNoCacheIsSelected_ReturnsBadRequest`

#### Bulk Album

- [x] `Update_WhenItemsAreEmpty_ReturnsBadRequest`
- [x] `Update_WhenRequestContainsDuplicateIds_ReturnsBadRequest`
- [x] `Update_WhenItemHasNoFieldsToUpdate_ReturnsBadRequest`
- [x] `Update_WhenValuesContainOuterSpaces_PassesNormalizedValuesToApplication`

#### Bulk Foto

- [x] `Update_WhenItemsAreEmpty_ReturnsBadRequest`
- [x] `Update_WhenRequestContainsDuplicateIds_ReturnsBadRequest`
- [x] `Update_WhenItemHasNoFieldsToUpdate_ReturnsBadRequest`
- [x] `Update_WhenDescriptionContainsOuterSpaces_PassesNormalizedDescriptionToApplication`

### Stato

- **Completato il 13 agosto 2026**
- Suite dedicata introdotta in `Portfolio.Api.IntegrationTests`; durante il collaudo è stata completata anche la regola dichiarativa generica di unicità dei payload bulk.

---

## Allineamento di Portfolio.Web alla Web Page Architecture

Riallineare la pagina Home di `Portfolio.Web` all'architettura descritta nell'[ADR-0012](ADR/ADR-0012-portfolio-web-adopts-page-architecture.md).

### Contesto

La pagina Album rispetta già il flusso architetturale previsto:

```text
Controller
    ↓
Page Service
    ↓
Page Model
    ↓
View
```

La pagina Home contiene invece ancora responsabilità di orchestrazione direttamente nel Controller:

- recupero degli album root tramite `AlbumService`;
- aggiornamento della routing cache tramite `RoutingCacheService`;
- coordinamento di più servizi applicativi;
- preparazione dello stato della pagina.

Questa responsabilità deve essere spostata in un Page Service dedicato, mantenendo il Controller il più sottile possibile.

### Obiettivi

- [ ] Introdurre `HomePageService`.
- [ ] Introdurre il relativo `HomePage` (Page Model).
- [ ] Spostare nel `HomePageService` il recupero degli album root.
- [ ] Spostare nel `HomePageService` l'aggiornamento della routing cache.
- [ ] Demandare al `HomePageService` la costruzione completa del modello della pagina.
- [ ] Ridurre `HomeController` alla sola gestione della richiesta HTTP e della selezione della View.
- [ ] Aggiornare la View affinché utilizzi esclusivamente il `HomePage`.
- [ ] Aggiungere o aggiornare i test di `HomeController` e `HomePageService`.
- [ ] Valutare successivamente l'introduzione di un meccanismo di Dependency Injection anche per Portfolio.Web, eliminando le istanziazioni dirette (`new`) dei Service.

### Vincoli

- Non modificare il comportamento pubblico della pagina Home.
- Non introdurre logica applicativa nella View.
- Non riportare logica applicativa nel Controller.
- Mantenere la pagina Album invariata, salvo eventuali refactoring condivisi.

### Stato

- **Pianificato**
- Da eseguire dopo il completamento della code review del backend.
- Attività derivata dalla verifica di conformità all'[ADR-0012](ADR/ADR-0012-portfolio-web-adopts-page-architecture.md).

## Separazione dei Response DTO FrontEnd e BackEnd

Separare progressivamente i Response DTO utilizzati dalle API FrontEnd da quelli utilizzati dalle API BackEnd.

### Contesto

Attualmente alcune API FrontEnd e BackEnd riutilizzano gli stessi Response DTO.

Questa condivisione è adeguata finché i due contratti espongono le stesse informazioni, ma non deve impedire la loro futura evoluzione indipendente.

Le pagine amministrative richiederanno presumibilmente informazioni aggiuntive rispetto al FrontEnd pubblico, tra cui:

- descrizione completa;
- metadati amministrativi;
- stato del contenuto;
- informazioni tecniche;
- dati necessari alle operazioni di modifica.

L'attuale `PhotoDto` deve rimanere un contratto minimale orientato alla consultazione e non deve essere ampliato automaticamente per soddisfare esigenze esclusivamente amministrative.

### Obiettivi

- [ ] Identificare i Response DTO attualmente condivisi tra API FrontEnd e BackEnd.
- [ ] Definire naming e collocazione dei DTO specifici dei due contesti.
- [ ] Introdurre un Response DTO BackEnd per le fotografie quando saranno richiesti metadati amministrativi aggiuntivi.
- [ ] Includere nel DTO BackEnd la proprietà `Description` quando necessaria alle pagine amministrative.
- [ ] Valutare la separazione analoga per Album e altri contratti condivisi.
- [ ] Mantenere i DTO FrontEnd minimali e orientati alla consultazione.
- [ ] Evitare dipendenze del FrontEnd da informazioni esclusivamente amministrative.
- [ ] Aggiornare Controller, OpenAPI e test contestualmente all'introduzione dei nuovi contratti.
- [ ] Verificare esplicitamente la compatibilità dei client prima di sostituire DTO già pubblicati.

### Vincoli

- Non modificare l'attuale `PhotoDto` esclusivamente per esigenze del BackEnd.
- Non duplicare DTO finché i relativi contratti non divergono realmente.
- Non introdurre suffissi o gerarchie generiche senza una responsabilità concreta.
- Mantenere indipendente l'evoluzione dei contratti FrontEnd e BackEnd.

### Stato

- **Pianificato**
- Da affrontare durante lo sviluppo delle pagine amministrative di Portfolio.
- Decisione emersa durante la code review della sezione Contracts e API.

## Exception Model

**Priorità:** Bassa

Valutare l'introduzione di una gerarchia di eccezioni applicative dedicate qualora emerga la necessità di:

- distinguere semanticamente gli errori di dominio da quelli infrastrutturali;
- tradurre automaticamente le eccezioni in HTTP Problem Details;
- supportare strategie di retry o recovery differenziate;
- condividere contratti di errore tra più moduli.

Attualmente l'utilizzo di `ArgumentException`, `KeyNotFoundException`,
`InvalidOperationException` e `HttpRequestException` è considerato
sufficientemente espressivo e non giustifica un modello dedicato.

## Logging policy

Valutare l'introduzione di una strategia di logging centralizzata tramite middleware globale.

Obiettivo:

- evitare duplicazione dei log;
- registrare una sola volta le eccezioni non gestite;
- mantenere logging locale solo per boundary esterni (filesystem, HTTP, provider esterni, cache, ecc.).

Attualmente il logging è considerato sufficiente per le responsabilità del dominio Portfolio.

## Gestione centralizzata delle eccezioni applicative

**Stato:** Completato il 13 agosto 2026

### Obiettivo

Mantenere i Controller privi di `try/catch` e limitati all'orchestrazione delle operazioni applicative.

### Intervento previsto

- [x] Introdurre un exception filter dedicato per `KeyNotFoundException`.
- [x] Tradurre `KeyNotFoundException` in `404 Not Found`.
- [x] Registrare il filtro nella pipeline MVC.
- [x] Rimuovere i `try/catch (KeyNotFoundException)` dagli endpoint puntuali; i bulk mantengono la gestione locale per produrre warning per item.
- [x] Aggiungere Integration Test sulla traduzione centralizzata in `404`.
- [x] Aggiornare i Controller Test affinché verifichino soltanto l'orchestrazione e non il comportamento della pipeline MVC.

### Vincolo

I Controller devono contenere soltanto la decisione su quali campi aggiornare e quali operazioni applicative invocare.

La traduzione delle eccezioni in risposte HTTP appartiene alla pipeline MVC.

## Logging dei Controller

**Stato:** Remaining

Valutare il ruolo del logging nei Controller.

Decisioni da prendere:

- [ ] Stabilire se i Controller debbano produrre log applicativi o demandare completamente il logging alla pipeline.
- [ ] Se il logging rimane nei Controller, definire gli eventi che meritano realmente un log.
- [ ] Valutare se la gerarchia dei Controller debba utilizzare `ILogger<TController>` per mantenere categorie specifiche invece di una categoria comune.
- [ ] Eliminare il warning `CS9113` in modo coerente con la decisione architetturale adottata.

## Documentazione XML delle API pubbliche

**Stato:** Remaining

La generazione della documentazione XML è temporaneamente disabilitata perché la maggior parte dei tipi e membri pubblici non è ancora documentata.

- [ ] Consolidare prima la documentazione architetturale, di dominio e le convenzioni del Playbook.
- [ ] Aggiungere la documentazione XML ai tipi e membri pubblici.
- [ ] Riabilitare `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.
- [ ] Verificare che la build non produca warning `CS1591`.

---

# 9. Future Improvements

Idee tecniche già emerse ma non ancora pianificate.

- [ ] Ottimizzazione ulteriore dei ValidationPlan
- [ ] Ottimizzazione ulteriore dei NormalizationPlan
- [ ] Analisi di eventuali Source Generator per ridurre ulteriormente la reflection
- [ ] Estensione del framework di Validation a nuovi attributi condivisi

---

# 10. Current Milestone

## Milestone

MVC Request Pipeline

### Stato

🚧 In Progress

### Completato

- Normalizzazione automatica
- Validazione automatica
- ValidationExceptionFilter
- Consolidamento dei test unitari di Portfolio.Api
- Rimozione dai Controller Test dei casi appartenenti alla pipeline MVC

### Prossimo passo

- Refactoring dei controller Bulk
- Eliminazione della normalizzazione e validazione manuale residue
- Progettazione degli Integration Test della pipeline MVC

---

# 11. Session Notes

Questa sezione viene aggiornata al termine di ogni sessione di sviluppo per facilitare la ripresa del lavoro.

## Ultimo aggiornamento

- Introduzione della Request Pipeline condivisa.
- Introduzione di IRequest come contratto comune.
- Introduzione di IBulk<TItem> e BulkRequest<TItem>.
- Introduzione di RequiredAtLeastOneTrue.
- Introduzione di ValidationExceptionFilter.
- Introduzione del bypass dell'autenticazione in ambiente Development.
- Migrazione della validazione del CacheController nel Validation Framework.
- Consolidamento dei progetti di test di Portfolio.Api.
- Riallineamento dei Controller Test alla pipeline centralizzata.
- Registrazione delle specifiche per i futuri Integration Test.

---

# 12. Backlog progettuale residuo

I seguenti temi provengono dalla chiusura del documento temporaneo di consolidamento, ora eliminato. Sono direzioni da progettare e non decisioni architetturali già adottate.

## Sicurezza e infrastruttura

- [ ] Definire meccanismi concreti per confidential e public client.
- [ ] Progettare User Authentication, recovery, MFA e step-up quando richiesti da casi reali.
- [ ] Progettare policy, permission evaluator e access scope applicativi.
- [ ] Definire la distribuzione dei media protetti per browser e altri client.
- [ ] Definire formato, retention e failure policy del security audit.
- [ ] Consolidare CORS, CSRF, rate limiting e security headers.
- [ ] Definire protezione a riposo, backup e lifecycle dei dati.
- [ ] Progettare separazione, distribuzione e disaster recovery definitivi dei segreti.
- [ ] Decidere quando l'indisponibilità del security audit richieda fail-closed.

## Shared Framework

- [ ] Definire il contratto opzionale con cui una Request espone una chiave.
- [ ] Valutare i nomi `[Id]` e `[Key]` evitando ambiguità con le convenzioni .NET.
- [ ] Decidere se chiave logica e vincolo di univocità rimangano concetti distinti.
- [ ] Mantenere inizialmente una sola chiave logica per tipo di Request.
- [ ] Progettare la validazione applicativa estensibile senza introdurre astrazioni premature.
- [ ] Implementare le strategie consolidate in `BulkOperations.md`.

## Testing specialistico

- [ ] Progettare test di performance per le Bulk Operation e i payload di grandi dimensioni.
- [ ] Definire load, stress e capacity test per i componenti critici.
- [ ] Progettare test di resilienza per database, filesystem e servizi esterni.
- [ ] Consolidare le convenzioni degli Authorization Boundary Test.
- [ ] Valutare test di compatibilità della rappresentazione pubblica dei Contracts.
- [ ] Definire verifiche di accessibilità per le Application con interfaccia utente.

## Documenti implementativi futuri

- [ ] Consolidare `TestingConventions.md`.
- [ ] Creare una guida alle convenzioni C#, a primary constructor, namespace e sintassi quando necessaria.
- [ ] Creare una guida editoriale e Markdown quando emergeranno convenzioni sufficienti.
- [ ] Definire documentazione specialistica di persistenza, Entity Framework, migration e lazy loading quando richiesta.
- [ ] Definire convenzioni implementative per struttura fisica dei domini, API concrete, interfacce, Dependency Injection e Repository generici quando emergeranno pattern stabili.
- [ ] Creare una checklist operativa per l'introduzione di un nuovo dominio quando verrà avviato il secondo dominio reale.

---

# 13. Vedi anche

- Architecture.md
- SharedFramework.md
- ApiArchitecture.md
- RequestProcessing.md
- BulkOperations.md
- DomainArchitecture.md
- TestingArchitecture.md
- Architecture Decision Records (ADR)
- ../Engineering/CodeReview.md
- ../Engineering/CodeReviewChecklist.md
