# Punti aperti e piano di lavoro

> Aggiornamento 21 ottobre 2026: per i temi rivisti prevale il
> [consolidamento DS-053-DS-065](Consolidamento-2026-10-21.md).
> Il testo sottostante conserva lo storico; data entry mono-esibizione, esclusioni manuali,
> primi ex aequo e aggregazione obbligatoria per disciplina non sono più proposte correnti.
> Spareggi definiti il 21 ottobre in DS-055 (AP-01 chiuso): rettifica ufficiale di un voto
> condivisa fra classifiche. Stampa e scelta grafica restano aperte. Vedere la fonte e lo stato
> di acquisizione nel consolidamento; non attribuire approvazioni implicite agli altri punti.

Revisione del 16 ottobre 2026. Vedere il [modello aggiornato](Modello.md) e il
[registro del riscontro cliente](RiscontroCliente-2026-10-09.md).
Questo documento non autorizza né descrive implementazioni già eseguite.

Le nuove decisioni funzionali sono registrate in [Decisioni semantiche](DecisioniSemantiche.md),
con approvazione del cliente distinta dall'accordo di Marco; quelle implementative restano
nel [registro tecnico interno](DecisioniTecniche.md).

Il [riepilogo per il cliente del 16 ottobre](RiepilogoCliente-2026-10-16.md) consolida
i chiarimenti e le proposte della revisione più recente, distinguendo le conferme
ancora necessarie. Non introduce nuove decisioni né trasforma l'accordo di Marco
in approvazione del cliente; riferimenti di dettaglio: DS-038–DS-052.

## Domande e stato delle risposte

Aggiornamento OP-04/OP-07 (DS-052): cambio della scuola rappresentata solo senza voti,
anche zero o parziali bloccano. La riapertura non aggira il vincolo. Distinta e consentita
la correzione del nome della scuola secondo DS-037. Proposta da riportare al cliente.

Aggiornamento OP-04 (DS-051): sostituzione atleta con conferma che mostri uscente ed
entrante, voti conservati nella stessa gara e riapertura preventiva dell'esibizione se
chiusa. Nessuna modifica alle altre iscrizioni o all'identità anagrafica. Procedura da
riportare al cliente, distinta dalla correzione del nome DS-037.

Aggiornamento OP-13 (DS-050): vista unica sempre disponibile, anche prima dell'inizio
della gara, aggiornata con i voti registrati. Indicazione Provvisoria fino alla chiusura;
esibizioni senza voti nell'ordine della scaletta, senza piazzamenti. Nessuna modalità
separata per la classifica provvisoria. Proposta di Marco da riportare al cliente.

Aggiornamento DS-049: proporre al cliente la numerazione ex aequo 1-1-3, indicata da
Marco come probabile ma ancora da convalidare. L'accesso di tutti i primi a pari merito
ai passaggi successivi resta deciso in DS-042; non dedurre da questa ipotesi i premi materiali.

Aggiornamento DS-048: tutte le azioni/stati che impediscono la valutazione devono avere
un percorso reversibile, comprese Non presentata, squalifica e chiusure. Revoca con
riapertura preventiva della gara se chiusa, ritorno alla valutazione senza perdita di
dati e senza rendere automaticamente definitive le classifiche. Verificare il recupero
da errori dell'operatore per ogni stato bloccante, non soltanto per la squalifica.

Aggiornamento revoca squalifica (DS-047): previa riapertura della gara se chiusa,
recupero dei voti conservati e ritorno dell'esibizione aperta da verificare/richiudere.
Nessun voto generato e nessun ripristino automatico delle classifiche definitive;
ricalcolo e riconferma dei risultati interessati. Proposta da validare con il cliente.

Aggiornamento OP-04/OP-12 (DS-046): squalifica anche durante l'esibizione e senza voti,
conservando gli eventuali voti ma escludendo l'esibizione da piazzamenti, qualificazioni
e confronto della numerosità. Se la gara è chiusa: riapertura preventiva, ricalcolo e nuova
chiusura, con riconferma delle classifiche successive interessate. Superato il rinvio
DS-036 per questo caso; proposta concordata con Marco da sottoporre al cliente.

