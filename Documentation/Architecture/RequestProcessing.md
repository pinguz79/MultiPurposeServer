# Elaborazione delle Request

> **Stato: Stable 1.0 — autorevole.**

## 1. Scopo

Questo documento approfondisce i Contracts Shared e la sequenza tecnica con cui una Request viene normalizzata, validata e consegnata al Controller.

Lo Shared Framework definisce i meccanismi. I domini dichiarano regole tecniche e conservano la semantica applicativa.

---

## 2. IRequest

`IRequest` identifica una richiesta che partecipa alle convenzioni condivise di MultiPurposeServer.

Una Request concreta:

- espone i dati del contratto;
- dichiara tramite attributi le regole tecniche applicabili;
- non implementa gli algoritmi Shared;
- non contiene business logic o persistenza.

`IRequest` espone `Normalize()` e `Validate()` tramite implementazioni predefinite che fungono da façade semantica verso i motori Shared.

```csharp
request.Normalize();
request.Validate();
```

La pipeline dipende dalla superficie della Request senza esporre direttamente i motori. Le Request concrete non assumono la responsabilità degli algoritmi.

---

## 3. Pipeline

La sequenza canonica è:

```text
Model Binding
    ↓
Normalizzazione
    ↓
Validazione tecnica
    ↓
Controller
    ↓
Validazione e comportamento applicativi
```

Request non deserializzabili o tecnicamente invalide non raggiungono il Controller. La pipeline aggrega e traduce gli errori secondo la tassonomia tecnica condivisa.

Il Controller e i Service continuano a gestire regole di dominio, autorizzazioni contestuali e violazioni che richiedono accesso a persistenza o risorse esterne.

---

## 4. Normalizzazione

La normalizzazione porta i dati in una rappresentazione tecnica canonica senza modificarne il significato.

Deve essere:

- deterministica;
- per quanto possibile idempotente;
- applicata prima della validazione;
- dichiarata dal Contract;
- implementata dal motore Shared.

Regole, ricorsione, piani per tipo e cache dei piani appartengono al servizio di normalizzazione e non alle Request concrete.

Una normalizzazione custom del dominio rimane una possibile estensione futura, non una capacità pianificata né un contratto da anticipare.

---

## 5. Validazione tecnica

La validazione canonica verifica regole generiche dichiarate dal Contract, per esempio:

- valori obbligatori;
- almeno un campo valorizzato;
- struttura di oggetti e collezioni;
- univocità tecnica di una lista;
- altre regole indipendenti dal significato del dominio.

Il motore Shared conosce come applicare la regola. La Request dichiara quali proprietà e regole partecipano alla validazione.

Gli errori possono essere aggregati per permettere al client di correggere più problemi nello stesso ciclo.

---

## 6. Validazione applicativa

Regole che richiedono business logic, autorizzazione, stato persistito o servizi esterni non appartengono agli attributi canonici.

Esempi:

- un child modificabile soltanto quando il parent è in uno stato specifico;
- autorizzazione dell'utente sull'oggetto richiesto;
- violazione di foreign key o di vincoli del database.

È pianificata una futura estensione con cui i domini potranno aggiungere validazione applicativa orchestrata dal framework. Contratti come `IValidatable` o validatori astratti non vengono introdotti prima che emerga una progettazione concreta.

---

## 7. Capacità opzionali

Normalizzazione e validazione canonica appartengono al normale ciclo di vita della quasi totalità delle Request e non richiedono marker dedicati.

Capacità meno comuni possono essere rappresentate tramite contratti opzionali separati:

- ordinabilità intrinseca;
- esposizione di una chiave di identificazione;
- altre capacità con un consumatore programmatico reale.

L'ordinabilità è definita dalla Request o dal DTO: il framework conosce che l'item è ordinabile, ma non la semantica usata per ordinarlo.

Il contratto delle chiavi non è ancora definito. Prima dell'introduzione devono essere risolte nomenclatura di `[Id]` e `[Key]`, relazione fra chiave logica e vincolo di univocità e compatibilità con le convenzioni .NET.

---

## 8. Stato delle capacità

### Attuali

- contratto `IRequest`;
- normalizzazione e validazione dichiarative;
- motori Shared;
- esecuzione automatica nella pipeline MVC;
- ricorsione su oggetti e collezioni;
- costruzione e riuso di piani per tipo.

### Pianificate

- validazione applicativa estensibile implementata dai domini;
- contratti opzionali per ordinabilità e chiavi, quando progettati;
- capacità bulk descritte nel documento specialistico.

### Soltanto possibili

- normalizzazione custom di dominio;
- ulteriori capacità opzionali non ancora emerse da casi reali.

---

## 9. Riferimenti

- [Shared Framework](SharedFramework.md)
- [API Architecture](ApiArchitecture.md)
- [Bulk Operations](BulkOperations.md)
- [Testing Architecture](TestingArchitecture.md)
- [ADR-0003 — L'elaborazione delle Request è centralizzata nella pipeline MVC](ADR/ADR-0003-request-processing-is-centralized-in-the-mvc-pipeline.md)
- [ADR-0004 — IRequest espone Normalize e Validate](ADR/ADR-0004-irequest-uses-default-interface-implementations.md)
- [ADR-0005 — Normalizzazione e validazione sono dichiarative](ADR/ADR-0005-normalization-and-validation-are-declarative.md)
