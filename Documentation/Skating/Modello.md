# Modello della manifestazione

> Aggiornamento 21 ottobre 2026: per i temi rivisti prevale il
> [consolidamento DS-053-DS-065](Consolidamento-2026-10-21.md).
> Il testo sottostante conserva lo storico; data entry mono-esibizione, esclusioni manuali,
> primi ex aequo e aggregazione obbligatoria per disciplina non sono più proposte correnti.
> Spareggi definiti il 21 ottobre in DS-055 (AP-01 chiuso): rettifica ufficiale di un voto
> condivisa fra classifiche. Stampa e scelta grafica restano aperte. Vedere la fonte e lo stato
> di acquisizione nel consolidamento; non attribuire approvazioni implicite agli altri punti.

Revisione del 9 ottobre 2026: integrazione delle osservazioni del cliente sul PDF dell'8 ottobre.
Le richieste esplicite sostituiscono le precedenti ipotesi incompatibili; gli aspetti ambigui
restano aperti e non sono regole implementabili già approvate. Si descrive il modello futuro,
non quanto già disponibile. Tracciabilità in [Riscontro cliente](RiscontroCliente-2026-10-09.md)
e domande identificate nel [piano di lavoro](PianoDiLavoro.md).

## 1. Contesto e utilizzo offline

Il software nasce per una singola manifestazione che si ripete ogni anno. Giudici e iscritti
cambiano a ogni edizione; scuole e discipline tendono a essere più stabili, ma possono cambiare.
Uno storico centralizzato delle edizioni non è richiesto nel perimetro iniziale.

La preparazione può avvenire in ufficio con connettività. Durante la manifestazione tutte le
funzioni operative devono funzionare offline, anche su un diverso portatile. Prima dell'avvio
delle votazioni deve essere possibile correggere atleti, coreografie e giurie sul posto.
Il portatile può anche essere lo stesso utilizzato in ufficio.

- Una manifestazione è trasportabile come file di database locale.
- Si lavora su una sola copia attiva: niente sincronizzazione o fusione tra copie concorrenti.
- Il trasferimento tra PC deve avvenire tramite una copia consistente, ad applicazione chiusa
  oppure mediante una funzione di backup adatta a SQLite, non copiando alla cieca un DB aperto.
- Sono previsti backup identificabili e datati e possibilità di ripristino.
- Una nuova manifestazione può ripartire da discipline, categorie, livelli e scuole di una
  precedente, senza trascinare atleti, coreografie, giudici, voti o stati della gara.

## 2. Entità e relazioni

### Scuola e atleta

Un atleta può rappresentare scuole diverse in coreografie diverse: non ha un'appartenenza
esclusiva a una sola scuola. DS-004, condivisa con Marco e da validare con il cliente (OP-07),
prevede un'unica anagrafica per atleta e una sola scuola rappresentata per coreografia.
Lo stesso atleta può partecipare per la scuola A in una coreografia e per la scuola B in
un'altra. L'elenco degli atleti di una scuola si ricava dalle coreografie iscritte per essa,
senza duplicare chi compare in più coreografie. Non si introducono coreografie che
rappresentano più scuole contemporaneamente né un'appartenenza esclusiva dell'atleta.

Un atleta può comparire in più coreografie, anche in gruppi diversi. Il numero di coreografie
di ciascun atleta è calcolato dalle iscrizioni, non inserito manualmente. Dalla scheda atleta
deve essere consultabile anche il relativo elenco.

DS-006, proposta da Marco e da validare con il cliente, distingue l'anagrafica completa
(anche atleti senza coreografie, selezionabili per nuove iscrizioni) dall'elenco partecipanti
(solo atleti attualmente presenti in almeno una coreografia). Cancellare l'ultima coreografia
toglie l'atleta dai partecipanti, non dall'anagrafica; una nuova iscrizione riusa la stessa identità.
DS-011, condivisa con Marco e da validare, precisa che partecipanti significa iscritti:
anche gli atleti la cui unica coreografia è "Non presentata" restano in questo elenco.
Chi si è effettivamente esibito è un dato distinto, non una ridefinizione dei partecipanti.

