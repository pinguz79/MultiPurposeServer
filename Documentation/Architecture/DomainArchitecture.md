# Architettura dei Domini

## 1. Scopo

Questo documento descrive gli invarianti architetturali comuni ai domini di MultiPurposeServer e il modello di riferimento per organizzarne protocollo pubblico, logica applicativa e persistenza.

Non definisce un template fisico obbligatorio. Cartelle, progetti, assembly, pattern implementativi e tecnologie vengono introdotti soltanto quando rappresentano responsabilità reali e sono approfonditi nei documenti specialistici e negli ADR.

Le scelte nate dall'implementazione di un singolo dominio non diventano automaticamente regole dell'intera piattaforma.

---

## 2. Invarianti di un dominio

Un dominio rappresenta una responsabilità funzionale autonoma della piattaforma.

Ogni dominio possiede:

- protocollo pubblico;
- logica applicativa;
- dati e persistenza;
- configurazione;
- dipendenze;
- regole di sicurezza;
- ciclo evolutivo.

Un dominio deve poter evolvere senza richiedere modifiche invasive agli altri domini e deve poter essere ricomposto in un host dedicato senza modificare la propria logica applicativa.

Non condivide con altri domini implementazioni, Entity persistite, database logici, transazioni, account o regole di business. Un eventuale consumo delle API pubbliche di un altro dominio viene trattato come l'integrazione con un servizio esterno.

L'host compone il dominio esclusivamente attraverso i suoi punti di ingresso pubblici e non ne conosce la struttura interna.

---

## 3. Modulo server del dominio

Il modulo `Domain.Api` rappresenta il componente server componibile del dominio.

Espone le API e i punti di composizione richiesti dall'host, ma non costituisce necessariamente un processo eseguibile e non deve possedere un proprio `Program.cs`.

Nella configurazione corrente MPS ospita i moduli dei domini. In una ricomposizione dedicata, un nuovo host come `Portfolio.WebApi` può sostituire MPS e comporre `Portfolio.Api`, i relativi Contracts, il Data Model, la configurazione e le dipendenze Shared necessarie.

`Domain.Api` può contenere fisicamente Controller, Service, Repository e componenti infrastrutturali specifici del dominio. La convivenza nello stesso assembly non annulla i confini logici tra tali responsabilità.

La separazione futura in DLL differenti è un possibile refactoring, non un requisito né un'attività pianificata.

---

## 4. Modello logico di riferimento

Il flusso applicativo generale è:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Data Model / DbContext
```

Il Service può introdurre un Business Model quando esiste una reale divergenza semantica rispetto al Data Model:

```text
Controller ───────→ Service ───────→ Repository ───────→ Data Model
                       │                                      │
                       └────→ Business Model opzionale ←──────┘

