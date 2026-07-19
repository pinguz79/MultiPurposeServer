# ADR-0002 — Ogni dominio registra le proprie dipendenze e possiede il proprio database

## Stato

Accettata

## Contesto

MultiPurposeServer è un host modulare per domini indipendenti come Portfolio, ModelBook e Skating.

L'host deve comporre l'applicazione senza conoscere i dettagli interni di ciascun dominio. Una registrazione centralizzata di Repository, Service, DbContext, Authentication e Options renderebbe l'host dipendente dalle implementazioni dei moduli.

Un database condiviso introdurrebbe inoltre accoppiamenti strutturali e limiterebbe l'evoluzione autonoma dei domini.

## Decisione

Ogni dominio espone un unico punto di registrazione tramite un'extension dedicata.

```csharp
builder.Services.AddPortfolio(configuration);
```

L'extension del dominio registra autonomamente:

- DbContext;
- Repository;
- Service;
- Authentication e Authorization specifiche;
- HttpClient;
- Options;
- ogni altra dipendenza interna.

Ogni dominio possiede il proprio DbContext, le proprie migration e il proprio database o schema di persistenza.

L'host non mantiene un database condiviso implicito tra domini.

Il file `Program.cs` rimane un compositore leggibile formato principalmente da chiamate `Add...` e `Use...`.

## Conseguenze

### Vantaggi

- I domini possono evolvere e migrare in modo indipendente.
- L'host rimane semplice e privo di logica specifica.
- L'aggiunta di un nuovo dominio richiede modifiche minime alla composizione.
- I confini tra moduli sono più chiari.

### Costi

- Alcune configurazioni comuni possono essere ripetute.
- I dati realmente trasversali richiederanno un modulo esplicito o una responsabilità dell'host.
- Le operazioni multi-dominio non possono affidarsi implicitamente a una singola transazione database.
