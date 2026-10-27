# Gare, giudici e criteri - blocco 2

Implementazione richiesta da Marco il 22 ottobre 2026. Solo preparazione offline:
avvio, schede voti, assenze operative, risultati e classifiche non sono ancora disponibili.

## Gare derivate

La combinazione disciplina/categoria/livello identifica una gara (DS-053).
Il salvataggio di una coreografia aggiorna atomicamente le combinazioni effettive:
nessun prodotto cartesiano, nessuna gara vuota. Spostando l'ultima coreografia si
rimuove la vecchia configurazione di gara, senza cancellare cataloghi o giudici.
La composizione Assolo/Duo/Gruppo non è una quarta chiave.

Il menu Gare elenca combinazione, numero di coreografie, numerosità e modalità di
giuria, stato di configurazione. Doppio clic o Partecipanti e giuria apre la scheda
con elenco delle coreografie e selezione dei giudici.

## Giudici e giurie

Anagrafiche -> Giudici consente inserimento e modifica del nome, con identità stabile.
Per il prototipo i nomi sono obbligatori e univoci ignorando maiuscole/minuscole,
come gli altri cataloghi. Nessuna eliminazione o sostituzione operativa durante la gara.

Scelta iniziale comunicata a Marco, da verificare nell'uso rispetto a DS-033:

- Default: tutti i giudici correnti della manifestazione, anche aggiunti successivamente.
- Salva giuria: selezione esplicita di almeno tre giudici distinti; nuovi giudici non
  vengono aggiunti automaticamente a questa selezione.
- Usa tutti i giudici: torna al default.
- Annulla: nessuna modifica persistita.

Una gara con meno di tre giudici nel default resta visibile come da completare:
si possono quindi preparare le iscrizioni prima dell'anagrafica giudici.
Non si impone lo stesso numero di giudici a tutte le gare.
La selezione locale non elimina giudici dall'anagrafica o dalle altre gare.

## Criteri comuni

Anagrafiche -> Criteri di valutazione consente aggiunta, rinomina ed eliminazione con
conferma. L'elenco è unico per tutta la manifestazione (DS-012), inizialmente:
Tecnica, Interpretazione / Musicalità, Show. Nessun peso o criterio specifico per gara.
I nomi sono obbligatori e univoci ignorando maiuscole/minuscole.

Il prototipo è sempre in preparazione: non espone ancora Avvia manifestazione.
Il vincolo di non modificabilità dopo l'avvio deve essere implementato insieme a quel
flusso, prima di introdurre votazioni. Nessuna modifica dei criteri con voti esistenti
è resa possibile da questo blocco, perché i voti non sono ancora modellati.

## Segnalazioni

Nell'elenco gare: giuria inferiore a tre, nessun criterio, più categorie presenti nel
percorso PRO. La categoria unica PRO resta requisito; il prototipo segnala la discrepanza
senza accorpare implicitamente coreografie o rinominare cataloghi. Nessuna notifica
persistente o validazione di avvio in questa fase.

## Prova consigliata

1. Aprire l'archivio dimostrativo: aggiornamento v4 con le tre opzioni consuete.
2. Inserire quattro giudici; controllare che i tre criteri iniziali siano presenti.
3. Aprire Gare: le coreografie con la stessa combinazione devono essere raggruppate.
4. Aprire una gara, scegliere tre giudici, salvare e riaprire per verificare.
5. Aggiungere un quinto giudice: giurie di default aggiornate, personalizzate invariate.
6. Provare Usa tutti i giudici e modificare una coreografia cambiandone la categoria.
7. Rinominare un criterio e verificare l'elenco comune nella vista Gare.

Test automatici su raggruppamenti, assenza di scritture in lettura, aggiornamento e
rimozione combinazioni vuote, giurie, rollback, migrazione e salvataggio da WinForms.
Verifica visiva e di usabilità con l'operatore ancora necessaria. Solo dati dimostrativi.