Response Contract ←──── Business Model oppure Data Model
```

Il Repository continua ad accedere al Data Model e a restituirne le Entity al Service. Il Business Model non costituisce un passaggio obbligatorio: quando modello applicativo e Data Model rappresentano lo stesso concetto senza effetti indesiderati, il Service può utilizzare direttamente le Entity persistite.

Quando esiste, il Business Model appartiene allo stesso livello architetturale interno del Data Model ma può dipendere da esso per effettuare il mapping. La dipendenza inversa non è ammessa.

I layer sono confini di responsabilità. Non implicano necessariamente cartelle, progetti o assembly separati e non autorizzano dipendenze circolari.

---

## 5. Contracts

I Contracts rappresentano il protocollo pubblico del dominio, non il suo linguaggio applicativo interno.

Comprendono Request e Response DTO e descrivono esclusivamente i dati scambiati attraverso le API. Non contengono logica di business, query, accessi al `DbContext` o operazioni di persistenza.

`Domain.Contracts` contiene l'implementazione server-side del protocollo. I client non devono necessariamente riutilizzarne l'assembly o le classi: la descrizione autorevole del wire contract è OpenAPI e ogni client può implementare i modelli con la tecnologia più adatta.

### 5.1 Request DTO

Il Controller riceve Request già elaborate dalla pipeline Shared e traduce i dati nei parametri richiesti dai Service.

I Service non dipendono dai Contracts. Il Controller passa normalmente valori singoli e può costruire un Business Model soltanto quando esiste una reale esigenza semantica.

### 5.2 Response DTO

Il Response DTO è responsabile della traduzione del modello interno nella rappresentazione pubblica.

Può essere costruito da un Business Model oppure direttamente da una Entity del Data Model. La dipendenza di `Contracts` dal modello interno è ammessa esclusivamente per il mapping e rimane unidirezionale; Data Model e Business Model non dipendono dai Contracts.

L'Entity non viene serializzata direttamente come contratto pubblico.

Una breaking change nasce dalla modifica del contratto server ed è conclusa quando tutti i client interessati sono stati aggiornati. Server e client vengono normalmente rilasciati insieme, salvo deroghe valutate caso per caso.

---

## 6. Controller

Il Controller rappresenta il confine HTTP e orchestra l'operazione esposta dall'API.

È responsabile di:

- interpretare route, query string e body;
- tradurre le Request nei parametri applicativi;
- invocare uno o più Service nella sequenza richiesta;
- comporre i risultati;
- costruire i Response DTO;
- tradurre gli esiti applicativi in risposte HTTP;
- governare l'atomicità complessiva del caso d'uso.

Il Controller non implementa regole di business, non accede direttamente ai Repository o al `DbContext` e non modifica direttamente le Entity.

Un Controller può contenere più codice rispetto ad architetture che affidano l'intero caso d'uso a un Service. Ciò è coerente con MPS finché il codice rimane orchestrazione esplicita e non assorbe responsabilità applicative.

Quando un'orchestrazione diventa complessa o deve essere riutilizzata fuori dal trasporto HTTP, può essere estratta in un coordinatore applicativo dedicato. Tale componente viene introdotto per necessità e non costituisce un layer obbligatorio.

---

## 7. Service

I Service espongono capacità applicative o di business focalizzate e riutilizzabili in operazioni differenti.

Un Service:

- applica le regole di business;
- coordina Repository, filesystem, servizi esterni e infrastrutture necessarie alla singola capacità;
- non conosce HTTP, Controller, status code o Model Binding;
- non dipende dai Contracts pubblici;
- non orchestra necessariamente l'intero caso d'uso API;
- può restituire Entity del Data Model oppure Business Model quando necessario.

Lo stesso Service può essere utilizzato da operazioni singole, bulk, importazioni o altri flussi senza conoscere il contesto che lo invoca.

---

## 8. Repository e Data Model

### 8.1 Repository

Il Repository separa le capacità applicative dai meccanismi di accesso ai dati.

È responsabile di query e persistenza e può restituire Entity del Data Model ai Service. Non conosce Contracts, Controller, HTTP, presentazione o identità del client e non contiene regole di business.

La granularità dei Repository, l'uso di interfacce e l'eventuale adozione di astrazioni generiche sono decisioni di livello implementativo e non vengono prescritte da questo documento.

### 8.2 Data Model

Il Data Model contiene lo stato persistito e i componenti di persistenza di basso livello, come Entity, `DbContext`, configurazioni, migration e dettagli del provider.

Quando questo confine viene rappresentato da un progetto fisico, il progetto adotta il nome `Domain.DataModel`.

Ogni dominio possiede autonomamente:

- modello e schema dei dati;
- ciclo delle migration;
- configurazione della persistenza;
- vincoli e integrità dei dati.

Domini differenti possono utilizzare lo stesso database server, provider o infrastruttura fisica, ma operano su database o schemi logici indipendenti. Non esistono accessi diretti, foreign key o transazioni tra i dati di domini differenti.

L'host non possiede un database applicativo comune. Un concetto funzionale appartiene sempre a un dominio; la somiglianza dei dati non giustifica una persistenza condivisa.

---

## 9. Atomicità applicativa

Ogni operazione API ordinaria deve produrre un esito applicativo coerente e atomico. Le operazioni bulk seguono invece la strategia dichiarata dal relativo contratto.

Il Controller, in quanto orchestratore del caso d'uso HTTP, è responsabile dell'atomicità complessiva. Service e Repository cooperano senza introdurre completamenti intermedi incompatibili con il confine dell'operazione.

L'atomicità applicativa non coincide necessariamente con una singola transazione database. Un caso d'uso può coinvolgere database, filesystem, pagamenti o servizi esterni.

Quando gli attori non possono partecipare a un'unica transazione tecnica, l'operazione adotta strategie esplicite di compensazione, idempotenza, stato intermedio e riconciliazione. Il Controller continua a delegare l'esecuzione ai Service e governa sequenza ed esito complessivo.

I meccanismi concreti appartengono alla documentazione implementativa e agli ADR dedicati.

---

## 10. Composizione e configurazione

Ogni dominio è responsabile della registrazione delle proprie dipendenze, della configurazione e del contributo alla pipeline.

Normalmente espone due punti di ingresso:

```csharp
Add<Domain>(configuration);
Use<Domain>();
```

Il primo configura servizi e dipendenze; il secondo contribuisce alla pipeline. Ulteriori punti di ingresso sono ammessi soltanto per esigenze eccezionali del dominio.

L'host non registra singolarmente Service, Repository, Options o componenti interni e non contiene logiche di inizializzazione specifiche del dominio.

Ogni dominio configura autonomamente le istanze dei servizi Shared utilizzati. Le configurazioni possono essere duplicate tra domini per preservarne l'indipendenza.

---

## 11. Errori e sicurezza

Ogni layer gestisce o propaga gli errori secondo la propria responsabilità:

- la pipeline Shared gestisce gli errori tecnici generici del contratto;
- il dominio definisce semantica e codici degli errori applicativi;
- il Service rappresenta gli esiti delle proprie operazioni;
- il Repository segnala problemi di persistenza senza tradurli in HTTP;
- il Controller costruisce la risposta pubblica coerente;
- l'infrastruttura gestisce e registra gli errori inattesi.

Account, ruoli, permessi e configurazione di sicurezza appartengono al dominio. I dettagli di autenticazione, autorizzazione e identità del client sono definiti nella Security Architecture.

---

## 12. Evoluzione e conformità

Il modello descritto in questo documento è il riferimento comune, non un template rigido.

Sono invarianti obbligatori l'autonomia, l'estraibilità, l'ownership dei dati, l'assenza di dipendenze interne tra domini, la separazione dal trasporto e la direzione coerente delle dipendenze.

Layer, cartelle, progetti e astrazioni opzionali vengono introdotti soltanto quando risolvono una responsabilità o un accoppiamento concreto. Un dominio può alleggerire o specializzare il modello di riferimento purché preservi gli invarianti.

Una deviazione richiede un ADR soltanto quando rappresenta una scelta significativa, duratura o non ovvia.

Gli ADR nati dall'analisi di un singolo dominio devono dichiararne correttamente lo scope e non impongono automaticamente vincoli ai domini futuri.

---

## Vedi anche

- [Architecture](Architecture.md)
- [API Architecture](ApiArchitecture.md)
- [Bulk Operations](BulkOperations.md)
- [Shared Framework](SharedFramework.md)
- [ADR-0001 — I domini sono autonomi e ricomponibili](ADR/ADR-0001-domains-are-autonomous-and-recomposable.md)
- [ADR-0007 — I Service non dipendono dai Contracts](ADR/ADR-0007-services-do-not-depend-on-contracts.md)
- [ADR-0008 — I Response DTO mappano i modelli interni](ADR/ADR-0008-response-dtos-map-internal-models.md)
- [ADR-0009 — I Controller orchestrano le operazioni applicative](ADR/ADR-0009-controllers-orchestrate-application-operations.md)
- [Infrastructure Architecture](InfrastructureArchitecture.md)
- [Security Architecture](SecurityArchitecture.md)
- [Testing Architecture](TestingArchitecture.md)
- [Architecture Roadmap](ArchitectureRoadmap.md)
- [Architecture Decision Records](ADR/README.md)
- [MPS Playbook](../Engineering/MpsPlaybook.md)
