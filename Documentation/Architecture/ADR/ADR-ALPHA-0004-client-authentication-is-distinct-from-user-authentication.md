# ADR-ALPHA-0004 — L'autenticazione del client è distinta dall'autenticazione dell'utente

## Stato

Accettato

---

## Contesto

MultiPurposeServer supporta applicazioni differenti, come Portfolio.Web, future applicazioni Mobile, Desktop e altri client autorizzati.

Per poter accedere alle API è necessario identificare innanzitutto l'applicazione che effettua la richiesta.

Questa autenticazione, tuttavia, non identifica la persona che utilizza l'applicazione.

Con l'evoluzione del progetto, domini come ModelBook richiederanno autenticazione utente, ruoli e permessi.

Era quindi necessario distinguere chiaramente queste due responsabilità fin dall'architettura.

---

## Decisione

L'autenticazione del client e l'autenticazione dell'utente rappresentano due livelli distinti del modello di sicurezza.

Il flusso architetturale è il seguente:

```text
Client Authentication
        ↓
User Authentication
        ↓
Authorization
```

La **Client Authentication** risponde alla domanda:

> Quale applicazione sta chiamando il server?

La **User Authentication** risponde invece alla domanda:

> Quale persona sta utilizzando l'applicazione?

L'identità del client e quella dell'utente devono rimanere completamente indipendenti.

Un client può autenticarsi anche in assenza di un utente autenticato.

In futuro un utente potrà autenticarsi esclusivamente attraverso un client autorizzato.

Le attuali distinzioni tra FrontEnd e BackEnd rappresentano livelli di accesso del client applicativo e non ruoli dell'utente finale.

L'autorizzazione costituisce una responsabilità separata e viene valutata esclusivamente dopo l'identificazione del client e dell'eventuale utente.

---

## Conseguenze

### Vantaggi

- È possibile revocare o limitare un client senza modificare gli account utente.
- L'identità dell'applicazione e quella della persona rimangono chiaramente distinte.
- Il modello di sicurezza può evolvere introducendo autenticazione utente senza modificare il significato dell'autenticazione del client.
- Client Web, Mobile e Desktop possono possedere privilegi applicativi differenti.
- L'autorizzazione può evolvere verso un modello permission-based senza ridefinire il ruolo delle API Key o delle credenziali del client.

### Costi

- Le richieste future potranno richiedere due meccanismi distinti di autenticazione.
- Configurazione, policy e test di sicurezza risultano più articolati.
- Il modello di sicurezza introduce un livello concettuale aggiuntivo che deve essere compreso durante lo sviluppo.

---

## Vedi anche

- `Architecture.md`
- `SecurityArchitecture.md`
- `InfrastructureArchitecture.md`
- `MpsPlaybook.md`
