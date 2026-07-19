# ADR-0004 — L'autenticazione del client è distinta dall'autenticazione dell'utente

## Stato

Accettata

## Contesto

Portfolio.Api utilizza API key per identificare il client applicativo che effettua la chiamata, per esempio Portfolio.Web o un futuro client amministrativo.

Questa autenticazione non identifica la persona che utilizza il client.

In futuro ModelBook e altri domini potranno richiedere autenticazione utente, ruoli e permessi.

## Decisione

L'autenticazione del client e l'autenticazione dell'utente sono due livelli distinti.

```text
Client authentication
    ↓
User authentication
    ↓
Authorization
```

L'autenticazione tramite API key risponde alla domanda:

> Quale applicazione sta chiamando il server?

La futura autenticazione utente risponderà alla domanda:

> Quale persona sta utilizzando l'applicazione?

Le distinzioni FrontEnd e BackEnd attuali rappresentano livelli di accesso del client e non ruoli dell'utente finale.

## Conseguenze

### Vantaggi

- È possibile revocare o limitare un client senza modificare gli account utente.
- L'identità dell'applicazione e quella della persona rimangono esplicite.
- La futura autorizzazione permission-based può essere introdotta senza ridefinire il significato delle API key.
- Client Web, Mobile e Desktop possono avere privilegi applicativi differenti.

### Costi

- Le richieste future potranno richiedere due meccanismi di autenticazione.
- Configurazione, policy e test di sicurezza saranno più articolati.
