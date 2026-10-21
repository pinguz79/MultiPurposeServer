# Consolidamento funzionale - 21 ottobre 2026

## Stato e precedenza

Ultima integrazione: DS-066 chiude le scelte di configurazione, esclude la produzione delle
schede e chiarisce l'assenza di blocchi temporali per nuove coreografie. Prevale sui punti
AP-02/AP-03/AP-04 e sui limiti ancora indicati come aperti nel testo storico sottostante.
Fonte corrente per il nuovo prototipo: Documentation/Skating nel repository MultiPurposeServer.

Consolida le decisioni discusse con Marco dal 19 al 21 ottobre 2026 dopo il PDF annotato
`Skating-System-riepilogo-cliente-2026-10-16-1.pdf`. Le voci DS-053-DS-065 sono condivise
con Marco, da sottoporre al cliente salvo le richieste esplicitamente attribuite al suo
riscontro. L'accordo interno non equivale a una nuova approvazione del cliente.
Non attesta implementazioni, test o funzionalità già disponibili.

Per i temi qui trattati questo documento prevale sulle proposte incompatibili di
Modello.md, PianoDiLavoro.md, DecisioniSemantiche.md e dei PDF precedenti; il resto delle
regole pregresse resta valido. Le decisioni storiche non sono cancellate.
Le scelte tecniche sono nel registro DecisioniTecniche.md, non nel futuro PDF cliente.

## DS-053 - Discipline, scuole e accorpamenti

- Struttura uniforme: disciplina + categoria + livello identificano la gara.
- Assolo/Duo resta una disciplina. Possibili nuove discipline sono Assolo/Duo Caraibico,
  Assolo/Duo Moderno oppure Assolo Caraibico, Duo Caraibico, Assolo Moderno, Duo Moderno.
  Non si introduce una gerarchia di sotto-discipline né una regola speciale per questi nomi.
- Un atleta ha un'anagrafica unica; la scuola rappresentata è sulla coreografia.
  Può rappresentare scuole diverse in coreografie diverse, compare negli elenchi di tutte
  queste scuole ed è segnalato per i controlli sulle iscrizioni e sui pagamenti.
- Il totale degli atleti della manifestazione conta persone distinte, non presenze per scuola.
  Questo conteggio non stabilisce quote o regole di pagamento.
- Non si introducono coreografie che rappresentano più scuole contemporaneamente.
- Per evitare gare senza sfidanti basta modificare categoria/livello della coreografia;
  nessuna funzione distinta di fusione gare. Con voti già presenti si applica DS-045:
  conservazione dei voti, riaperture necessarie e nuova verifica dei risultati interessati.
- Il cambio scuola resta consentito solo prima di qualsiasi voto, incluso zero.

Conferma/precisa DS-004, DS-038, DS-045, DS-051 e DS-052. Restano ammesse coreografie
con gli stessi atleti nella stessa gara se hanno nomi diversi (DS-040).

## DS-054 - Percorso dei premi e convalida

- Non PRO: vincitori delle singole gare -> premio intermedio -> vincitori dei premi
  intermedi all'assoluta non PRO.
- Aggregazione intermedia configurabile per manifestazione: per disciplina oppure per
  categoria, in quest'ultimo caso trasversalmente alle discipline.
- Si prevedono entrambe le modalità; il criterio è indicato nel JSON letto all'avvio
  dell'applicazione e non modificabile a runtime (DS-066). La scelta del regolamento è attesa al
  massimo un mese prima della manifestazione. Non si sceglie una modalità al suo posto.
- PRO: categoria unica, vincitori delle singole gare direttamente all'assoluta PRO,
  senza premio intermedio. Le due assolute restano separate.
- Classifiche intermedie e assolute provvisorie fino alla definitività di tutte le fonti;
  successiva verifica e convalida esplicita, non chiusura automatica.
- La convalida richiede la risoluzione delle parità che coinvolgono il podio e genera
  la notifica di premio pronto per la premiazione.

Precisa/sostituisce il criterio fisso di disciplina di DS-016 e conferma il percorso PRO.
Il riuso dei voti segue DS-055: rettifiche ufficiali condivise, non bonus locali al premio.

## DS-055 - Parità: modifica ufficiale di un voto e propagazione

