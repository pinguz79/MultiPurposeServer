# Roadmap Architetturale di MultiPurposeServer

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

- [ ] Definire un ordinamento coerente dei membri all'interno delle classi.
- [ ] Raggruppare i membri per responsabilità.
- [ ] Introdurre sezioni `#region` significative quando migliorano realmente la navigazione del codice.
- [ ] Separare chiaramente i metodi di test dai metodi di supporto.
- [ ] Raggruppare helper, fixture, factory, dati di test e tipi annidati in sezioni dedicate.
- [ ] Applicare la convenzione in modo uniforme a tutta la solution.
- [ ] Evitare di introdurre formattazioni e ritorni a capo non necessari durante refactoring non stilistici.

### Stato

- **Pianificato**
- Da eseguire dopo il consolidamento dei progetti di test.

> **Nota**
>
> Prima di formalizzare la convenzione nel `MpsPlaybook.md`, essa dovrà essere applicata e verificata su un numero significativo di classi, in particolare nei progetti di test, così da validarne l'efficacia e l'usabilità.

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

- [ ] Verificare che le Request non valide vengano respinte prima dell'esecuzione del Controller.
- [ ] Verificare che la normalizzazione venga applicata prima della validazione e dell'invocazione dei Service.
- [ ] Verificare la validazione ricorsiva delle Request Bulk.
- [ ] Verificare la conversione di ValidationException in una risposta HTTP 400 Bad Request.
- [ ] Verificare che i Service non vengano invocati quando la Request non supera la validazione.
- [ ] Evitare di duplicare nei test unitari dei Controller comportamenti appartenenti alla pipeline MVC.

### Specifiche derivate dai test unitari rimossi

#### Album

- [ ] `Create_WhenNameIsMissing_ReturnsBadRequest`
- [ ] `Create_WhenNameContainsOuterSpaces_PassesNormalizedNameToApplication`
- [ ] `Update_WhenNoFieldsAreSpecified_ReturnsBadRequest`
- [ ] `Update_WhenFieldsContainOuterSpaces_PassesNormalizedValuesToApplication`

#### Foto

- [ ] `Update_WhenDescriptionIsMissing_ReturnsBadRequest`
- [ ] `Update_WhenDescriptionContainsOuterSpaces_PassesNormalizedDescriptionToApplication`

#### Cache

- [ ] `ClearCache_WhenNoCacheIsSelected_ReturnsBadRequest`

#### Bulk Album

- [ ] `Update_WhenItemsAreEmpty_ReturnsBadRequest`
- [ ] `Update_WhenRequestContainsDuplicateIds_ReturnsBadRequest`
- [ ] `Update_WhenItemHasNoFieldsToUpdate_ReturnsBadRequest`
- [ ] `Update_WhenValuesContainOuterSpaces_PassesNormalizedValuesToApplication`

#### Bulk Foto

- [ ] `Update_WhenItemsAreEmpty_ReturnsBadRequest`
- [ ] `Update_WhenRequestContainsDuplicateIds_ReturnsBadRequest`
- [ ] `Update_WhenItemHasNoFieldsToUpdate_ReturnsBadRequest`
- [ ] `Update_WhenDescriptionContainsOuterSpaces_PassesNormalizedDescriptionToApplication`

### Stato

- **Pianificato**
- Da affrontare dopo il consolidamento e la pulizia dei test unitari di Portfolio.Api.

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

# 12. Vedi anche

- Architecture.md
- SharedFramework.md
- DomainArchitecture.md
- TestingArchitecture.md
- Architecture Decision Records (ADR)
- ../Engineering/CodeReview.md
- ../Engineering/CodeReviewChecklist.md