### Coreografia: l'entità che compete

Non si introduce un'entità separata chiamata Formazione. La Coreografia rappresenta l'unità
iscritta e valutata, con i suoi atleti e la sua collocazione nella gara.

| Tipo | Componenti |
| --- | --- |
| Assolo | Esattamente 1 atleta |
| Duo | Esattamente 2 atleti distinti |
| Gruppo | Almeno 3 atleti distinti |

La coreografia appartiene a una sola disciplina e ha una categoria e un livello propri.
Categoria e livello vengono memorizzati sulla coreografia, non dedotti automaticamente dagli
atleti. Le eventuali regole anagrafiche di composizione non sono da inventare né irrigidire
senza una conferma del cliente.

DS-040 recepisce il riscontro cliente, confermato da Marco il 14 ottobre: la stessa
composizione di atleti può partecipare anche nella stessa gara con coreografie dai nomi
diversi. Ogni coreografia ha iscrizione, esibizione e voti distinti, senza copie automatiche.
Il blocco sulla composizione identica di DS-005 è superato. Restano vietate ripetizioni
dello stesso atleta dentro una singola coreografia. Non si deduce da questa regola
un'unicità generale dei nomi fra composizioni diverse. La gara è definita da DS-038.

L'esibizione è la presentazione in gara della coreografia, una sola per iscrizione nel
perimetro della singola gara. Come confermato da Marco in DS-017, i passaggi ai premi di
disciplina e all'assoluto riutilizzano le votazioni già assegnate, senza nuove esibizioni
né ulteriori turni di votazione.
La scelta di rappresentarla come entità autonoma o come stato dell'iscrizione è tecnica e
non deve introdurre una seconda nozione di Formazione.

### Disciplina, categoria, livello e gara

Discipline, categorie (ad esempio Baby, Kids, Senior) e livelli (ad esempio Open, Pro) sono
elenchi separati. Categorie e livelli sono condivisi tra le discipline: creare una nuova
disciplina deve rendere immediatamente disponibili le voci già esistenti. DS-031, condivisa
con Marco e da validare con il cliente, elimina la matrice preventiva di ammissibilità:
le gare si ricavano dalle combinazioni distinte effettivamente presenti nelle iscrizioni.
Una nuova combinazione crea una nuova gara; quelle già presenti non vincolano le successive
iscrizioni. Nessuna gara vuota viene generata dal prodotto cartesiano delle anagrafiche.
La proposta sostituisce le precedenti esclusioni locali per disciplina, differenza da
validare con il cliente. Resta l'eccezione PRO a categoria unica. Ogni coreografia ha una
disciplina, categoria e livello; gli stessi atleti partecipano ad altre discipline con
altre coreografie, senza reintrodurre un'entità Formazione.

DS-032, condivisa con Marco e da validare con il cliente, rimuove la gara non ancora
iniziata se eliminare o spostare l'ultima coreografia la lascia vuota. La regola non si
estende a gare iniziate o concluse, resta distinta dalla procedura di spostamento DS-045 e non elimina
le anagrafiche condivise. Una coreografia "Non presentata" resta iscritta e non rende
vuota la gara.

Il cliente elenca attualmente cinque discipline, estendibili: Assolo/Duo, Caribbean/Kizomba,
Danza, Urban/Street Dance, Show Dance. Sono le denominazioni riportate nel riscontro, non un
elenco fisso da codificare. DS-038 recepisce il chiarimento del 14 ottobre: Assolo/Duo è
oggi una specialità unica. L'interpretazione concordata con Marco usa come chiave della gara:

**Specialità (disciplina) + Categoria + Livello.**

Il tipo Assolo/Duo/Gruppo conserva il significato numerico dei componenti ma non separa
le gare. Suddivisioni future per stile o tra assoli e duo si configurano come specialità
distinte, senza regole dedicate ai nomi né ulteriori gerarchie implicite. La precedente
chiave comprensiva del tipo è superata. Si rappresentano solo le combinazioni effettivamente
iscritte. Questa interpretazione va riportata al cliente; le altre risposte restano da esaminare.

### Giudice e giuria

