# Finance — Architettura iniziale

> **Stato: Alpha.0 — in consolidamento.**

## 1. Scopo

Questo documento definisce progressivamente l'architettura iniziale del dominio Finance e il primo vertical slice implementativo. Le regole funzionali restano autorevoli in `Domain.md` e `DomainModel.md`.

## 2. Struttura iniziale

Finance viene introdotto mediante tre progetti:

- `MultiPurposeServer.Domain.Finance.Api`;
- `MultiPurposeServer.Domain.Finance.Contracts`;
- `MultiPurposeServer.Domain.Finance.DataModel`.

Service e Repository rimangono inizialmente in `Finance.Api`, separati mediante folder e namespace. `Finance.BusinessModel` non viene introdotto finché le Entity EF rappresentano adeguatamente anche i dati di business.

Il dominio usa lazy loading come regola architetturale. Eventuali deroghe richiedono una necessità concreta di performance.

Finance usa `MultiPurposeServer.Shared.Persistence` per il lifecycle di Operation, transazioni e checkpoint. Il Controller orchestra l'operazione applicativa e apre una Operation soltanto quando necessaria.

## 3. Primo vertical slice — Anagrafica Conti end-to-end

Il primo vertical slice attraversa Finance.Desktop, Contracts, Controller, Service, Repository, Data Model e persistenza mediante l'anagrafica dei Conti.

Comprende:

- creazione e modifica della configurazione di un Conto tramite BackEnd;
- elenco e dettaglio dei Conti tramite FrontEnd;
- lettura della configurazione completa tramite BackEnd;
- visualizzazione dello stato dei Conti nella home di Finance.Desktop;
- creazione di un Conto da Finance.Desktop e aggiornamento immediato della home.

Il secondo vertical slice estende lo stesso percorso end-to-end con la navigazione dai Conti all'elenco dei
Movimenti e con le API Bulk di creazione e aggiornamento necessarie a popolare e bonificare i dati. Finance.Desktop
rimane di sola consultazione per i Movimenti: non comprende UI Bulk, modifica dei dati anagrafici dei Conti o CRUD
puntuale sui Movimenti. Contratti, comportamento della UI, ordinamento, criteri di consultazione e strategie Bulk
vengono consolidati prima dell'implementazione. Pianificazioni, formule dinamiche e proiezioni ulteriori restano fuori
perimetro; il Movimento usa già il Contract definitivo `Formula`, limitato in questo slice alle sole costanti.

Nella home di Finance.Desktop, il clic singolo su una card seleziona il Conto e rende visibile la selezione mediante
un diverso colore di sfondo. Il doppio clic apre l'elenco dei Movimenti del Conto. Un comando equivalente nel menu
contestuale della card rende la navigazione disponibile anche senza conoscere la gesture del doppio clic.

L'elenco dei Movimenti viene mostrato nella finestra principale al posto della home e dispone di un comando
esplicito per tornare all'elenco dei Conti. Il secondo vertical slice non apre finestre di dettaglio separate. La
possibilità di mantenere aperti e affiancare più Conti viene rinviata finché non emergerà un'esigenza concreta di
confronto simultaneo.

I Movimenti sono ordinati dal più antico al più recente. La UI li presenta come un'unica sequenza cronologica,
raggruppata visivamente per mese mediante separatori espliciti che riportano mese e anno. Il raggruppamento riprende
la scansione mensile del foglio di calcolo preesistente senza trasferire nel client il vincolo di una pagina o scheda
separata per ogni mese.

All'interno della sequenza, la UI distingue in modo immediato i Movimenti passati, quelli del giorno corrente e quelli
futuri. La distinzione deriva dalla data del Movimento valutata nel fuso `Europe/Rome` e non modifica la natura o lo
stato persistito del Movimento. Il giorno corrente deve avere un'evidenza dedicata; passato e futuro devono restare
riconoscibili anche senza affidarsi esclusivamente al colore, così da preservare leggibilità e accessibilità. I
dettagli grafici vengono verificati sulla prima implementazione, mantenendo come riferimento una presentazione più
neutra per lo storico e una separazione esplicita per la parte previsionale.

La colorazione interessa l'intera riga: il futuro mantiene lo sfondo bianco, il passato usa un colore tenue e il
giorno corrente un colore più evidente. Non viene aggiunta una legenda permanente, perché ordine e data rendono già
comprensibile la separazione. Se il mese selezionato non contiene Movimenti, il relativo separatore resta visibile
con il messaggio `Nessun movimento nel mese`, mentre il contesto laterale continua a essere mostrato.

