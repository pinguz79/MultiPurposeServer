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

Finance.Desktop costituisce il client principale e deve consentire la gestione completa delle funzionalità previste dal dominio.

È prevista in prospettiva Finance.Mobile, con una superficie funzionale più ristretta e orientata principalmente alla consultazione della situazione finanziaria e all'inserimento rapido delle spese.

I client non definiscono il modello funzionale del dominio: Desktop e Mobile utilizzano le capacità esposte da Finance.Api secondo le rispettive esigenze.

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

### 5.2 Movimento

Un Movimento rappresenta una variazione del valore di un Conto associata a una determinata data.

Ogni Movimento appartiene a un Conto e dispone almeno di una data, una descrizione e un importo.

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

La valutazione dell'evoluzione di un Conto non deve limitarsi al valore finale di una proiezione. Finance deve poter considerare i valori assunti dal Conto durante l'intervallo analizzato, in modo da individuare eventuali situazioni temporanee di insufficiente disponibilità anche quando il valore alla data finale risulta sostenibile.

### 5.4 Pianificazione

La Pianificazione definisce regole utilizzabili per generare automaticamente Movimenti attesi nel tempo.

Una regola di Pianificazione può essere applicata a un determinato intervallo temporale. La sua applicazione genera i Movimenti previsti dalla regola all'interno dell'intervallo richiesto.

La Pianificazione determina le informazioni necessarie alla creazione dei Movimenti, compresa la loro data iniziale e, quando previsto, la regola utilizzata per calcolarne l'importo.

Una volta generato, il Movimento esiste autonomamente dalla Pianificazione. La data determinata durante la generazione viene memorizzata nel Movimento e può essere successivamente modificata senza modificare la Pianificazione che lo ha originato.

La successiva modifica di una Pianificazione non modifica automaticamente i Movimenti precedentemente generati.

Finance può mantenere l'informazione relativa alla Pianificazione che ha originato un Movimento, in modo da consentire l'individuazione dei Movimenti futuri interessati da una successiva variazione della regola e supportarne un eventuale adeguamento assistito.

### 5.5 Parametri temporali

Un Parametro temporale rappresenta un'informazione utilizzabile dai calcoli di Finance il cui valore può variare nel tempo.

Uno stesso Parametro può disporre di più definizioni, ciascuna delle quali associa un valore a un intervallo di validità. I valori possono rappresentare, ad esempio, importi monetari, percentuali o altre informazioni necessarie alle regole di calcolo.

Sono esempi di Parametri temporali:

- il canone di affitto;
- il costo di una determinata tratta autostradale;
- il plafond contrattuale di una carta;
- la rata ordinaria di una carta revolving;
- la percentuale utilizzata per determinarne la rata;
- un valore minimo previsto dalle condizioni contrattuali;
- il valore ordinario utilizzato per la previsione dello stipendio.

Le definizioni di uno stesso Parametro costituiscono un insieme ordinato e i relativi intervalli di validità possono sovrapporsi.

Per determinare il valore del Parametro a una determinata data, Finance esamina le definizioni secondo il loro ordine e utilizza il valore della prima definizione il cui intervallo comprende la data richiesta.

L'ordine delle definizioni rappresenta pertanto la precedenza da utilizzare nella risoluzione del valore ed è configurabile indipendentemente dall'ampiezza o dalla specificità dei rispettivi intervalli.

L'assenza di una definizione applicabile a una determinata data non equivale automaticamente al valore zero. È la regola di calcolo che utilizza il Parametro a stabilire come comportarsi quando nessun valore risulta disponibile.

I valori associati a un Parametro temporale rappresentano le informazioni applicabili ai calcoli dinamici e non costituiscono necessariamente lo storico delle variazioni del Parametro. Lo storico economicamente rilevante è rappresentato dai Movimenti consolidati.

La modifica di una definizione esistente può essere utilizzata per aggiornare la previsione corrente quando non è necessario rappresentare contemporaneamente valori differenti a date future. Definizioni temporali distinte sono necessarie quando Finance deve rappresentare contemporaneamente valori differenti applicabili a date diverse, ad esempio quando una variazione futura è conosciuta in anticipo.

Le definizioni che non possono più influenzare calcoli dinamici possono essere eliminate senza compromettere lo storico finanziario.

### 5.6 Calcolo dell'importo

L'importo di un Movimento può essere espresso mediante un valore costante oppure determinato dinamicamente attraverso una regola di calcolo.

Un importo costante rappresenta direttamente il valore del Movimento e non viene rivalutato automaticamente.

Un importo dinamico viene invece calcolato utilizzando le informazioni disponibili al momento della valutazione del Movimento. La regola di calcolo può utilizzare:

- la data del Movimento;
- uno o più Parametri temporali;
- proprietà del Conto;
- valori e aggregazioni relative al Conto in determinati intervalli temporali;
- altre informazioni del dominio necessarie allo specifico calcolo.

Le regole di calcolo non introducono dipendenze dirette fra singoli Movimenti. Quando un calcolo necessita di informazioni derivanti dall'attività di un Conto, queste vengono considerate attraverso il relativo valore o mediante aggregazioni definite sull'insieme dei Movimenti interessati.

La modifica delle informazioni utilizzate da una regola di calcolo determina la rivalutazione degli importi dinamici interessati.

La modifica della data di un Movimento può modificarne indirettamente l'importo quando la regola di calcolo utilizza informazioni dipendenti dalla data.

Le modalità con cui le differenti regole di calcolo vengono rappresentate e implementate non fanno parte della definizione concettuale del dominio.

### 5.7 Consolidamento dei Movimenti passati

Gli importi dinamici sono necessari per mantenere aggiornata la parte corrente e futura della situazione finanziaria, ma non devono consentire che successive variazioni delle informazioni utilizzate nei calcoli modifichino indirettamente il passato.

Quando la data di un Movimento diventa precedente alla data corrente, il suo eventuale importo dinamico viene consolidato.

Il consolidamento consiste nel valutare la regola di calcolo utilizzando le informazioni disponibili e sostituire l'importo dinamico con il valore risultante. Da quel momento l'importo del Movimento è costante e non viene più influenzato dalle successive variazioni dei Parametri temporali o delle altre informazioni originariamente utilizzate per calcolarlo.

I Movimenti con data uguale alla data corrente non vengono consolidati. Rimangono dinamici per l'intera giornata, consentendo di aggiornare le informazioni da cui dipende il loro importo prima che questo venga congelato.

Il consolidamento non rende il Movimento immutabile. Un Movimento passato può essere successivamente modificato o eliminato quando Finance deve essere bonificato per riallinearlo alla realtà osservata.

Analogamente, nuovi Movimenti possono essere inseriti con una data già trascorsa quando vengono registrate operazioni non precedentemente presenti in Finance.

L'eventuale legame operativo con la Pianificazione che ha originato il Movimento non deve consentire modifiche automatiche ai Movimenti appartenenti alla parte consolidata della situazione finanziaria.

### 5.8 Bonifica

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

