# Decisioni funzionali da sottoporre al cliente

> Aggiornamento 21 ottobre 2026: per i temi rivisti prevale il
> [consolidamento DS-053-DS-065](Consolidamento-2026-10-21.md).
> Il testo sottostante conserva lo storico; data entry mono-esibizione, esclusioni manuali,
> primi ex aequo e aggregazione obbligatoria per disciplina non sono più proposte correnti.
> Spareggi definiti il 21 ottobre in DS-055 (AP-01 chiuso): rettifica ufficiale di un voto
> condivisa fra classifiche. Stampa e scelta grafica restano aperte. Vedere la fonte e lo stato
> di acquisizione nel consolidamento; non attribuire approvazioni implicite agli altri punti.

## Regola di tracciamento

Dal 9 ottobre 2026 ogni decisione che modifica il significato dei dati o il comportamento
per l'organizzatore viene registrata qui con ID stabile, data, stato, regola proposta,
motivazione e questioni residue. L'accordo di Marco non equivale alla validazione del cliente.
Stati: proposta, condivisa con Marco / da validare, validata dal cliente, superata.
Un cambiamento successivo deve mantenere traccia della decisione sostituita e della fonte.

Le scelte puramente implementative si registrano in [Decisioni tecniche](DecisioniTecniche.md)
e non entrano nel documento di validazione del cliente. Se una scelta tecnica introduce un
vincolo o un effetto visibile sui dati o sul lavoro dell'operatore, la parte funzionale va
registrata anche qui. Le richieste pregresse restano tracciate nel modello e nel riscontro
cliente: questo registro non attribuisce retroattivamente nuove approvazioni.

## DS-001 - Correzioni delle coreografie durante la manifestazione

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da verificare con il cliente.
- **Fonte:** confronto su OP-04 e conferma di Marco nella conversazione.
- **Collegamenti:** OP-04; OP-03 per chiusura/riapertura; OP-06 per appartenenza alla gara.

### Regola proposta

1. Senza voti inseriti si possono correggere componenti, disciplina, categoria, livello e
   tipo della coreografia, oppure ritirarla, nel rispetto dei vincoli di iscrizione applicabili.
2. Se sono presenti voti, le modifiche non devono avere effetti silenziosi sui risultati.
   Se cambiano soltanto i componenti e la coreografia resta nella stessa gara, i voti vengono
   mantenuti: valutano l'esibizione nel suo insieme, non i singoli atleti.
3. Precisazione successiva DS-002: se esiste almeno un voto, lo spostamento in un'altra gara
   è vietato. Sostituisce la precedente ipotesi di una procedura ordinaria di gestione dei voti
   durante lo spostamento; tale procedura non viene introdotta.
4. Per una coreografia già chiusa è necessaria prima la riapertura, segnalando l'effetto
   sulle classifiche. La granularità concreta della chiusura resta da chiarire in OP-03.
5. La giuria è modificabile fino all'avvio della relativa gara, come richiesto dal cliente.

Un passaggio da Gruppo a Duo può cambiare anche la gara: in quel caso è uno spostamento,
non una semplice correzione dei componenti. La regola di mantenimento dei voti vale solo se
si rimane nella medesima gara; non autorizza altri cambiamenti impliciti.

### Motivazione e conferma richiesta

Consentire le correzioni reali del giorno della manifestazione senza perdere i voti per una
semplice rettifica dei componenti, ma rendere espliciti gli effetti sulle gare e sui risultati.
Chiedere al cliente conferma della distinzione, in particolare del mantenimento dei voti
quando cambia la composizione ma non la gara.

### Da decidere separatamente

- Eventuale manovra eccezionale di spostamento, solo su richiesta esplicita futura (DS-002).
- Effetti su classifiche già chiuse e qualificazioni ai premi successivi.
- Procedura per ritiri quando esistono già voti.
- Chiusura e riapertura della gara/esibizione e interpretazione del "gruppo" (OP-03, DS-003).

OP-04 resta aperto: questa proposta ne definisce una parte, non ne conclude la validazione.

## DS-002 - Blocco dello spostamento di coreografie già giudicate

**Superata il 14 ottobre 2026 da DS-045:** il cliente conferma il caso reale di spostamento
di coreografie già valutate. La nuova procedura è proposta da validare; segue il testo storico.

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferme di Marco nella conversazione; nessuna richiesta esplicita di questo
  caso d'uso individuata nel riscontro del cliente.
- **Collegamenti:** OP-04; precisa e sostituisce il punto 3 originario di DS-001.

### Regola proposta

- Senza voti inseriti lo spostamento resta consentito, nel rispetto degli altri vincoli.
- Con almeno un voto inserito, anche se la valutazione è incompleta o il voto è zero,
  lo spostamento in un'altra gara è bloccato con una spiegazione visibile.
- Il blocco riguarda anche modifiche indirette che cambino la gara di appartenenza,
  ad esempio un cambio di tipo da Gruppo a Duo quando comporta uno spostamento.
- La riapertura di una coreografia non rimuove questo vincolo. Rimane distinta la correzione
  dei componenti nella stessa gara prevista da DS-001.
- Non si introduce trasferimento, azzeramento o archiviazione automatica dei voti tra gare,
  né un comando per forzare lo spostamento.

### Eccezioni e perimetro

Un eventuale spostamento di una coreografia già giudicata a gara iniziata è un caso ultra
eccezionale: resta vietato in prima battuta. Una manovra esplicita potrà essere concordata
se il cliente ne richiederà la gestione, definendone prima effetti e protezioni. Non è
compresa nell'implementazione ordinaria e non è autorizzata implicitamente da questa nota.
La regola è basata sulla presenza dei voti della coreografia, non sul solo avvio della
manifestazione o della gara; non introduce un nuovo blocco globale.

### Verifiche funzionali

- Nessun voto: spostamento consentito se gli altri vincoli sono soddisfatti.
- Un solo voto, anche zero: spostamento rifiutato senza modificare gara o voti.
- Valutazione completa o riaperta con voti: stesso blocco.
- Modifica indiretta della gara: stesso blocco.
- Rettifica dei soli componenti senza cambio gara: comportamento DS-001.

## DS-003 - Significato di "gruppo" nella richiesta sulla chiusura

- **Data:** 9 ottobre 2026.
- **Stato:** ipotesi interpretativa proposta da Marco, da confermare con il cliente.
- **Fonte:** osservazione del cliente su OP-03 e successiva lettura proposta da Marco.
- **Collegamenti:** OP-03; OP-06 per composizione delle gare.

### Ipotesi da sottoporre al cliente

La richiesta di scegliere quante coreografie stanno in un "gruppo" prima di chiudere
si riferisce al numero di coreografie appartenenti a una singola gara, non a un blocco
operativo di esibizioni da chiudere insieme né al tipo Gruppo composto da più atleti.

È l'ipotesi di lavoro corrente, non una risposta già confermata. Sostituisce come lettura
preferita l'interpretazione di un blocco di chiusura, che non era stata approvata.
Non introduce una nuova entità, un numero da inserire manualmente, un limite di capienza
o una regola di chiusura automatica. La scelta fra chiusura per esibizione e per gara,
e le relative condizioni di completamento, restano da chiarire.

### Domanda di conferma

"Quando parli di scegliere quante coreografie stanno in un gruppo prima di chiudere,
intendi quante coreografie partecipano alla stessa gara?"

OP-03 rimane aperto fino alla risposta del cliente e alla definizione del flusso di chiusura.

## DS-004 - Scuola rappresentata dalla coreografia e identità dell'atleta

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** riscontro cliente sulla partecipazione per scuole diverse e conferma di Marco
  alla proposta funzionale nella conversazione.
- **Collegamenti:** OP-07; OP-11 e DS-006 per visibilità degli atleti senza coreografie.

### Regola proposta

- Ogni atleta ha un'unica anagrafica, senza essere duplicato per ciascuna scuola rappresentata.
- Ogni coreografia rappresenta una sola scuola, comune all'iscrizione nel suo insieme.
- Lo stesso atleta può comparire in una coreografia per la scuola A e in un'altra per la scuola B.
- L'elenco degli atleti di una scuola è ricavato dalle coreografie iscritte per quella scuola,
  contando ciascun atleta una sola volta nell'elenco anche se presente in più coreografie.
- Non si introducono coreografie che rappresentano contemporaneamente più scuole: non sono
  state richieste. Questa delimitazione del perimetro va sottoposta al cliente.

La scuola della coreografia non implica un'appartenenza personale esclusiva dell'atleta e
non introduce controlli su iscrizioni scolastiche esterne alla manifestazione. Una gestione
separata di tesseramenti o appartenenze scolastiche non è decisa da questa proposta.

### Verifiche funzionali

- Un atleta in due coreografie per scuole diverse mantiene la medesima identità e compare
  negli elenchi di entrambe le scuole.
- Più coreografie della stessa scuola non duplicano l'atleta nel relativo elenco.
- Ogni coreografia ha una sola scuola rappresentata.
- Le verifiche sulle altre coreografie dell'atleta considerano tutte le scuole. La successiva
  DS-006 mantiene l'anagrafica anche senza coreografie e ne aggiorna solo la visibilità tra
  i partecipanti, sostituendo l'ipotesi di eliminazione automatica richiamata in origine.

OP-07 resta in attesa di validazione del cliente, con DS-004 come proposta concordata con Marco.

## DS-005 - Blocco delle composizioni duplicate nella stessa gara

**Superata il 14 ottobre 2026 da DS-040:** il cliente ammette coreografie con gli stessi
atleti nella stessa gara, purché con nomi diversi. Il testo seguente è storico.

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** richiesta del cliente di segnalare l'errore e conferma di Marco al mantenimento
  del blocco, senza possibilità di forzare il duplicato.
- **Collegamenti:** OP-08; OP-06 per la definizione della gara.

### Regola proposta

Nella stessa gara non possono essere iscritte due coreografie con la medesima composizione
di atleti. Il confronto riguarda le identità degli atleti, indipendentemente dall'ordine
in cui sono elencati; non basta cambiare il nome della coreografia per renderla diversa.
Il controllo vale sia in inserimento sia nelle modifiche che producono un duplicato.

In caso di conflitto, l'operazione viene bloccata e il messaggio indica la coreografia già
presente e gli atleti coincidenti, così che l'operatore possa individuare e correggere l'errore.
Non viene introdotto un comando per proseguire comunque: eventuali deroghe richiederanno
una futura richiesta esplicita e una nuova decisione funzionale.