**Aggiornamento del 14 ottobre, DS-043:** il cliente indica tutti i giudici su tutte le
gare nella situazione ordinaria. Per assenze, sostituzioni e ritorni, la proposta preferita
condivisa con Marco è conservare giudici e voti e consentire all'operatore di escludere
giudici dal calcolo della specifica classifica. Nessun posto in giuria e nessuna sostituzione
memorizzata nella soluzione preferita. Il precedente confronto DS-033 sotto è superato
per il caso ordinario; la politica delle eccezioni resta da validare con il cliente.

Esiste un elenco di giudici della manifestazione. Ogni gara ha una giuria composta da
giudici di questo elenco: le giurie possono coincidere o differire. Pur aspettandosi normalmente lo
stesso numero di giudici, non si impone un vincolo di numerosità uguale per tutte le gare.
Il cliente richiede almeno **3 giudici**. I giudici possono essere modificati fino all'avvio
della singola gara, non soltanto fino all'avvio della manifestazione.

DS-033 lascia al confronto con il cliente il flusso di assegnazione (OP-14): giuria
inizialmente vuota con selezione esplicita e possibile copia da un'altra gara, oppure tutti
i giudici inizialmente inclusi con esclusioni locali per gara. Nessuna alternativa è già
approvata. L'esclusione locale non elimina il giudice dall'anagrafica o dalle altre giurie.
Va verificato se normalmente giudichino tutti tutte le gare e, nel secondo caso, come
gestire i giudici aggiunti successivamente, senza assumere propagazioni automatiche.

## 3. Preparazione e avvio

DS-052 permette di correggere la scuola rappresentata dalla coreografia soltanto prima
dell'inserimento di qualsiasi voto e nel rispetto degli altri vincoli di stato. Anche un
voto zero o parziale impedisce il cambio; riaprire non aggira il vincolo. Non si introduce
una procedura dedicata dopo la valutazione né si cancellano voti per consentire il cambio.
La semplice correzione del nome della scuola resta distinta, secondo DS-037. La modifica
di appartenenza riguarda soltanto la coreografia selezionata. Proposta condivisa con Marco
il 15 ottobre, da riportare al cliente.

DS-051 precisa la sostituzione di un atleta: conferma esplicita con uscente ed entrante
visibili; voti conservati se la coreografia resta nella stessa gara. Se l'esibizione è
chiusa va prima riaperta secondo le regole esistenti. La modifica riguarda solo i componenti
della coreografia selezionata, senza alterare le altre iscrizioni degli atleti o rinominarne
l'anagrafica. Restano i vincoli di composizione; annullare lascia invariati i dati.
La procedura condivisa con Marco va riportata al cliente.

DS-037, condivisa con Marco e da validare con il cliente, permette di correggere il nome
di giudici, atleti e scuole anche durante la manifestazione o dopo la chiusura delle gare.
Tutte le viste aggiornano il nome, conservando identità, iscrizioni, giurie e voti; la sola
correzione nominale non richiede riaperture né modifica classifiche o qualificazioni.
La sostituzione della persona o della scuola rappresentata è un'operazione distinta,
soggetta ai vincoli previsti, non una rinomina.

Prima dell'avvio si possono correggere le anagrafiche e la composizione di gare e giurie.
È superata la proposta di un comando **Avvia manifestazione** che blocchi globalmente le
modifiche strutturali. Anche durante l'evento vanno gestiti ballerini mancanti, coreografie
iscritte nella disciplina errata, ritiri e gruppi di tre che diventano duo.
L'eventuale comando di avvio non deve introdurre quel blocco globale. I limiti e gli effetti
delle modifiche quando esistono già voti, chiusure o qualificazioni sono ancora da definire
(OP-04); non assumere né la cancellazione né il trasferimento automatico dei voti.