Aggiornamento OP-04 (DS-045, supera i riferimenti al divieto DS-002 sotto): spostamento
di coreografie già valutate richiesto dal cliente. Proposta condivisa con Marco: avviso
e conferma, voti e autori conservati, gare chiuse coinvolte riaperte e classifiche collegate
ricalcolate/provvisorie fino a riconferma. Discrepanze di numerosità secondo DS-043/044.
Procedura da validare con il cliente, distinta da ritiro, squalifica e cancellazione.

Aggiornamento DS-044: l'esclusione è guidata dalla discrepanza, non libera. Si parte
dalle giurie più numerose, solo con giudici aventi valutazioni mancanti; a numerosità
uniforme niente altre esclusioni, salvo reinclusione per rivedere la scelta. Verificare
i due esempi concordati in DS-044 e approfondire le combinazioni più articolate.

Aggiornamento giuria (DS-043, prevale su OP-09/OP-14 e sul riuso del totale in OP-05):
tutti i giudici giudicano tutto nel caso ordinario. Per assenze/abbandoni/sostituzioni,
proporre al cliente l'esclusione manuale, reversibile e locale alla classifica, conservando
i voti e verificando la numerosità risultante anche nei premi successivi. Stesso numero
non richiede stessi giudici. Nessun posto in giuria; relazione interna di sostituzione
solo come alternativa. Da chiarire minimo tre e finalizzazione delle discrepanze irrisolte.

Aggiornamento parità (DS-042, prevale sui riferimenti a DS-018 nella tabella): voti
0-10 con un decimale ordinario; nessuno spareggio dedicato e nessun blocco alla chiusura.
Tutti i primi ex aequo accedono alla classifica successiva del rispettivo percorso.
Restano da precisare numerazione dei piazzamenti successivi e premi materiali, senza
riaprire la decisione sull'accesso di tutti i primi. Il flusso UI di chiusura singola
è ancora da definire; la chiusura massiva non è approvata.

Aggiornamento denominazioni (DS-041): Open/PRO/Semi-Pro sono nomi modificabili;
rinominare conserva identità del livello, iscrizioni e regole dei premi. Nessuna regola
si ricava dal testo del nome; modificarla è un'operazione distinta, qui non definita.

Aggiornamento data entry (DS-039): confermata con Marco la scheda mono-esibizione,
distinta dal foglio cartaceo multi-coreografia. Nessun blocco di inserimento ogni venti
esibizioni; stampa e chiusura multipla delle gare sono questioni separate.

Aggiornamento OP-06 (DS-038): il riscontro del 14 ottobre è stato esaminato con Marco
per il ruolo di Assolo/Duo. La chiave corrente è specialità + categoria + livello,
senza tipo; le suddivisioni future sono nuove specialità configurabili. Questo supera
il punto aperto DS-030 nella tabella sottostante. Le altre risposte del nuovo PDF
saranno valutate singolarmente, senza considerare il silenzio come approvazione.

