# ADR-0006 — `IRequest` fornisce implementazioni predefinite per `Normalize()` e `Validate()`

## Stato

Accettato

---

## Contesto

Tutte le Request di MultiPurposeServer partecipano alla pipeline condivisa di normalizzazione e validazione.

Nella versione iniziale del framework ogni Request era obbligata a implementare esplicitamente i metodi:

```csharp
Normalize();

Validate();
```

Anche quando una Request non richiedeva alcuna logica specifica.

Questo approccio introduceva una notevole quantità di codice ripetitivo costituito esclusivamente da implementazioni vuote.

La presenza di tali implementazioni non aggiungeva alcun valore funzionale e rendeva meno leggibile il codice dei Contracts.

---

## Decisione

L'interfaccia `IRequest` fornisce implementazioni predefinite dei metodi:

```csharp
Normalize();

Validate();
```

Entrambi i metodi hanno, per impostazione predefinita, un comportamento nullo.

Le Request implementano questi metodi esclusivamente quando è realmente necessario introdurre logica di normalizzazione o validazione personalizzata.

Il comportamento della pipeline rimane invariato.

L'Action Filter continua infatti a invocare sempre:

1. `Normalize()`
2. `Validate()`

indipendentemente dal fatto che la Request ne fornisca un'implementazione personalizzata oppure utilizzi quella predefinita.

---

## Conseguenze

### Vantaggi

- Le Request semplici non devono più contenere implementazioni vuote.
- I Contracts risultano più compatti e leggibili.
- La pipeline mantiene un comportamento uniforme per tutte le Request.
- Le Request implementano esclusivamente il comportamento realmente necessario.
- La quantità di codice boilerplate viene significativamente ridotta.

### Costi

- Il comportamento predefinito dell'interfaccia deve essere compreso dagli sviluppatori.
- La presenza di implementazioni di default richiede il supporto delle funzionalità del linguaggio utilizzate dal progetto.
- È necessario ricordare che l'assenza di override implica l'utilizzo del comportamento predefinito e non la mancata esecuzione della pipeline.

---

## Vedi anche

- `Architecture.md`
- `SharedFramework.md`
- `InfrastructureArchitecture.md`
- `TestingArchitecture.md`
- `MpsPlaybook.md`