Il selettore della timeline opera per mese e anno e consente la traslazione al periodo precedente o successivo; non
espone un intervallo arbitrario `dal/al`. All'apertura del mese corrente il client porta in vista i Movimenti di oggi,
oppure il primo Movimento futuro e, in loro assenza, l'ultimo passato. Per un mese differente porta in vista il primo
Movimento del mese selezionato.

La cancellazione del Conto è esclusa dal vertical slice e dalla V1 perché non risponde a un'esigenza operativa prevista. Un'eventuale introduzione futura richiederà una specifica esplicita e non viene anticipata mediante endpoint o comportamenti impliciti.

### 3.1 Contratti dei Conti

`CreateContoRequest` richiede:

- `Name`;
- `DisplayName`;
- `InitialBalance`, valorizzato esplicitamente anche quando è `0`.

`Name` è un identificatore tecnico univoco senza distinzione di casing e persistito in PascalCase. Deve iniziare con una lettera e contenere soltanto lettere e numeri; spazi e separatori accettabili in input vengono normalizzati prima della validazione e della persistenza.

`UpdateContoRequest` espone come nullable `Name`, `DisplayName` e `InitialBalance`. `null` significa non modificare il campo e almeno un campo deve essere valorizzato. Un `Name` presente viene normalizzato e deve rispettare le stesse regole della creazione; il cambio viene rifiutato con `409 Conflict` quando il valore normalizzato appartiene già a un altro Conto. Un `DisplayName` presente non può essere vuoto o composto soltanto da spazi.

Nel perimetro attuale, privo di Formule dinamiche persistite, la modifica di `Name` aggiorna soltanto il Conto. Quando verranno introdotte Formule che referenziano Conti, il rename dovrà diventare un'operazione coordinata e atomica che aggiorna anche tutti i riferimenti interessati; non sarà ammesso lasciare Formule persistite con il precedente identificatore.

Gli importi ricevuti dai Contract devono essere già espressi al centesimo. Un valore con più di due cifre decimali è invalido e produce `400 Bad Request`: il server non corregge implicitamente un input monetario ambiguo. I calcoli interni che possono generare frazioni di centesimo applicano invece `MidpointRounding.AwayFromZero` dopo ogni singola operazione, secondo la semantica definita in `Domain.md`.

Contract, API, servizi ed Entity rappresentano gli importi in euro mediante `decimal`. Il solo confine di persistenza converte ogni importo nei centesimi interi corrispondenti: EF Core moltiplica per `100` in scrittura, persiste un `INTEGER` SQLite e divide per `100` in lettura. Client e livelli applicativi non applicano conversioni compensative e non conoscono la rappresentazione fisica in unità minori.

Il FrontEnd usa `ContoDto`, composto da `Id`, `Name`, `DisplayName` e `Balance`. Il BackEnd usa `ContoConfigurationDto`, che aggiunge `InitialBalance` per consentire consultazione e modifica della configurazione.

L'elenco FrontEnd viene ordinato per `DisplayName` senza distinzione fra maiuscole e minuscole e, in caso di omonimia, per `Name`, così da garantire un risultato deterministico senza introdurre un ordinamento funzionale persistito.

L'eventuale Conto principale evidenziato nella home è una preferenza di layout del singolo client, non una classificazione del dominio. Quando la selezione verrà introdotta, il client potrà conservarne localmente l'identificativo insieme alle altre preferenze della home; Finance non persiste sul Conto priorità, posizione o rilevanza grafica. Una preferenza assente o riferita a un Conto non più disponibile viene gestita dal client mediante il proprio fallback di presentazione.

### 3.2 Route

Il vertical slice espone:

```text
POST  /Finance/BackEnd/Conto
PATCH /Finance/BackEnd/Conto/{contoId}
GET   /Finance/BackEnd/Conto/{contoId}
GET   /Finance/FrontEnd/Conto/{contoId}
GET   /Finance/FrontEnd/Conto/List
```

Le route seguono la convenzione MPS consolidata in `../Architecture/ApiArchitecture.md`: `List` identifica
esplicitamente una collection, mentre la creazione usa `POST` sulla risorsa senza aggiungere l'Action `Create`.

La creazione restituisce `201 Created`; aggiornamenti e letture riuscite restituiscono `200 OK`. Un identificativo inesistente produce `404 Not Found`, un `Name` sintatticamente invalido produce `400 Bad Request` e un `Name` già esistente produce `409 Conflict` anche quando differisce soltanto per casing.

