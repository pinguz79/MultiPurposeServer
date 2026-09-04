# Finance — Dominio

## 1. Scopo

Finance è il dominio di MultiPurposeServer dedicato principalmente alla pianificazione e alla previsione delle finanze personali.

Il dominio deve mantenere una rappresentazione coerente della migliore conoscenza disponibile sulla situazione finanziaria corrente e futura dell'utente, consentendo di monitorare più posizioni finanziarie e prevederne l'evoluzione sulla base delle informazioni disponibili.

L'obiettivo principale di Finance non è registrare le spese sostenute, ma valutare la sostenibilità delle operazioni future e individuare preventivamente eventuali situazioni di insufficiente disponibilità.

Lo storico delle operazioni viene mantenuto perché consente di ricostruire la situazione corrente, verificare e bonificare i dati rispetto alle fonti finanziarie reali e analizzare l'andamento delle finanze nel tempo.

Finance non costituisce un sistema contabile immutabile: le informazioni gestite rappresentano la migliore conoscenza disponibile e possono essere corrette quando non corrispondono più alla realtà osservata.

## 2. Obiettivi

Finance deve consentire di:

- monitorare più posizioni finanziarie indipendenti;
- rappresentare disponibilità, debiti e crediti;
- registrare e correggere le variazioni delle posizioni finanziarie;
- conoscere la situazione finanziaria corrente;
- pianificare operazioni future;
- calcolare operazioni future sulla base di informazioni e regole note;
- proiettare l'evoluzione delle singole posizioni finanziarie a una determinata data;
- valutare la sostenibilità temporale delle spese pianificate;
- individuare preventivamente situazioni di insufficiente disponibilità;
- valutare l'impatto di nuove operazioni future modificando la normale pianificazione;
- ottenere una visione complessiva della situazione finanziaria;
- mantenere uno storico delle variazioni avvenute nel tempo;
- bonificare le informazioni gestite quando divergono dalla realtà osservata.

Il dominio deve supportare almeno conti correnti, carte di credito a saldo e revolving, prestiti e rapporti di credito o debito verso terzi, senza assumere che tutte le posizioni finanziarie abbiano necessariamente lo stesso comportamento.

## 3. Confini del dominio

Finance è responsabile della rappresentazione e dell'evoluzione prevista della situazione finanziaria personale.

Rientrano nel dominio:

- le posizioni finanziarie monitorate;
- le variazioni economiche che interessano tali posizioni;
- le operazioni future conosciute, pianificate o ipotizzate;
- le informazioni e le regole utilizzate per calcolare operazioni future;
- il calcolo della situazione finanziaria a una determinata data;
- la valutazione della disponibilità nel tempo;
- l'individuazione di situazioni future di insufficiente disponibilità;
- la conservazione dello storico necessario alla ricostruzione e all'analisi della situazione finanziaria;
- la correzione e la bonifica delle informazioni gestite rispetto alla realtà osservata.

Non rientrano inizialmente nel dominio:

- l'operatività bancaria reale;
- l'esecuzione di pagamenti o bonifici;
- l'accesso diretto ai conti presso istituti finanziari;
- la sincronizzazione automatica con banche o circuiti di pagamento;
- la gestione fiscale o contabile professionale;
- la gestione di investimenti e portafogli finanziari;
- sistemi di budgeting avanzato o consulenza finanziaria.

Queste capacità potranno essere valutate successivamente senza essere assunte come requisiti dell'architettura iniziale.

## 4. Client del dominio

Finance espone le proprie capacità attraverso Finance.Api.

Finance è deliberatamente un dominio single-owner. L'owner è implicito e non viene rappresentato nelle entità mediante `OwnerId`, `UserId`, tenant o altre predisposizioni multiutente.

Un solo account del dominio può accedere ai dati. Qualsiasi operazione, comprese quelle di sola consultazione, richiede autenticazione; Finance non espone funzionalità applicative anonime.

Finance.Desktop costituisce il client principale e deve consentire la gestione completa delle funzionalità previste dal dominio.

