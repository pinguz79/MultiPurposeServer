# Glossario di MultiPurposeServer

> **Stato: consolidato — candidato alla promozione Stable 1.0.**

## 1. Scopo

Questo documento definisce i termini che in MultiPurposeServer possiedono un significato specifico, trasversale o facilmente ambiguo.

Le definizioni sono intenzionalmente sintetiche. Il Glossario disambigua il linguaggio, ma non sostituisce i documenti proprietari dei concetti, che rimangono autorevoli per invarianti, responsabilità e dettagli.

---

## 2. Piattaforma

### Host

Processo eseguibile che compone e avvia uno o più domini insieme ai servizi tecnici necessari. L'host MPS non contiene logica applicativa dei domini.

### Composition Root

Punto dell'host in cui moduli, configurazione e dipendenze vengono registrati e collegati.

### Domain

Modulo funzionale autonomo che possiede semantica, dati, configurazione e API del proprio contesto. Non coopera implicitamente con altri domini.

### Application

Client Web, Mobile, Desktop o amministrativo che utilizza le API di un dominio. Più Application possono servire utenti o dispositivi differenti dello stesso dominio.

### Shared Framework

Insieme di capacità tecniche indipendenti dal business, emerse da esigenze concrete e riutilizzabili da più domini.

### Estraibilità per ricomposizione

Proprietà architetturale per cui un dominio o servizio può essere ricomposto in un altro host o progetto senza trascinare dipendenze applicative estranee. È un test mentale contro gli accoppiamenti indesiderati, non l'obbligo di distribuire subito DLL o package separati.

---

## 3. Architettura dei domini

### Contract

Descrizione pubblica del protocollo di un'API. OpenAPI rappresenta il wire contract autorevole; server e client possono usare implementazioni differenti dei relativi DTO.

### Request DTO

Rappresentazione serializzabile dell'input di un'operazione API. Dichiara i dati e le regole tecniche applicabili alla richiesta, senza implementare gli algoritmi condivisi di normalizzazione e validazione.

### Response DTO

Rappresentazione pubblica prodotta dall'API a partire da Data Model o Business Model. Espone soltanto i campi previsti dal contratto.

### Controller

Adapter HTTP del dominio. Riceve la Request, orchestra Service e altri collaboratori, governa l'atomicità applicativa dell'operazione e traduce l'esito nel protocollo HTTP.

### Service

Componente che implementa un caso d'uso o una regola applicativa del dominio. Non dipende da HTTP o dai Contracts pubblici.

### Repository

Adapter che incapsula accesso e operazioni di persistenza. Non contiene logica di business e restituisce il Data Model previsto dal dominio.

### Data Model

Modello interno orientato alla persistenza, attualmente rappresentato dalle Entity EF quando il dominio usa Entity Framework.

### Business Model

Modello interno opzionale che rappresenta concetti e comportamento di business quando il Data Model non è una rappresentazione sufficiente. Può essere omesso nei flussi in cui introdurrebbe soltanto mapping meccanico.

### Atomicità applicativa

Proprietà per cui un'operazione API produce interamente l'esito previsto oppure governa esplicitamente rollback, compensazioni e stato residuo. Può coinvolgere database, filesystem, pagamenti o servizi esterni e non coincide necessariamente con una singola transazione database.

---

## 4. Web Application

### API Client

Adapter della Web Application che incapsula trasporto HTTP, autenticazione, serializzazione e traduzione degli esiti remoti. Non compone lo stato della pagina.

### Page Service

Orchestratore di una pagina MVC non banale. Coordina API Client e servizi specialistici e produce un esito applicativo con l'eventuale Page Model.

### Page Model

Stato completo necessario al rendering di una pagina. Appartiene alla Web Application e non coincide necessariamente con un DTO ricevuto dalle API.

### Navigation Context

Informazioni che descrivono il percorso con cui una risorsa è stata raggiunta. Permettono di preservare breadcrumb e navigazione quando la stessa risorsa possiede più navigation path validi.

---

## 5. Sicurezza e identità

### Client Identity

Identità o contesto dell'Application che invoca un'API. È distinta dall'identità della persona che utilizza il client.

### User Identity

Identità autenticata dell'utente umano o applicativo per conto del quale viene eseguita una richiesta.

### Account

Identità di sicurezza registrata all'interno di un singolo dominio. Credenziali, sessioni e permessi appartengono all'Account e non sono condivisi automaticamente con altri domini.

### Person

Entità di dominio che rappresenta una persona biologica. Può esistere senza Account e non coincide con l'identità usata per autenticarsi.

### Confidential Client

Client capace di custodire una credenziale e autenticarsi con un meccanismo adeguato al proprio ambiente di esecuzione.

### Public Client