Aggiornamento del 21 ottobre 2026. Stato: requisito acquisito su esplicita decisione di
Marco, sulla base della risposta scritta del cliente del 21 ottobre (10:13) e del chiarimento
verbale, riportato da Marco, sul fatto che si modifica uno dei voti dei giudici. Il cliente
non ha ancora risposto al messaggio riassuntivo delle 10:16, ma Marco richiede di considerare
la regola assodata; eventuali revisioni saranno change request. AP-01 è chiuso.

- Parità consentite fuori dal podio. Quelle che coinvolgono primo, secondo o terzo posto,
  inclusa la parità terzo/quarto, devono essere risolte prima della chiusura.
- Il cliente propone un incremento di 0,1 deciso dai giudici/giudice tecnico; non è un
  incremento attribuito automaticamente dal software.
- Superate DS-042 e DS-049 nella parte che ammette primi ex aequo e avanzamento di tutti.
- Su indicazione dei giudici l'operatore modifica uno specifico voto/criterio di un giudice,
  normalmente di +0,1. Non si modifica direttamente il totale e non si aggiunge un bonus.
- Il voto rettificato diventa ufficiale per tutti gli utilizzi: gara originaria, premio
  intermedio e assoluto. Restano applicabili le esclusioni di voti proprie della classifica:
  la rettifica contribuisce solo dove quel voto è conteggiato.
- Anche una rettifica decisa nel premio intermedio o nell'assoluto si riflette nei risultati
  precedenti collegati. Superata l'ipotesi di conservarvi un punteggio definitivo immutabile.
  Restano le protezioni di riapertura, consenso esplicito e nuova convalida: nessuna modifica
  silenziosa di schede chiuse. Applicare la procedura di correzione e la notifica dei risultati
  da riverificare; non introdurre una nuova regola di assenza retroattiva dei giudici.
- Il software evidenzia tutte le parità, anche fuori dal podio; fuori dal podio è ammesso
  lasciarle. Non sceglie il vincitore, il giudice/criterio da correggere o incrementi a cascata.
- Dopo ogni modifica ricalcola ed evidenzia eventuali nuove parità. Per esempio una parità
  al terzo posto può diventare una parità al secondo, poi al primo: ogni ulteriore rettifica
  è una decisione umana separata, non una risoluzione automatica. Solo i primi accedono ai premi.
- Si mantiene il range 0-10 con un decimale: la gestione spareggi non autorizza voti oltre 10.
- Ipotesi superate, conservate qui come storico: modificatore locale al premio; copia dei
  voti senza rettifiche per i premi successivi; gare derivate con voti indipendenti per isolare
  gli spareggi. Nessuna duplicazione semantica dei voti è richiesta per questo caso d'uso.

Esempio cliente: Baby 30/30 -> un voto corretto porta il primo a 30,1; Senior primo a 30,1.
Nel premio intermedio nuova parità: correggendo un voto del Baby, il suo punteggio diventa
30,2 anche nella gara Baby e viene riutilizzato nell'assoluto. Il secondo Baby non accede
al premio intermedio. Gli effetti sui confronti successivi sono intenzionali secondo il cliente.

## DS-056 - Giudici: assenze ed esclusione automatica

- Sostituzione del giudice solo prima dell'inizio della manifestazione: semplice modifica
  del nome, senza gestione di sostituzioni a evento iniziato. Distinta dalla correzione
  puramente nominale di un refuso. Il normale insieme dei giudici valuta tutte le esibizioni.
- Lo stato presente/assente si gestisce fuori dalla scheda voti, nella schermata giudici.
  Nessuna decorrenza da inserire. Anche l'abbandono definitivo è trattato come assenza.
- Al primo accesso un blocco usa lo stato corrente per proporre campi abilitati/disabilitati;
  quelli già aperti conservano le abilitazioni assegnate. Rientrare non cambia retroattivamente
  le presenze delle esibizioni già configurate.
- Gli assenti hanno campi visibili ma disabilitati. I voti già registrati non sono cancellati.
- Possibile abilitare il giudice sulla singola esibizione o temporaneamente sull'intero blocco.
  Lo sblocco collettivo termina uscendo dalla pagina; non riapre schede confermate e non cambia
  lo stato generale del giudice. Le righe lasciate vuote mantengono l'assenza.
