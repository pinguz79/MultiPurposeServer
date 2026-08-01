# Roadmap Architetturale di MultiPurposeServer

## 1. Scopo del documento

Questo documento descrive lo stato di avanzamento dell'architettura di MultiPurposeServer.

A differenza della Roadmap del progetto, che descrive la visione di lungo periodo e l'evoluzione funzionale della piattaforma, questo documento segue esclusivamente l'evoluzione tecnica del framework condiviso e dell'architettura applicativa.

Le decisioni già adottate vengono invece documentate negli Architecture Decision Records (ADR), mentre i concetti emergenti dei singoli domini appartengono alla rispettiva documentazione di dominio.

Le attività presenti in questo documento rappresentano milestone architetturali e non attività operative.
Questo documento descrive esclusivamente evoluzioni tecniche già individuate.

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
- [ ] Gestione centralizzata delle eccezioni applicative (es. KeyNotFoundException)

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

- [ ] Valutare la fattorizzazione tra RequiredAtLeastOne e RequiredAtLeastOneTrue

## Bulk

- [ ] Revisione completa della pipeline Bulk

## Tests

- [ ] Test end-to-end della pipeline MVC

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

### Prossimo passo

- Test di integrazione della pipeline MVC
- Refactoring dei controller Bulk

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
- Migrazione della validazione del CacheController nel Validation Framework.---

# 12. Vedi anche

- Architecture.md
- SharedFramework.md
- DomainArchitecture.md
- Architecture Decision Records (ADR)