È prevista in prospettiva Finance.Mobile, con una superficie funzionale più ristretta e orientata principalmente alla consultazione della situazione finanziaria e all'inserimento rapido delle spese.

I client utilizzano lo stesso account e gli stessi dati, ma ciascun tipo di client deve essere autorizzato separatamente alle sole API previste per la propria funzione.

I client non definiscono il modello funzionale del dominio: Desktop e Mobile utilizzano le capacità esposte da Finance.Api secondo le rispettive esigenze. Un'eventuale evoluzione multiutente richiederebbe una revisione esplicita del modello e non viene anticipata dall'architettura iniziale.

## 5. Concetti fondamentali

### 5.1 Conto

Un Conto rappresenta una posizione finanziaria monitorata da Finance il cui valore varia nel tempo attraverso Movimenti.

Un Conto può rappresentare disponibilità, debiti o crediti e può avere comportamenti differenti in funzione della posizione finanziaria rappresentata.

Sono esempi di Conto:

- un conto corrente;
- una carta di credito a saldo;
- una carta di credito revolving;
- un rapporto Telepass;
- un prestito;
- un credito verso terzi;
- un debito verso terzi.

Finance non distingue strutturalmente fra Conti principali e secondari. Le capacità di proiezione e valutazione dell'evoluzione futura sono disponibili per qualsiasi Conto, anche quando risultano particolarmente significative solo per alcune tipologie.

Il significato economico del valore dipende dalla posizione finanziaria rappresentata dal Conto. Un valore positivo non rappresenta necessariamente una condizione economicamente positiva per l'utente: può indicare, ad esempio, una disponibilità oppure un debito residuo.

Il valore di un Conto a una determinata data deriva dal suo valore iniziale e dai Movimenti che lo interessano fino a tale data.

Finance non attribuisce autonomamente una polarità economica ai Conti. Un importo positivo aumenta sempre il valore del Conto e un importo negativo lo diminuisce; è l'utente, attraverso saldo iniziale e Formule, a scegliere una convenzione coerente per rappresentare disponibilità, crediti e debiti. Un'eventuale visione complessiva somma i valori dei Conti senza reinterpretarne il segno.

### 5.2 Movimento

Un Movimento rappresenta una variazione del valore di un Conto associata a una determinata data.

Ogni Movimento appartiene a un Conto e dispone almeno di una data, una descrizione e un importo.

Finance opera inizialmente esclusivamente in euro. Gli importi monetari persistiti hanno due cifre decimali; non viene memorizzata una valuta distinta su Conti o Movimenti.

Il segno dell'importo ha una semantica uniforme: un importo positivo aumenta il valore del Conto, mentre un importo negativo lo diminuisce. Il significato economico dell'aumento o della diminuzione dipende dalla posizione finanziaria rappresentata dal Conto.

La data del Movimento determina quando la variazione concorre al valore del Conto. Può essere inizialmente determinata da una regola di pianificazione, ma viene memorizzata come valore del Movimento e può essere successivamente modificata per rappresentare meglio la realtà osservata o una diversa previsione.

L'importo può essere espresso direttamente oppure determinato dinamicamente attraverso una regola di calcolo. Un importo calcolato rimane dinamico finché il Movimento appartiene alla parte non consolidata della situazione finanziaria.

Finance non distingue strutturalmente fra Movimenti effettivi, previsti o ipotetici. Un Movimento futuro rappresenta un'informazione utilizzata nella previsione indipendentemente dal suo grado di certezza.

I Movimenti non costituiscono registrazioni contabili immutabili. Possono essere inseriti, modificati o eliminati anche dopo la propria data quando ciò è necessario per riallineare Finance alla realtà osservata.

### 5.3 Valore di un Conto a una data

Il valore di un Conto a una determinata data è dato dal suo valore iniziale e dalla somma degli importi dei Movimenti con data minore o uguale alla data considerata.

In forma concettuale:

Valore(Data) = Valore iniziale + somma dei Movimenti con DataMovimento <= Data

La stessa regola viene utilizzata per determinare sia la situazione corrente sia qualsiasi proiezione futura.

Il valore corrente di un Conto corrisponde pertanto al suo valore calcolato alla data odierna, mentre il valore a una data successiva rappresenta la previsione della sua situazione a quella data.

