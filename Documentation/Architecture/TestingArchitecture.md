# Architettura del Testing

## 1. Scopo del documento

Questo documento descrive l'architettura della suite di test di MultiPurposeServer.

L'obiettivo non è definire come scrivere un singolo test, ma stabilire come organizzare, strutturare ed evolvere l'intero sistema di testing affinché rifletta l'architettura del progetto.

La suite di test rappresenta un'estensione dell'architettura.

Ogni livello verifica responsabilità differenti e contribuisce a mantenere stabile l'evoluzione del sistema.

Le convenzioni di scrittura dei test appartengono alla documentazione Engineering di livello inferiore.

---

## 2. Filosofia del testing

La suite di test deve riflettere la struttura dell'applicazione.

Ogni componente viene verificato al livello più appropriato, evitando sia duplicazioni inutili sia lacune di copertura.

L'obiettivo non consiste nell'aumentare artificialmente la percentuale di code coverage, ma nel garantire che ogni responsabilità significativa sia verificata almeno una volta nel punto corretto dell'architettura.

### 2.1 Piramide del testing

MultiPurposeServer adotta una strategia ispirata alla Test Pyramid.

```text
                End-to-End
             Integration Test
     Contract Configuration / Framework Test
               Unit Test
```

La maggior parte della suite dovrebbe essere costituita da Unit Test, senza trasformare la piramide in un vincolo numerico rigido. La distribuzione dei test dipende dai rischi e dalle responsabilità effettive del componente.

I livelli superiori verificano l'integrazione tra componenti e non devono sostituire i test unitari.

### 2.2 Una responsabilità, un livello

Ogni comportamento dovrebbe essere verificato nel livello che ne possiede la responsabilità.

Ad esempio:

- il comportamento di un Repository viene verificato dai relativi Unit Test e, per i rischi propri della persistenza, dagli Integration Test;
- il motore della Request Pipeline viene verificato dai Framework Test, mentre la sua integrazione con MVC, middleware e filtri viene verificata dagli Integration Test;
- il Controller verifica la traduzione HTTP;
- il Service verifica la logica applicativa.

Ogni rischio o responsabilità deve avere un livello primario che ne possiede la verifica esaustiva. Un test di livello superiore può attraversare un comportamento già verificato per dimostrare la collaborazione tra componenti, ma non deve duplicarne sistematicamente tutti i casi.

### 2.3 Test come documentazione

I test rappresentano una forma di documentazione eseguibile.

Un buon test descrive chiaramente:

- il comportamento atteso;
- le condizioni iniziali;
- l'esito previsto.

Il lettore deve poter comprendere il comportamento del componente leggendo i suoi test principali.

### 2.4 Test di regressione

Ogni correzione di un difetto deve introdurre un test che fallisca in presenza del problema e passi dopo la correzione. Il test deve appartenere al livello primario che possiede la responsabilità violata.

Un difetto nella logica di un Service appartiene normalmente ai relativi Unit Test; un comportamento specifico del provider ai Repository Integration Test; una configurazione errata di un DTO ai Contract Configuration Test; una collaborazione errata con MVC agli Integration Test; un flusso applicativo completo agli End-to-End Test.

Non è necessario aggiungere automaticamente un End-to-End Test per ogni difetto. Quando il problema ha superato tutti i livelli esistenti, deve però essere valutata anche la garanzia superiore mancante. In tal caso possono essere opportuni sia il test preciso del comportamento sia un test più ampio che impedisca allo stesso rischio di sfuggire nuovamente.

---

## 3. Architettura della suite

La struttura dei progetti di test dovrebbe riflettere quella della soluzione principale.

Ad esempio:

```text
src/
    Domains/
    Applications/
    Shared/

tests/
    Domains/
    Applications/
    Shared/
```

I progetti di test dovrebbero mantenere una corrispondenza riconoscibile con i confini del codice di produzione. Non è tuttavia obbligatorio creare un progetto di test per ogni progetto produttivo quando un diverso raggruppamento rende più chiara o più efficiente la suite.

Quando necessario possono essere introdotti progetti dedicati ai test di integrazione o infrastrutturali.

### 3.1 Indipendenza

Ogni progetto di test deve essere il più possibile indipendente dagli altri.

Un progetto di test non deve dipendere dal progetto di test di un altro dominio, Application o layer. La presenza di queste dipendenze rappresenta un segnale di responsabilità non ben separate.