Creazione e aggiornamento restituiscono `ContoConfigurationDto`, includendo i valori normalizzati dal server.

### 3.3 Saldo iniziale nella timeline

`InitialBalance` rimane atemporale nel modello e nel calcolo. Nella timeline la UI rappresenta `OpeningBalance` come
una riga virtuale `Saldo precedente`, datata il giorno antecedente a `From`, esclusivamente espositiva. La riga non
viene persistita, non fa parte di `Items` e non partecipa autonomamente al calcolo del saldo. La data visualizzata
mantiene la natura indicativa attribuita alle date dei Movimenti storici e garantisce che la voce preceda
temporalmente il range realmente consultato.

### 3.4 Modello e Contracts dei Movimenti

`Movimento` persiste `Id`, `ContoId`, `Date`, `Description` e `Formula`. `Date` usa `DateOnly`; passato e futuro sono
entrambi validi e gli importi pari a zero sono ammessi, anche come placeholder di ricorrenze che non producono un
effetto economico in una specifica occorrenza. `Description` è obbligatoria e normalizzata, senza limite applicativo
iniziale di lunghezza. Non esistono vincoli di unicità sulla combinazione dei dati funzionali: due Movimenti identici
restano entità distinte e valide.

Le Request di creazione e aggiornamento espongono direttamente `Formula` come stringa. Nel secondo vertical slice il
validatore ammette soltanto formule costanti, ma il Contract non dovrà cambiare quando verranno introdotti riferimenti
e operatori. La normalizzazione Finance produce una costante monetaria italiana canonica equivalente al formato
Excel `#.##0,00`: inserisce i separatori delle migliaia, mantiene sempre due decimali, accetta `.` o `,` come
separatore decimale in assenza di raggruppamento e rimuove un eventuale segno `+`; il segno `-` viene conservato.
La forma inglese raggruppata `1,234.56`, la notazione scientifica, i simboli di valuta e le parentesi contabili sono
invalidi. Per esempio `34800`, `3480.5` e `3.480,50` vengono normalizzati rispettivamente in `34.800,00`,
`3.480,50` e `3.480,50`.

Il normalizzatore delle future formule dinamiche risolverà i riferimenti senza distinzione di casing e userà nella
forma persistita il camelCase canonico dei Parametri, dei Conti e delle Configurazioni. Un riferimento sconosciuto
rimane invariato durante la normalizzazione ed è rifiutato dalla validazione successiva. Nel presente slice qualsiasi
formula non costante viene rifiutata come funzionalità non ancora supportata.

Il FrontEnd espone `MovimentoDto`, composto da `Id`, `Date`, `Description`, `Amount` valutato e `BalanceAfter`. Il
BackEnd usa `MovimentoConfigurationDto`, composto da `Id`, `Date`, `Description`, `Formula` e `ContoName`.
`Amount` e `BalanceAfter` sono dati calcolati della timeline e non vengono persistiti nel Movimento.

### 3.5 Consultazione mensile dei Movimenti

La timeline è esposta mediante:

```text
GET /Finance/FrontEnd/Conto/{contoName}/Movimento/List?month={month}&year={year}
```

`contoName` identifica il Conto mediante la chiave logica immutabile ed è risolto senza distinzione di casing; la
risposta restituisce sempre il `Name` canonico. `month` e `year` sono opzionali indipendentemente: ogni parametro
mancante assume la relativa componente della data corrente in `Europe/Rome`; un mese esterno a `1..12` produce
`400 Bad Request` e un Conto inesistente produce `404 Not Found`.

La risposta `ContoMovimentiDto` contiene `Conto`, `SelectedMonth`, `SelectedYear`, `From`, `To`, `OpeningBalance`,
`ClosingBalance` e `Items`. `Items` è un `IReadOnlyList<MovimentoDto>` e garantisce l'ordine crescente `Date, Id`;
l'ordine nello stesso giorno non possiede semantica di dominio e l'Id costituisce soltanto il tie-breaker stabile.
`From` e `To` sono inclusivi e descrivono il range effettivamente restituito.

Il mese selezionato viene incluso per intero. Sul lato precedente la consultazione include almeno il mese precedente
e, se prima dell'inizio del mese selezionato non raggiunge `15` Movimenti, estende il range all'indietro fino alla
soglia. Sul lato successivo applica simmetricamente il mese seguente e almeno `15` Movimenti dopo la fine del mese
selezionato. Tutti i Movimenti che condividono la data dell'elemento di soglia vengono inclusi; in assenza di dati
sufficienti vengono restituiti tutti quelli disponibili. La soglia rimane temporaneamente cablata come registrato in
`TD-0012`.