Finance utilizza date civili prive di orario. La data corrente è determinata secondo il fuso italiano `Europe/Rome`.

Il saldo iniziale è atemporale e costituisce la base matematica del Conto per qualsiasi data richiesta, anche precedente al primo Movimento.

La valutazione dell'evoluzione di un Conto non deve limitarsi al valore finale di una proiezione. Finance deve poter considerare i valori assunti dal Conto durante l'intervallo analizzato, in modo da individuare eventuali situazioni temporanee di insufficiente disponibilità anche quando il valore alla data finale risulta sostenibile.

### 5.4 Pianificazione

La Pianificazione definisce regole utilizzabili per generare automaticamente Movimenti attesi nel tempo.

Una regola di Pianificazione può essere applicata a un determinato intervallo temporale. La sua applicazione genera i Movimenti previsti dalla regola all'interno dell'intervallo richiesto.

La Pianificazione determina le informazioni necessarie alla creazione dei Movimenti, compresa la loro data iniziale e, quando previsto, la regola utilizzata per calcolarne l'importo.

Una volta generato, il Movimento costituisce un'entità autonoma, ma può rimanere gestito dalla Pianificazione che lo ha originato finché tale legame risulta necessario alla previsione dinamica.

La successiva modifica di una Pianificazione può determinare l'adeguamento dei Movimenti da essa gestiti. L'adeguamento interessa esclusivamente le informazioni del Movimento derivate dalla proprietà della Pianificazione modificata e deve preservare, quando possibile, le modifiche manuali apportate alle altre informazioni del Movimento.

La riapplicazione della stessa Pianificazione riallinea i Movimenti che essa gestisce e non ne genera duplicati. Pianificazioni distinte possono invece avere parametri identici e generare correttamente serie di Movimenti identiche.

Prima di sovrascrivere una proprietà, Finance confronta il valore corrente del Movimento con il valore che la Pianificazione avrebbe prodotto usando i parametri precedenti. Se i valori coincidono, la proprietà può essere aggiornata automaticamente. Se differiscono, la proprietà è considerata modificata manualmente e richiede conferma esplicita; un rifiuto preserva il valore sul singolo Movimento senza interrompere l'elaborazione degli altri e senza scollegarlo dalla Pianificazione.

Quando il nuovo intervallo produce meno occorrenze, i Movimenti gestiti eccedenti vengono proposti per la cancellazione. Un Movimento che l'utente decide di conservare viene scollegato dalla Pianificazione e diventa autonomo. Quando le occorrenze aumentano, Finance crea i nuovi Movimenti necessari senza adottare automaticamente Movimenti autonomi già esistenti, anche se possiedono dati identici.

L'eliminazione di una Pianificazione applica la stessa regola: i Movimenti già consolidati rimangono invariati; quelli ancora gestiti vengono proposti per la cancellazione e gli elementi preservati dall'utente diventano autonomi.

Le modifiche che comportano l'eliminazione di Movimenti, la perdita di modifiche manuali o altri effetti potenzialmente distruttivi devono essere individuate preventivamente e sottoposte all'approvazione dell'utente.

Le Pianificazioni possono essere correlate quando una variazione di una di esse può rendere necessario valutare una variazione di un'altra. L'analisi segnala inizialmente le sole Pianificazioni correlate direttamente; se l'utente decide di modificare anche una di queste, l'analisi prosegue sui suoi correlati diretti e sui Movimenti da essa gestiti, evitando di riproporre elementi già esaminati.

La correlazione non implica che una modifica venga propagata automaticamente. Finance utilizza tali relazioni per individuare gli effetti potenziali della modifica e supportare l'utente nella valutazione e nell'eventuale applicazione coordinata delle variazioni necessarie.

Il legame operativo fra Movimento e Pianificazione termina con il consolidamento del Movimento.

### 5.5 Voci ricorrenti

Una Voce ricorrente rappresenta un valore economico direttamente utilizzabile nella Formula di un Movimento e il
cui importo può variare nel tempo. Il valore è sempre monetario, espresso in euro con due cifre decimali; percentuali,
coefficienti e altre informazioni tecniche appartengono invece ai Parametri del Conto.

