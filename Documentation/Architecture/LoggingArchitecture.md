# Logging Architecture

## 1. Scopo

Questo documento definisce la policy autorevole di logging e osservabilità applicativa di MultiPurposeServer.

Il logging supporta diagnosi, esercizio e correlazione delle operazioni. Non controlla il flusso applicativo, non sostituisce i contratti di errore e rimane distinto dai futuri audit di sicurezza o di business.

---

## 2. Responsabilità

L'host configura il provider, i sink, il formato, la retention e il routing fisico. Serilog è il provider corrente, ma rimane un dettaglio dell'host.

Lo Shared Framework definisce la semantica comune, il contesto strutturato, la correlazione e la diagnostica dinamica tramite il progetto autonomo `MultiPurposeServer.Shared.Logging`.

Ogni dominio:

- definisce il proprio catalogo stabile di eventi;
- configura e controlla in modo indipendente il proprio stato diagnostico;
- espone eventuali endpoint amministrativi del dominio;
- registra gli eventi applicativi che appartengono alla propria responsabilità.

I Controller non producono log ordinari. Restano orchestratori HTTP e possono registrare un evento soltanto quando rappresentano concretamente il confine che lo gestisce e la motivazione è esplicita. Le eccezioni non gestite appartengono alla pipeline globale.

Un componente registra un'eccezione quando la assorbe, applica un fallback o completa localmente il recupero. Registrare e rilanciare la stessa eccezione è normalmente vietato. L'eccezione non gestita viene registrata una sola volta dal boundary globale.

---

## 3. Servizio condiviso

Il logging condiviso risiede in una DLL dedicata:

```text
Shared/
└── MultiPurposeServer.Shared.Logging/
    ├── Abstractions/
    ├── Models/
    └── Services/
```

I namespace iniziano da `MultiPurposeServer.Shared.Logging`.

Il progetto espone `ILoggerService<T>` e la relativa implementazione `LoggerService<T>`. L'implementazione utilizza `Microsoft.Extensions.Logging.ILogger<T>` e non dipende direttamente da Serilog. La dipendenza pubblica prevista è `Microsoft.Extensions.Logging.Abstractions`.

La separazione fisica è intenzionale: il logging è un servizio infrastrutturale autonomo. `MultiPurposeServer.Shared.Utils` rimane riservato a helper e trasformazioni generiche.

I tipi iniziali previsti comprendono:

- `ILoggerService<T>`;
- `LoggerService<T>`;
- `DiagnosticMode`;
- un registro dello stato diagnostico per dominio;
- un accessore al contesto corrente;
- `LogEventId` come identificatore tecnico stabile.

---

## 4. Livelli semantici

- `Trace`: dettaglio diagnostico molto granulare.
- `Debug`: condizione prevista, temporanea o utile alla diagnosi.
- `Information`: transizione applicativa o operativa significativa.
- `Warning`: anomalia gestita, fallback o funzionamento degradato.
- `Error`: operazione tecnica fallita o errore inatteso assorbito.
- `Critical`: host o dominio non utilizzabile.

Una condizione prevista dell'utente o del contratto non diventa `Warning` o `Error` soltanto perché produce una risposta non positiva. Le eccezioni applicative previste appartengono normalmente a `Debug`.

Il livello rappresenta la semantica originale dell'evento. La diagnostica dinamica può modificarne il livello effettivo di emissione senza perderne l'origine.

---

## 5. Eventi strutturati

Gli eventi usano proprietà strutturate e un identificatore testuale, stabile e gerarchico, per esempio:

```text
Portfolio.Bulk.ItemUnexpectedFailure
```

Ogni dominio conserva il proprio catalogo di eventi. I messaggi destinati alla lettura umana sono in italiano; nomi delle proprietà ed Event ID rimangono tecnici e in inglese.

I campi comuni sono:

- `Timestamp`;
- `Level`;
- `OriginalLevel`, quando differisce dal livello emesso;
- `Domain`;
- `SourceContext`;
- `EventId`;
- `Message`;
- `Exception`, quando presente;
- `CorrelationId`;
- `RequestId`;
- `Origin`.

Metodo, path, status code, durata e identificatori delle entità vengono aggiunti soltanto quando pertinenti.

Non devono essere registrati password, token, API Key, segreti, body completi, query string non filtrate, percorsi fisici non necessari o dati personali privi di una motivazione esplicita.

---

## 6. Destinazioni e formato

I file sono separati per dominio:

```text
logs/host/mps-yyyyMMdd.log
logs/portfolio/portfolio-yyyyMMdd.log
logs/sample-app/sample-app-yyyyMMdd.log
```

Un evento viene scritto in una sola destinazione. Un errore del sink di dominio non deve riversare l'evento nel file dell'host; il fallback ammesso è la console o il canale diagnostico interno del provider.

