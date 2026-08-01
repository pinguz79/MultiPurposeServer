# ADR-0005 — La normalizzazione e la validazione delle Request sono centralizzate nella pipeline MVC

## Stato

Accettato

---

## Contesto

Le API di MultiPurposeServer ricevono Request che devono essere normalizzate e validate prima dell'esecuzione della logica applicativa.

Nelle prime versioni del progetto ogni Controller invocava manualmente:

```csharp
request.Normalize();
request.Validate();
```

Questo approccio introduceva codice ripetitivo in ogni endpoint e rendeva possibile:

- dimenticare una delle due operazioni;
- eseguirle nell'ordine sbagliato;
- ottenere comportamenti differenti tra Controller.

La normalizzazione e la validazione rappresentano comportamenti infrastrutturali condivisi.

Non appartengono alla logica specifica dei Controller.

---

## Decisione

La normalizzazione e la validazione delle Request vengono eseguite automaticamente dalla pipeline MVC prima dell'invocazione del Controller.

Il flusso architetturale diventa:

```text
Authentication
        ↓
Authorization
        ↓
Model Binding
        ↓
Normalize
        ↓
Validate
        ↓
Controller
        ↓
Application Service
```

Un Action Filter individua automaticamente gli argomenti che implementano `IRequest` ed esegue:

1. `Normalize()`
2. `Validate()`

nell'ordine previsto.

I Controller non devono invocare manualmente queste operazioni.

Le `ValidationException` vengono intercettate da un componente dedicato dell'infrastruttura e convertite in risposte HTTP `400 Bad Request` contenenti gli errori strutturati.

---

## Conseguenze

### Vantaggi

- La normalizzazione e la validazione vengono applicate in modo uniforme a tutte le Request.
- L'ordine `Normalize()` seguito da `Validate()` è garantito centralmente.
- I Controller rimangono focalizzati esclusivamente sull'orchestrazione HTTP.
- Viene eliminato codice ripetitivo dagli endpoint.
- Una Request non valida non raggiunge il layer applicativo.
- La traduzione degli errori di validazione in risposte HTTP è uniforme per tutte le API.
- Il comportamento della pipeline può essere verificato indipendentemente dai singoli Controller.

### Costi

- Il comportamento effettivo di un endpoint dipende anche dai componenti registrati nella pipeline MVC.
- I test unitari che invocano direttamente un Controller non attraversano automaticamente la pipeline.
- Sono necessari Integration Test per verificare l'interazione tra Model Binding, Action Filter, Controller e risposta HTTP.
- La gestione centralizzata della validazione richiede un'infrastruttura dedicata per la traduzione delle eccezioni.

---

## Vedi anche

- `Architecture.md`
- `InfrastructureArchitecture.md`
- `SharedFramework.md`
- `TestingArchitecture.md`
- `MpsPlaybook.md`