Una stessa Voce ricorrente può disporre di più definizioni, ciascuna delle quali associa un valore monetario a un
intervallo di validità.

Sono esempi di Voci ricorrenti:

- il canone di affitto;
- il valore ordinario utilizzato per la previsione dello stipendio;
- il canone periodico di un servizio;
- l'importo ordinario di una spesa ricorrente.

Le definizioni della stessa Voce costituiscono un insieme esplicitamente ordinato dall'utente e i relativi intervalli
di validità possono sovrapporsi, anche coincidendo completamente.

Per determinare il valore della Voce a una determinata data, Finance esamina le definizioni secondo il loro ordine e
utilizza il valore della prima definizione il cui intervallo comprende la data richiesta.

L'ordine visibile rappresenta la precedenza da utilizzare nella risoluzione del valore ed è configurabile indipendentemente dall'ampiezza o dalla specificità dei rispettivi intervalli. Il criterio tecnico utilizzato per persistere l'ordine appartiene all'implementazione.

Gli estremi `ValidoDa` e `ValidoA` sono inclusivi. Un estremo assente rappresenta validità illimitata nella relativa direzione; entrambi assenti rappresentano validità permanente. Un intervallo con `ValidoDa` successivo a `ValidoA` non è valido.

Finance deve poter determinare la copertura temporale effettiva delle definizioni secondo il loro ordine. I client possono rappresentarla graficamente, anche senza scala temporale, per rendere immediatamente visibili le definizioni completamente oscurate da alternative più prioritarie. Una definizione oscurata rimane valida e può diventare raggiungibile modificando l'ordine; sovrapposizioni parziali e intervalli scoperti non costituiscono di per sé warning.

Nella V1, l'assenza di una definizione applicabile a una determinata data restituisce il valore predefinito `0`. La
UI distingue questa assenza da una definizione applicabile il cui valore sia realmente zero.

I valori associati a una Voce ricorrente rappresentano le informazioni applicabili ai calcoli dinamici e non
costituiscono necessariamente lo storico delle relative variazioni. Lo storico economicamente rilevante è
rappresentato dai Movimenti consolidati.

La modifica di una definizione esistente può essere utilizzata per aggiornare la previsione corrente quando non è necessario rappresentare contemporaneamente valori differenti a date future. Definizioni temporali distinte sono necessarie quando Finance deve rappresentare contemporaneamente valori differenti applicabili a date diverse, ad esempio quando una variazione futura è conosciuta in anticipo.

Le definizioni che non possono più influenzare calcoli dinamici possono essere eliminate senza compromettere lo storico finanziario.

### 5.6 Parametri del Conto

Un Parametro del Conto rappresenta un'informazione funzionale associata a uno specifico Conto e utilizzabile dalle
regole di calcolo di Finance. A differenza di una Voce ricorrente, non rappresenta direttamente un valore economico
destinato a generare Movimenti, ma un'informazione necessaria a determinarne il comportamento o il valore.

Analogamente alle Voci ricorrenti, un Parametro del Conto può disporre di più definizioni applicabili a intervalli
temporali differenti, anche quando nella pratica il relativo valore non è destinato a variare.

Ogni gruppo logico di Parametri del Conto deve contenere almeno una definizione permanente con `ValidoDa` e
`ValidoA` assenti. Tale definizione costituisce il fallback e non può essere eliminata se lascerebbe il gruppo privo
di copertura permanente. Gli override più prioritari possono sostituirla negli intervalli specificati.

Sono esempi di Parametri del Conto:

- il plafond contrattuale di una carta;
- il giorno di chiusura del ciclo di fatturazione;
- la rata ordinaria di una carta;
- la percentuale utilizzata per determinare la rata;
- il valore minimo previsto per la rata;
- qualsiasi altra informazione funzionale propria del Conto necessaria a determinarne i calcoli.

I Parametri del Conto consentono di estendere le informazioni disponibili senza introdurre proprietà specifiche nel
modello generale del Conto.