Il vincolo non impedisce a un atleta di partecipare a composizioni differenti nella stessa
gara, né estende il divieto a gare diverse. La definizione dell'ambito gara resta in OP-06.

### Verifiche funzionali

- Stessa gara e identico insieme di atleti, anche in ordine diverso: blocco con spiegazione.
- Nome diverso della coreografia ma medesima composizione nella stessa gara: stesso blocco.
- Modifica che produce un duplicato: blocco senza alterare l'iscrizione esistente.
- Composizioni diverse con alcuni atleti in comune: nessun conflitto per questa regola.
- Stessa composizione in gare diverse: nessun conflitto per questa regola.

OP-08 resta da validare con il cliente; la proposta condivisa con Marco è il blocco esplicito.

## DS-006 - Anagrafica completa e atleti partecipanti

- **Data:** 9 ottobre 2026.
- **Stato:** proposta di Marco, da sottoporre al cliente in alternativa alla sua richiesta
  di cancellazione automatica degli atleti rimasti senza coreografie.
- **Fonte:** caso d'uso e proposta di Marco nella conversazione.
- **Collegamenti:** OP-11; DS-004 per identità unica e scuole rappresentate.

### Regola proposta

- L'elenco completo degli atleti comprende anche quelli senza coreografie. È utilizzabile
  per popolare anticipatamente l'anagrafica e selezionare rapidamente i componenti di nuove coreografie.
- L'elenco degli atleti partecipanti mostra soltanto quelli attualmente presenti in almeno
  una coreografia, considerando tutte le scuole rappresentate e senza duplicare gli atleti.
- Eliminare una coreografia non elimina automaticamente nessuna anagrafica atleta.
- Se l'atleta perde l'ultima coreografia, scompare dall'elenco dei partecipanti ma resta
  nell'elenco completo, selezionabile con la stessa identità per una nuova iscrizione.
- L'eventuale eliminazione esplicita dell'anagrafica è un'operazione distinta, soggetta ai
  controlli sui riferimenti; non è un effetto collaterale della cancellazione della coreografia.

### Motivazione da presentare al cliente

Durante la preparazione si può creare un atleta, iscriverlo a una coreografia e cancellare
quest'ultima per correggere un errore prima di inserirne un'altra. La cancellazione automatica
costringerebbe a ricreare l'atleta. Separare anagrafica e partecipanti mantiene puliti gli
elenchi di gara senza perdere il lavoro già fatto né creare identità nuove inutilmente.

### Ambito e verifiche

- Atleta appena creato senza coreografie: presente nell'anagrafica, assente dai partecipanti,
  selezionabile per una coreografia.
- Inserimento nella prima coreografia: compare tra i partecipanti con la stessa identità.
- Cancellazione dell'ultima coreografia: sparisce solo dai partecipanti; successiva iscrizione
  possibile senza ricrearlo.
- Cancellazione di una delle sue coreografie: resta partecipante se ne esiste almeno un'altra,
  anche per una scuola diversa.
- Gli elenchi per scuola derivati dalle coreografie (DS-004) non equivalgono all'anagrafica
  completa: un atleta senza coreografie non riceve un'appartenenza scolastica implicita.

Questa proposta non decide quando sia consentito cancellare una coreografia già giudicata,
né gli effetti di uno stato di ritiro/non presentazione (OP-04 e OP-12). La precedente proposta
dell'assistente di limitare la cancellazione alle coreografie senza voti non è stata approvata
da questa risposta e non diventa una regola acquisita.
OP-11 resta da validare con il cliente, evidenziando esplicitamente la modifica rispetto al PDF.

## DS-007 - Non presentata: visibile in fondo, senza posizione né voti fittizi

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** confronto su OP-12 e conferma di Marco alla proposta di visualizzazione.
- **Collegamenti:** OP-12; OP-03 per chiusura; DS-006 per distinzione fra anagrafica e partecipanti.

### Regola proposta

- Rappresentare la mancata esibizione con la dicitura esplicita "Non presentata".
- Conservare l'iscrizione, la coreografia e i componenti, distinguendo l'assenza da una
  cancellazione per errore di inserimento.
- Visualizzarla in fondo alla classifica, dopo le coreografie valutate, senza posizione
  numerica: compare nel resoconto ma non è classificata.
- Non generare voti o punteggi zero per simulare l'assenza. Zero resta un voto valido di
  un'esibizione effettivamente valutata; non crea ex aequo con le non presentate.

### Alternativa da discutere con il cliente

Se il regolamento richiede un ultimo posto numerico con punteggio zero, sottoporre questa
alternativa al cliente. Non è la regola scelta dalla proposta corrente e non diventa una
seconda modalità configurabile senza una decisione ulteriore. La richiesta originaria del
PDF (tutti zero oppure eliminazione) resta documentata nel registro del riscontro.

### Aspetti non ancora decisi

La conferma di questa proposta riguarda lo stato e la sua resa nella classifica. Non assume
come già approvate le precedenti ipotesi dell'assistente su chiusura o blocchi operativi.
Restano da definire: passaggio allo stato e sua revoca, caso con voti già presenti, effetto
sulla chiusura della gara, ordine rispetto alle esibizioni incomplete e distinzione negli
elenchi tra iscritti e atleti che hanno effettivamente gareggiato.

### Verifiche funzionali

- Una non presentata resta consultabile senza posizione numerica e senza voti creati automaticamente.
- È visualizzata dopo tutte le coreografie valutate, anche quelle con punteggio effettivo zero.
- Non condivide il piazzamento o l'ex aequo delle esibizioni valutate zero.
- La segnalazione di assenza non cancella automaticamente iscrizione o anagrafiche.

OP-12 resta da validare con il cliente e da completare negli aspetti operativi indicati.

## DS-008 - Accesso allo stato Non presentata

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma di Marco alla proposta operativa successiva a DS-007.
- **Collegamenti:** OP-12 e DS-007.

### Regola proposta

Il passaggio a "Non presentata" richiede una conferma esplicita ed è consentito soltanto
se non è presente alcun voto per la coreografia. Anche un solo voto, inclusi zero e una
valutazione parziale, blocca il cambio con spiegazione visibile. Non cancellare, azzerare
o nascondere voti per consentire il passaggio. Annullare la conferma lascia tutto invariato.

Questa decisione precisa l'accesso allo stato lasciato aperto in DS-007. Non definisce la
revoca dello stato, le altre condizioni legate a gare già chiuse, gli effetti sulla chiusura
della gara o sugli elenchi degli atleti: tali aspetti restano aperti in OP-12/OP-03.

### Verifiche funzionali

- Nessun voto e conferma esplicita: passaggio consentito, fatti salvi gli altri vincoli da definire.
- Conferma annullata: stato e dati invariati.
- Almeno un voto, anche zero o parziale: passaggio bloccato senza alterare stato o voti.
- Il messaggio di blocco spiega che la coreografia ha già voti inseriti.

## DS-009 - Revoca dello stato Non presentata

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma di Marco alla proposta di revoca successiva a DS-008.
- **Collegamenti:** OP-12, OP-03, DS-007 e DS-008.

### Regola proposta

La revoca di "Non presentata" richiede conferma esplicita e riporta la coreografia da
valutare, senza generare voti. Se la gara è già chiusa deve essere prima riaperta: la revoca
non modifica silenziosamente una classifica definitiva e non riapre implicitamente la gara.
Annullare la conferma lascia invariati stato e dati.

La decisione risolve il punto sulla revoca lasciato aperto in DS-007/DS-008. Restano da
definire la procedura generale di riapertura della gara (OP-03), l'effetto delle non presentate
sulle condizioni di chiusura e sugli elenchi degli atleti, nonché eventuali qualificazioni
già determinate per premi successivi.

### Verifiche funzionali

- Gara aperta e revoca confermata: coreografia da valutare, nessun voto creato.
- Revoca annullata: stato e dati invariati.
- Gara chiusa: revoca non eseguibile prima della riapertura esplicita della gara.
- Dopo riapertura: la revoca segue la stessa conferma prevista a gara aperta.

## DS-010 - Non presentata gestita ai fini della chiusura

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma di Marco alla proposta successiva a DS-009.
- **Collegamenti:** OP-12, OP-03, DS-007, DS-008 e DS-009.

### Regola proposta

Una coreografia in stato "Non presentata" conta come gestita ai fini della chiusura della
gara: la sua assenza di voti non impedisce la chiusura. Resta senza voti fittizi e senza
posizione numerica, secondo DS-007. La revoca la riporta tra le coreografie ancora da valutare,
rispettando la riapertura preventiva della gara chiusa prevista da DS-009.

La decisione risolve questo aspetto lasciato aperto in DS-007/DS-008/DS-009, ma non rende
automatica la chiusura della gara né decide le altre condizioni o il comando di chiusura:
il flusso generale resta da chiarire in OP-03. Restano aperti gli effetti sugli elenchi degli
atleti e sulle eventuali qualificazioni ai premi successivi.

### Verifiche funzionali

- Una non presentata non è segnalata come valutazione mancante che blocca la chiusura.
- Le altre coreografie ancora da valutare restano tali; lo stato di una non presentata
  non soddisfa le condizioni di completamento delle altre iscrizioni.
- La revoca riporta la coreografia tra quelle da valutare, senza creare voti.
- Il conteggio come gestita non attribuisce punteggio né posizione in classifica.

## DS-011 - Partecipanti come iscritti, distinti dagli effettivamente esibiti

- **Data:** 9 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma di Marco alla proposta successiva a DS-010.
- **Collegamenti:** OP-12, DS-006 e DS-007.

### Regola proposta

L'elenco dei partecipanti comprende gli atleti attualmente iscritti ad almeno una coreografia,
anche se questa risulta "Non presentata". Il passaggio a tale stato non cancella l'iscrizione
e non fa scomparire i componenti dall'elenco dei partecipanti. L'atleta senza alcuna coreografia
resta invece soltanto nell'anagrafica completa, secondo DS-006.

L'eventuale conteggio degli atleti effettivamente esibiti è un dato distinto, non un altro
significato attribuito alla parola partecipanti. Non si introduce in questa decisione una
nuova schermata o una formula di calcolo dell'effettiva esibizione, né la si deduce dalla
sola presenza di voti: questi dettagli potranno essere concordati se necessari.

### Verifiche funzionali