Il riuso tecnico è ammesso attraverso progetti o pacchetti esplicitamente dedicati alla Test Infrastructure. Questi componenti non devono contenere conoscenza di business né introdurre classi base che accoppino suite appartenenti a responsabilità differenti.

Fixture e utility specifiche di un dominio restano nella relativa suite anche quando somigliano a componenti presenti altrove. Un progetto End-to-End orchestra le applicazioni mediante superfici pubbliche o artefatti distribuiti e non importa internamente le rispettive suite di test.

### 3.2 Organizzazione interna

L'organizzazione interna di ciascun progetto di test deve rendere riconoscibile la corrispondenza tra responsabilità produttive e responsabilità verificate. Le convenzioni concrete per cartelle e namespace appartengono alla documentazione Engineering.

---

## 4. Unit Test

Gli Unit Test verificano il comportamento di un singolo componente isolato.

Costituiscono il livello principale della suite.

### 4.1 Responsabilità

Uno Unit Test dovrebbe verificare:

- un singolo comportamento osservabile;
- uno scenario chiaramente definito;
- un esito coerente.

Può contenere più asserzioni quando concorrono a descrivere lo stesso esito. Se le asserzioni rappresentano responsabilità o scenari indipendenti, devono appartenere a test distinti.

Il fallimento deve indicare chiaramente quale comportamento atteso è stato violato.

### 4.2 Isolamento

Le dipendenze esterne devono essere sostituite tramite stub, fake o mock quando necessario.

L'obiettivo consiste nel verificare esclusivamente il comportamento del componente sottoposto a test.

### 4.3 Cosa non verificare

Uno Unit Test non dovrebbe verificare:

- il comportamento di Entity Framework;
- il comportamento di ASP.NET Core;
- il funzionamento del filesystem;
- il comportamento di librerie esterne già testate.

Queste responsabilità appartengono ad altri livelli della suite.

### 4.4 Service

Gli Unit Test dei Service sostituiscono i Repository e verificano la logica applicativa posseduta dal Service.

Possono verificare gli argomenti e le interazioni con i Repository quando esprimono una parte significativa del contratto. Non devono invece verificare il funzionamento interno del Repository né imporre dettagli fragili, come il numero esatto delle chiamate, quando questo non modifica il comportamento atteso.

### 4.5 Controller

Gli Unit Test dei Controller sostituiscono i Service e verificano:

- l'orchestrazione delle operazioni;
- i parametri trasmessi ai Service;
- la composizione del risultato;
- la traduzione HTTP di responsabilità del Controller.

Il comportamento prodotto da pipeline, middleware e filtri appartiene agli Integration Test.

### 4.6 Repository

I Repository possiedono Unit Test che sostituiscono i collaboratori di Entity Framework. Questi test verificano le decisioni proprie del Repository, come query selezionate, filtri, parametri e operazioni richieste al livello di persistenza.

I test con mock di Entity Framework non dimostrano la corretta traduzione SQL, l'applicazione dei constraint, il comportamento delle transazioni o le caratteristiche specifiche del provider. Tali rischi appartengono agli Integration Test con un database reale o sufficientemente equivalente.

Se il mock richiede di simulare in modo complesso LINQ, tracking, navigation property o altri comportamenti di Entity Framework, quel caso dovrebbe essere verificato mediante un Integration Test.

---

## 5. Framework Test

Il progetto utilizza uno Shared Framework che implementa comportamenti comuni come:

- Request Pipeline;
- normalizzazione;
- validazione;
- gestione Bulk;
- componenti condivisi.

Questi comportamenti vengono verificati una sola volta mediante Framework Test dedicati.

### 5.1 Obiettivo

Lo scopo dei Framework Test consiste nel garantire che il comportamento condiviso rimanga stabile.

Quando il framework è stato verificato, i domini non devono ripetere gli stessi test.

### 5.2 Benefici

Questo approccio permette di:

- ridurre duplicazioni;
- diminuire il numero complessivo di test;
- mantenere la suite più veloce;
- concentrare i test dei domini sulla logica di business.

---

## 6. Contract Configuration Test

I Contract Configuration Test verificano la configurazione dei contratti pubblici esposti dai domini. Il nome li distingue dai consumer/provider contract test, che rappresentano una tipologia differente e non sono introdotti implicitamente da questa architettura.

Non verificano la logica applicativa.

### 6.1 Responsabilità

I Contract Configuration Test controllano principalmente:

- configurazione dichiarativa;
- attributi;
- mapping;
- serializzazione;
- compatibilità del protocollo pubblico.

### 6.2 Request Pipeline

