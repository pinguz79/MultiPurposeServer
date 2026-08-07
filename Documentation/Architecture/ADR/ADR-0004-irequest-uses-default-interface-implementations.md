# ADR-0004 — `IRequest` espone `Normalize()` e `Validate()` tramite implementazioni predefinite

## Stato

Accettato

## Ambito

Shared Framework

## Data della decisione

2026-08-06

## Origine

`ADR-ALPHA-0006`.

## Contesto

La pipeline deve applicare normalizzazione e validazione a tutte le Request senza conoscere i motori concreti. Ripetere i metodi nei DTO produrrebbe boilerplate; una classe base imporrebbe una gerarchia artificiale e occuperebbe l'ereditarietà disponibile; i soli extension method non costituirebbero un contratto esplicito della Request.

Era inoltre desiderabile che la pipeline esprimesse semanticamente le operazioni come comportamento della Request senza trasferire ai DTO l'implementazione degli algoritmi.

## Decisione

`IRequest` espone `Normalize()` e `Validate()` tramite default interface implementation.

Le implementazioni predefinite delegano esplicitamente ai motori Shared. La pipeline utilizza quindi:

```csharp
request.Normalize();
request.Validate();
```

anziché dipendere direttamente da `Normalizer` e `Validator`.

Le Request concrete implementano `IRequest`, dichiarano dati e attributi, ma non duplicano i metodi né implementano gli algoritmi.

La delega interna deve essere qualificata esplicitamente per evitare che il metodo d'istanza richiami ricorsivamente se stesso al posto dell'extension method.

## Conseguenze

### Positive

- La pipeline utilizza un contratto uniforme e semanticamente leggibile.
- I motori rimangono nascosti dietro la superficie pubblica di `IRequest`.
- Classi e record condividono il comportamento senza una classe base.
- Le Request concrete non contengono boilerplate.
- Le operazioni possono essere invocate anche fuori dalla pipeline MVC.

### Negative

- `IRequest` non è una semplice marker interface.
- Le implementazioni predefinite costituiscono comportamento da proteggere con test.
- Una delega non qualificata può provocare ricorsione infinita e `StackOverflowException`.

## Alternative considerate

### Metodi duplicati in ogni Request

Scartati per boilerplate e rischio di implementazioni incoerenti.

### Classe base comune

Scartata perché introdurrebbe una gerarchia artificiale e limiterebbe l'ereditarietà.

### Chiamata diretta ai motori dalla pipeline

Scartata perché renderebbe la pipeline dipendente dalle implementazioni concrete e meno leggibile semanticamente.

## Riferimenti

- [Shared Framework](../SharedFramework.md)
- [ADR-0003](ADR-0003-request-processing-is-centralized-in-the-mvc-pipeline.md)
- [ADR-0005](ADR-0005-normalization-and-validation-are-declarative.md)