- Atleta iscritto soltanto a una coreografia non presentata: resta nell'elenco partecipanti.
- Passaggio a Non presentata o sua revoca: nessuna perdita dell'iscrizione o dell'anagrafica.
- Eliminazione dell'ultima coreografia: uscita dai partecipanti ma permanenza nell'anagrafica.
- Atleta presente in più coreografie, anche di scuole diverse: una sola presenza nell'elenco.

Questa decisione risolve l'aspetto degli elenchi lasciato aperto nelle decisioni precedenti
sulle non presentate. Rimangono da chiarire le qualificazioni ai premi (OP-05) e da validare
con il cliente tutte le proposte DS-007-DS-011.

## DS-012 - Criteri uniformi, configurabili prima dell'evento

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** Marco conferma criteri uniformi su tutte le gare, modificabili prima dell'inizio dell'evento.
- **Collegamenti:** OP-10, OP-09.

### Regola proposta

La manifestazione usa un unico elenco di criteri di valutazione, uguale per tutte le gare.
L'elenco è configurabile prima dell'inizio dell'evento; dopo l'inizio non è modificabile.
Non sono previsti criteri specifici per singola gara. I criteri iniziali restano Tecnica,
Interpretazione / Musicalità e Show.

Il limite è l'inizio dell'evento, non l'inserimento del primo voto: non si assume che i due
momenti coincidano. Resta da definire come dichiarare l'avvio nel sistema. Questo blocco
riguarda i criteri e non introduce un blocco generale di atleti, coreografie, scaletta o giurie,
le cui regole restano quelle dei rispettivi punti aperti e decisioni.

L'uniformità dei criteri non risolve il confronto tra gare con numerosità di giuria diversa
(OP-09), né modifica pesi, calcolo dei totali o regole dei premi.

### Verifiche funzionali

- Prima dell'avvio: modificare i criteri aggiorna l'elenco comune a tutte le gare.
- Dopo l'avvio: nessuna modifica ai criteri, anche se non è ancora stato inserito un voto.
- Non è possibile assegnare a una gara un elenco diverso da quello della manifestazione.
- Il blocco dei criteri non blocca implicitamente le altre operazioni ammesse durante l'evento.

## DS-013 - Avvio esplicito della manifestazione

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma di Marco al comando proposto dopo DS-012.
- **Collegamenti:** OP-10, DS-012.

### Regola proposta

Il comando "Avvia manifestazione" richiede conferma esplicita. Confermandolo, la manifestazione
risulta avviata e i criteri comuni non sono più modificabili, secondo DS-012. Annullare la
conferma lascia invariati lo stato e la possibilità di configurare i criteri.

L'avvio non blocca indiscriminatamente atleti, coreografie, scaletta o giurie: restano valide
le regole specifiche delle operazioni ammesse durante l'evento. Non equivale ad avviare la
votazione di tutte le gare né a chiuderle. Questa decisione risolve il meccanismo di avvio
lasciato aperto in DS-012; non introduce una revoca dell'avvio o un ripristino dei dati.
Il rapporto tra avvio e abilitazione dell'inserimento voti resta da definire.

### Verifiche funzionali

- Conferma dell'avvio: stato avviato e criteri bloccati, anche senza voti inseriti.
- Annullamento della conferma: nessuna modifica di stato o dei criteri.
- Dopo l'avvio restano disponibili le altre operazioni ammesse dalle rispettive regole.

## DS-014 - Inserimento voti solo dopo l'avvio

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma di Marco al vincolo proposto dopo DS-013.
- **Collegamenti:** OP-10, DS-012 e DS-013.

### Regola proposta

L'inserimento dei voti è consentito soltanto dopo la conferma del comando "Avvia
manifestazione". Prima dell'avvio non è possibile registrare valutazioni, neppure parziali
o con valore zero: non devono esistere voti mentre i criteri sono ancora modificabili.

L'avvio abilita l'inserimento senza creare voti né avviare automaticamente tutte le gare.
Restano applicabili i vincoli specifici di gara ed esibizione, incluse le chiusure e lo stato
"Non presentata". Questa decisione risolve il rapporto tra avvio e inserimento voti lasciato
aperto in DS-013; non introduce una procedura di annullamento dell'avvio.

### Verifiche funzionali

- Prima dell'avvio: nessun voto registrabile, anche zero o parziale.
- Conferma dell'avvio: inserimento abilitato, criteri bloccati, nessun voto generato.
- Annullamento della conferma di avvio: inserimento voti ancora non consentito.
- Dopo l'avvio: i vincoli delle singole gare ed esibizioni restano rispettati.

## DS-015 - Punteggio come somma, senza normalizzazione

- **Data:** 12 ottobre 2026.
- **Stato:** decisione di Marco, da sottoporre al cliente.
- **Fonte:** Marco richiede la somma anziché la media; prevede giurie della stessa numerosità.
- **Collegamenti:** OP-09, DS-012.

Il punteggio è la somma dei voti dei criteri di tutti i giudici, senza divisione per il
numero dei giudici. Non si introduce un vincolo di pari numerosità tra le giurie: qualora
fossero diverse, si userebbe comunque la somma, accettandone la diversa scala. Nessuna
normalizzazione automatica o nuovo spareggio. Il giudice tecnico resta un precedente storico.

Verifica funzionale: a parità di voti individuali, aggiungere un giudice aumenta il totale;
il sistema non converte il risultato in media nei confronti tra gare.

## DS-016 - Percorso dei premi di gara, disciplina e assoluto

- **Data:** 12 ottobre 2026.
- **Stato:** chiarimento riportato da Marco, da includere nella documentazione per il cliente.
- **Fonte:** spiegazione di Marco sul funzionamento dei premi speciali e dell'assoluto.
- **Collegamenti:** OP-01, OP-05, OP-06, DS-015.

### Percorso non PRO

Ogni disciplina comprende gare distinte per categoria e livello. In ciascuna gara vengono
premiati i primi tre. I soli primi classificati delle gare confluiscono nella classifica
di disciplina, che riunisce categorie e livelli non PRO e premia i primi tre. I soli primi
classificati di ogni disciplina confluiscono nella classifica assoluta, che premia i primi tre.

Essere sul podio non equivale a qualificarsi al passaggio successivo: secondo la regola
descritta, il secondo e il terzo sono premiati ma non accedono alla classifica successiva.
Il caso dei primi posti ex aequo resta da chiarire, senza scegliere arbitrariamente un vincitore.

### Percorso PRO

Il PRO resta separato dal non PRO in tutto il percorso. Ha categoria unica, quindi una gara
PRO per disciplina, con premio ai primi tre. Non serve una classifica intermedia che riunisca
categorie e livelli PRO: il primo di ciascuna disciplina accede direttamente all'assoluto PRO,
anch'esso con premio ai primi tre. Non si duplicano gara PRO e classifica di disciplina PRO.

### Confini e verifiche

- I qualificati sono coreografie, coerentemente con l'entità valutata, non singoli atleti.
- Una gara non PRO con tre classificati premia tutti e tre, ma qualifica soltanto il primo.
- Una disciplina non PRO riunisce i vincitori delle sue gare; il suo primo accede all'assoluto.
- Il vincitore della gara PRO accede all'assoluto PRO, mai a quello non PRO.
- Resta da confermare se i passaggi riusano i punteggi o prevedono nuove esibizioni/voti.
- Restano aperti ex aequo ai confini del podio/qualificazione ed effetti delle correzioni
  alle classifiche di origine. Non vengono introdotti premi separati per singolo criterio.

## DS-017 - Riuso delle votazioni per disciplina e assoluto

- **Data:** 12 ottobre 2026.
- **Stato:** chiarimento confermato da Marco, da includere nella documentazione per il cliente.
- **Fonte:** conferma esplicita che si usano le votazioni già assegnate.
- **Collegamenti:** OP-05, DS-015 e DS-016.

Le classifiche di disciplina e assolute riutilizzano le votazioni della gara di origine.
Non prevedono nuove esibizioni né nuove votazioni. Ogni coreografia ammessa mantiene il
proprio punteggio, calcolato come somma secondo DS-015; cambia l'insieme delle coreografie
messe a confronto, secondo il percorso di qualificazione DS-016.

La regola vale sia per il percorso non PRO sia per l'assoluto PRO. Risolve il punto sul
riuso dei voti lasciato aperto in DS-016. Restano da concordare ex aequo nelle qualificazioni
e gestione delle correzioni dei risultati di origine, senza introdurre in questa decisione
automatismi di riapertura o ripubblicazione delle classifiche.

### Verifiche funzionali

- Una coreografia qualificata conserva lo stesso punteggio nei successivi confronti.
- L'accesso alla classifica di disciplina o assoluta non richiede una nuova esibizione.
- Non si richiede né si registra una nuova serie di voti per i premi successivi.
- I percorsi PRO e non PRO restano separati anche se utilizzano la stessa regola di calcolo.

## DS-018 - Gestione delle parità rimessa al cliente

**Aggiornamento:** DS-042 supera il punto aperto sulle qualificazioni e l'ipotesi di
spareggio: parità ammesse, tutti i primi ex aequo accedono al passaggio successivo.
Il testo seguente conserva lo stato storico della discussione.

- **Data:** 12 ottobre 2026.
- **Stato:** punto di attenzione da sottoporre al cliente; nessuna regola di spareggio approvata.
- **Fonte:** Marco chiede di lasciare aperto il trattamento degli ex aequo.
- **Collegamenti:** OP-05, OP-09, DS-016 e DS-017.

Non viene approvata la proposta di ammettere automaticamente tutti i primi ex aequo alla
classifica successiva. Occorre chiedere al cliente come si risolvono le parità nelle singole
gare, chi determina l'esito e come questo incide su podio e qualificazioni.

Marco ipotizza che lo spareggio avvenga già nella gara di origine, rendendo univoco il suo
vincitore. È un'ipotesi, non una regola acquisita. Anche con vincitori univoci delle singole
gare, coreografie provenienti da gare diverse possono avere lo stesso totale: va quindi
chiesta anche la gestione delle parità nelle classifiche di disciplina e assolute, PRO incluse.

La precedente impostazione di ex aequo con posizioni 1, 1, 3 non è più da considerare una
regola definitiva per premi e qualificazioni: il trattamento delle parità è riaperto in
attesa del cliente. Non si introducono criteri di spareggio, correzioni dei voti, selezioni
arbitrarie o automatismi di qualificazione. L'ordine grafico non determina un vincitore.

## DS-019 - Propagazione delle correzioni ai risultati successivi

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della proposta sulle correzioni dei voti dopo la chiusura.
- **Collegamenti:** OP-03, OP-05, DS-016, DS-017 e DS-018.