| ID | Domanda | Impostazione iniziale / impatto |
| --- | --- | --- |
| OP-01 | Ammessi ai premi successivi? | **Risposto:** solo vincitori delle singole gare. Dettaglio dei passaggi in OP-05. |
| OP-02 | Scaletta modificabile durante l'evento? | **DS-022–DS-025 da validare:** una riga per esibizione; riordino delle non iniziate anche tra gare diverse, posizione bloccata per iniziate/concluse con orari correggibili. "Posticipa da qui…" sposta solo gli orari previsti della riga non iniziata selezionata e delle successive non iniziate, mantenendo intervalli e ordine. Effettivo registrabile e correggibile separatamente; nessun effetto sui risultati. |
| OP-03 | Qual è l'unità di chiusura? | **DS-027–DS-029 da validare:** data entry dopo l'avvio senza prerequisito di conclusione fisica; chiusura esplicita dell'esibizione dopo tutti i voti, poi gara chiudibile quando tutte chiuse o non presentate. Riaprire un'esibizione di gara chiusa richiede avviso/conferma: riapre la gara e rende provvisorie le classifiche collegate, ma sblocca solo l'esibizione scelta. Restano validazione del cliente, parità DS-018 e ipotesi DS-003 sul "gruppo". |
| OP-04 | Quali modifiche sono ammesse dopo voti o chiusure? | **Da validare e completare:** DS-001/DS-002: voti mantenuti per soli cambi componenti nella stessa gara, spostamento vietato con almeno un voto, riapertura preventiva delle chiuse. DS-035: eliminazione solo senza voti; anche zero/parziale blocca, anche dopo riapertura. Correzioni e classifiche collegate secondo DS-019–DS-021/DS-029. **DS-036:** ritiri/annullamenti dopo valutazione rinviati al cliente, senza procedure dedicate nel perimetro attuale. |
| OP-05 | Come funzionano specialità e assoluto? | **DS-016/DS-017:** premi ai primi tre, solo i primi accedono al passaggio successivo; PRO separato a categoria unica e accesso diretto all'assoluto PRO; riuso delle votazioni. **DS-019/DS-020/DS-021 da validare:** correzioni propagate; già la sola riapertura rende provvisorie le classifiche collegate, da riconfermare anche senza correzioni. Disciplina definitiva solo con gare alimentanti chiuse, assoluto non PRO con discipline definitive, assoluto PRO con gare PRO chiuse. Restano ex aequo (DS-018) e procedura generale di chiusura/riapertura (OP-03). |
| OP-06 | Come combinare discipline, tipi, categorie e livelli? | **DS-031/DS-032 da validare:** categorie e livelli separati, gare dalle combinazioni iscritte senza matrice preventiva né esclusioni locali; nuova combinazione genera gara. Gara non iniziata rimossa se svuotata dall'eliminazione/spostamento dell'ultima coreografia, conservando le anagrafiche. PRO a categoria unica (DS-016). Differenza dalle esclusioni del riscontro originale da validare. **DS-030 aperto:** Assolo/Duo come disciplina e ruolo del tipo nella chiave delle gare. |
| OP-07 | Sono ammesse scuole miste nella stessa coreografia? | **Da validare con il cliente:** DS-004 condivisa con Marco: atleta unico, una scuola per coreografia, partecipazione per scuole diverse in coreografie diverse; elenchi atleti per scuola derivati dalle coreografie. Nessuna coreografia multi-scuola nel perimetro proposto. |
| OP-08 | Stessi atleti in più coreografie della stessa gara? | **Risposto, DS-040:** consentito con nomi diversi, iscrizioni/esibizioni/voti distinti. Confermato da Marco; superato il blocco DS-005. |
| OP-09 | Confronto tra giurie diverse e spareggi? | **DS-015:** somma senza normalizzazione, anche con diversa numerosità; nessun vincolo di uguale numero, minimo 3 giudici e uguale peso. **DS-018, da chiedere al cliente:** come risolvere parità nelle singole gare e nelle classifiche di disciplina/assolute, chi decide e con quali effetti su podi e qualificazioni (OP-05). Spareggio nella gara di origine solo ipotesi di Marco; nessuna ammissione automatica di tutti i primi ex aequo. Il giudice tecnico resta solo un precedente storico. |
| OP-10 | Dove e quando si possono aggiungere criteri? | **Da validare con il cliente:** DS-012/DS-013/DS-014 condivise con Marco: elenco uniforme, modificabile solo prima del comando "Avvia manifestazione", con conferma esplicita. I voti sono inseribili soltanto dopo l'avvio, nel rispetto dei vincoli di gara/esibizione. L'avvio non blocca le altre modifiche ammesse né avvia tutte le gare. Confronto tra giurie diverse in OP-09. |
| OP-11 | Cosa accade all'atleta rimasto senza coreografie? | **Proposta alternativa da validare:** DS-006 di Marco mantiene l'anagrafica completa e toglie l'atleta solo dall'elenco partecipanti; sostituisce la cancellazione automatica richiesta nel PDF. |
| OP-12 | Come gestire una coreografia non presentata? | **Da validare e completare:** DS-007-DS-011 condivise con Marco: iscrizione conservata, in fondo senza posizione né voti fittizi; accesso con conferma solo senza voti, anche zero/parziali bloccano. Conta come gestita per la chiusura; revoca confermata verso da valutare, previa riapertura se gara chiusa. Gli atleti restano partecipanti in quanto iscritti, distinti dagli effettivamente esibiti. Ultimo posto a zero solo come alternativa sul regolamento. Qualificazioni in OP-05 e flusso generale di chiusura in OP-03. |
| OP-13 | È chiara e utile la classifica provvisoria proposta? | **Aperto:** spiegare la vista durante l'inserimento, non definitiva, e validare il trattamento delle incomplete. |
| OP-14 | Come assegnare i giudici alle gare? | **DS-033, da chiedere al cliente:** giuria vuota con assegnazione/copia, oppure tutti i giudici inizialmente inclusi e rimozioni locali. Nessuna scelta approvata; verificare se giudicano normalmente tutti tutte le gare e la gestione dei giudici aggiunti dopo la configurazione. Restano minimo tre giudici e modificabilità fino all'avvio della gara. |