`OpeningBalance` è il saldo immediatamente precedente a `From`; `BalanceAfter` è il saldo successivo al Movimento;
`ClosingBalance` è il saldo dopo tutti i Movimenti con Data fino a `To`. In assenza di Movimenti nell'intervallo,
`OpeningBalance` e `ClosingBalance` coincidono. `Conto.Balance` rappresenta invece il saldo alla data corrente e
include tutti i Movimenti con `Date <= oggi`.

La riga espositiva `Saldo precedente` viene costruita dal client usando `OpeningBalance` e non fa parte di `Items`.
Il server carica in un'unica query i Movimenti necessari fino a `To`, li valuta una sola volta in memoria partendo
da `InitialBalance` e restituisce soltanto quelli compresi nel range effettivo. Non viene persistito un saldo
calcolato, che diverrebbe obsoleto al variare delle future formule dinamiche.

Se una Formula non è valutabile, la consultazione e ogni altra API che deve produrre `Conto.Balance` restituiscono
`422 Unprocessable Entity` e non espongono saldi parziali come validi. L'errore identifica `MovimentoId`, `Date`,
`Description`, `Formula`, codice e messaggio della causa anche quando il Movimento responsabile precede `From` e
viene incontrato durante il calcolo di `OpeningBalance`. Una futura risposta parziale fino all'errore richiederà una
change request esplicita.

La tabella dei Movimenti dispone di un indice composto `ContoId, Date, Id`, coerente con filtro e ordinamento della
timeline.

### 3.6 Operazioni Bulk sui Movimenti

Il secondo vertical slice espone:

```text
POST  /Finance/BackEnd/Bulk/Conto/{contoName}/Movimento/Create
PATCH /Finance/BackEnd/Bulk/Movimento/Update
```

La create opera su un singolo Conto identificato dalla chiave logica nella route. Ogni item contiene `RequestId`
intero positivo, univoco nel payload e non persistito, `Date`, `Description` e `Formula`. La update può attraversare
più Conti perché ogni item è identificato dal `Movimento.Id` e contiene come nullable `Date`, `Description` e
`Formula`; almeno uno deve essere valorizzato. Le liste sono obbligatorie e non vuote, ma non hanno un limite massimo
applicativo iniziale.

Le Bulk API adottano integralmente opzioni, esiti e strategie di Portfolio. Un contenitore invalido produce
`400 Bad Request`; una richiesta strutturalmente valida restituisce `200 OK` anche quando contiene item `Failed` o
`NotProcessed`. Con `StopOnFirstFailure`, gli elementi successivi al primo fallimento vengono restituiti nello stesso
ordine del payload con esito `NotProcessed`; con `EvaluateAll` ogni elemento viene valutato. `Index` e chiave
mantengono la corrispondenza ordinata con la richiesta: `RequestId` è la chiave della create, mentre `Id` è quella
della update.

Un `contoName` inesistente nella route della create invalida la risorsa padre comune e produce `404 Not Found` senza
avviare l'elaborazione. Un `Movimento.Id` inesistente nella update riguarda invece il solo item, viene classificato
come errore `Persistence` con codice `MovimentoNotFound` e segue la strategia di valutazione selezionata. Il risultato
riuscito usa `MovimentoConfigurationDto` e contiene la Formula normalizzata dal server.

## 4. Finance.Desktop

`Applications/Finance/Finance.Desktop` è un'applicazione Windows Forms su .NET 10 e costituisce il client principale del dominio. La scelta privilegia la manutenibilità diretta e non condiziona il futuro `Finance.Mobile`, che rimane un client separato con superficie funzionale più ristretta.

Finance.Desktop utilizza esclusivamente Finance.Api e non referenzia Finance.DataModel, Repository o altri componenti interni del server. Le chiamate HTTP, i modelli del client e lo stato dell'interfaccia rimangono separati dal codice dei Form, senza imporre preventivamente un framework architetturale aggiuntivo.

Form, dialog e UserControl vengono mantenuti compatibili con il designer di Visual Studio. In particolare, i rispettivi file `*.Designer.cs` seguono lo stile generato dallo strumento come previsto dalle convenzioni di coding, così che una successiva modifica visuale produca una diff circoscritta alla sola variazione effettuata.