- Il primo voto inserito su una riga abilitata registra la partecipazione del giudice a quella
  valutazione: da quel momento tutti i suoi criteri sono richiesti per confermare la scheda.
- Operazione inversa: segnare assente per singola esibizione, su scheda aperta e soltanto se
  tutti i campi del giudice sono vuoti. Nessuna eliminazione implicita di voti.
- Per ogni gara/classifica, se le esibizioni confrontate hanno diverso numero di valutazioni,
  si ignorano automaticamente i voti dei giudici non presenti in tutte. Se il numero è uguale,
  non si escludono giudici soltanto perché le identità differiscono.
- Conservare i voti esclusi, evidenziandoli come non conteggiati; nessuna selezione manuale
  dei giudici da scartare per uniformare i totali. Supera DS-043 e DS-044.
- Escludere dal confronto non presentate e squalificate. Nella provvisoria confrontare solo
  schede complete, anche ancora aperte; una trascrizione incompleta non simula un'assenza.
- Ricalcolo autonomo per ciascuna classifica. L'assenza successiva non riscrive gare già chiuse.
- Nessun blocco o flusso eccezionale se dopo l'esclusione restano meno di tre giudici;
  questo non introduce una deroga al normale requisito iniziale di giuria.
- Esclusioni visibili durante il data entry; notifica riepilogativa solo quando la classifica
  è pronta per la verifica, non a ogni completamento o ricalcolo provvisorio.

## DS-057 - Data entry paginato e chiusure

- Unica lista di esibizioni ordinata secondo i blocchi, paginata; pagina = blocco cartaceo.
  Niente tab per gara né apertura obbligatoria di dialog per ogni riga.
- Voti editabili direttamente nella griglia; colonne raggruppate per giudice e criteri,
  con identificativi della coreografia/gara sempre riconoscibili. Schermo desktop ampio.
- Ogni riga può essere incompleta, confermata o riaperta indipendentemente dalle altre.
- Comando Conferma esibizioni complete della pagina: chiude solo quelle complete,
  lascia aperte le incomplete. Un blocco può rimanere incompleto passando ad altri blocchi.
- La conferma della pagina non chiude gare. Quando tutte le esibizioni della gara sono
  confermate, non presentate o squalificate, avviso Gara pronta per la chiusura.
- Apertura classifica, verifica e chiusura esplicita; solo allora pronta per la premiazione.
- Schede confermate non modificabili senza riapertura; stati non presentata/squalificata
  e revoche restano reversibili secondo le regole pregresse.

Supera DS-039 (mono-esibizione) e l'interpretazione di gruppo di DS-003; precisa DS-027-DS-029.

## DS-058 - Salvataggio, tastiera e validazione

- Salvataggio automatico del voto valido lasciando la cella, distinto dalla conferma.
- Zero è voto valido; vuoto significa voto mancante. Valori 0-10 con massimo un decimale.
- Testo selezionato all'ingresso; punto del tastierino e virgola accettati come separatore.
- Tab e Maiusc+Tab navigano avanti/indietro saltando le celle bloccate.
- Invio configurabile stabilmente come preferenza dell'operatore: prossimo voto della stessa
  esibizione oppure stessa colonna della successiva. Ricordata fra sessioni, modificabile
  senza effetti su dati o regole della manifestazione; non opzione limitata al prototipo.
- Valore errato: messaggio e focus nuovamente sulla cella con testo selezionato; non si può
  proseguire con Tab, Invio, clic o cambio pagina finché corretto. Esc ripristina l'ultimo
  valore salvato. Nessun arrotondamento/correzione silenziosa o sovrascrittura del valore valido.
- Svuotare una cella di scheda aperta è editing ordinario, senza backup; vale anche dopo
  riapertura. Scheda incompleta finché manca un voto richiesto. Vuoto non trattiene il focus.

## DS-059 - Blocchi stabili e scaletta operativa

- L'organizzatore decide la composizione/dimensione dei blocchi; possono attraversare gare.
- Conferma blocchi della scaletta è un comando esplicito indipendente dalla stampa.
  Prima della conferma composizione libera; dopo, numeri e appartenenza stabili.