### Contesto da riportare al cliente: "gruppo" (OP-03)

Nel PDF annotato, a pagina 5, la domanda originale era: "Preferisci chiudere ogni esibizione
appena trascritti tutti i voti o chiudere un'intera gara alla fine?". La proposta di allora
era chiudere le singole esibizioni e chiudere implicitamente la gara quando fossero tutte chiuse.
Il cliente ha aggiunto:

> devo poter scegliere quante coreografie stanno in un gruppo prima di chiudere
> su questo ne parliamo a voce.

Domanda da riproporre: con "gruppo" intendevi tutte le coreografie appartenenti alla stessa
gara, oppure un insieme di esibizioni da chiudere insieme, eventualmente diverso dalla gara?
Il riferimento non va confuso automaticamente con il tipo di coreografia Gruppo.
La nostra ipotesi è la prima (DS-003), ma serve la sua conferma. La proposta attuale prevede
chiusura delle esibizioni e successiva chiusura esplicita della gara (DS-027/DS-028): va
verificato che risponda all'esigenza espressa, non presentata come già accettata dal cliente.

### Spiegazione da dare al cliente: classifica provvisoria (OP-13)

A pagina 4, nella sezione "Classifiche e premi", il cliente ha scritto:

> (per classifica provvisoria cosa si intende?)

Per classifica provvisoria intendiamo la vista dei risultati mentre il lavoro di inserimento
e controllo non è ancora concluso: non è una gara o una votazione aggiuntiva. Può cambiare
quando arrivano altre valutazioni o si correggono quelle già inserite.

Nella proposta da validare:

- le esibizioni con tutti i voti possono essere ordinate per punteggio, segnalando quelle
  la cui valutazione non è ancora chiusa;
- le esibizioni con voti mancanti restano visibili in una sezione separata/in fondo senza
  posizione: i voti mancanti non sono trattati come zero;
- la classifica della gara diventa definitiva con la chiusura esplicita della gara;
- le classifiche di disciplina e assolute restano provvisorie finché non sono soddisfatte
  le condizioni di DS-020 e non vengono confermate;
- una riapertura fa tornare provvisori i risultati interessati secondo DS-021/DS-029.

Esempio: una coreografia con scheda completa compare tra i risultati ordinabili; un'altra
con un voto ancora da trascrivere resta visibile come incompleta, senza un piazzamento
basato su un totale parziale. Chiedere se questa vista è chiara e utile per il lavoro in segreteria.

