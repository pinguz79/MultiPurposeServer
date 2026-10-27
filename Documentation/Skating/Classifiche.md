# Classifica della singola gara

Blocco del prototipo del 23 ottobre 2026, basato su DS-055/056/057/062.
Premi intermedi, assolute, centro notifiche e catalogo/ripristino dei backup restano successivi.

## Uso

Da **Gare**, selezionare una gara e aprire **Classifica...**. La vista è disponibile anche
prima di preparare la scaletta. Mostra posizione, numero, coreografia, scuola, totale, stato
e parità. Selezionando una riga si consultano tutti i suoi voti originali; quelli non
conteggiati sono grigi. **Scheda voti...** o doppio clic apre il blocco dell'esibizione.

- Le schede complete, anche aperte, hanno punteggio e posizione; le incomplete non hanno
  posizione né totale artificiale e seguono l'ordine operativo della scaletta.
- Non presentate e squalificate rimangono visibili fuori dai piazzamenti. I voti non si cancellano.
- Si sommano i voti in decimi interi, senza media o arrotondamenti. Zero è valido.
- Se le schede complete ammesse hanno numerosità differenti, si conteggiano solo i giudici
  presenti in tutte. A parità di numerosità non si esclude per sola differenza di identità.
  Le schede incomplete e quelle escluse non provocano esclusioni di giudici.
- Ogni parità è evidenziata. Le posizioni seguono il criterio competitivo (1, 1, 3).
  Qualsiasi parità che coinvolge il podio, incluso terzo/quarto, impedisce la chiusura.
  Le parità dal quarto posto in poi sono ammesse.
- Nessun incremento automatico: correggere uno specifico voto su indicazione dei giudici,
  riaprendo prima gara e scheda quando necessario, quindi confermare di nuovo.

**Chiudi gara** è abilitato solo quando tutte le esibizioni sono confermate oppure escluse
e non vi sono parità sul podio. La classifica diventa definitiva; le API locali verificano
nuovamente le condizioni nella stessa transazione della chiusura.
**Riapri gara** la rende provvisoria ma lascia le schede confermate protette.

**Non presentata**, **Squalifica** e **Revoca esclusione** richiedono consenso esplicito.
Su gara chiusa occorre prima riaprire. La revoca conserva i voti e lascia la scheda aperta
per una nuova verifica/conferma. Le schede escluse non sono editabili né confermabili.
Anche aggiunte/modifiche alle iscrizioni di una gara chiusa richiedono la riapertura.

L'assenza globale successiva di un giudice non altera le presenze storiche e quindi non
riscrive la classifica chiusa. Nella scheda voti i voti esclusi dalla classifica della gara
sono grigi con spiegazione al passaggio del mouse; il totale scheda resta la somma originale,
come indicato nella guida della schermata, distinta dal totale della classifica.

## Archivio e verifiche

Formato v6: `RaceConfiguration.IsClosed` e `Choreography.Status`. L'esito è sulla coreografia
per poterlo impostare anche prima di preparare i blocchi; non è perso rigenerando la scaletta.
Aggiornamento esplicito v1–v5, transazionale, con scelta backup già prevista.
La lettura della classifica non inizializza presenze e non scrive nell'archivio.

Test: lettura senza scritture, giurie differenti, incomplete ed escluse, zero, confine del
podio, riaperture, conservazione dei voti, assenze successive, migrazione v5 e resa WinForms.
Restano nel blocco dedicato ai backup/notifiche il catalogo separato, il ripristino e il trigger
transazionale per variazioni delle esclusioni automatiche (DS-060); nessun dump a ogni lettura.
Il prototipo va ancora provato con dati dimostrativi, non usato come prodotto completo in gara.