Ogni gruppo logico dichiara un tipo di rappresentazione e validazione comune a tutte le proprie definizioni:

- `Importo`, espresso in euro con due cifre decimali;
- `Percentuale`, persistita come coefficiente decimale e visualizzata come percentuale;
- `Intero`, privo di parte decimale;
- `Decimale`, utilizzato per coefficienti generici.

Il tipo appartiene al gruppo, è immutabile insieme al Nome e non modifica la rappresentazione numerica fornita alle
Formule. Le singole definizioni possono mantenere `DisplayName` differenti per descrivere il significato dello
specifico override temporale.

Quando una regola di calcolo accede a una proprietà di un Conto, Finance utilizza la proprietà effettiva del Conto
quando questa esiste; in caso contrario può risolvere un Parametro associato al Conto con il nome richiesto.

Il nome di un Parametro del Conto non deve pertanto collidere con una proprietà persistita o calcolata del Conto.

#### 5.6.1 Profilo convenzionale di una carta a saldo

Finance non persiste un tipo strutturale del Conto. Un Conto acquisisce le capacità di una carta a saldo quando
dispone contemporaneamente dei Parametri convenzionali `plafond`, `percentualeScoperto`, `chiusuraCiclo`, `addebito`
e `ripristinoPlafond`.

`plafond` è un `Importo` positivo. `percentualeScoperto` è una `Percentuale` compresa fra zero e uno; l'importo di
scoperto disponibile è calcolato moltiplicando il plafond per tale coefficiente. Gli altri tre Parametri sono
`Intero` e rappresentano giorni del mese compresi fra 1 e 31. Addebito e ripristino appartengono al mese successivo
alla chiusura; il giorno di ripristino deve essere maggiore o uguale al giorno di addebito. Se un giorno non esiste
nel mese considerato viene utilizzato l'ultimo giorno disponibile.

Per una carta a saldo gli acquisti aumentano il valore del Conto e i rimborsi lo diminuiscono. Alla chiusura viene
cristallizzato il valore del Conto; il relativo addebito diminuisce il Conto di pagamento e il successivo ripristino
diminuisce il Conto carta dello stesso importo. Addebito e ripristino sono Movimenti economicamente collegati ma
indipendenti, generati da Pianificazioni correlate senza propagazione automatica delle modifiche.

La proprietà calcolata `saldoUltimoCicloChiuso`, valutata alla data richiesta, individua l'ultima chiusura precedente
o coincidente e restituisce il valore della carta a quella data. Le Formule di addebito e ripristino possono quindi
usare `-[carta.saldoUltimoCicloChiuso]` senza costruire manualmente la data di chiusura.

Il valore matematico del Conto comprende anche l'eventuale ciclo già chiuso e non ancora ripristinato. La
rappresentazione operativa distingue invece lo speso del ciclo corrente, l'importo del ciclo chiuso in addebito e il
plafond residuo. Il plafond residuo ordinario è `plafond - valore del Conto`; la disponibilità comprensiva di scoperto
è `plafond + plafond * percentualeScoperto - valore del Conto`.

Il primo superamento previsto del plafond viene segnalato come utilizzo dello scoperto; il primo superamento previsto
della disponibilità complessiva costituisce una criticità distinta. Se una soglia è già superata alla data corrente,
Finance rappresenta lo stato attuale e non lo ripete come previsione, continuando però a cercare l'eventuale soglia
successiva.

### 5.7 Calcolo dell'importo

L'importo di un Movimento può essere espresso mediante un valore costante oppure determinato dinamicamente attraverso una regola di calcolo.

Un importo costante rappresenta direttamente il valore del Movimento e non viene rivalutato automaticamente.

Un importo dinamico viene invece calcolato utilizzando le informazioni disponibili al momento della valutazione del Movimento. La regola di calcolo può utilizzare:

- la data del Movimento;
- una o più Voci ricorrenti;
- proprietà del Conto;
- valori e aggregazioni relative al Conto in determinati intervalli temporali;
- altre informazioni del dominio necessarie allo specifico calcolo.