### Regola proposta

Dopo la riapertura di una gara, la correzione di un voto comporta il ricalcolo dei risultati
di origine e dei qualificati e delle classifiche di disciplina/assolute coinvolte. Le
classifiche collegate già definitive tornano provvisorie, con un avviso esplicito e necessità
di riconferma: non devono restare definitive sulla base di risultati superati.

La propagazione segue il percorso dei premi: gara non PRO, disciplina, assoluto non PRO;
per il PRO, gara di disciplina e assoluto PRO. Non modifica classifiche estranee al percorso
coinvolto né mescola i due percorsi. La procedura generale di chiusura/riapertura resta da
concordare in OP-03. Se la correzione introduce una parità, non viene inventato un vincitore:
resta applicabile il punto aperto DS-018.

### Verifiche funzionali

- Una correzione che cambia il vincitore aggiorna i qualificati delle classifiche collegate.
- Le classifiche coinvolte già definitive diventano provvisorie e richiedono riconferma.
- L'operatore riceve un avviso esplicito sul ritorno allo stato provvisorio.
- Una correzione PRO non altera le classifiche non PRO e viceversa.
- Nessuno spareggio automatico non concordato viene introdotto dal ricalcolo.

## DS-020 - Condizioni per rendere definitive le classifiche dei premi

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma delle condizioni di definitività proposte dopo DS-019.
- **Collegamenti:** OP-03, OP-05, DS-016, DS-018 e DS-019.

### Regola proposta

La classifica di disciplina non PRO può diventare definitiva soltanto quando tutte le
gare che la alimentano sono chiuse. L'assoluto non PRO può diventare definitivo soltanto
quando tutte le classifiche di disciplina che lo alimentano sono definitive.

L'assoluto PRO dipende direttamente dalle gare PRO delle discipline, senza classifica
intermedia aggiuntiva: tutte queste gare devono essere chiuse prima di renderlo definitivo.
Prima del soddisfacimento dei rispettivi requisiti le classifiche restano consultabili
come provvisorie.

Queste sono condizioni necessarie, non un comando di chiusura automatica. Restano aperti
il flusso generale di chiusura/riapertura (OP-03) e la gestione delle parità (DS-018).
La decisione non sceglie come propagare la sola riapertura senza correzioni; per le
correzioni successive alla riapertura vale DS-019.

### Verifiche funzionali

- Una gara alimentante ancora aperta impedisce di rendere definitiva la disciplina non PRO.
- Una disciplina alimentante ancora provvisoria impedisce di rendere definitivo l'assoluto non PRO.
- Una gara PRO alimentante ancora aperta impedisce di rendere definitivo l'assoluto PRO.
- In attesa del completamento le classifiche restano consultabili come provvisorie.
- I requisiti dei due percorsi non si mescolano: le gare non PRO non bloccano l'assoluto PRO e viceversa.

## DS-021 - Riapertura con immediata perdita della definitività collegata

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma alla proposta successiva a DS-020.
- **Collegamenti:** OP-03, OP-05, DS-019 e DS-020.

### Regola proposta

La sola riapertura di una gara rende immediatamente provvisorie le classifiche collegate
già definitive, senza attendere una modifica dei voti. Per una gara non PRO la propagazione
riguarda la sua classifica di disciplina e l'assoluto non PRO; per una gara PRO riguarda
l'assoluto PRO. Le classifiche estranee al percorso coinvolto restano invariate.

Alla richiusura della gara le classifiche collegate richiedono riconferma esplicita nel
rispetto delle condizioni DS-020, anche se nessun voto è cambiato. Non tornano definitive
automaticamente. La riapertura da sola non modifica i voti; eventuali correzioni comportano
il ricalcolo previsto da DS-019. Questa decisione risolve il punto lasciato aperto in DS-020
sull'effetto della sola riapertura; il flusso generale di chiusura resta in OP-03.

### Verifiche funzionali

- Riaprire una gara rende subito provvisorie le classifiche definitive collegate.
- Richiudere senza modificare voti non ripristina automaticamente la definitività.
- La riconferma segue le dipendenze DS-020, mantenendo separati PRO e non PRO.
- La riapertura non altera voti né classifiche estranee al percorso coinvolto.

## DS-022 - Riordino della scaletta e aggiornamento degli orari

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente; dettaglio temporale da completare.
- **Fonte:** approvazione del riordino delle esibizioni non iniziate e precisazione di Marco sugli orari.
- **Collegamenti:** OP-02.

### Regola proposta

Le esibizioni non ancora iniziate possono essere riordinate, anche alternando gare diverse
per agevolare i cambi costume. La posizione nella scaletta non modifica l'appartenenza alla
gara né le classifiche. Le esibizioni già iniziate o concluse mantengono la propria posizione.

Marco precisa che per le gare già iniziate o concluse si possono modificare soltanto gli
orari e spostare in avanti la scaletta delle gare successive. La correzione temporale non
riapre la valutazione e non modifica voti o risultati. È quindi prevista la possibilità di
posticipare la parte successiva della scaletta, senza riordinare quanto già iniziato o concluso.

Resta da precisare se gli orari siano gestiti per gara, per singola esibizione o a entrambi
i livelli, tenendo conto della possibilità di alternare esibizioni di gare diverse. Restano
da definire il comando di traslazione, il suo punto di partenza e il rapporto tra orari
previsti ed effettivi. Non si assume un ricalcolo automatico a ogni modifica di orario.

### Verifiche funzionali

- Riordinare un'esibizione non iniziata non ne cambia gara, voti o classifica.
- Esibizioni già iniziate o concluse non vengono riordinate.
- È possibile correggere gli orari del già iniziato/concluso senza riaprire le valutazioni.
- È possibile posticipare la parte successiva della scaletta senza modificare i risultati.

## DS-023 - Scaletta per singola esibizione

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** scelta esplicita di Marco: una riga per esibizione.
- **Collegamenti:** OP-02, DS-022.

La scaletta contiene una riga per esibizione, non una riga per gara. Ordine e orari della
scaletta sono quindi riferiti alle singole esibizioni; la gara rimane l'appartenenza della
coreografia e non viene modificata dal riordino. Possono alternarsi esibizioni di gare diverse.

I vincoli DS-022 di riordino e correzione degli orari si applicano a questa granularità.
La decisione risolve il punto gara/esibizione; restano da concordare distinzione tra orari
previsti ed effettivi, comando di traslazione e scelta delle righe successive interessate.

### Verifiche funzionali

- Una gara con più esibizioni occupa più righe della scaletta.
- Riordinare una riga non sposta automaticamente tutte le esibizioni della stessa gara.
- Alternare esibizioni di gare diverse non ne modifica l'appartenenza o la classifica.

## DS-024 - Posticipo esplicito della scaletta dalla riga selezionata

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma del comando "Posticipa da qui…" proposto dopo DS-023.
- **Collegamenti:** OP-02, DS-022 e DS-023.

### Regola proposta

L'operatore seleziona un'esibizione non ancora iniziata e usa il comando "Posticipa da qui…",
indicando il numero di minuti di posticipo. Il comando sposta dello stesso intervallo gli
orari dell'esibizione selezionata e di tutte le successive non iniziate nell'ordine della
scaletta, anche se appartengono a gare diverse. Gli intervalli tra le esibizioni interessate
restano invariati e l'ordine delle righe non cambia.

Le esibizioni già iniziate o concluse non sono interessate dal comando e conservano i propri
orari. La loro correzione puntuale resta distinta, secondo DS-022. Il posticipo non modifica
appartenenza alle gare, voti, qualificazioni o classifiche. Non è un ricalcolo automatico
conseguente alla modifica di un singolo orario e non introduce un comando di anticipo.

Questa decisione risolve comando e righe interessate lasciati aperti in DS-023; resta da
definire la distinzione tra orari previsti ed effettivi.

### Verifiche funzionali

- Posticipo di 10 minuti: riga selezionata e successive non iniziate avanzano di 10 minuti.
- Righe precedenti, già iniziate o concluse: orari invariati.
- Gli intervalli tra le righe interessate e il loro ordine restano invariati.
- Il comando opera anche su righe successive appartenenti a gare diverse.
- Nessun effetto su voti o classifiche.

## DS-025 - Orari previsti ed effettivi distinti

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della distinzione proposta dopo DS-024.
- **Collegamenti:** OP-02, DS-022, DS-023 e DS-024.

Per ogni esibizione si distinguono orario previsto ed effettivo. Il comando "Posticipa da
qui…" modifica soltanto gli orari previsti delle righe interessate secondo DS-024.
L'orario effettivo delle esibizioni svolte può essere registrato e corretto separatamente,
senza sovrascrivere il previsto. Il previsto e l'effettivo restano confrontabili.

La decisione risolve la distinzione lasciata aperta in DS-024; non introduce registrazione
automatica dell'effettivo, cronometro, storico delle revisioni del previsto o un legame
automatico tra orario effettivo e inserimento/chiusura dei voti.

### Verifiche funzionali

- Il posticipo cambia il previsto, non l'effettivo.
- Registrare o correggere l'effettivo non sovrascrive l'orario previsto.
- Previsto ed effettivo possono differire e rimangono distinguibili nella consultazione.
- Le modifiche degli orari non modificano voti, qualificazioni o classifiche.

## DS-026 - Data entry successivo alla conclusione dell'esibizione

**Rettifica del 13 ottobre 2026:** il vincolo software descritto qui sotto è superato da
DS-027. La sequenza indicata da Marco descrive il lavoro reale, non un prerequisito per
abilitare il data entry. Il testo originale è conservato soltanto come traccia della rettifica.

- **Data:** 12 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** Marco precisa che il data entry inizia dopo la conclusione dell'esibizione.
- **Collegamenti:** OP-02, OP-03, DS-014 e DS-025.

La conclusione dell'esibizione in pista e la chiusura della votazione sono momenti distinti.
Il data entry dei voti inizia soltanto dopo che l'esibizione è conclusa, non mentre è ancora
in corso. Resta necessario anche l'avvio della manifestazione secondo DS-014.

Un'esibizione conclusa può quindi avere voti non ancora inseriti, parziali o completi:
la conclusione non genera voti e non chiude automaticamente la votazione o la gara.
La procedura di chiusura della valutazione resta da concordare in OP-03. La regola riguarda
la trascrizione nel sistema, non impone quando il giudice debba compilare la propria scheda.