Le Request che implementano `IRequest` non devono ripetere test già coperti dal framework.

Ad esempio, non è necessario verificare in ogni dominio:

- normalizzazione;
- validazione automatica;
- esecuzione della pipeline.

Questi comportamenti appartengono allo Shared Framework.

I Contract Configuration Test verificano esclusivamente che il contratto sia configurato correttamente.

Ad esempio, il Framework Test verifica tutte le combinazioni supportate da `RequiredAtLeastOne`, il Contract Configuration Test verifica che attributi e gruppi siano dichiarati correttamente su uno specifico DTO e un Integration Test può verificare un caso rappresentativo in cui una Request non valida produce la risposta HTTP prevista.

### 6.3 Rappresentazione pubblica

I Contract Configuration Test devono distinguere la configurazione interna del DTO dalla sua rappresentazione pubblica serializzata.

La configurazione interna comprende attributi, gruppi di validazione, mapping e metadati. La rappresentazione pubblica comprende almeno nomi delle proprietà, nullabilità, valori enum, formati e comportamento di serializzazione e deserializzazione.

La specifica OpenAPI costituisce la fonte normativa del contratto pubblico tra server e client. Il payload effettivamente esposto dall'API ne rappresenta l'implementazione osservabile e deve essere verificato rispetto alla specifica. La sola struttura della classe utilizzata dal server non è sufficiente, poiché server e client possono adottare implementazioni e tecnologie differenti.

Una divergenza involontaria tra payload e OpenAPI costituisce un difetto dell'implementazione e non modifica implicitamente il contratto. Un cambiamento interno che preserva la rappresentazione pubblica può essere un refactoring non breaking. Un cambiamento intenzionale del contratto pubblico deve invece essere rappresentato nella specifica e verificato rispetto a tutti i client coinvolti.

I Contract Configuration Test devono fornire una garanzia coerente tra DTO, configurazione del serializer, payload risultante e specifica OpenAPI.

Gli strumenti utilizzati per descrivere o confrontare il contratto pubblico appartengono al livello implementativo della strategia di testing.

---

## 7. Integration Test

Gli Integration Test verificano la collaborazione tra più componenti.

Il loro obiettivo consiste nel garantire che le diverse parti del sistema cooperino correttamente.

### 7.1 Quando utilizzarli

Gli Integration Test sono appropriati quando occorre verificare:

- Repository e database;
- API e middleware;
- autenticazione;
- provider infrastrutturali;
- filesystem;
- cache;
- servizi esterni simulati.

### 7.2 Ambito

Un Integration Test può coinvolgere più componenti reali.

Tuttavia deve mantenere un obiettivo preciso.

Non dovrebbe trasformarsi in un test end-to-end involontario.

La categoria dipende dallo scopo e dall'ampiezza della verifica, non dalla sola presenza di HTTP, database o altri componenti reali. Un test di Repository con database reale o un test della pipeline MVC con un Service sostituito rimangono Integration Test perché verificano collaborazioni circoscritte.

Il semplice attraversamento di un componente non costituisce automaticamente una verifica dei suoi rischi. Un test di livello superiore può sostituire un Repository Integration Test soltanto quando osserva esplicitamente lo stesso comportamento di persistenza.

### 7.3 Velocità

Gli Integration Test sono generalmente più costosi degli Unit Test.

Per questo motivo devono essere utilizzati quando il comportamento non può essere verificato efficacemente tramite test unitari.

### 7.4 Persistenza

Gli Integration Test di persistenza devono utilizzare, quando possibile, lo stesso database engine e lo stesso provider realmente supportati dal dominio in produzione.

L'ambiente deve essere isolato e riproducibile e lo schema deve essere creato mediante le migration reali. In questo modo la suite può verificare query, traduzione SQL, constraint, chiavi esterne, indici univoci, transazioni e comportamenti specifici del provider.

Un provider simulato, come `EF Core InMemory`, può essere utilizzato come supporto a test più circoscritti, ma non è equivalente a un database relazionale reale e non sostituisce gli Integration Test di persistenza.

Ogni dominio deve essere verificato rispetto alla configurazione di persistenza che dichiara di supportare. Se domini differenti adottano provider differenti, ciascuno possiede la relativa garanzia di integrazione.

La tecnologia utilizzata per creare e gestire gli ambienti di test appartiene alle convenzioni implementative di livello inferiore.

---

## 8. End-to-End Test

Gli End-to-End Test verificano flussi significativi attraverso il sistema completo, dal punto di ingresso fino agli effetti osservabili finali.