La prima home comprende:

- un Conto principale evidenziato;
- l'elenco degli altri Conti;
- il saldo corrente di ciascun Conto;
- accesso dal menu al comando per creare un nuovo Conto;
- aggiornamento della home dopo una creazione riuscita;
- rappresentazione comprensibile degli errori di validazione, dei conflitti e dell'indisponibilità del server.

Le card dei Conti mostrano soltanto `DisplayName` e saldo corrente. Il `Name` tecnico non compare nella home e rimane destinato ai contratti e alle schermate di configurazione che ne richiedano la consultazione. Un saldo negativo viene evidenziato in rosso; zero e valori positivi usano il normale colore del testo, senza associare automaticamente il verde a una semantica finanziaria favorevole. Nel primo vertical slice le card sono puramente informative e non risultano cliccabili, poiché non esiste ancora una destinazione di dettaglio.

Finance.Desktop visualizza gli importi monetari secondo la cultura italiana, con separatore delle migliaia, virgola decimale, due cifre decimali sempre presenti e simbolo euro, per esempio `1.234,56 €` e `-1.234,56 €`. Le date vengono visualizzate nel formato italiano `gg/MM/aa`; contratti e logica applicativa mantengono valori data tipizzati e non dipendono dalla rappresentazione testuale adottata dal client.

Quando non esistono ancora Conti, la home mostra semplicemente l'elenco vuoto e il comando `&Conti > &Nuovo conto...` rimane disponibile nel menu. Il comando espone anche la scorciatoia globale `Ctrl+N`. Il primo vertical slice non introduce uno stato grafico dedicato a una condizione limitata al solo avvio iniziale né comportamenti differenti tra la creazione del primo Conto e quelle successive. In accordo con le convenzioni adottate per i menu desktop, `&` definisce il mnemonico per la navigazione da tastiera e i puntini di sospensione indicano che la voce apre un dialog anziché eseguire immediatamente l'azione.

Il dialog di creazione espone `Name`, `DisplayName` e `InitialBalance`, oltre ai comandi `Crea` e `Annulla`; all'apertura assegna immediatamente il focus a `Name`. Le etichette e i comandi espongono i mnemonici distinti `&Nome`, `Nome &visualizzato`, `&Saldo iniziale`, `&Crea` e `&Annulla`. `Crea` è l'azione predefinita attivabile con `Invio`, mentre `Esc` equivale ad `Annulla` e chiude il dialog senza inviare richieste né conservare modifiche. L'ordine di tabulazione è `Name`, `DisplayName`, `InitialBalance`, `Crea`, `Annulla`. `InitialBalance` è precompilato a `0,00 €` e rimane liberamente modificabile prima della conferma. Durante la compilazione, le modifiche a `Name` aggiornano automaticamente il suggerimento di `DisplayName` soltanto finché quest'ultimo non è stato modificato esplicitamente dall'utente. Dal primo intervento manuale su `DisplayName` i due campi proseguono indipendentemente; l'automatismo non deve sovrascrivere una scelta consapevole. Il form mostra inoltre il `Name` normalizzato che verrà persistito e utilizzato nelle Formule prima di confermare la creazione.

I dati numerici vengono inseriti mediante controlli numerici e non tramite textbox con parsing manuale. Il controllo di `InitialBalance` espone due cifre decimali, il separatore delle migliaia e il valore iniziale `0,00`; accetta valori positivi e negativi nell'intero intervallo tecnico supportato da `decimal`, senza limiti applicativi arbitrari introdotti dal client.

Gli errori riferiti a un singolo campo vengono mostrati in prossimità del controllo interessato. La posizione può essere a destra oppure sotto il controllo in base alle dimensioni e alla leggibilità effettiva del dialog: è vincolante l'associazione visiva inequivocabile, non una geometria preventiva. Errori generali come indisponibilità del server o conflitti vengono invece mostrati in un'area riepilogativa del dialog.

Il `409 Conflict` prodotto da un `Name` normalizzato già esistente viene ricondotto al campo `Name`, mostrato inline e seguito dallo spostamento del focus sul relativo controllo. Soltanto i conflitti che non possono essere attribuiti con certezza a un singolo campo rimangono nell'area riepilogativa generale.