Non si deduce la conclusione dal solo orario previsto, dal trascorrere del tempo o dalla
presenza di un orario effettivo: resta da definire come l'operatore segnala la conclusione.

### Verifiche funzionali

- Prima della conclusione dell'esibizione il data entry dei suoi voti non è consentito.
- Dopo la conclusione e con manifestazione avviata, il data entry è consentito nel rispetto
  degli altri vincoli della gara e della valutazione.
- Un'esibizione conclusa senza voti non risulta automaticamente valutata o chiusa.
- La conclusione non assegna zeri ai voti mancanti.

## DS-027 - Data entry disponibile e chiusura dopo l'inserimento

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** rettifica di Marco a DS-026 e successiva conferma a proseguire.
- **Collegamenti:** OP-03, DS-014, DS-019, DS-020 e DS-021; sostituisce il vincolo di DS-026.

Dopo l'avvio della manifestazione il data entry è disponibile senza dichiarare prima
conclusa l'esibizione in pista. Il fatto che l'operatore inizi normalmente a trascrivere
dopo l'esibizione descrive il flusso operativo, non un blocco imposto dal software.
Non si introduce il comando "Esibizione conclusa" come prerequisito dei voti.

Dopo l'inserimento completo dei voti l'operatore può chiudere l'esibizione, proteggendone
la valutazione da modifiche accidentali. Per correggere i voti occorre riaprirla.
La chiusura della valutazione non viene dedotta dagli orari né dalla fine fisica
dell'esibizione. Restano i vincoli di manifestazione avviata e dello stato Non presentata.

La chiusura complessiva della gara resta da concordare in OP-03: non si assume automatica
alla chiusura dell'ultima esibizione. Le regole di propagazione delle riaperture e correzioni
alle classifiche collegate restano DS-019–DS-021; i comandi operativi vanno allineati.

### Verifiche funzionali

- Con manifestazione avviata non serve dichiarare la fine dell'esibizione per inserire voti.
- La chiusura della valutazione richiede tutti i voti, senza convertire le assenze in zero.
- I voti di un'esibizione chiusa sono protetti; la correzione richiede riapertura.
- Fine fisica, orari e completamento dei voti non chiudono automaticamente la gara.

## DS-028 - Chiusura esplicita della gara pronta

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della proposta successiva a DS-027.
- **Collegamenti:** OP-03, DS-003, DS-010, DS-020 e DS-027.

La gara si chiude con un comando esplicito, disponibile quando tutte le sue esibizioni
sono chiuse oppure "Non presentate". La chiusura dell'ultima esibizione rende la gara
pronta da chiudere, ma non la chiude e non rende automaticamente definitiva la classifica.
Il comando di chiusura della gara rende definitiva la sua classifica, ferma la gestione
delle parità ancora da concordare con il cliente (DS-018).

La regola completa la proposta di chiusura lasciata aperta in DS-027 e mantiene quanto
previsto da DS-010: le non presentate non richiedono voti fittizi. Non rende automaticamente
definitive le classifiche di disciplina o assolute, che seguono DS-020. Resta da validare
con il cliente l'interpretazione del "gruppo" (DS-003) e da coordinare operativamente la
riapertura della singola esibizione con quella della gara e le propagazioni DS-019–DS-021.

### Verifiche funzionali

- Una esibizione ancora aperta impedisce la chiusura della gara, anche con voti completi.
- Tutte chiuse o non presentate: gara pronta, senza chiusura automatica.
- Le non presentate non richiedono voti per soddisfare questa condizione.
- La chiusura della gara richiede l'azione esplicita dell'operatore.
- La chiusura non rende definitive automaticamente disciplina e assoluto.

## DS-029 - Riapertura guidata della singola esibizione

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma esplicita del flusso proposto dopo DS-028.
- **Collegamenti:** OP-03, DS-019–DS-021, DS-027 e DS-028.

Il comando "Riapri esibizione" sblocca la valutazione dell'esibizione scelta. Se la gara
è chiusa, il sistema avvisa che l'operazione riaprirà anche la gara e renderà provvisorie
le classifiche definitive collegate, secondo DS-021. La conferma esplicita applica questi
effetti insieme; annullare lascia invariati tutti gli stati.

Le altre esibizioni rimangono chiuse e protette: riaprire la gara non sblocca tutti i voti.
I voti già registrati vengono conservati. Le eventuali correzioni seguono DS-019; per
tornare a risultati definitivi occorre richiudere l'esibizione, chiudere esplicitamente
la gara e riconfermare le classifiche collegate secondo DS-020/DS-021.

La decisione coordina le riaperture lasciate aperte in DS-027/DS-028, senza introdurre
spareggi né modificare i criteri della manifestazione. Se la gara è già aperta, non serve
riaprirla di nuovo: si sblocca soltanto l'esibizione selezionata.

### Verifiche funzionali

- Gara chiusa: avviso degli effetti prima della conferma della riapertura.
- Conferma: esibizione scelta e gara riaperte, classifiche definitive collegate provvisorie.
- Annullamento: nessuno stato o voto modificato.
- Altre esibizioni: voti ancora protetti, anche dopo la riapertura della gara.
- Nessuna cancellazione dei voti né ripristino automatico della definitività.

## DS-030 - Assolo/Duo come disciplina: chiarimento necessario

**Aggiornamento del 14 ottobre 2026:** chiarimento recepito e interpretazione corrente in DS-038.

- **Data:** 13 ottobre 2026.
- **Stato:** punto aperto da chiedere al cliente, non una scelta di modellazione approvata.
- **Fonte:** Marco richiede di chiarire se Assolo/Duo vadano trattati come una disciplina.
- **Collegamenti:** OP-06.

La presenza di "Assolo/Duo" tra le discipline elencate nel riscontro cliente non basta a
consolidarne il significato nel modello. Occorre chiedere se rappresenti effettivamente
una disciplina e come si rapporti agli stili e al tipo di coreografia.

Non si assume né una sezione trasversale agli stili né una semplice denominazione provvisoria.
Resta acquisito il significato del tipo rispetto al numero di componenti; rimangono aperti
il suo rapporto con la disciplina, l'eventuale separazione delle gare Assolo e Duo e la
chiave definitiva di raggruppamento delle gare. Le precedenti descrizioni di "Assolo/Duo"
come disciplina rappresentano il testo ricevuto, non una decisione già confermata.

## DS-031 - Gare derivate dalle iscrizioni, senza matrice di ammissibilità

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della soluzione basata sulle combinazioni effettivamente iscritte.
- **Collegamenti:** OP-06, DS-016 e DS-030.

Categorie e livelli sono anagrafiche separate, condivise tra le discipline. Non si configura
una matrice preventiva categoria/livello né un elenco di combinazioni abilitate per disciplina.
Le gare effettive si ricavano dalle combinazioni distinte presenti nelle iscrizioni:
una nuova combinazione utilizzata da una coreografia dà origine a una nuova gara.
Le combinazioni già presenti descrivono l'utilizzato, non limitano le nuove iscrizioni.
Non si generano gare vuote dal prodotto cartesiano delle anagrafiche.

Ogni coreografia mantiene una disciplina, una categoria e un livello. Gli stessi atleti
possono partecipare a un'altra disciplina con un'altra coreografia: non si reintroduce
un'entità Formazione distinta. Resta l'eccezione PRO a categoria unica (DS-016).
Il ruolo del tipo Assolo/Duo/Gruppo nella chiave di raggruppamento resta aperto (DS-030):
questa decisione non accorpa implicitamente gare di tipi diversi.

La proposta sostituisce, nel modello da sottoporre al cliente, le esclusioni locali di
categorie/livelli per disciplina descritte in precedenza. Tale differenza rispetto al
riscontro originale va validata, senza considerarla già approvata dal cliente.
Restano invariati i vincoli sulle modifiche di coreografie già votate (DS-002).

### Verifiche funzionali

- Una nuova disciplina può usare categorie e livelli già presenti nelle anagrafiche.
- Una nuova combinazione iscritta non richiede abilitazione preventiva.
- Più iscrizioni con la stessa combinazione confluiscono nella stessa gara, ferma la
  definizione del ruolo del tipo ancora aperta in DS-030.
- Una combinazione mai utilizzata non genera una gara vuota.
- Il PRO conserva la categoria unica e il percorso di premi separato.

## DS-032 - Rimozione della gara vuota non ancora iniziata

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della proposta successiva a DS-031.
- **Collegamenti:** OP-06, DS-002, DS-006 e DS-031.

Quando l'eliminazione o lo spostamento dell'ultima coreografia lascia vuota una gara non
ancora iniziata, viene rimossa anche la gara. Non resta una gara vuota generata da una
combinazione che non è più presente nelle iscrizioni.

La regola non si estende alle gare già iniziate o concluse e non autorizza spostamenti
vietati da DS-002. Rimuovere la gara non elimina le anagrafiche di disciplina, categoria,
livello, atleti o giudici. Gli atleti senza coreografie restano nell'anagrafica secondo DS-006.
La marcatura "Non presentata" conserva l'iscrizione e non rende la gara vuota.

### Verifiche funzionali

- Eliminazione dell'ultima coreografia di gara non iniziata: gara rimossa.
- Spostamento consentito dell'ultima coreografia: gara di origine non iniziata rimossa.
- Almeno una coreografia ancora iscritta, anche non presentata: gara non vuota.
- Nessuna rimozione automatica di gare già iniziate o concluse in base a questa regola.
- Anagrafiche condivise conservate dopo la rimozione della gara.

## DS-033 - Assegnazione della giuria: alternative da discutere

- **Data:** 13 ottobre 2026.
- **Stato:** punto aperto da chiarire con il cliente; nessuna alternativa selezionata.
- **Fonte:** Marco chiede di verificare il flusso reale e propone l'inclusione iniziale di tutti i giudici.
- **Collegamenti:** OP-14, DS-031.

L'assegnazione della giuria a una gara va chiarita con il cliente. La proposta di generare
gare inizialmente senza giuria non è approvata come comportamento predefinito. Si presentano
due alternative:

- **Assegnazione esplicita:** selezionare i giudici per ogni gara, eventualmente tramite
  "Copia giuria da un'altra gara". Utile con composizioni diverse, ma potenzialmente macchinosa
  quando tutti i giudici partecipano a tutte le gare.
- **Inclusione iniziale di tutti:** ogni gara include inizialmente tutti i giudici della
  manifestazione; l'operatore può escluderli dalla giuria di una specifica gara. L'esclusione
  locale non elimina il giudice dall'anagrafica né dalle giurie delle altre gare.

