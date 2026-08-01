# ADR-0002 — Ogni dominio registra autonomamente le proprie dipendenze e possiede il proprio database

## Stato

Accettato

---

## Contesto

MultiPurposeServer è progettato come una piattaforma composta da domini indipendenti, come Portfolio, ModelBook e Skating.

L'host deve limitarsi a comporre l'applicazione senza conoscere i dettagli implementativi dei singoli domini.

Una registrazione centralizzata di Repository, Service, DbContext, Authentication, Authorization e Options renderebbe il progetto `MultiPurposeServer` progressivamente dipendente dall'implementazione interna dei moduli.

Anche un database condiviso introdurrebbe un forte accoppiamento tra domini, limitandone l'evoluzione indipendente e rendendo più complesse migrazioni, deployment e manutenzione.

---

## Decisione

Ogni dominio espone un unico punto di ingresso per la registrazione delle proprie dipendenze tramite un'extension dedicata.

Ad esempio:

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

Ogni dominio possiede inoltre:

- il proprio DbContext;
- le proprie migration;
- il proprio database oppure il proprio schema di persistenza.

L'host non mantiene un database condiviso implicito tra domini.

Il file `Program.cs` rimane un semplice **Composition Root**, composto prevalentemente da chiamate `Add...` e `Use...`, senza conoscere i dettagli implementativi dei moduli registrati.

---

## Conseguenze

### Vantaggi

- I domini possono evolvere indipendentemente gli uni dagli altri.
- L'host rimane semplice e privo di logica specifica dei domini.
- L'aggiunta di un nuovo dominio richiede modifiche minime alla composizione dell'applicazione.
- I confini architetturali tra i moduli risultano chiaramente definiti.
- Ogni dominio può evolvere il proprio modello dati senza influenzare gli altri.

### Costi

- Alcune configurazioni comuni possono essere ripetute in domini differenti.
- I dati realmente trasversali richiedono un modulo condiviso oppure una responsabilità esplicita dell'host.
- Le operazioni che coinvolgono più domini non possono fare affidamento implicitamente su una singola transazione database.

---

## Vedi anche

- `Architecture.md`
- `DomainArchitecture.md`
- `InfrastructureArchitecture.md`
- `SharedFramework.md`
- `MpsPlaybook.md`