Le regole di calcolo non introducono dipendenze dirette fra singoli Movimenti. Quando un calcolo necessita di informazioni derivanti dall'attività di un Conto, queste vengono considerate attraverso il relativo valore o mediante aggregazioni definite sull'insieme dei Movimenti interessati.

La modifica delle informazioni utilizzate da una regola di calcolo determina la rivalutazione degli importi dinamici interessati.

La modifica della data di un Movimento può modificarne indirettamente l'importo quando la regola di calcolo utilizza informazioni dipendenti dalla data.

Le regole di calcolo sono rappresentate mediante Formule valutabili a una determinata data. Lo stesso meccanismo di
valutazione può essere utilizzato da differenti concetti del dominio, fra cui Movimenti, Parametri del Conto e
Tariffe delle tratte. Le Voci ricorrenti espongono invece direttamente un valore monetario temporale e non
contengono una Formula propria.

Una Formula può fare riferimento a variabili e alle proprietà degli oggetti da esse rappresentati. Finance adotta
direttamente la sintassi NCalc V1: i riferimenti sono racchiusi fra parentesi quadre e il carattere `.` separa i
segmenti semantici risolti dal dominio, per esempio `[affitto]` e `[helloCard.quotaRata]`.

La sintassi deve supportare inizialmente almeno i quattro operatori aritmetici fondamentali, il meno unario, le parentesi e le funzioni `Min` e `Max`.

Prima del salvataggio una Formula deve superare la validazione strutturale: sintassi, operatori e funzioni ammessi, riferimenti esistenti, compatibilità del tipo risultante e assenza di dipendenze cicliche dirette o indirette. La UI può inoltre eseguire una preview reale tramite l'Evaluator, utilizzando la data del Movimento, la prima occorrenza della Pianificazione oppure una data di prova appropriata al contesto.

Il fallimento della preview dovuto alle condizioni presenti nella data scelta non rende necessariamente invalida una Formula strutturalmente corretta. Durante un calcolo effettivo, invece, un errore di valutazione impedisce di produrre un saldo apparentemente valido e deve identificare puntualmente Formula ed entità responsabile.

L'assenza di una definizione temporale applicabile restituisce nella V1 normalmente il valore predefinito `0`; le operazioni successive possono comunque rendere la Formula non valutabile, per esempio attraverso una divisione per zero o la costruzione di una data inesistente.

Le variabili vengono risolte senza distinzione di maiuscole e minuscole. Input come `[AffItTo]` o
`[HELLOCARD.quotarata]` vengono ricondotti ai codici autorevoli di Voci ricorrenti, Conti e Parametri del Conto. La
rappresentazione canonica persistita e visualizzata normalizza ogni segmento in camelCase.

Gli importi monetari hanno sempre due cifre decimali. L'expression engine esegue l'intera Formula mantenendo la
precisione `decimal` disponibile; Finance arrotonda al centesimo soltanto il risultato monetario finale mediante la
regola commerciale del valore assoluto crescente (`MidpointRounding.AwayFromZero`).

La regola costituisce il comportamento autorevole di Finance per rate, interessi e altre Formule monetarie. Gli
estratti conto dei gestori non espongono sempre dettagli sufficienti per riprodurre con certezza ogni centesimo:
eventuali scostamenti osservati vengono registrati mediante Movimenti puntuali di rettifica, senza introdurre nel
motore regole intermedie non dimostrate.

L'expression engine utilizzato per interpretare le Formule costituisce una decisione implementativa e non modifica
la semantica delle regole di calcolo.

### 5.8 Consolidamento dei Movimenti passati

Gli importi dinamici sono necessari per mantenere aggiornata la parte corrente e futura della situazione finanziaria, ma non devono consentire che successive variazioni delle informazioni utilizzate nei calcoli modifichino indirettamente il passato.

Quando la data di un Movimento diventa precedente alla data corrente, il suo eventuale importo dinamico viene consolidato.

Il consolidamento consiste nel valutare la regola di calcolo utilizzando le informazioni disponibili e sostituire
l'importo dinamico con il valore risultante. Da quel momento l'importo del Movimento è costante e non viene più
influenzato dalle successive variazioni delle Voci ricorrenti o delle altre informazioni originariamente utilizzate
per calcolarlo.