- Modifica strutturale successiva esplicita, con avviso di aggiornare/ristampare le schede.
- Avvio manifestazione distinto dalla conferma blocchi e richiede i blocchi iniziali confermati.
- Nuova coreografia dopo la preparazione definitiva: nuovo progressivo e blocco supplementare,
  con propria conferma, senza rinumerare i vecchi. Normalmente prima dell'avvio, ma nessun
  blocco temporale esplicito: consentita anche dopo sotto responsabilità dell'operatore (DS-066).
- Ordine operativo separato dal blocco cartaceo/data entry: la n. 20 può essere rimandata
  dopo la n. 30 mantenendo numero, pagina, voti e stato.
- Durata per singola coreografia in minuti/secondi, possibile valore predefinito modificabile.
  Nessun limite regolamentare o avviso di range: valutazione affidata all'operatore.
  Solo formato valido e valori non negativi.
- Tempo tecnico tra esibizioni predefinito e modificabile sul singolo intervallo.
- Pause esplicite con descrizione/durata; sostituiscono il tempo tecnico fra le due esibizioni,
  non vi si sommano automaticamente. Nessuna numerazione di esibizione né blocco voti.
- Orari indicativi indipendenti dal data entry e senza cronometraggio automatico. Cambiare
  l'orario previsto di una riga ricalcola le successive, mantenendo durate/pause; precedenti
  invariati, nessun cambiamento a numeri, voti o blocchi. Riordino ricalcola gli orari interessati.
- Pausa Premiazione collegabile a una o più classifiche: clic diretto se unica, scelta se più;
  mostra Provvisoria/Definitiva. Solo navigazione, nessuna chiusura automatica.
- AP-02 chiuso per perimetro: produzione schede, PDF e stampa esclusi, eventuale futura
  change request. Blocco/pagina resta confermato indipendentemente dalla stampa (DS-066).

Precisa/sostituisce le precedenti proposte DS-022-DS-025 dove incompatibili.

## DS-060 - Backup e ripristino: comportamento funzionale

- Prima delle operazioni elencate sotto, copia preventiva; operazione consentita solo dopo
  il completamento del backup. Fallimento: messaggio immediato, notifica, nessuna operazione.
- Trigger confermati: esclusione/reinclusione giudice; spostamento coreografia già votata;
  eliminazioni di entità; conferma esibizioni (un dump per tutta la pagina); ripristino backup.
- Nessun dump per semplice riapertura gara, squalifica/non presentata e relative revoche,
  singola modifica o svuotamento di voti su scheda aperta/riaperta, normali ricalcoli provvisori.
- Backup manuale disponibile con descrizione inserita dall'operatore.
- Elenco copie: data/ora e descrizione, recenti prima. Nome file non esposto nella GUI.
- Nessuna cancellazione delle copie, automatica o tramite GUI in questa versione.
- Prima di ripristinare: verifica copia leggibile/compatibile e conferma esplicita; backup
  dello stato corrente descritto come precedente al ripristino richiesto.
- Ripristino dell'intera manifestazione, non di singole schede. Catalogo backup e notifiche
  NON tornano indietro. Anche il ripristino errato è recuperabile tramite copia preventiva.
- L'elenco dei trigger non introduce un nuovo comando manuale di esclusione punteggi:
  il calcolo delle esclusioni rimane quello automatico di DS-056. Il momento transazionale
  del backup relativo a tali variazioni andrà precisato nell'implementazione senza produrre
  dump a ogni visualizzazione né estendere arbitrariamente la lista approvata.

## DS-061 - Centro notifiche

- Campanella sempre visibile, badge rosso col numero di tutte le notifiche non lette.
- Elenco completo, recenti prima, data/ora, evidenza delle non lette. Aprire l'elenco non
  marca tutto letto; clic sulla notifica marca letta e naviga. Comando Segna tutte come lette.
- Letta non significa gestita; nessuna modifica dei dati di gara al clic.
- Nessuna gestione di non più attuale: gli avvisi storici restano invariati e contano nel
  badge se non letti, anche dopo ripristino. La destinazione mostra lo stato corrente.
- Se la gara non esiste nello stato ripristinato, messaggio comprensibile, senza errore.
- Apertura schermate, ripristino e semplice ricalcolo non generano nuovi avvisi.
  Una successiva operazione che riporta la gara nello stato rilevante ne genera uno nuovo.
- Tutto locale, offline e persistente; nessun servizio notifiche esterno richiesto.