I file usano JSON Lines. La console mantiene un formato leggibile per l'operatore.

Rotazione giornaliera, limite dimensionale e numero di file conservati sono configurabili indipendentemente per destinazione.

Il `Domain` non viene dedotto esclusivamente dal namespace: la pipeline HTTP lo determina dalla route, mentre gli entry point non HTTP impostano esplicitamente il contesto.

---

## 7. Correlazione

Il client può inviare `X-Correlation-ID` per un workflow significativo e riutilizzarlo fra più chiamate. Non è obbligato a farlo per la normale navigazione.

Il server:

- accetta un identificatore valido ricevuto dal client;
- ne genera uno quando manca o non è valido;
- non rifiuta la richiesta per un identificatore assente o errato;
- restituisce sempre l'identificatore effettivo in `X-Correlation-ID`.

`CorrelationId` identifica l'operazione logica; `RequestId` distingue le singole richieste che la compongono.

L'eventuale ingestione futura dei log dei client è una feature separata. Richiederà autenticazione, rate limiting, batching, validazione e protezione dalla ricorsione.

---

## 8. Diagnostica dinamica

Il livello minimo del provider rimane `Information`. La diagnostica non riconfigura il provider: `LoggerService<T>` promuove gli eventi diagnostici a `Information`, conservando `OriginalLevel`, `IsDiagnostic` e `DiagnosticMode`.

Le modalità sono:

- `Off`: `Debug` e `Trace` non vengono emessi;
- `Diagnostic`: `Debug` viene promosso a `Information`, `Trace` non viene emesso;
- `Verbose`: `Debug` e `Trace` vengono promossi a `Information`.

Lo stato è indipendente per dominio, risiede in memoria, scade automaticamente e torna `Off` al riavvio. La durata massima è configurabile; `Verbose` deve avere una durata massima più breve.

Ogni dominio può esporre nel proprio BackEnd operazioni protette per:

- leggere lo stato;
- abilitare o aggiornare una modalità;
- disabilitarla.

Attivazione, disattivazione e scadenza producono eventi `Information`. Non viene introdotto inizialmente un endpoint globale per tutti i domini.

---

## 9. Logging HTTP

Il middleware di request logging è sempre installato e opera indipendentemente dalla diagnostica applicativa.

- richiesta conclusa normalmente: `Debug`;
- richiesta lenta oltre una soglia configurabile: `Warning`;
- risposta `5xx`: `Error`.

Il logging esteso delle richieste può essere abilitato a runtime con un'opzione distinta dalla modalità diagnostica. Header sensibili e body non vengono registrati.

La pipeline globale registra le eccezioni non gestite una sola volta come `Error` e restituisce una risposta `ProblemDetails` uniforme contenente il `CorrelationId`. Un fallimento di bootstrap viene registrato come `Critical` dall'host prima del flush del logger.

---

## 10. Criteri applicativi

- Le operazioni Bulk producono un riepilogo `Information`.
- Gli errori previsti dei singoli item non producono `Warning`.
- Un errore tecnico inatteso assorbito da `PartialSuccess` produce `Error`.
- Un errore rilanciato da `AllOrNothing` viene registrato dal boundary globale.
- Le sincronizzazioni producono un riepilogo `Information` o `Warning`; il report dedicato conserva il dettaglio.
- L'attivazione di un fallback produce `Warning`, possibilmente con limitazione degli eventi ripetitivi.
- Gli eventi applicativi sintetici, come la creazione effettiva di un Album, producono `Information`.

I futuri audit di sicurezza o di business avranno policy, retention e failure mode propri. Non vengono simulati tramite normali eventi diagnostici.

---

## 11. Resilienza e health

Un fallimento del logging diagnostico non deve far fallire l'operazione applicativa. Il servizio tenta un fallback sicuro verso console o diagnostica interna e porta il proprio stato di salute a `Degraded`.

Un health check può esporre uno stato sintetico e non sensibile per dominio. Il rilevamento del degrado non deve generare ricorsivamente altri errori sul sink guasto.

Un futuro audit con requisiti fail-closed costituisce una decisione architetturale distinta.

---

## 12. Verifica

L'implementazione deve verificare almeno:

- selezione del livello semantico;
- promozione nelle modalità diagnostiche;
- isolamento dello stato fra domini;
- scadenza automatica;
- generazione, accettazione e propagazione della correlazione;
- routing verso una sola destinazione;
- registrazione singola delle eccezioni non gestite;
- assenza di informazioni sensibili nei casi coperti;
- comportamento non bloccante in caso di sink indisponibile.

---

## Vedi anche

- [Architecture](Architecture.md)
- [Shared Framework](SharedFramework.md)
- [Infrastructure Architecture](InfrastructureArchitecture.md)
- [API Architecture](ApiArchitecture.md)
- [Security Architecture](SecurityArchitecture.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
