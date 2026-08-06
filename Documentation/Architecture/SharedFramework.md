# Shared Framework di MultiPurposeServer

## 1. Scopo

Lo Shared Framework raccoglie contratti, componenti e comportamenti tecnici riutilizzabili dai domini di MultiPurposeServer.

Non contiene logica di business, non coordina i domini e non costituisce un dominio applicativo. Il suo obiettivo non è diventare una libreria generica indipendente da MPS, ma offrire capacità tecniche stabili della piattaforma senza accoppiare i domini tra loro.

Questo documento descrive i confini e il modello evolutivo dello Shared Framework. I dettagli delle singole capacità appartengono ai documenti specialistici e agli ADR collegati.

---

## 2. Criteri di ingresso in Shared

Un componente può entrare nello Shared Framework quando:

- risponde a un'esigenza concreta già emersa;
- ha natura esclusivamente tecnica;
- rappresenta una responsabilità stabile della piattaforma;
- non dipende dalla semantica o dall'implementazione interna di un dominio.

Una capacità strutturale della piattaforma, come la Request Pipeline, può nascere direttamente in Shared anche se inizialmente viene utilizzata da un solo dominio.

Quando invece una soluzione nasce dall'implementazione specifica di un dominio, normalmente rimane nel dominio fino a quando almeno un secondo utilizzatore reale ne dimostra la generalità. Il secondo utilizzatore non è un requisito assoluto, ma una protezione contro astrazioni premature.

La sola riusabilità ipotetica non giustifica l'estrazione. La duplicazione può essere temporaneamente preferibile finché il concetto condiviso non è sufficientemente compreso.

---

## 3. Confini e modularità

Lo Shared Framework è una famiglia di servizi tecnici con confini logici distinti.

Attualmente più servizi possono convivere nello stesso progetto e nella stessa DLL. Ciascun servizio deve però possedere namespace, alberatura di cartelle, responsabilità e superficie pubblica chiaramente separati.

La separazione fisica in progetti o package differenti non è un requisito attuale. Ogni servizio deve comunque essere progettato in modo da poter essere estratto in una DLL dedicata con modifiche minime, secondo lo stesso principio di estraibilità applicato ai domini rispetto all'host MPS.

L'estraibilità non richiede che ogni servizio sia isolato. Le dipendenze tra servizi Shared sono ammesse quando tecnicamente sensate, purché siano:

- esplicite;
- unidirezionali;
- prive di cicli;
- rivolte esclusivamente alla superficie pubblica del servizio utilizzato.

Per superficie pubblica si intende il contratto pubblico del servizio, non necessariamente una `interface` C#. Non è attualmente obbligatorio adottare sempre il pattern interfaccia più implementazione.

Configurabilità, estensibilità e sostituibilità non sono sinonimi:

- la configurabilità è un requisito generale dei servizi che possiedono opzioni specifiche del dominio;
- l'estensibilità viene introdotta quando esiste un punto di variazione concreto;
- la sostituibilità è desiderabile, ma non impone preventivamente un'astrazione a ogni servizio;
- il pattern interfaccia più implementazione potrà diventare una convenzione generale solo se emergerà con continuità dalle prime implementazioni.

---

## 4. Dipendenze esterne

Lo Shared Framework deve essere indipendente dai domini, ma può dipendere da framework e librerie di terze parti.

Ogni servizio possiede le proprie dipendenze esterne e normalmente ne nasconde i dettagli dietro la propria superficie pubblica. Un tipo esterno può essere esposto quando viene adottato deliberatamente come parte del contratto tecnico del servizio e non rappresenta un dettaglio accidentale di implementazione.

Non devono essere introdotti wrapper privi di valore al solo scopo di nascondere qualsiasi libreria esterna. L'incapsulamento serve a preservare un confine architetturale reale.

Una dipendenza da Entity Framework Core, ASP.NET Core o un provider di logging può quindi essere ammessa in un servizio tecnico che ne assume esplicitamente la responsabilità. Una dipendenza da `Portfolio`, `ModelBook` o un altro dominio non è ammessa.

---

## 5. Composizione e isolamento per dominio

Ogni servizio Shared espone i propri punti pubblici di registrazione e configurazione. Il dominio compone internamente i servizi che utilizza e continua a presentare all'host i soli punti di ingresso del dominio, normalmente:

```csharp
Add<Domain>(configuration);
Use<Domain>();
```

L'host non orchestra direttamente i servizi Shared interni ai domini e non deve scoprirli tramite scansioni o stato globale nascosto.

La configurazione appartiene all'istanza del dominio che consuma il servizio. Domini differenti possono quindi utilizzare la stessa implementazione con opzioni, destinazioni e comportamenti indipendenti, anche quando ciò comporta configurazioni in parte duplicate.

L'isolamento richiesto è comportamentale e configurativo:

- un servizio stateful o configurato per dominio deve avere istanze separate;
- un servizio completamente stateless può essere condiviso fisicamente;
- la condivisione non deve permettere a un dominio di influenzare il comportamento osservabile di un altro.