### Priorità del confronto

Prima OP-04 e OP-03 (modifiche e chiusure), poi OP-06/OP-07/OP-08 (iscrizioni e gare),
infine OP-05/OP-09/OP-10 (votazione e premi). Gli altri punti si collegano a queste decisioni.
I requisiti espliciti sono recepiti, ma non si avvia lo sviluppo delle parti ambigue
trasformando proposte tecniche in risposte attribuite al cliente.

## Segnalazioni del cliente sul prototipo

Segnalazioni ancora da riprodurre e verificare nel codice; non marcate come risolte.

| ID | Segnalazione | Risultato atteso |
| --- | --- | --- |
| BUG-01 | I filtri non funzionano. | Ogni filtro restringe coerentemente i risultati; il reset ripristina l'elenco. |
| BUG-02 | Non si riesce a deselezionare nella schermata iscritti. | La selezione può essere rimossa senza modificare un'iscrizione. |
| BUG-03 | Una nuova disciplina non vede le categorie esistenti fino alla riapertura. | Elenchi condivisi aggiornati immediatamente. |
| BUG-04 | Per aggiungere un iscritto bisogna modificare e salvare un vecchio iscritto. | Inserimento nuovo indipendente dalla modifica di altri dati. |
| BUG-05 | La modifica del nome di un giudice non aggiorna il nome visualizzato. | DS-037 da validare: nomi di giudici, atleti e scuole aggiornati in tutte le viste anche dopo l'avvio/chiusura, senza cambiare identità, associazioni, voti o classifiche. Distinguere correzione nominale e sostituzione. |
| UX-01 | I voti dei giudici sono troppo distanti. | Scheda compatta per esibizione, con giudici consultabili nella stessa visuale per le dimensioni operative concordate. |
| FUN-01 | Giudici, specialità/discipline e categorie non sono eliminabili. | DS-034 da validare: scuole, atleti, giudici, discipline, categorie e livelli eliminabili solo se non utilizzati; altrimenti blocco con riferimenti visibili, nessuna cascata su iscrizioni, giurie o voti. |

Le ulteriori richieste sono incorporate nel modello: scuole e relativi atleti, coreografie
Assolo/Duo/Gruppo, conteggio coreografie per atleta, distinzione categoria/livello Open-Pro.

## Percorso proposto, da approvare prima dell'implementazione

1. **Manifestazione locale e anagrafiche:** gestione del file/backup, Scuola, Atleta,
   Coreografia, Disciplina, Categoria e Livello; validazione delle composizioni e conteggio
   iscrizioni e scuole rappresentate per coreografia. Risolvere prima OP-06/OP-07/OP-08.
   Definire una strategia per i dati già presenti nella demo: nessun reset
   automatico o conversione distruttiva del database esistente.
2. **Gare, giurie e avvio:** gare delle combinazioni utilizzate, selezione dei giudici,
   minimo 3 giudici, modifiche durante l'evento, scaletta aggiornabile e cancellazioni protette.
   Prima definire gli effetti sui dati già votati (OP-04).
3. **Voti e chiusure:** scheda compatta, voti opzionali fino all'inserimento, salvataggio,
   validazione, criteri estendibili e chiusure/riaperture secondo OP-03 e OP-10; gestione
   dei non presentati secondo OP-12, senza conversioni implicite delle assenze in zero.
4. **Classifiche e premi:** provvisorie/definitive, incomplete, ex aequo e premi speciali
   subordinati alle risposte sui punti aperti, percorso specialità/assoluto e Pro separato.
5. **Verifica sul campo:** riproduzione delle segnalazioni del cliente, prova completa
   senza rete, riapertura del file, passaggio tra PC e ripristino backup.

Non si tratta ancora di un piano di commit o di una promessa di completamento in cinque
rilasci: serve a rendere esplicite le dipendenze e a scegliere il primo vertical slice.