Client incapace di mantenere riservato un segreto statico incorporato, per esempio codice distribuito agli utenti o eseguito nel browser.

### Access Grant

Relazione che attribuisce a un Account accesso o capacità su risorse specifiche. Rimane distinta dalle relazioni editoriali o dalla presenza di una Person nei contenuti.

### Access Policy

Regola che determina quali identità possono conoscere, consultare o modificare una risorsa o una sua rappresentazione.

---

## 6. Dominio Portfolio

### Owner

Unico soggetto con autorità editoriale e amministrativa sul Portfolio. Può essere anche rappresentato come Person, ma il ruolo di owner rimane unico nel dominio.

### Album

Nodo fondamentale dell'organizzazione del Portfolio. Un Album fisico corrisponde a una folder e appartiene alla gerarchia fisica canonica.

### AlbumKind

Classificazione derivata usata dai client per il rendering di un Album. Non rappresenta tipi di entità o lifecycle differenti.

### Gallery

Album fisico privo di parent e punto di ingresso radice della navigazione fisica. Non contiene direttamente Photo.

### Collection

Album che organizza altri Album. Un Album virtuale è sempre una Collection; un Album fisico viene classificato Collection quando possiede children.

### PhotoAlbum

Album fisico privo di children che può contenere Photo. Un Album fisico vuoto viene convenzionalmente classificato PhotoAlbum.

### Album virtuale

Collection priva di folder e di Photo dirette che costruisce percorsi alternativi mediante link persistiti verso Album virtuali o fisici.

### Navigation Link

Relazione diretta e persistita del grafo di navigazione alternativo. Non modifica la gerarchia fisica e non concede accesso alla risorsa collegata.

### Path fisico canonico

Full path determinato dalla gerarchia filesystem autorevole di un Album fisico. Costituisce la sua chiave logica e il locator canonico.

### Navigation Path

Percorso valido attraverso il grafo di navigazione. Uno stesso Album fisico può possederne più di uno senza duplicare contenuti o identità.

### Photo

Asset fotografico appartenente a un solo Album fisico. Il file originale costituisce il contenuto binario autorevole.

### Variante media

Rappresentazione derivata e ricostruibile di una Photo, per esempio thumbnail, preview, cover, versione Web o watermark. Può avere una policy di accesso differente dall'originale.

### Participation

Relazione che descrive il coinvolgimento e il ruolo contestuale di una Person in un contenuto o progetto fotografico.

### Appearance

Relazione che indica che una Person è effettivamente rappresentata in una Photo. Non coincide automaticamente con Participation o Access Grant.

---

## 7. Testing

### Framework Test

Test dei motori e dei meccanismi tecnici riutilizzabili dello Shared Framework, indipendenti dalla configurazione di un singolo dominio.

### Contract Configuration Test

Test che verifica come i Contracts di un dominio dichiarano normalizzazione, validazione e altri comportamenti tecnici forniti dal framework.

### Authorization Boundary Test

Test specialistico che verifica dall'esterno che funzionalità e risorse non siano accessibili oltre le policy dichiarate.

---

## 8. Documentazione e pianificazione

### ADR

Architecture Decision Record che conserva contesto, motivazioni, alternative e conseguenze di una decisione significativa. Può essere successivamente superato, ma non viene riscritto per rappresentare una decisione diversa.

### Stable 1.0

Stato di un documento ufficiale e autorevole nel proprio ambito.

### Release Candidate

Stato di un documento tematicamente consolidato che attende la verifica finale prima della promozione a Stable 1.0.

### Alpha 0

Stato di un documento non autorevole, potenzialmente incompleto, incoerente o superato, che deve essere verificato prima della promozione.

### Playbook

Documento che definisce workflow e pratiche di engineering applicabili ai contributi al progetto.

### Vision

Documento che conserva direzioni e possibilità di lungo periodo senza trasformarle automaticamente in lavoro pianificato.

### Roadmap

Documento che organizza la sequenza intenzionale delle milestone in `Now`, `Next` e `Later`.

### Backlog

Registro del lavoro funzionale noto ma non necessariamente pianificato.

### Technical Debt

Registro delle carenze tecniche accettate o degli interventi migliorativi rinviati, classificati per priorità, impatto, costi/benefici e urgenza strategica.

---

## 9. Riferimenti

- [Platform](../Platform.md)
- [Architecture](Architecture.md)
- [Domain Architecture](DomainArchitecture.md)
- [Shared Framework](SharedFramework.md)
- [Security Architecture](SecurityArchitecture.md)
- [Web Application Architecture](WebApplicationArchitecture.md)
- [Testing Architecture](TestingArchitecture.md)
- [Portfolio Domain](../Portfolio/Domain.md)
- [MPS Playbook](../Engineering/MpsPlaybook.md)
- [Architecture Decision Records](ADR/README.md)