La proprietà rilevante del consolidamento è l'indipendenza del Movimento dalle informazioni dinamiche utilizzate per calcolarlo. Un Movimento il cui importo è già espresso mediante una Formula costante non richiede pertanto alcuna ulteriore operazione di consolidamento, indipendentemente dalla propria data.

Analogamente, un Movimento futuro può essere consolidato anticipatamente quando si intende congelarne esplicitamente il valore rispetto alle successive variazioni delle condizioni utilizzate per calcolarlo.

I Movimenti con data uguale alla data corrente non vengono consolidati. Rimangono dinamici per l'intera giornata, consentendo di aggiornare le informazioni da cui dipende il loro importo prima che questo venga congelato.

Il consolidamento non rende il Movimento immutabile. Un Movimento passato può essere successivamente modificato o eliminato quando Finance deve essere bonificato per riallinearlo alla realtà osservata.

Analogamente, nuovi Movimenti possono essere inseriti con una data già trascorsa quando vengono registrate operazioni non precedentemente presenti in Finance.

L'eventuale legame operativo con la Pianificazione che ha originato il Movimento non deve consentire modifiche automatiche ai Movimenti appartenenti alla parte consolidata della situazione finanziaria.

### 5.9 Bonifica

La Bonifica rappresenta l'insieme delle modifiche effettuate sui dati di Finance per riallinearne la rappresentazione alla realtà osservata.

Finance mantiene la migliore conoscenza disponibile della situazione finanziaria, ma tale rappresentazione può risultare temporaneamente differente dalle fonti finanziarie reali a causa, ad esempio, di operazioni non ancora registrate, previsioni non verificatesi, date differenti da quelle previste o importi inizialmente stimati.

La Bonifica può pertanto comportare:

- l'inserimento di Movimenti mancanti;
- la modifica della data di Movimenti esistenti;
- la modifica dell'importo di Movimenti esistenti;
- l'eliminazione di Movimenti che non si sono verificati;
- la correzione delle altre informazioni necessarie a riallineare la situazione finanziaria.

La Bonifica può interessare anche Movimenti già consolidati. Il consolidamento protegge il passato da variazioni indirette dei calcoli dinamici, ma non impedisce la correzione esplicita di informazioni risultate errate.

La Bonifica non costituisce un'entità autonoma del dominio né implica necessariamente la conservazione di uno storico delle correzioni effettuate.

Il confronto con fonti finanziarie reali può essere effettuato manualmente o, in futuro, essere supportato da strumenti di riconciliazione automatizzata senza modificare il significato della Bonifica.

### 5.10 Pedaggi e tariffe autostradali

Finance rappresenta un rapporto Telepass come un normale Conto. Le caratteristiche specifiche delle diverse operazioni Telepass vengono modellate senza introdurre comportamenti speciali nel Conto.

Il canone periodico costituisce una normale operazione pianificabile, mentre un parcheggio costituisce un normale Movimento.

Un Movimento relativo a un pedaggio può essere identificato anche come Pedaggio. Questa classificazione è indipendente dalla Categoria del Movimento e consente, ad esempio, di distinguere fra pedaggi relativi a viaggi di lavoro e di piacere mantenendo contemporaneamente la possibilità di analizzare complessivamente tutti i pedaggi.

Il costo di una tratta autostradale è determinato da un tariffario associato alla coppia dei caselli interessati. La tariffa è indipendente dalla direzione di percorrenza e può variare nel tempo.

Un pedaggio futuro può mantenere dinamicamente il riferimento alla tariffa applicabile alla propria data, in modo che una variazione futura del tariffario aggiorni le previsioni non ancora consolidate. Il consolidamento congela invece l'importo effettivamente applicato.

## 6. Modello di dettaglio

La specifica dettagliata delle entità del dominio, delle relative relazioni, delle interfacce condivise e delle regole di valutazione è descritta in [Finance Domain Model](DomainModel.md).

L'organizzazione tecnica del dominio e i primi vertical slice sono descritti progressivamente in [Finance Architecture](Architecture.md).