La proposta [DS-001](DecisioniSemantiche.md#ds-001---correzioni-delle-coreografie-durante-la-manifestazione)
è condivisa con Marco ma **da validare con il cliente**: correzioni consentite senza voti;
conservazione dei voti se cambiano solo i componenti e non la gara; riapertura preventiva per
una coreografia chiusa, segnalando l'effetto sulle classifiche. DS-045 supera il divieto
DS-002: il cliente conferma l'esigenza di spostare coreografie già valutate cambiando
specialità, categoria o livello. La procedura condivisa con Marco, da validare, prevede
avviso e conferma, conservazione dei voti originali e dei loro autori, uscita dalla classifica
di origine e ingresso in quella di destinazione. Le gare coinvolte già chiuse sono riaperte;
le classifiche successive interessate tornano provvisorie, sono ricalcolate e richiedono
riconferma. Nessun effetto sulle classifiche estranee. Le differenze di numerosità seguono
DS-043/044, senza esclusioni automatiche. Annullare lascia tutti i dati invariati.
Ritiri, squalifiche ed eliminazione restano operazioni distinte.

DS-034, condivisa con Marco e da validare con il cliente, consente l'eliminazione di scuole,
atleti, giudici, discipline, categorie e livelli soltanto quando non utilizzati. Per le voci
utilizzate la cancellazione è bloccata con indicazione dei riferimenti, senza cancellazioni
a cascata di iscrizioni, giurie o voti né forzature. La rimozione di un'associazione resta
distinta e soggetta alle proprie regole. Nel PDF il cliente chiedeva di eliminare anche
gli atleti rimasti senza coreografie. La proposta corrente DS-006 sostituisce tale automatismo
con la sola esclusione dall'elenco partecipanti, mantenendo sempre l'anagrafica completa.
È da sottoporre al cliente: evita di dover ricreare l'atleta quando una coreografia errata
viene cancellata e poi reinserita. Il conteggio delle coreografie considera tutte le scuole.
La cancellazione esplicita dell'atleta resta distinta. DS-035, condivisa con Marco e da
validare con il cliente, consente di eliminare una coreografia soltanto in assenza di voti,
nel rispetto degli altri vincoli di stato. Anche un solo voto zero o parziale blocca
l'operazione senza modificare dati; riaprire l'esibizione non aggira il vincolo. Non si
cancellano automaticamente i voti. Per la gara non iniziata rimasta vuota vale DS-032.
DS-036 rinvia ritiri e annullamenti dopo la valutazione al confronto con il cliente (OP-04):
nessuna procedura dedicata nel perimetro attualmente definito. Restano il blocco di
eliminazione DS-035 e il divieto di Non presentata in presenza di voti DS-008. Le normali
correzioni tramite riapertura non costituiscono una procedura di ritiro o annullamento.

## 4. Inserimento voti e chiusura

DS-048 stabilisce il principio generale di reversibilità: ogni azione/stato dell'operatore
che impedisce di inserire anche un solo voto deve poter essere revocato o riaperto per
correggere errori umani. Vale per Non presentata, Squalificata, chiusure e futuri stati
analoghi. A gara chiusa occorre prima riaprirla; la revoca riporta l'esibizione aperta,
da valutare/verificare, senza perdere voti né generarne. Restano i vincoli di validità,
il ricalcolo e le riconferme: reversibile non significa ripristino automatico dei vecchi
risultati o della definitività. Per Non presentata resta il requisito iniziale di assenza
di voti. Principio richiesto da Marco, da riportare al cliente.

### Squalifica (DS-046)

La proposta condivisa con Marco consente la squalifica in qualsiasi momento, anche durante
l'esibizione e senza voti. A gara aperta è applicabile direttamente; a gara chiusa occorre
prima riaprire e poi richiudere. L'esibizione conta come gestita senza completare la scheda.
Resta visibile come Squalificata, senza posizione né accesso ai premi successivi. Eventuali
voti originali restano conservati ma esclusi dai conteggi; la coreografia non entra nel
confronto della numerosità dei giudici. L'effetto è analogo a Non presentata, con stato
distinto e senza il requisito di assenza di voti.

Anche dopo una premiazione, il ricalcolo elimina la squalificata dai piazzamenti e aggiorna
posizioni e qualificati. Le classifiche successive interessate tornano provvisorie e devono
essere riconfermate. Ricorsi, verifiche video e restituzione materiale dei premi non sono
modellati. DS-046 supera il rinvio DS-036 per le squalifiche. DS-047 consente la revoca:
se la gara è chiusa va prima riaperta; i voti conservati sono recuperati invariati e
l'esibizione torna aperta, da verificare prima di richiuderla, anche con scheda completa.
Nessun voto mancante viene generato. Si ricalcolano risultati e qualificati secondo i dati
correnti; gara e classifiche collegate richiedono le normali chiusure e riconferme, senza
ripristinare automaticamente piazzamenti storici o definitività. La procedura è da
sottoporre al cliente.

### Inserimento delle valutazioni

DS-039 conferma con Marco, il 14 ottobre, il data entry mono-esibizione. Le schede cartacee
dei giudici possono contenere più coreografie in ordine di uscita: non si richiede che la
schermata ne replichi la struttura. La raccolta ogni circa venti esibizioni è una modalità
operativa, non un blocco obbligatorio del sistema. Eventuali stampe e la richiesta di chiudere
più gare insieme restano aspetti separati, non definiti da questa decisione.

Un operatore trascrive le schede ricevute dai giudici: non si prevedono dispositivi o accessi
diretti dei giudici. La schermata operativa è dedicata a una singola esibizione e mostra i
giudici della relativa gara in una disposizione compatta e facilmente confrontabile.

DS-027 rettifica DS-026: dopo l'avvio della manifestazione (DS-014), il data entry è
disponibile senza dichiarare prima conclusa l'esibizione. La trascrizione dopo l'esibizione
è il flusso operativo, non un prerequisito software. Dopo l'inserimento completo dei voti
l'operatore può chiudere l'esibizione per proteggerne la valutazione; per correggerla deve
riaprirla. Nessuna chiusura viene dedotta dagli orari o dalla fine fisica dell'esibizione.
La decisione è condivisa con Marco e da validare con il cliente.

I voti sono riferiti alla **coreografia**, non al singolo atleta. Per ciascun giudice i criteri
iniziali sono tre, con voti tra **0 e 10 inclusi**, con un decimale:

- Tecnica;
- Interpretazione / Musicalità;
- Show.

Il cliente richiede la possibilità di aggiungere criteri: l'elenco non deve essere fisso.
La proposta DS-012, condivisa con Marco e da validare con il cliente, prevede un elenco unico
per tutte le gare, modificabile prima dell'inizio dell'evento e bloccato dopo l'avvio, anche
in assenza di voti. DS-013 propone il comando "Avvia manifestazione" con conferma esplicita:
confermare blocca i criteri, annullare lascia invariati stato e configurabilità. Anche questa
decisione è da validare con il cliente. L'avvio non blocca le altre modifiche ammesse durante
la manifestazione e non avvia implicitamente le votazioni di tutte le gare. DS-014, condivisa
con Marco e da validare con il cliente, consente l'inserimento dei voti soltanto dopo la
conferma dell'avvio: prima non sono registrabili neppure voti parziali o zero. Dopo l'avvio
restano applicabili i vincoli di gara ed esibizione; nessun voto viene generato automaticamente.
DS-015 conferma la somma dei voti di tutti i criteri e giudici, senza media o normalizzazione.
Si prevedono giurie della stessa numerosità, senza imporlo come vincolo: se il numero cambia,
la somma resta tale anche nei confronti tra gare. La decisione di Marco è da sottoporre al cliente.
Resta il pari peso dei criteri. Oggi i giudici hanno lo
stesso peso. Il precedente giudice tecnico decisivo a parimerito è solo un caso storico
citato dal cliente, non una richiesta approvata di ripristino (OP-09).
Non si introducono pesi, scarti degli estremi o spareggi non concordati. Il valore zero è
un voto valido; un voto non ancora inserito è assente e deve essere distinto da zero.

I dati vengono salvati al termine dell'editing della cella. Un valore non valido non deve
essere registrato come un voto valido; eventuali errori di salvataggio devono essere visibili.

DS-027 conferma la chiusura esplicita della singola esibizione dopo il completamento dei voti.
DS-028, condivisa con Marco e da validare con il cliente, prevede chiusura esplicita della
gara quando tutte le esibizioni sono chiuse oppure "Non presentate". L'ultima esibizione
chiusa rende la gara pronta da chiudere, non automaticamente chiusa né con classifica
definitiva. Il comando esplicito rende definitiva la classifica della gara, ferma la gestione
delle parità ancora aperta; disciplina e assoluto restano soggetti a DS-020.
Il cliente vuole scegliere quante coreografie stanno in un
"gruppo" prima di chiudere e chiede di parlarne a voce (OP-03). L'ipotesi DS-003 proposta da
Marco è che intenda il numero di coreografie nella stessa gara, non un blocco operativo
di chiusura e non il tipo Gruppo composto da almeno tre atleti. È da confermare con il
cliente e non definisce l'inserimento manuale di un numero. La proposta DS-028 non sostituisce
la necessità di validare questa interpretazione con lui.

DS-029, condivisa con Marco e da validare con il cliente, coordina la riapertura:
"Riapri esibizione", se la gara è chiusa, avvisa degli effetti e richiede conferma esplicita.
Confermando riapre anche la gara e rende provvisorie le classifiche definitive collegate
secondo DS-021; annullare lascia tutto invariato. Soltanto l'esibizione scelta viene
sbloccata, le altre restano chiuse e protette e tutti i voti sono conservati. Se la gara è
già aperta si sblocca solo l'esibizione scelta. Il ritorno alla definitività richiede
richiusura dell'esibizione, chiusura esplicita della gara e riconferma delle classifiche
collegate nel rispetto di DS-020/DS-021.

Per una coreografia non presentata il cliente propone tutti voti zero oppure eliminazione.
La proposta alternativa DS-007, condivisa con Marco e da validare con il cliente (OP-12),
conserva l'iscrizione con stato "Non presentata", visibile in fondo alla classifica dopo le
coreografie valutate ma senza posizione numerica né voti fittizi. Un'esibizione valutata zero
resta distinta dall'assenza. L'ultimo posto numerico a zero è un'alternativa da sottoporre al
cliente solo se richiesta dal regolamento, non una modalità già approvata. Restano da definire
gli effetti sulle qualificazioni; gli elenchi degli atleti sono precisati da DS-011. DS-008,
condivisa con Marco e da validare con il cliente, precisa l'accesso: conferma esplicita e
nessun voto presente. Anche un solo voto zero o parziale blocca il passaggio, senza cancellare
o nascondere voti; annullare la conferma non modifica nulla.
DS-009, condivisa con Marco e da validare con il cliente, consente la revoca con conferma
esplicita: la coreografia torna da valutare senza generare voti. Se la gara è chiusa, prima
deve essere riaperta esplicitamente; annullare la revoca lascia invariati stato e dati.
DS-010, anch'essa da validare con il cliente, considera una non presentata come gestita ai
fini della chiusura: l'assenza di voti per quella coreografia non blocca la gara. La revoca
la riporta tra quelle da valutare. Non viene introdotta una chiusura automatica: le altre
condizioni e il flusso generale restano in OP-03.

## 5. Classifiche e premi

DS-044 precisa DS-043: esclusione abilitata solo con numerosità differenti, partendo
dai giudici delle giurie più numerose e solo fra quelli con valutazioni mancanti altrove.
Non si riduce la giuria meno numerosa. Dopo ogni scelta si ricalcolano i candidati;
a numerosità uniforme si disabilitano ulteriori esclusioni, lasciando la reinclusione
per rivedere la scelta. A+B+C / A+B+D non permette esclusioni; A+B+C+D / A+B+E permette
di escludere C oppure D e poi si ferma. Nessuna selezione automatica del giudice.

DS-043 propone un controllo della numerosità delle valutazioni per ogni classifica,
anche di specialità e assoluta. In caso di differenze l'operatore sceglie quali giudici
escludere dal calcolo di tutta quella classifica. I loro voti restano conservati e visibili
ingrigiti; l'esclusione è reversibile e non si propaga automaticamente ad altre classifiche.
Si accetta uguale numerosità anche con giudici diversi, senza sostituzioni dichiarate.
I totali vengono ricalcolati dai voti utilizzabili: il riuso dei voti non implica più il
riuso del medesimo totale nei passaggi successivi. Una classifica conclusa non cambia per
la sola assenza successiva. La somma resta il criterio; niente normalizzazione. Politica
da validare con il cliente, compresi i casi con meno di tre valutazioni e discrepanze
irrisolte; nessuna deroga automatica al minimo. Vedere DS-043 per l'alternativa interna
basata sulla relazione di sostituzione, non scelta come proposta preferita.

DS-041 distingue la denominazione del livello dalle regole associate. Open è il nome
richiesto per il percorso finora descritto come non PRO; PRO può essere rinominato, ad
esempio in Semi-Pro, senza cambiare identità, iscrizioni, categoria unica o percorso
verso l'assoluto. Le denominazioni PRO/non PRO usate sotto descrivono i percorsi, non
valori testuali da riconoscere per applicare le regole. Cambiare le regole è distinto
dal rinominare: questa decisione non introduce una maschera o una procedura per farlo.

DS-050 propone una sola vista della classifica, sempre disponibile anche per gare non
iniziate e aggiornata con i voti registrati. L'indicazione è Provvisoria fino alla chiusura,
Definitiva dopo; riaprire la gara la rende nuovamente Provvisoria. Non esistono due modalità
separate. Le esibizioni senza voti sono ordinate secondo la scaletta, senza piazzamento
né zeri fittizi. La vista comprende anche le esibizioni aperte:

- quelle con voti completi sono ordinabili, ma chiaramente segnalate come non chiuse;
- quelle incomplete sono elencate senza posizione, separate/in fondo, evidenziando che
  mancano voti; non competono mediante totali parziali che trattino le assenze come zeri;
- la classifica diventa definitiva con la chiusura esplicita già definita; gli stati
  Non presentata e Squalificata restano visibili senza posizione.

DS-042 chiarisce con Marco che i voti restano 0-10 con un decimale ordinario: nessun
decimale speciale di spareggio. Le parità sono ammesse, evidenziate quando coinvolgono
il podio e non bloccano la chiusura. Tutti i primi ex aequo accedono alla classifica
successiva del proprio percorso, riutilizzando gli stessi voti, sia in Open sia in PRO.
Non si sceglie un vincitore tramite ordine grafico e non si introducono spareggi dedicati.
DS-049 propone la numerazione 1-1-3 (e 1-2-2-4) come ipotesi di Marco da convalidare
con il cliente, non come regola definitiva. Restano da precisare i premi
materiali; l'accesso di tutti i primi è invece deciso. DS-018 è superata su questi punti.
Le non presentate, secondo DS-007 da validare, figurano in fondo senza posizione e non
concorrono agli ex aequo con le coreografie valutate, nemmeno se queste hanno totale zero.

Il chiarimento riportato da Marco il 12 ottobre 2026 (DS-016) distingue premi e qualificazioni:

- **Non PRO:** gare per disciplina, categoria e livello, con premio ai primi tre di ciascuna.
  I soli primi delle gare accedono alla classifica di disciplina, comune alle categorie e ai
  livelli non PRO; qui si premiano i primi tre. Il solo primo di ogni disciplina accede
  all'assoluto non PRO, dove si premiano i primi tre.
- **PRO:** categoria unica e una gara per disciplina, con premio ai primi tre. Il primo
  di ciascuna disciplina accede direttamente all'assoluto PRO, dove si premiano i primi tre.
  Non esiste un ulteriore raggruppamento intermedio di categorie/livelli PRO.

I due percorsi restano separati. Le classifiche riuniscono coreografie, non atleti singoli.
Non si introducono premi per singolo criterio. DS-017 conferma il riuso delle votazioni
già assegnate: il punteggio della coreografia resta lo stesso, senza nuove esibizioni o voti,
sia nel percorso non PRO sia nell'assoluto PRO. I riferimenti al solo primo includono
tutti i primi ex aequo secondo DS-042.
DS-015 esclude la normalizzazione per numero di giudici.

DS-019, condivisa con Marco e da validare con il cliente, prevede che correggere un voto
dopo la riapertura della gara ricalcoli anche qualificati e classifiche di disciplina/assolute
coinvolte. Quelle già definitive tornano provvisorie, con avviso esplicito e necessità di
riconferma. La propagazione segue il percorso dei premi e mantiene separati PRO e non PRO;
non altera classifiche estranee. Eventuali parità seguono DS-042.
La procedura generale di chiusura e riapertura resta in OP-03.

DS-020, condivisa con Marco e da validare con il cliente, stabilisce le condizioni necessarie
per rendere definitive le classifiche dei premi: disciplina non PRO solo dopo la chiusura
di tutte le gare che la alimentano; assoluto non PRO solo dopo la definitività di tutte le
discipline alimentanti; assoluto PRO direttamente dopo la chiusura di tutte le gare PRO
alimentanti. Prima restano consultabili come provvisorie. Non si introduce una chiusura
automatica; le parità seguono DS-042 e non sono bloccanti.

DS-021, condivisa con Marco e da validare con il cliente, precisa che la sola riapertura
di una gara rende subito provvisorie le classifiche definitive collegate, prima di qualsiasi
correzione. La richiusura richiede riconferma esplicita delle classifiche secondo DS-020,
anche se i voti sono rimasti invariati. Nessun ritorno automatico alla definitività né
modifica ai voti per effetto della sola riapertura; le correzioni seguono DS-019.
La propagazione resta limitata al percorso coinvolto, separando PRO e non PRO.

## 6. Scaletta

La scaletta deve essere modificabile durante la manifestazione per ritardi nei cambi e altre
problematiche (OP-02 risposto). È superata l'esclusione iniziale delle modifiche in tempo reale.
DS-022, condivisa con Marco e da validare con il cliente, permette il riordino delle esibizioni
non ancora iniziate, anche alternando gare diverse, senza modificarne appartenenza o classifica.
Quelle già iniziate o concluse mantengono la posizione. Marco ammette per le gare già iniziate
o concluse la sola correzione degli orari e lo spostamento in avanti della scaletta successiva,
senza effetti su voti e risultati.

DS-023, condivisa con Marco e da validare con il cliente, stabilisce una riga per esibizione:
ordine e orari sono riferiti alla singola esibizione, non all'intera gara. Riordinare una riga
non trascina automaticamente le altre esibizioni della stessa gara.
DS-024, condivisa con Marco e da validare con il cliente, introduce "Posticipa da qui…":
selezionata un'esibizione non iniziata, l'operatore indica i minuti di posticipo. Si spostano
dello stesso intervallo lei e tutte le successive non iniziate nell'ordine della scaletta,
anche di gare diverse, conservando gli intervalli e senza riordinare le righe. Quelle già
iniziate o concluse mantengono gli orari; la correzione puntuale resta distinta. Nessun effetto
su voti o classifiche. La modifica di un singolo orario non implica questa traslazione.
DS-025, condivisa con Marco e da validare con il cliente, distingue per ogni esibizione
orario previsto ed effettivo. Il posticipo modifica solo il previsto; l'effettivo delle
esibizioni svolte è registrabile e correggibile separatamente, senza sovrascrivere il previsto.
I due restano confrontabili. Non sono introdotti automatismi di registrazione dell'effettivo
o collegamenti automatici con inserimento e chiusura dei voti, né uno storico delle revisioni
del previsto.

## 7. Riferimenti utilizzati

- Decisioni e osservazioni raccolte nella conversazione con Marco.
- Segnalazioni del cliente sul prototipo, riportate nel piano di lavoro.
- File Excel precedentemente fornito: `voti con formule 2026 nuovo.xlsx`, utilizzato durante
  la discussione dei requisiti. Non è stato nuovamente analizzato in questo consolidamento.
- Codice locale del prototipo, letto per descrivere lo stato attuale nel README.
- PDF annotato `Skating-System-documentazione-cliente-1.pdf`, ricevuto il 9 ottobre 2026:
  osservazioni alle pagine 2-7, censite nel registro di riscontro cliente.

Le osservazioni del cliente sono requisiti da analizzare e verificare, non prove di una
causa tecnica già diagnosticata. Nessuna scelta pendente viene considerata approvata.