Il cliente deve chiarire quale caso sia abituale. Resta il requisito di almeno tre giudici
e la modificabilità della giuria fino all'avvio della singola gara. Per l'alternativa con
tutti inclusi va chiarito anche cosa accade aggiungendo successivamente un giudice
all'anagrafica: non si assume la propagazione automatica alle giurie già configurate.
Non si introduce una preferenza configurabile tra le alternative senza una decisione.

## DS-034 - Cancellazione protetta delle anagrafiche

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della proposta di cancellazione delle sole anagrafiche non utilizzate.
- **Collegamenti:** FUN-01, DS-006 e DS-033.

Scuole, atleti, giudici, discipline, categorie e livelli sono eliminabili soltanto se non
utilizzati. In presenza di riferimenti il sistema impedisce l'eliminazione e indica dove
la voce è usata, senza cancellare a cascata iscrizioni, giurie o voti per liberarla.

La cancellazione dell'anagrafica resta distinta dalla rimozione di un'associazione ammessa
dalle relative regole, come l'esclusione di un giudice dalla giuria di una gara. Eliminare
una coreografia non elimina automaticamente i suoi atleti (DS-006). Non si introduce una
cancellazione forzata né una bonifica automatica dei riferimenti.

### Verifiche funzionali

- Voce non utilizzata: cancellazione consentita.
- Voce utilizzata: cancellazione bloccata, riferimenti visibili e dati invariati.
- Nessuna eliminazione a cascata di iscrizioni o voti.
- Eliminazione di una coreografia: anagrafiche dei suoi atleti conservate.

## DS-035 - Eliminazione delle coreografie senza voti

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della proposta di protezione delle coreografie già valutate.
- **Collegamenti:** OP-04, DS-006, DS-032 e DS-034.

Una coreografia è eliminabile soltanto se priva di voti, nel rispetto degli altri vincoli
di stato già previsti. La presenza anche di un solo voto, incluso zero o una valutazione
parziale, impedisce l'eliminazione. Il blocco conserva coreografia e voti; non si cancellano
automaticamente le valutazioni per rendere possibile l'operazione. La riapertura non
aggira il vincolo, poiché conserva i voti.

Gli atleti restano nell'anagrafica secondo DS-006. Se l'eliminazione consentita svuota una
gara non ancora iniziata, si applica DS-032. Non vengono introdotti ritiro o annullamento
di risultati già votati: eventuali esigenze eccezionali restano da chiarire con il cliente.

### Verifiche funzionali

- Nessun voto presente: eliminazione consentita nel rispetto degli altri vincoli di stato.
- Un voto zero o parziale presente: eliminazione bloccata, dati invariati.
- Esibizione riaperta con voti conservati: eliminazione ancora bloccata.
- Eliminazione consentita: atleti conservati e regola DS-032 applicata ove pertinente.

## DS-036 - Ritiri e annullamenti dopo la valutazione rinviati

**Aggiornamento del 14 ottobre 2026:** DS-046 definisce la squalifica, superando il rinvio
per questo caso. Ritiri e altri annullamenti restano distinti; segue il testo storico.

- **Data:** 13 ottobre 2026.
- **Stato:** rinvio condiviso con Marco; esigenza e regole da chiedere al cliente.
- **Fonte:** conferma di lasciare i casi eccezionali fuori dalle procedure attualmente definite.
- **Collegamenti:** OP-04, DS-008 e DS-035.

Ritiri o annullamenti dopo la valutazione restano casi eccezionali da discutere con il
cliente. Non si introducono ora comandi o procedure dedicate, né automatismi per eliminare
voti o risultati. Il cliente dovrà chiarire se tali casi servono realmente e quali effetti
debbano avere sulle classifiche e sulle qualificazioni.

Restano valide le protezioni già concordate: una coreografia con voti non può essere
eliminata (DS-035) né marcata Non presentata (DS-008). La normale correzione dei voti tramite
riapertura resta distinta dal ritiro o annullamento di una partecipazione valutata.

## DS-037 - Correzione dei nomi senza sostituzione dell'identità

- **Data:** 13 ottobre 2026.
- **Stato:** condivisa con Marco, da validare con il cliente.
- **Fonte:** conferma della distinzione tra correzione del nome e sostituzione della persona.
- **Collegamenti:** BUG-05, DS-001, DS-002 e DS-033.

La correzione del nome di un giudice, atleta o scuola aggiorna coerentemente tutte le
viste che lo mostrano, anche a manifestazione iniziata. Non cambia l'identità della voce,
le iscrizioni, le composizioni delle giurie o i voti e non richiede la riapertura di una
gara o esibizione solo per correggere il nome.

Rinominare non equivale a sostituire una persona o cambiare la scuola rappresentata:
queste operazioni restano distinte e soggette ai rispettivi vincoli. Una correzione
puramente nominale non modifica punteggi, posizioni o qualificazioni.

### Verifiche funzionali

- Nome corretto: aggiornamento coerente nelle viste di anagrafica, iscrizione e risultati.
- Correzione dopo l'avvio o la chiusura: identità, associazioni e voti invariati.
- Nessuna riapertura o riclassificazione causata dalla sola correzione del nome.
- Sostituzione di persona o scuola: non trattata come semplice rinomina.

## DS-038 - Specialità configurabili e tipo escluso dalla chiave della gara

- **Data:** 14 ottobre 2026.
- **Stato:** interpretazione del riscontro cliente condivisa con Marco; da riportare al cliente.
- **Fonte:** risposte cliente del 14 ottobre a D1 e conferma di Marco della proposta di modellazione.
- **Collegamenti:** OP-06, DS-030 e DS-031.

La gara è identificata da specialità (disciplina), categoria e livello. Il tipo Assolo,
Duo o Gruppo descrive la composizione della coreografia ma non separa automaticamente le gare.
Assolo richiede un atleta, Duo due, Gruppo più di due.

Assolo/Duo è oggi una specialità configurata unica. Le future suddivisioni per stile
(ad esempio Assolo/Duo Caraibico e Assolo/Duo Urban), o la separazione tra assoli e duo,
si rappresentano con specialità distinte nell'anagrafica, senza logiche dedicate ai nomi.
Non si introduce un ulteriore livello gerarchico né si deducono raggruppamenti per i premi
dalle denominazioni. Restano le regole di categoria e livello, incluso il percorso PRO.

Questa decisione risolve il punto sul ruolo del tipo lasciato aperto in DS-030/DS-031;
non approva implicitamente le altre proposte né anticipa le decisioni sulle ulteriori risposte.

### Verifiche funzionali

- Stessa specialità, categoria e livello: assolo e duo appartengono alla stessa gara.
- Specialità distinte: gare distinte anche a parità di categoria, livello e tipo.
- Nuova suddivisione: configurazione di nuove specialità, senza modificare le regole di raggruppamento.

## DS-039 - Data entry mono-esibizione distinto dalle schede cartacee

- **Data:** 14 ottobre 2026.
- **Stato:** confermato da Marco; distinzione da riportare al cliente.
- **Fonte:** osservazione del cliente sulle schede con più coreografie e conferma di Marco di mantenere il data entry mono-esibizione.
- **Collegamenti:** DS-027, OP-03, UX-01.

La schermata di inserimento voti resta dedicata a una sola esibizione, con i giudici
della relativa gara. La scheda cartacea del giudice può invece contenere più coreografie
in ordine di uscita, con criteri e totale: i due supporti non devono avere la stessa struttura.

La raccolta dei fogli ogni circa venti coreografie descritta dal cliente non introduce
un'entità o un blocco obbligatorio di data entry. Questa decisione non definisce la chiusura
di più gare insieme, da esaminare separatamente, né autorizza una nuova funzionalità di stampa.
Se prevista, la stampa delle schede sarà trattata separatamente dalla schermata di inserimento.

### Verifiche funzionali

- L'operatore inserisce e controlla i voti di una sola esibizione alla volta.
- Un foglio con più coreografie non impone inserimento o chiusura in blocco.
- Nessun vincolo di venti esibizioni e nessun legame obbligatorio fra foglio e gara.

## DS-040 - Stessi atleti in coreografie distinte della stessa gara

- **Data:** 14 ottobre 2026.
- **Stato:** richiesta esplicita del cliente recepita e confermata da Marco.
- **Fonte:** annotazione a P4 nel riscontro del 14 ottobre e successiva conferma di Marco.
- **Collegamenti:** OP-08, DS-005 (superata), DS-038.

Gli stessi atleti possono partecipare con più coreografie nella stessa gara, purché
le coreografie abbiano nomi diversi. Non si applica il precedente blocco basato sulla
composizione identica, né si richiede una forzatura per questa iscrizione.
Ogni coreografia resta un'iscrizione distinta, con esibizione e voti propri; non si
condividono o copiano automaticamente le valutazioni tra coreografie degli stessi atleti.

La decisione non introduce un vincolo generale di unicità dei nomi tra composizioni
diverse. Restano vietati atleti duplicati all'interno della singola coreografia e
restano le regole numeriche di Assolo/Duo/Gruppo.

### Verifiche funzionali

- Stessa gara, stessi atleti e nomi diversi: iscrizioni consentite e distinte.
- Valutare una coreografia non modifica i voti dell'altra con gli stessi atleti.
- Cambiare l'ordine dei componenti non altera l'identità degli atleti coinvolti.
- Ripetere un atleta nella stessa coreografia resta non consentito.

## DS-041 - Denominazione del livello distinta dalle regole

- **Data:** 14 ottobre 2026.
- **Stato:** interpretazione del riscontro cliente condivisa con Marco; da riportare al cliente.
- **Fonte:** richiesta di denominazioni modificabili Open/PRO/Semi-Pro e conferma di Marco.
- **Collegamenti:** DS-016, DS-017, OP-05 e OP-06.

Il nome di un livello è modificabile e non determina le sue regole. Rinominare PRO in
Semi-Pro conserva l'identità del livello, le iscrizioni e le regole associate, comprese
categoria unica e percorso verso l'assoluto. Analogamente, la denominazione Open
sostituisce l'etichetta descrittiva non PRO senza modificare il percorso dei premi.

Cambiare le regole di gara o qualificazione è un'operazione distinta dalla rinomina:
non si deducono comportamenti dal testo del nome. Questa decisione non definisce una
nuova interfaccia per configurare tali regole né le condizioni per modificarle durante
la manifestazione. Non risolve implicitamente gli altri punti aperti sui premi.

### Verifiche funzionali