### Catalogo completo approvato (dieci situazioni)

| Evento | Avviso / destinazione |
| --- | --- |
| Tutte le esibizioni della gara gestite | Gara pronta per la chiusura -> classifica |
| Chiusura gara | Gara pronta per la premiazione -> classifica definitiva |
| Tutte le gare fonti del premio intermedio chiuse | Premio disciplina/categoria pronto per verifica -> classifica intermedia |
| Tutte le fonti dell'assoluta definitive | Assoluta pronta per verifica -> assoluta, anche PRO |
| Convalida premio intermedio/assoluto | Premio pronto per premiazione -> classifica definitiva |
| Parità sul podio quando la classifica è pronta | Parità da risolvere -> classifica interessata |
| Correzione ammessa incide su risultati/qualificazioni convalidati | Risultati da verificare nuovamente -> classifica interessata |
| Esclusioni automatiche dei voti alla verifica della classifica | Riepilogo voti esclusi -> dettaglio punteggi |
| Atleta iscritto con una seconda scuola | Partecipazione con più scuole -> partecipazioni atleta |
| Creazione backup fallita | Backup non riuscito, operazione annullata -> elenco backup |

Il precedente conteggio verbale di nove era inesatto: l'approvazione in blocco comprende
tutti i dieci eventi, incluso il backup fallito. Nessuna notifica per salvataggi ordinari,
conferma singola scheda in sé o backup riuscito.

## DS-062 - Classifiche sempre consultabili

- Vista unica disponibile prima, durante e dopo la gara; Provvisoria fino alla chiusura,
  Definitiva dopo, nuovamente provvisoria a riapertura.
- Schede complete ordinabili con indicazione di quelle ancora aperte; incomplete riconoscibili
  senza posizione; senza voti in ordine di scaletta. Nessun voto mancante convertito in zero.
- Non presentate/squalificate visibili fuori dai piazzamenti, senza voti artificiali.
- Una successiva assenza non riscrive classifiche concluse; distinta dalle correzioni esplicite
  per cui restano necessari riapertura, verifica delle conseguenze e nuova convalida.

## DS-063 - Alternative grafiche da presentare al cliente

Due esempi della stessa scheda, con funzionalità identiche e dati/criteri dimostrativi:

- A: etichette colorate di disciplina, categoria e livello; righe neutre.
- B: colore disciplina come sfondo della colonna Esibizione/gara; celle voto neutre.

AP-03 chiuso da DS-066: entrambe le varianti implementate e configurabili a runtime.
Nel prossimo PDF inserire ENTRAMBE le immagini come modalità disponibili, non scelta esclusiva.
Non rigenerare PDF a ogni decisione: le immagini sono pronte per il prossimo documento.
L'appendice PDF generata il 20 ottobre è solo un esempio grafico con testo storico non consolidato.

Immagini conservate per il prossimo documento:
[alternativa A](Assets/SchedaVoti-Etichette.png) e
[alternativa B](Assets/SchedaVoti-SfondoDisciplina.png).

## DS-064 - Conferme pregresse mantenute

Restano applicabili identità unica atleta e anagrafica completa anche senza iscrizioni,
conteggio coreografie, una scuola rappresentata per coreografia, categorie/livelli configurabili,
criteri uniformi prima dell'avvio, voti 0-10 con un decimale, somma senza media/normalizzazione,
correzioni protette e stati reversibili. Le gare derivano dalle combinazioni realmente iscritte.
Non modificare database o codice sulla sola base di questo consolidamento documentale.

## DS-065 - Punti aperti e prossimi passi

1. AP-01 CHIUSO il 21 ottobre: modifica ufficiale di un voto dei giudici, propagata alle
   classifiche collegate secondo DS-055. Modificatore e voti duplicati superati.
2. AP-02 CHIUSO per perimetro: produzione PDF/stampa schede esclusa; eventuale change request.
3. AP-03 CHIUSO: entrambe le grafiche implementate, preferenza configurabile a runtime.
4. AP-04 NON BLOCCANTE: entrambi gli aggregatori implementati, JSON letto all'avvio sceglie
   quello da usare. Resta al cliente la scelta di regolamento, non un impedimento allo sviluppo.