Ogni campo dispone di una funzione di validazione dedicata e leggera, richiamata in prima istanza durante il relativo evento `TextChanged`. Per `Name` la validazione condivide il flusso già necessario a mostrare il valore normalizzato e ad aggiornare il suggerimento di `DisplayName`. In questo modo gli errori evidenti possono emergere durante la digitazione; qualora la validazione reattiva rendesse l'interfaccia poco fluida, l'attivazione potrà essere spostata alla perdita del focus senza modificare le regole funzionali. La validazione del server rimane in ogni caso autorevole quando viene richiesta la creazione.

Il comando `Crea` rimane attivo anche in presenza di errori locali. Alla pressione esegue una validazione completa, aggiorna contemporaneamente i messaggi di tutti i campi invalidi e, se la richiesta non può essere inviata, assegna il focus al primo controllo errato secondo l'ordine di tabulazione. La prima implementazione mantiene questa responsabilità nel dialog Finance; l'eventuale estrazione di UserControl riutilizzabili che accorpino etichetta, editor, validazione e messaggio d'errore verrà valutata soltanto dopo l'emersione di ulteriori casi concreti, senza introdurre preventivamente un nuovo componente Shared.

Dopo una creazione riuscita il dialog si chiude e la home aggiorna l'elenco dei Conti. Non viene mostrato un popup di conferma: la comparsa del nuovo Conto nella home costituisce il feedback visivo sufficiente dell'operazione completata.

Il Conto evidenziato viene scelto dal client. Nel primo vertical slice, in assenza di una selezione esplicita, Finance.Desktop evidenzia il primo Conto dell'elenco già ordinato per `DisplayName` e quindi per `Name`; questo fallback rimane una scelta puramente visiva e non viene salvato automaticamente come preferenza. La creazione di un nuovo Conto aggiorna l'elenco visualizzato e non lo rende automaticamente principale quando l'ordinamento individua un diverso primo elemento.

Il primo vertical slice non espone ancora l'azione `Imposta come principale`: finché esiste un solo Conto, il fallback è sufficiente e non occorre persistere alcuna scelta. La selezione locale diventerà utile quando l'interfaccia gestirà più Conti; l'eventuale persistenza server-side di un layout condiviso tra più client rimane un'evoluzione separata e non modifica il modello del primo vertical slice.

### 4.1 Autenticazione del client

Il primo vertical slice utilizza una API key Finance dedicata, distinta dalle credenziali degli altri domini e inviata da Finance.Desktop mediante l'header `X-Finance-Api-Key`. La stessa chiave viene utilizzata sui PC di casa e di lavoro e consente l'accesso sia alla superficie FrontEnd sia alla superficie BackEnd del dominio.

Come nell'implementazione iniziale di Portfolio, la chiave identifica una chiamata proveniente da un client considerato autorevole e impedisce l'esecuzione casuale degli endpoint dalla documentazione Scalar. Non identifica la persona che utilizza il client e non costituisce una protezione forte contro un attore che conosca la chiave.

Per il primo vertical slice è esplicitamente accettato che la chiave sia presente nella configurazione di Finance.Desktop e versionata nel repository. Questa scelta privilegia la disponibilità rapida del flusso end-to-end e accetta il rischio che un soggetto in possesso della chiave possa consultare o alterare la rappresentazione interna di Finance. Tali operazioni non producono effetti sui sistemi finanziari reali; Finance rimane una proiezione correggibile della situazione personale e non dispone di integrazioni dispositive con banche o altri operatori.

Fuori dall'ambiente `Development`, tutti gli endpoint Finance richiedono comunque la chiave. Una chiave mancante o invalida produce `401 Unauthorized`; eventuali future chiavi con capacità limitate possono produrre `403 Forbidden` quando non autorizzano la superficie richiesta. In `Development` la policy viene soddisfatta senza chiave, coerentemente con Portfolio, per consentire l'uso diretto di Scalar durante il debug senza indebolire la configurazione di produzione.

L'autenticazione Microsoft con sessione persistente rimane l'evoluzione pianificata dopo il completamento funzionale iniziale del dominio. Dovrà consentire allo stesso account personale di accedere da entrambe le installazioni senza dipendere dall'utente Windows locale e senza richiedere credenziali a ogni avvio.

L'introduzione della MFA per l'account Finance è una feature pianificata ma non ancora schedulata. Non costituisce un requisito del primo vertical slice e verrà affrontata dopo il completamento funzionale iniziale del dominio. Questa decisione è specifica di Finance: non rende la MFA obbligatoria per Portfolio o per gli altri domini, che possono continuare a valutarla esclusivamente in presenza di un'esigenza concreta.