## Casi di accettazione da trasformare in test

Le verifiche dipendenti da punti aperti sono scenari da completare, non comportamenti già
approvati. Le vecchie verifiche sul blocco globale e sulla scuola unica dell'atleta sono superate.

- Preparare e utilizzare una manifestazione senza connessione né MPS.
- Riutilizzare gli elenchi stabili senza copiare iscritti, voti, giudici o stati precedenti.
- Rifiutare Assolo con due atleti, Duo con uno o tre e Gruppo con meno di tre.
- Rifiutare componenti duplicati nella stessa coreografia; una scuola rappresentata per coreografia
  e liste atleti per scuola senza duplicati secondo DS-004, da validare in OP-07.
- Consentire allo stesso atleta coreografie per scuole diverse, senza duplicarne l'identità.
- Consentire composizioni identiche nella stessa gara con nomi diversi (DS-040), mantenendo
  iscrizioni, esibizioni e valutazioni distinte, senza copie automatiche dei voti.
- Rendere disponibili categorie/livelli alla creazione di una disciplina; ricavare le gare
  dalle combinazioni iscritte senza matrice preventiva né esclusioni locali (DS-031).
- Verificare cancellazione delle anagrafiche non utilizzate e blocco con riferimenti visibili
  per quelle utilizzate, senza cancellazioni a cascata (DS-034).
- Consentire giurie differenti di almeno 3 giudici; consentire correzioni della giuria fino
  all'avvio della relativa gara anche a manifestazione iniziata.
- Gestire durante l'evento assenze, correzioni disciplina e passaggi Gruppo/Duo; trattamento
  subordinato a DS-001/DS-002 e alle questioni residue di OP-04.
- Bloccare cambi gara diretti o indiretti con almeno un voto, anche zero/parziale e anche
  dopo riapertura, senza modificare i dati; senza voti consentirli se rispettano gli altri vincoli.
- Consentire aggiornamenti della scaletta durante l'evento; dettagli operativi da definire.
- Distinguere voto zero da voto mancante; rifiutare valori fuori 0–10 o con precisione non ammessa.
- Verificare nuovi criteri, chiusura/riapertura e protezione dei voti secondo OP-03 e OP-10.
- Secondo DS-007 da validare, mostrare le non presentate dopo le valutate senza posizione
  o voti fittizi, distinguendole da uno zero effettivo ed evitando ex aequo con esso.
  Verificare la conservazione dell'iscrizione; aspetti operativi residui in OP-12.
- Secondo DS-008, richiedere conferma per "Non presentata", non modificare nulla se annullata
  e bloccare il passaggio con almeno un voto, anche zero/parziale, spiegando il motivo.
- Secondo DS-009, richiedere conferma anche per la revoca, riportare da valutare senza creare
  voti e impedire la revoca a gara chiusa finché non viene esplicitamente riaperta.
- Secondo DS-010, una non presentata conta come gestita per la chiusura senza voti né posizione;
  la revoca la riporta tra quelle da valutare, senza implicare una chiusura automatica della gara.
- Secondo DS-011, mantenere nei partecipanti anche chi è iscritto soltanto a coreografie non
  presentate; distinguere questo elenco dall'eventuale conteggio degli effettivamente esibiti.
- Verificare vista provvisoria e incomplete secondo OP-13; ex aequo e accessi ai premi secondo OP-05/OP-09.
- Ammettere ai premi successivi solo vincitori, mantenendo Pro separato; dettagli di qualificazione da completare.
- Eliminando una coreografia, preservare le anagrafiche: secondo DS-006 l'atleta senza altre
  coreografie sparisce solo dai partecipanti, resta selezionabile e può essere reiscritto
  senza ricrearlo. Chi è iscritto altrove, anche per altre scuole, resta partecipante.
- Verificare salvataggio/riapertura e recupero da backup senza perdita dei voti registrati.