Sono riservati a scenari critici e rappresentativi. Non devono riprodurre esaustivamente i casi già posseduti dai livelli inferiori, perché hanno costo, durata e fragilità maggiori.

Il confine end-to-end coincide con la singola applicazione distribuibile o con il singolo dominio sottoposto a test, non necessariamente con l'intera piattaforma MPS.

Un test è End-to-End quando attraversa l'intero confine significativo dell'applicazione senza interrompere il flusso mediante la sostituzione di componenti interni rilevanti. La presenza di un punto di ingresso HTTP non è da sola sufficiente a classificarlo come End-to-End.

Per un dominio server il flusso parte dalla richiesta HTTP e raggiunge gli effetti osservabili sulle infrastrutture possedute, come database e filesystem. Per un client Web, Mobile o Desktop il flusso parte dall'interfaccia utente, attraversa l'API pubblica del dominio e termina nel risultato osservabile dall'utente.

Le dipendenze esterne al dominio vengono trattate come servizi terzi e sostituite con ambienti controllati o simulatori. Non sono previsti End-to-End Test trasversali tra domini indipendenti.

I flussi completi client-server devono essere limitati ai percorsi critici e non devono replicare sistematicamente tutti i casi già verificati dalle API.

---

## 9. Test Infrastructure

La Test Infrastructure raccoglie tutti i componenti condivisi necessari all'esecuzione della suite di test. È un supporto trasversale e non costituisce un ulteriore livello di verifica.

Il suo obiettivo consiste nel ridurre le duplicazioni mantenendo i test semplici, leggibili e indipendenti.

La Test Infrastructure non deve contenere logica di business.

I componenti condivisi devono rappresentare responsabilità tecniche reali e non devono nascondere il comportamento rilevante del test. Le convenzioni relative a fixture, builder, factory, fake, mock e utility appartengono alla documentazione implementativa del testing.

---

## 10. Isolamento e riproducibilità

Ogni test deve preparare autonomamente il proprio stato iniziale e non deve dipendere dall'esecuzione di altri test.

L'ordine di esecuzione non deve influenzare il risultato. Le risorse condivise e i dati persistenti devono essere gestiti in modo da preservare isolamento e riproducibilità.

Le suite automatizzate non devono dipendere da database, filesystem, account o servizi di produzione e non devono poter modificare risorse operative. Questa separazione deve essere garantita dalla configurazione e dall'infrastruttura, non soltanto dall'attenzione dello sviluppatore.

I dati di test devono essere sintetici per impostazione predefinita. Eventuali dati derivati dalla produzione sono ammessi soltanto quando anonimizzati, minimizzati e trasferiti deliberatamente in un ambiente controllato.

Credenziali e configurazioni di test devono essere distinte da quelle operative. Quando è necessario coinvolgere un servizio esterno reale, la suite deve utilizzare un ambiente sandbox esplicitamente predisposto.

Le convenzioni per la costruzione, il naming e il lifecycle dei dati di test appartengono alla documentazione implementativa.

---

## 11. Organizzazione della suite

La suite di test deve rimanere facilmente navigabile.

La struttura deve rendere riconoscibile la corrispondenza con responsabilità e componenti del codice di produzione. Le convenzioni concrete per progetti, cartelle, namespace e naming appartengono alla documentazione implementativa.

### 11.1 Evoluzione

Quando un componente viene spostato o rinominato, anche i relativi test devono seguire la nuova struttura.

La suite deve evolvere insieme all'architettura.

---

## 12. Code coverage

La code coverage è una metrica diagnostica: evidenzia codice e percorsi non esercitati, ma non dimostra da sola la qualità o l'efficacia della suite. Una riga eseguita non implica che il relativo comportamento sia stato verificato mediante asserzioni significative.

MultiPurposeServer non adotta inizialmente una soglia globale arbitraria. Line coverage e branch coverage devono essere raccolte per costruire una baseline osservabile e valutare consapevolmente le aree scoperte.

L'evoluzione della coverage segue questi criteri:

- il codice nuovo o modificato deve essere coperto in misura coerente con il rischio;
- una diminuzione della coverage deve essere esaminata e giustificata;
- le parti non coperte devono essere valutate in base a criticità, complessità e frequenza di modifica;
- il raggiungimento di una percentuale non sostituisce la verifica dei comportamenti attesi;
- eventuali soglie future devono essere definite per progetto o area di rischio a partire dalla baseline reale;
- il mutation testing può essere introdotto per valutare l'efficacia delle asserzioni nelle logiche condivise o particolarmente critiche.