- Rinominare PRO in Semi-Pro aggiorna il nome visualizzato e conserva regole e iscrizioni.
- La sola rinomina non sposta coreografie tra gare e non cambia punteggi o qualificati.
- Nessun percorso di qualificazione viene scelto confrontando il nome con Open o PRO.

## DS-042 - Parità ammesse e qualificazione di tutti i primi ex aequo

- **Data:** 14 ottobre 2026.
- **Stato:** chiarimento e decisione espliciti di Marco, da riportare al cliente.
- **Fonte:** precisazione sul decimale ordinario e conferma dell'accesso di tutti i primi a pari merito.
- **Collegamenti:** DS-015, DS-016, DS-017, DS-018, OP-05 e OP-09.

I voti validi vanno da 0 a 10 inclusi, con una cifra decimale. Il riferimento a un minimo
di 1 era un refuso. Il decimale è parte del voto ordinario, introdotto per ridurre le
parità, non un incremento speciale da assegnare dopo la votazione.

Le parità restano ammesse e non impediscono la chiusura della gara. Non si introduce
uno spareggio dedicato né una modifica automatica dei voti. Si evidenziano le parità
che coinvolgono il podio, senza scegliere arbitrariamente un vincitore.

Quando il primo posto è condiviso, tutte le coreografie prime a pari merito partecipano
alla classifica successiva prevista dal proprio percorso, usando i voti già assegnati.
La regola vale dalla gara alla disciplina e dalla disciplina all'assoluto nel percorso
Open, e dalle gare di disciplina direttamente all'assoluto nel percorso PRO.
I percorsi rimangono separati; la classifica assoluta non ha un ulteriore passaggio.

La decisione non stabilisce una nuova convenzione numerica dei piazzamenti successivi
a una parità né il numero materiale di premi da consegnare: non si deducono queste regole
dall'ammissione di tutti i primi. Non approva la proposta di UI della chiusura singola
né introduce la chiusura massiva.

### Verifiche funzionali

- Zero è valido; 10 è valido; un voto mancante resta distinto da zero.
- Due o più primi ex aequo: tutti accedono al passaggio previsto, senza alterare i voti.
- Parità sul podio: evidenziata ma non bloccante per la chiusura.
- Nessuna selezione del qualificato tramite ordine grafico, nome o identificativo.

## DS-043 - Esclusione manuale dei giudici dal calcolo della singola classifica

- **Data:** 14 ottobre 2026.
- **Stato:** proposta preferita condivisa con Marco, da sottoporre al cliente; non ancora approvata dal cliente.
- **Fonte:** confronto sui casi di abbandono, sostituzione e assenza temporanea, seguito dalla scelta di proporre la gestione manuale.
- **Collegamenti:** OP-09, OP-14, DS-015, DS-017, DS-020 e DS-033.

Il riscontro cliente indica che normalmente tutti i giudici giudicano tutto. Si distinguono
assenza prima delle votazioni, abbandono con voti già espressi, sostituzione e assenza
temporanea con ritorno. Il giudice che si assenta non viene cancellato: si conserva la sua
identità e ogni voto originale. Non si introduce un'entità "posto in giuria".

Per ogni classifica, di gara, specialità o assoluta, il sistema segnala differenze nel
numero di valutazioni disponibili tra le coreografie partecipanti. La proposta preferita
lascia all'operatore la scelta dei giudici da escludere dal calcolo, senza dedurre o
memorizzare sostituzioni. Prima di rendere definitiva la classifica si verifica la
numerosità risultante. È ammesso che giudici diversi abbiano valutato esibizioni diverse,
purché il numero risultante di valutazioni sia uguale; non si richiede identità dei giudici.
La gestione di schede parzialmente compilate resta distinta da un'assenza e non viene
risolta convertendo voti mancanti in zero.

L'esclusione è locale alla classifica, reversibile e applicata a tutti i voti di quel
giudice nella classifica. Non cancella o modifica i voti originali. I voti esclusi restano
visibili ingrigiti, con totali ricalcolati. Una valutazione può essere utilizzata nella
gara ed esclusa nella specialità o nell'assoluto: si riusano i voti, non necessariamente
lo stesso totale. Questo precisa DS-017. Si mantiene la somma, senza normalizzazione.
Una successiva assenza non modifica automaticamente una classifica già conclusa.

Esempio: tre esibizioni valutate rispettivamente da A+B+C+D, A+B+D+E e A+B+E.
Escludendo D per quella classifica restano A+B+C, A+B+E e A+B+E: tre valutazioni ciascuna.
Nessun voto di C viene attribuito a E e non si registra che E abbia sostituito C.

Alternativa da presentare, non scelta come soluzione preferita: una relazione interna
"E sostituisce C" può aiutare a discriminare i giudici da escludere, senza introdurre
posti in giuria o una rappresentazione grafica dedicata della sostituzione. Non si
prevede per ora di implementare entrambe le politiche come modalità selezionabili.

Restano da validare con il cliente la politica proposta, il comportamento se rimangono
meno di tre giudici per esibizione e le condizioni operative di finalizzazione quando
la discrepanza non è risolta. Non si autorizza una deroga automatica al minimo di tre.

### Verifiche funzionali

- Escludere D nell'esempio rende uniforme la numerosità senza eliminare i suoi voti.
- L'esclusione nella specialità non altera il risultato già concluso della gara.
- Riabilitare un giudice ripristina il suo contributo e ricalcola il totale della classifica.
- Uguale numerosità con giudici diversi non richiede una sostituzione dichiarata.
- Abbandono e ritorno non cancellano né riassegnano voti originali.

## DS-044 - Esclusione guidata dalle differenze di numerosità

- **Data:** 14 ottobre 2026.
- **Stato:** precisazione condivisa con Marco della proposta da sottoporre al cliente.
- **Fonte:** correzioni di Marco sull'abilitazione dell'esclusione e priorità alle giurie più numerose.
- **Collegamenti:** DS-043, OP-09.

L'esclusione non è una scelta libera sempre disponibile. Nella specifica classifica si
confrontano le numerosità delle valutazioni incluse per esibizione: se sono tutte uguali,
non si possono escludere altri giudici, anche se le identità dei valutatori differiscono.
In presenza di discrepanze si parte dai giudici delle giurie più numerose, considerando
solo giudici con valutazioni mancanti su altre esibizioni; non si riduce la giuria già
meno numerosa per inseguire l'uguaglianza. La scelta resta all'operatore, non automatica.

Dopo ogni esclusione si ricalcolano le numerosità e i candidati. Appena il numero è
uniforme, ulteriori esclusioni sono disabilitate. È sempre distinta la possibilità di
reincludere un giudice precedentemente escluso, che ripristina i suoi voti nel calcolo e
fa rivalutare la disponibilità delle esclusioni. Restano conservazione dei voti ed effetti
locali alla classifica di DS-043; nessuna deroga al minimo di tre valutazioni.

Esempi vincolanti:

- A+B+C e A+B+D: tre valutazioni ciascuna, nessuna esclusione abilitata.
- A+B+C+D e A+B+E: candidati C e D, non A/B (completi) né E (giuria meno numerosa).
  Escluso C oppure D, si raggiunge tre contro tre e non si può escludere altro.
  Reincludendo il giudice escluso si può rivedere la scelta.

Questa precisazione supera l'ipotesi di consentire esclusioni arbitrarie seguite dal solo
controllo del minimo. Per combinazioni più articolate resta da verificare il criterio
generale dei candidati, senza inventare rimozioni automatiche o un vincitore fra scelte
alternative; gli esempi sopra costituiscono i casi di verifica concordati.

## DS-045 - Spostamento di coreografie valutate con conservazione dei voti

- **Data:** 14 ottobre 2026.
- **Stato:** esigenza esplicita del cliente; procedura condivisa con Marco, da validare con il cliente.
- **Fonte:** riscontro su cambi di specialità/categoria/livello dopo la valutazione e conferma della proposta.
- **Collegamenti:** DS-002 (superata), DS-019, DS-021, DS-038, DS-043 e DS-044, OP-04.

Si consente di modificare specialità, categoria o livello anche per una coreografia già
valutata, con avviso e conferma espliciti. La coreografia esce dalla classifica della
gara di origine ed entra in quella di destinazione, conservando tutti i voti originali
e l'identità dei giudici che li hanno espressi. Non si attribuiscono voti ai giudici
della destinazione e non si eliminano o generano valutazioni per effetto dello spostamento.

Le gare coinvolte già chiuse devono essere riaperte. Le classifiche successive interessate
nei percorsi di origine e destinazione tornano provvisorie e vengono ricalcolate, inclusi
i qualificati; per renderle nuovamente definitive serve riconferma. Le classifiche estranee
non sono modificate. Annullare la conferma lascia invariati appartenenza, voti e stati.

Le eventuali differenze di numerosità delle valutazioni si gestiscono secondo DS-043/044,
senza esclusioni automatiche. Lo spostamento non autorizza la cancellazione della coreografia
con voti né costituisce una procedura di ritiro o squalifica. Questi aspetti restano distinti.

### Verifiche funzionali

- Coreografia con voti spostata: stessa identità, stessi voti e autori, nuova appartenenza.
- La classifica di origine non include più la coreografia; quella di destinazione la include.
- Gare chiuse coinvolte riaperte; classifiche collegate ricalcolate e da riconfermare.
- Discrepanze di numerosità segnalate senza modificare i voti originali.
- Conferma annullata: nessuna modifica.

## DS-046 - Squalifica durante o dopo la gara

- **Data:** 14 ottobre 2026.
- **Stato:** proposta condivisa con Marco, da sottoporre al cliente.
- **Fonte:** conferma dei casi di interruzione immediata e squalifica dopo la premiazione.
- **Collegamenti:** DS-007/010, DS-019/021, DS-036, DS-043/044, OP-04 e OP-12.

Un'esibizione può essere squalificata in qualsiasi momento, anche mentre è in corso,
senza voti o con voti parziali o completi. Non è necessario attendere la fine fisica
dell'esibizione o completare la valutazione. A gara aperta si applica direttamente la
squalifica e si può procedere con la scaletta; l'esibizione conta come gestita per la chiusura.

Se la gara è già conclusa deve prima essere riaperta. Dopo la squalifica serve una nuova
chiusura. Vale anche quando la premiazione è già iniziata o la coreografia è stata premiata.
Il sistema ricalcola la classifica senza la squalificata, con avanzamento delle posizioni
successive secondo le regole di piazzamento e aggiornamento dei qualificati. Le classifiche
successive interessate tornano provvisorie e richiedono riconferma.

