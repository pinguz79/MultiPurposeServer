# Blocco 3 - scaletta e scheda voti del prototipo

## Funzionalità disponibili

Dal menu **Scaletta e voti** si prepara la suddivisione iniziale delle coreografie in blocchi
di dimensione scelta. Prima della conferma si possono cambiare blocco e ordine operativo.
**Conferma blocchi** rende stabili numeri e appartenenza; **Avvia manifestazione** è un
passaggio distinto e richiede blocchi confermati, criteri presenti, giurie di almeno tre
giudici e una sola categoria PRO. L'ordine operativo resta distinto dalla numerazione.
Nuove coreografie ricevono numero successivo e blocco supplementare da confermare.

**Scheda voti** apre il blocco dell'esibizione selezionata: una riga per esibizione e una
colonna per ciascuna coppia giudice/criterio. Frecce precedente/successivo navigano i blocchi
confermati. Le colonne si adattano alla finestra, con scorrimento quando necessario.

- Voti 0–10, al massimo un decimale, punto o virgola; vuoto è diverso da zero.
- Il voto valido viene salvato lasciando la cella. Un errore blocca l'uscita dalla cella,
  mostra un popup di avviso oltre al messaggio in basso e seleziona il testo dopo OK.
  Il popup non si duplica in caso di validazione rientrante. Esc annulla l'edit non salvato.
- Invio passa al voto successivo oppure alla stessa colonna dell'esibizione successiva;
  Tab/Shift+Tab saltano le celle non modificabili.
- Due preferenze modificabili nella barra della scheda: direzione di Invio e colore della
  disciplina sulla prima colonna. Persistono localmente, non nell'archivio manifestazione.
- Conferma singola o delle righe complete; le incomplete restano aperte. La conferma non
  equivale alla chiusura della gara. La riapertura esplicita conserva i voti.

## Presenze e protezioni

### Presentazione approvata (23 ottobre 2026)

La scheda riprende i due mockup in Assets: intestazione su due livelli con giudice unico
e criteri sottostanti; numero, nome, scuola/composizione ed etichette della gara sulla sinistra;
caselle voto distinte; totale, stato e azione Conferma/Riapri su ciascuna riga.
Conferma è disabilitato finché mancano voti richiesti. Dopo l'ultimo voto valido, Tab
porta al pulsante abilitato della riga; Invio e Spazio lo attivano. Shift+Tab permette
di tornare al voto precedente. I giudici assenti/non appartenenti alla giuria non
richiedono voti. La preferenza di Invio fra voti/esibizioni resta invariata nelle celle voto.
Il totale è la somma dei voti della scheda completa, non una classifica né una normalizzazione
delle giurie; per una scheda incompleta si mostra un trattino.

La variante neutra usa etichette colorate; l'altra estende il colore della disciplina
allo sfondo della colonna esibizione. La preferenza resta modificabile nella barra superiore.
La conferma delle righe complete è in basso, con conteggi; navigazione e comandi restano
visibili mentre scorrono le esibizioni. Presenze e sblocchi sono nel menu contestuale
del voto (tasto destro). I criteri troppo lunghi hanno un tooltip con il nome completo.
Le colonne si adattano allo spazio, con scorrimento quando giudici/criteri non entrano.
Il controllo WinForms sottostante resta lo stesso per conservare il comportamento di editing:
la nuova presentazione non cambia persistenza, conferma o validazione dei voti.

L'anagrafica giudici permette di indicare **Assente** senza decorrenza. La prima apertura
del blocco fotografa le presenze; riaperture successive non cambiano la scheda. Celle di
giudici assenti sono visibili ma disabilitate. Sono disponibili abilitazione della riga
e dell'intera colonna del giudice, limitate alla pagina. Senza voto inserito lo sblocco non
modifica la presenza; il primo voto rende obbligatori tutti i criteri di quel giudice.
Per segnare assente un giudice sulla riga occorre prima svuotarne esplicitamente tutti i voti.
Giudici non appartenenti alla giuria non diventano votanti tramite lo sblocco.

Dopo l'avvio non si aggiungono giudici e non si modificano i criteri. La giuria e i dati
strutturali delle coreografie con scheda già predisposta non si modificano; restano nome
e durata. La conferma protegge i voti fino alla riapertura esplicita.

## Persistenza e limiti del blocco

Formato SQLite v5: stato manifestazione, blocchi, esibizioni, presenze per esibizione,
voti in decimi interi e stato confermato. Aggiornamento esplicito da v1–v4, transazionale,
con backup predefinito o rinuncia esplicita come nei blocchi precedenti.

Prima di confermare voti viene salvata una copia SQLite consistente
`.before-confirm-<identificativo>.bak` accanto all'archivio. Un errore nella copia impedisce
la conferma. Non si cancellano copie. Questo è un presidio tecnico del prototipo, **non**
il catalogo separato dei backup né il ripristino guidato previsto dalle specifiche.

Restano fuori da questo blocco: orari, pause e premiazioni della scaletta; classifiche,
chiusura gara, esclusioni automatiche dei voti, squalifica/non presentata, notifiche e
catalogo/ripristino dei backup. Usare esclusivamente manifestazioni dimostrative.

## Prova manuale consigliata

1. Creare almeno due coreografie, tre giudici e verificare la giuria.
2. Preparare due blocchi, cambiare ordine, confermare e avviare.
3. Inserire voti con tastiera, inclusi zero, punto decimale e un valore errato; provare Esc.
4. Lasciare una riga incompleta e confermare quelle complete; provare modifica e riapertura.
5. Segnare un giudice assente prima di aprire un blocco nuovo, poi provare gli sblocchi.
6. Cambiare modalità Invio e colore; chiudere/riaprire il programma e verificare persistenza.

I test automatici verificano modello, migrazioni, salvataggi, conferme e controlli WinForms;
non sostituiscono la prova visuale e di usabilità sul PC dell'operatore.

Regressione validazione: il solo `EndEdit` non garantisce la validazione della cella WinForms.
Navigazione da tastiera, azioni e chiusura verificano esplicitamente il voto pendente prima di
concludere l'editor. Il cambio cella rispetta l'annullamento senza usare il setter che genera
un'eccezione quando la validazione viene rifiutata. Test con editor reale coprono Invio, Tab,
correzione successiva e tentativo di chiusura con voto non valido.