5. Validazione cliente delle nuove procedure operative concordate con Marco; separare nel
   documento le richieste esplicite del riscontro dalle proposte successive.
6. Nuove coreografie normalmente prima dell'avvio, ma nessun blocco temporale nel software;
   applicare progressivo e blocco supplementare se i blocchi sono già confermati (DS-066).
7. Verificare col prototipo densità griglia su schermo reale, giuria/criteri reali, navigazione
   tastiera configurabile, assenze su blocchi misti e recupero da backup. Non risultano già testati.

### Scenari di accettazione prioritari

- N. 20 rimandata dopo n. 30: stessi numero/blocco, nessuna perdita di voti o rinumerazione.
- Giudice assente su 1 di 20 righe: sblocco colonna, inserimento 19, ventesima resta assente;
  voto parziale su una delle 19 impedisce la conferma di quella riga.
- A+B+C+D contro A+B+C: esclusione automatica D solo per quella classifica;
  A+B+C contro A+B+D: nessuna esclusione per sola differenza di identità.
- Squalificata senza voto di D: non provoca esclusione D per le ammesse.
- Cella con 11 o 7,85: focus trattenuto; Esc ripristina. Vuoto ammesso come mancante, zero valido.
- Conferma pagina con incomplete: backup unico, conferma solo complete, gara non chiusa da sola.
- Ripristino: dati gara tornano indietro, copie e notifiche restano; nessuna nuova notifica al
  ripristino, successiva transizione operativa può generare un nuovo avviso.
- Backup fallito: nessuna operazione delicata, messaggio immediato e notifica persistente.
- Premio PRO direttamente all'assoluta PRO; non PRO segue il parametro scelto.
- Baby 30/30, Senior 30,1: rettifica Baby a 30,1, poi a 30,2 nell'intermedio; verificare
  uso di 30,2 nella gara originaria e nei premi collegati, senza copiare punteggi pre-rettifica.
- Parità 30/30 al terzo posto sotto 30,1 e 30,2: evidenziare le nuove parità dopo ciascuna
  correzione, senza incrementi automatici. Parità all'ottavo posto segnalata ma consentita.
- Modifica di scheda chiusa protetta da riapertura e consenso; classifiche coinvolte da
  riverificare. La modifica non ammette automaticamente ai premi chi non è primo.

## DS-066 - Configurabilità definitiva e perimetro del prototipo

Data: 21 ottobre 2026. Stato: concordato esplicitamente con Marco; implementazione non avviata.

- Implementare entrambi gli aggregatori dei vincitori: per disciplina e per categoria.
  Parametro JSON letto all'avvio dell'applicazione indica quale usare; nessuna modifica
  a runtime. Resta la regola di scelta prima dell'avvio della manifestazione: nessun cambio
  silenzioso di regolamento sulle classifiche già convalidate al semplice riavvio.
- Non modellare per ora produzione delle schede, generazione PDF o stampa diretta.
  Eventuale richiesta successiva sarà change request; mantenere corrispondenza pagina/blocco.
- Implementare entrambe le grafiche dei mockup: etichette su fondo neutro e sfondo disciplina
  sulla colonna Esibizione/gara. Parametro modificabile dalla configurazione a runtime.
  Il prossimo PDF le presenta come modalità disponibili, non scelta esclusiva da approvare.
- Implementare entrambe le navigazioni Invio: prossimo voto della stessa esibizione oppure
  stessa colonna della successiva. Parametro modificabile dalla configurazione a runtime,
  persistente fra sessioni; Tab/Maiusc+Tab invariati, validazione/focus errori sempre applicati.
- Nuove coreografie normalmente inserite prima dell'avvio, senza blocco esplicito temporale.
  L'operatore può inserirle anche dopo sotto la propria responsabilità. Se i blocchi sono
  confermati: nuovo numero progressivo e blocco supplementare, senza cambiare quelli esistenti.
- Nessuna di queste decisioni modifica i voti o le classifiche attraverso preferenze grafiche
  o di tastiera. Regole di voto e spareggi restano DS-055.

Verifiche da predisporre: due aggregatori selezionabili all'avvio; cambio grafica e Invio
senza perdita dati o riavvio; inserimento successivo all'avvio senza rinumerazione; nessuna
funzione stampa implicitamente inclusa. Non risultano ancora implementate o verificate.