---

## 13. Finalità specialistiche

I livelli Unit, Framework, Contract Configuration, Integration ed End-to-End descrivono l'ampiezza e i confini della verifica.

Performance, carico, stress, capacità, resilienza, sicurezza, compatibilità e accessibilità costituiscono finalità trasversali e non ulteriori gradini della piramide. Una verifica specialistica può essere applicata a componenti appartenenti a livelli differenti.

Le strategie, gli strumenti, le soglie e gli scenari relativi a queste finalità devono essere definiti nei rispettivi documenti specialistici prima di generare attività operative di backlog.

### 13.1 Authorization Boundary Test

Gli Authorization Boundary Test costituiscono una finalità trasversale distinta dai test funzionali. Verificano dall'esterno che funzionalità e risorse non siano accessibili fuori dalle policy dichiarate.

I test funzionali di Controller e Service possono assumere che una richiesta abbia già superato il confine di sicurezza quando il componente non possiede decisioni autorizzative. Il componente che implementa una regola di autorizzazione deve invece verificarne sia gli esiti positivi sia quelli negativi nel proprio livello primario.

Gli Authorization Boundary Test coprono combinazioni rappresentative di credenziali, capacità client, permessi utente, ownership, classificazione delle risorse ed escalation. Possono essere implementati come Integration o End-to-End Test, ma la loro denominazione esprime la finalità e non introduce un ulteriore gradino della piramide.

---

## 14. Pipeline di esecuzione

I livelli di test devono essere eseguiti in modo progressivo, anticipando il feedback delle suite più rapide prima di sostenere il costo di quelle più ampie.

La pipeline concettuale prevede:

```text
Sviluppo locale
    Unit + Framework + Contract Configuration

Commit / Continuous Integration
    livelli precedenti + Integration

Validazione applicativa o rilascio
    livelli precedenti + End-to-End critici
```

La progressione descrive la garanzia minima dei diversi stadi, non limita la possibilità di eseguire localmente qualsiasi livello. Gli Integration Test sufficientemente rapidi possono entrare nel ciclo locale e gli End-to-End Test possono essere anticipati nella Continuous Integration quando costo e durata lo consentono.

Nessun livello previsto può essere omesso stabilmente dalla pipeline che precede il rilascio. I dettagli relativi a trigger, filtri, comandi e strumenti di automazione appartengono alle convenzioni implementative.

Un test instabile rappresenta un difetto della suite o del sistema e deve essere analizzato e risolto; non deve essere ignorato come comportamento ordinario della pipeline.

---

## 15. Evoluzione della strategia di testing

La strategia di testing deve evolvere insieme al progetto.

Nuovi livelli di test devono essere introdotti soltanto quando rappresentano una responsabilità distinta.

La crescita della suite non deve produrre duplicazioni sistematiche.

Ogni nuovo test dovrebbe contribuire ad aumentare la fiducia nell'architettura.

Non semplicemente la quantità di codice verificato.

### 15.1 Refactoring

Il refactoring della suite di test segue gli stessi principi del codice di produzione.

In particolare:

- eliminare duplicazioni;
- mantenere responsabilità chiare;
- introdurre astrazioni soltanto quando emergono naturalmente;
- mantenere i test leggibili.

### 15.2 Test come patrimonio architetturale

La suite di test rappresenta parte integrante dell'architettura del progetto.

Una modifica architetturale significativa dovrebbe riflettersi anche nell'organizzazione dei test.

La suite non deve essere considerata un elemento accessorio.

---

## 16. Checklist

Prima di considerare completa una nuova funzionalità verificare che:

- ogni responsabilità significativa sia verificata;
- il livello di test scelto sia appropriato;
- non esistano duplicazioni inutili;
- i test siano indipendenti tra loro;
- i dati utilizzati siano leggibili;
- il comportamento verificato sia chiaramente comprensibile;
- la struttura della suite rifletta quella del codice;
- eventuali componenti condivisi appartengano alla Test Infrastructure;
- il nuovo codice non riduca la qualità complessiva della suite;
- eventuali variazioni della coverage siano state valutate in relazione al rischio, senza usare la percentuale come unico criterio di qualità.

---

## 17. Vedi anche

- `Architecture.md`
- `DomainArchitecture.md`
- `InfrastructureArchitecture.md`
- `SecurityArchitecture.md`
- `WebApplicationArchitecture.md`
- `SharedFramework.md`
- `Documentation/Engineering/MpsPlaybook.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`