L'iscrizione e tutti gli eventuali voti originali sono conservati. La coreografia appare
come "Squalificata", senza posizione e senza accesso ai premi successivi. I voti non
concorrono al calcolo e l'esibizione è esclusa dal confronto sulla numerosità dei giudici.
L'effetto classificatorio è analogo a Non presentata, ma motivazione e stato sono distinti:
il divieto di impostare Non presentata con voti non si applica alla squalifica.

Il sistema registra la decisione dell'organizzazione: non modella ricorso, verifica video
o restituzione materiale dei premi. La squalifica non cancella coreografia, atleti o voti.
La procedura di revoca è precisata dalla successiva DS-047.

### Verifiche funzionali

- Esibizione interrotta senza voti: squalifica consentita, nessun voto fittizio necessario.
- Esibizione con voti parziali o completi: dati conservati ma esclusi dai conteggi.
- Gara conclusa: riapertura preventiva e nuova chiusura dopo l'applicazione.
- Coreografia già sul podio: rimossa dai piazzamenti, risultati e qualificati ricalcolati.
- Le classifiche collegate interessate richiedono riconferma; quelle estranee sono invariate.

## DS-047 - Revoca della squalifica e verifica dei voti recuperati

- **Data:** 14 ottobre 2026.
- **Stato:** proposta condivisa con Marco, da sottoporre al cliente.
- **Fonte:** conferma della possibilità di revocare una squalifica inserita per errore.
- **Collegamenti:** DS-046, DS-019/021, DS-027/029, OP-04 e OP-12.

La squalifica è revocabile. Se la gara è chiusa occorre prima riaprirla. La revoca
recupera tutti i voti originali conservati e riporta l'esibizione aperta, da verificare
prima di richiuderla, anche quando la scheda è già completa. Non si creano voti per
le valutazioni mancanti e non si reinseriscono manualmente quelli già registrati.

La coreografia torna nel normale percorso di valutazione e ricalcolo delle classifiche,
con trattamento delle eventuali incomplete e delle differenze di numerosità già previsto.
La verifica e successiva chiusura dell'esibizione precedono la nuova chiusura della gara.
Le classifiche successive interessate restano provvisorie fino alla riconferma prevista;
la revoca non ripristina automaticamente precedenti piazzamenti, qualificati o definitività.

### Verifiche funzionali

- Revoca a gara chiusa: necessaria la riapertura preventiva.
- Voti completi conservati: recuperati invariati, esibizione comunque da verificare e richiudere.
- Voti parziali o assenti: nessun voto generato e nessuna chiusura automatica.
- Risultati e qualificati ricalcolati secondo i dati correnti, senza ripristino automatico del podio storico.
- Nessuna classifica torna definitiva per il solo effetto della revoca.

## DS-048 - Reversibilità degli stati che impediscono la valutazione

- **Data:** 14 ottobre 2026.
- **Stato:** principio richiesto da Marco, da riportare al cliente.
- **Fonte:** richiesta di rendere reversibile ogni azione dell'operatore che impedisca di inserire anche un solo voto.
- **Collegamenti:** DS-009, DS-027/029, DS-046/047, OP-03 e OP-12.

Ogni azione o cambio di stato dell'operatore che impedisce a un'esibizione di ricevere
anche un solo voto deve avere un percorso di annullamento del blocco, per correggere
errori di data entry. Il principio comprende Non presentata, Squalificata e le chiusure
di esibizione e gara: nessuno di questi stati costituisce un blocco irreversibile.

La revoca di Non presentata segue la stessa tutela prevista per la revoca della squalifica:
se la gara è chiusa va prima riaperta; l'esibizione torna aperta, da valutare/verificare,
senza voti fittizi e senza chiusura automatica. Rimane la regola di accesso a Non presentata
solo in assenza di voti; non la si estende alle esibizioni già valutate. La squalifica
resta invece applicabile con voti, che vengono conservati secondo DS-046/047.

La reversibilità ripristina la possibilità di inserire o correggere i voti; non ripristina
automaticamente vecchi risultati o la definitività delle classifiche. Restano riaperture,
ricalcoli e riconferme già concordati. Non si eludono i vincoli di validità dei voti e
non si cancellano valutazioni per sbloccare l'esibizione. Anche eventuali futuri stati
che impediscano la valutazione dovranno rispettare questo principio.

### Verifiche funzionali

- Non presentata impostata per errore: revoca disponibile e inserimento voti nuovamente possibile.
- Gara già chiusa: riapertura e revoca consentono di riprendere la valutazione.
- Squalifica o chiusura errata: recupero dell'operatività senza perdita dei voti conservati.
- Nessuna revoca genera voti o rende automaticamente definitive le classifiche.

## DS-049 - Numerazione ex aequo 1-1-3 come ipotesi da convalidare

- **Data:** 14 ottobre 2026.
- **Stato:** ipotesi di Marco, da convalidare esplicitamente con il cliente.
- **Fonte:** indicazione di Marco della probabile regola 1-1-3.
- **Collegamenti:** DS-042, OP-05 e OP-09.

La proposta di numerazione attribuisce la stessa posizione alle coreografie a pari
punteggio e salta le posizioni occupate: due primi sono seguiti dal terzo (1-1-3),
due secondi dal quarto (1-2-2-4). Non è ancora una regola definitiva del regolamento.
Va richiesta conferma al cliente prima di adottarla come comportamento approvato.

Resta confermata con Marco la regola DS-042: tutti i primi ex aequo accedono alla
classifica successiva del rispettivo percorso. La presente ipotesi non modifica
quella decisione e non determina il numero materiale dei premi da consegnare.

## DS-050 - Classifica unica sempre disponibile e aggiornata in tempo reale

- **Data:** 14 ottobre 2026.
- **Stato:** proposta condivisa con Marco, da riportare al cliente.
- **Fonte:** richiesta di una sola vista disponibile anche prima dell'inizio della gara.
- **Collegamenti:** OP-13, DS-020/021, DS-027/028, DS-046 e DS-049.

Esiste un'unica vista della classifica, sempre accessibile anche per gare non ancora
iniziate. I risultati riflettono i voti registrati e le modifiche applicate: non occorre
una modalità distinta di "classifica provvisoria". La vista indica Provvisoria fino
alla chiusura della gara e Definitiva dopo la chiusura; una riapertura ripristina
l'indicazione Provvisoria secondo le regole già definite.

Prima dei voti, le esibizioni sono mostrate nell'ordine della scaletta, senza assegnare
posizioni di classifica né voti zero fittizi. Anche durante la gara le esibizioni senza
voto mantengono l'ordine della scaletta. Restano le regole correnti per schede complete
(ordinate per punteggio, con indicazione delle esibizioni ancora aperte) e incomplete
(visibili separatamente, senza posizione e senza equiparare voti mancanti a zero).
Non presentate e squalificate conservano le rispettive indicazioni e l'esclusione dai
piazzamenti. Le condizioni di definitività dei premi successivi non cambiano.

### Verifiche funzionali

- Gara non iniziata: classifica consultabile, indicazione Provvisoria, ordine di scaletta e nessun piazzamento.
- Inserimento o correzione dei voti: la stessa vista riflette i risultati aggiornati.
- Esibizione senza voti: nessun totale fittizio o piazzamento derivato da zero.
- Chiusura e riapertura cambiano lo stato indicato, non creano viste separate.

## DS-051 - Sostituzione di un atleta con conferma esplicita

- **Data:** 14 ottobre 2026.
- **Stato:** esigenza del cliente recepita; procedura condivisa con Marco, da riportare al cliente.
- **Fonte:** richiesta del cliente di consentire la sostituzione con avviso e conferma di Marco del flusso proposto.
- **Collegamenti:** DS-001, DS-029, DS-037 e DS-045, OP-04.

La sostituzione di un atleta in una coreografia richiede una conferma che mostri
chiaramente l'atleta uscente e quello entrante. Se la coreografia resta nella stessa
gara, conserva i voti già espressi: la valutazione appartiene alla coreografia, non
all'anagrafica del singolo atleta. Se l'esibizione è chiusa, occorre prima riaprirla
secondo la procedura esistente e i suoi effetti sulle classifiche collegate.

La sostituzione modifica i componenti della specifica coreografia, non rinomina o
sostituisce l'identità anagrafica dell'atleta nelle altre iscrizioni. La correzione
del nome resta distinta e segue DS-037. Restano i vincoli di composizione e il divieto
di ripetere lo stesso atleta nella coreografia; eventuali cambi di gara seguono DS-045.
Annullare la conferma lascia invariati componenti e voti.

### Verifiche funzionali

- Conferma: atleta uscente ed entrante visibili prima di applicare la sostituzione.
- Stessa gara: voti conservati e modifica limitata alla coreografia selezionata.
- Esibizione chiusa: riapertura necessaria prima della sostituzione.
- Annullamento: nessuna modifica a componenti e valutazioni.

## DS-052 - Cambio della scuola rappresentata prima dei voti

- **Data:** 15 ottobre 2026.
- **Stato:** proposta condivisa con Marco, da riportare al cliente.
- **Fonte:** conferma della possibilità di correggere la scuola d'iscrizione prima dei voti, senza una procedura dedicata dopo la valutazione.
- **Collegamenti:** DS-004, DS-037, DS-051, OP-04 e OP-07.

È consentito cambiare la scuola rappresentata da una coreografia prima dell'inserimento
di qualsiasi voto, nel rispetto degli altri vincoli di stato. La presenza anche di un
solo voto, zero o parziale, impedisce questa modifica: non si introduce una procedura
di cambio scuola dopo la valutazione. La riapertura non elimina i voti e non aggira
questa condizione; non si cancellano automaticamente valutazioni per consentire il cambio.

Il cambio riguarda la scuola rappresentata dalla specifica coreografia, non l'identità
della scuola o le altre iscrizioni. La semplice correzione del nome della scuola
nell'anagrafica resta distinta e consentita secondo DS-037 anche dopo la valutazione.
La coreografia continua a rappresentare una sola scuola.

### Verifiche funzionali

- Coreografia senza voti: cambio della scuola consentito nel rispetto degli altri vincoli di stato.
- Almeno un voto, incluso zero o parziale: cambio bloccato, dati conservati.
- Gara/esibizione riaperta con voti: il cambio rimane bloccato.
- Correzione nominale della scuola: nessun cambio di identità, iscrizioni o valutazioni.
