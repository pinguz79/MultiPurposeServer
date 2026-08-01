# ADR-0006 — `IRequest` utilizza implementazioni predefinite per `Normalize()` e `Validate()`

## Stato

Accettato

---

## Contesto

Tutte le Request di MultiPurposeServer devono esporre le operazioni comuni di normalizzazione e validazione.

Le possibili soluzioni considerate comprendevano:

- ripetere i metodi in ogni Request;
- introdurre una classe base comune;
- mantenere soltanto metodi di estensione;
- utilizzare implementazioni predefinite nell'interfaccia `IRequest`.

La duplicazione nei DTO avrebbe prodotto codice ripetitivo.

Una classe base avrebbe introdotto una gerarchia artificiale, avrebbe occupato l'unico slot di ereditarietà disponibile e avrebbe creato incompatibilità tra classi e record.

I soli metodi di estensione, invece, non costituiscono un contratto esplicito utilizzabile dalla pipeline MVC.

---

## Decisione

`IRequest` espone `Normalize()` e `Validate()` tramite default interface implementation.

Le implementazioni predefinite invocano esplicitamente i componenti condivisi responsabili delle due operazioni.

```csharp
public interface IRequest
{
    void Normalize() => NormalizationExtensions.Normalize(this);

    void Validate() => ValidationExtensions.Validate(this);
}
```

Le chiamate devono essere effettuate tramite il nome statico della classe che contiene le extension.

Non è ammessa una forma ricorsiva come:

```csharp
void Normalize() => this.Normalize();
```

Il metodo d'istanza dell'interfaccia avrebbe infatti precedenza sull'extension method e provocherebbe una ricorsione infinita.

Le Request concrete devono soltanto implementare `IRequest`, senza duplicare i metodi e senza derivare da una classe base.

---

## Conseguenze

### Vantaggi

- Il contratto delle Request è esplicito.
- La pipeline può elaborare uniformemente ogni `IRequest`.
- Non è necessaria una classe base comune.
- Classi e record possono implementare lo stesso contratto.
- Le implementazioni comuni rimangono centralizzate.
- Le Request concrete non contengono boilerplate.
- Il comportamento è riutilizzabile anche al di fuori della pipeline MVC.

### Costi

- Le default interface implementation rappresentano comportamento reale e devono essere protette da test dedicati.
- Una chiamata non qualificata all'interno dell'interfaccia può risolversi sul metodo stesso e provocare una ricorsione infinita.
- Un errore di ricorsione può terminare il processo di test con `StackOverflowException` anziché produrre un normale test fallito.
- `IRequest` non è una semplice marker interface, ma un contratto dotato di comportamento predefinito.

---

## Vedi anche

- `Architecture.md`
- `SharedFramework.md`
- `InfrastructureArchitecture.md`
- `TestingArchitecture.md`
- `MpsPlaybook.md`