I meccanismi realmente globali del processo rimangono responsabilità dell'host. Possono supportare istanze configurate per dominio senza introdurre decisioni applicative globali.

---

## 6. Errori tecnici ed errori di dominio

Lo Shared Framework definisce tassonomie e meccanismi tecnici comuni, come la rappresentazione degli errori di validazione, delle violazioni di persistenza e degli item bulk non processati.

Il dominio definisce invece il significato concreto degli errori e i relativi codici applicativi. Lo Shared Framework può trasformare un errore formalizzato dal dominio in una risposta infrastrutturale coerente, ma non deve conoscere concetti specifici come album, fotografie o persone.

---

## 7. Request Contracts

`IRequest` identifica una richiesta che partecipa alle convenzioni condivise di MultiPurposeServer.

Le Request concrete sono contratti dichiarativi: espongono i dati necessari e dichiarano tramite attributi le regole tecniche applicabili. Non implementano gli algoritmi di normalizzazione o validazione e non contengono logica di business o persistenza.

`IRequest` espone `Normalize()` e `Validate()` tramite implementazioni predefinite che delegano ai rispettivi motori Shared. La pipeline può così utilizzare la forma semanticamente leggibile:

```csharp
request.Normalize();
request.Validate();
```

senza dipendere direttamente dalle implementazioni dei motori. Questa scelta non trasferisce la responsabilità degli algoritmi alla Request concreta.

La normalizzazione precede sempre la validazione. Porta i dati in una rappresentazione tecnica canonica senza modificarne il significato e deve essere deterministica e, per quanto possibile, idempotente.

La validazione canonica verifica regole generiche dichiarate dal contratto. Le regole applicative che richiedono conoscenza del dominio non appartengono agli attributi canonici dello Shared Framework.

Le capacità comuni non richiedono un'interfaccia dedicata per ogni fase. Normalizzazione e validazione canonica fanno parte del normale ciclo di vita di `IRequest`; capacità meno comuni, come l'ordinabilità intrinseca o l'esposizione di una chiave, possono essere rappresentate in futuro tramite contratti opzionali separati.

Le scelte sono approfondite negli ADR:

- `ADR-0005`: elaborazione centralizzata nella pipeline MVC;
- `ADR-0006`: implementazioni predefinite di `IRequest`;
- `ADR-0008`: normalizzazione e validazione dichiarative.

---

## 8. Stato delle capacità

Le capacità sono classificate per distinguere il comportamento disponibile dalla direzione progettuale e dalle sole possibilità compatibili con l'architettura.

### Attuali

- contratto comune `IRequest`;
- normalizzazione e validazione dichiarative tramite attributi;
- motori condivisi di normalizzazione e validazione;
- esecuzione automatica, nell'ordine normalizzazione poi validazione, tramite Request Pipeline MVC;
- trattamento ricorsivo dichiarativo di oggetti e collezioni;
- costruzione e riutilizzo di piani per tipo;
- contratti bulk di base `IBulk<TItem>`, `BulkRequest<TItem>` e `BulkOptions`;
- strategia bulk corrente `WarningAndContinue`.

### Pianificate

- strategie bulk indipendenti per persistenza e gestione degli errori;
- atomicità configurabile delle operazioni bulk;
- risultati aggregati e per item;
- validazione globale del contenitore bulk e unicità degli item;
- identificazione opzionale degli item tramite chiave;
- ordinabilità intrinseca opzionale degli item;
- supporto futuro a validazioni di business implementate dai domini e orchestrate dallo Shared Framework.

La progettazione delle API di queste capacità non è ancora definitiva. In particolare, non sono ancora stabiliti i contratti per validatori di business, chiavi, ordinabilità o vincoli di unicità.

### Possibili estensioni

- normalizzazioni specifiche di dominio orchestrate dallo Shared Framework;
- separazione fisica dei servizi Shared in progetti, DLL o package dedicati;
- adozione generalizzata del pattern interfaccia più implementazione;
- sostituzione configurabile di ulteriori componenti.

Queste possibilità non costituiscono attività pianificate e non giustificano predisposizioni premature.

---

## 9. Evoluzione

L'evoluzione dello Shared Framework segue questi principi:

- i bisogni reali della piattaforma e dei domini guidano le astrazioni;
- Shared contiene meccanismi tecnici, non business logic né coordinamento tra domini;
- i confini logici precedono l'eventuale separazione fisica;
- le dipendenze sono esplicite e attraversano soltanto superfici pubbliche;
- ogni servizio preserva la propria estraibilità;
- le Request dichiarano cosa deve essere applicato e i motori Shared definiscono come;
- i dettagli implementativi vengono fissati soltanto quando necessari;
- nuove convenzioni generali vengono promosse dopo che un pattern si è dimostrato stabile.

---

## See also

- `Architecture.md`
- `ArchitectureConsolidation.md`
- `ArchitectureRoadmap.md`
- `ADR/README.md`
- `../Engineering/MpsPlaybook.md`
