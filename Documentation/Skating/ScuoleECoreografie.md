# Scuole e coreografie - blocco 1 del prototipo

Implementazione richiesta da Marco il 22 ottobre 2026. Questa pagina descrive la
funzionalità da provare, senza attribuire nuove approvazioni al cliente.

## Flusso disponibile

1. Creare o aprire un archivio dimostrativo; il formato corrente è v10.
2. Nel menu Anagrafiche inserire scuole, atleti, discipline, categorie e livelli.
3. Aprire Coreografie, scegliere Nuova coreografia e compilare nome, scuola,
   disciplina/categoria/livello, composizione, durata e componenti.
4. Salvare, oppure annullare senza scritture. Modifica e doppio clic riaprono la scheda.

I cataloghi vengono riletti a ogni apertura della scheda. **Aggiungi atleta** apre
l'anagrafica atleti senza chiudere la coreografia: al ritorno aggiorna l'elenco mantenendo
campi, filtro e componenti selezionati. I nuovi atleti non vengono selezionati automaticamente.
Per aggiungere elementi mancanti negli altri cataloghi, chiudere la scheda,
inserirli dall'anagrafica e riaprirla.
Il salvataggio è disabilitato quando manca un intero catalogo necessario.
La chiusura dopo modifiche richiede conferma di abbandono.

Il filtro atleti cerca parti di nome e cognome senza distinguere maiuscole/minuscole.
Più parole devono essere tutte presenti, in qualsiasi ordine (es. "rossi anna").
Non cerca nelle scuole o nelle altre informazioni visualizzate. Cambiare filtro non
modifica la selezione: il salvataggio include anche gli atleti selezionati non visibili.
Il contatore mostra visibili, totale e selezionati; svuotare il filtro ripristina l'elenco.

## Regole applicate

- Una scuola per coreografia. Nessun legame esclusivo atleta/scuola (DS-053).
- Assolo: un atleta; Duo: due; Gruppo: almeno tre. Non si ricava la composizione dal
  testo della disciplina: Assolo/Duo può restare una disciplina unica.
- Nome, scuola, disciplina, categoria e livello obbligatori.
- Nessun atleta ripetuto nella stessa coreografia.
- Stessi atleti nella stessa gara ammessi con nomi diversi (DS-040).
  Il duplicato con identico nome e insieme di componenti viene rifiutato, indipendentemente
  dall'ordine dei componenti. Nessun vincolo generale sul nome tra composizioni diverse.
  Il confronto dei nomi ignora maiuscole/minuscole e spazi esterni, come nei cataloghi.
- Durata in minuti/secondi, anche zero, senza range regolamentare o avvisi (DS-059).
- Aggiornare una coreografia conserva l'identificativo; cambiare i componenti non elimina
  le persone dall'anagrafica. Un riferimento mancante impedisce l'intero salvataggio.
- Nome scuola obbligatorio e univoco ignorando maiuscole/minuscole: scelta iniziale del
  prototipo coerente con gli altri cataloghi, da verificare nell'uso.

L'elenco mostra scuole, classificazione e componenti delle coreografie. Il riepilogo
conta atleti partecipanti distinti e anagrafica completa separatamente, segnalando quanti
atleti rappresentano più scuole. Nella selezione dei componenti compaiono numero di
coreografie e scuole rappresentate; gli omonimi sono distinti da un identificativo breve.
Non vengono introdotte quote o regole di pagamento.

## Verifiche e limiti

Test di modello, persistenza e GUI: cardinalità, durata, duplicati, riuso degli atleti,
aggiornamenti atomici, conservazione delle identità, precompilazione e salvataggio
della scheda, migrazione v1/v2 e cataloghi mancanti.

Prova manuale: creare due scuole e tre atleti; inserire un duo, cambiarne un componente
e riaprire l'archivio. Inserire gli stessi atleti in una seconda coreografia con nome
diverso e scuola diversa; verificare riepilogo e associazioni. Provare a salvare un duo
con un solo componente e un duplicato esatto: nessuna scrittura deve essere effettuata.

Gare derivate e giurie sono ora disponibili nel blocco [Gare e giurie](GareEGiurie.md).
Dal 17 novembre 2026 è disponibile **Elimina** nelle anagrafiche scuole/atleti e nelle
coreografie, con conferma esplicita. Scuole e atleti utilizzati da coreografie non possono
essere eliminati; nel modello corrente il legame scuola/atleta passa dalle iscrizioni,
non esiste un'associazione diretta. Nessuna cancellazione a cascata di altre anagrafiche.
Le coreografie si eliminano solo prima dell'avvio della manifestazione: si rimuovono
iscrizione e appartenenza alla scaletta, conservando tutti gli atleti. L'ordine viene
ricompattato, il blocco interessato richiede nuova conferma e quelli vuoti vengono rimossi.
Una gara rimasta vuota libera giuria locale e colore. Dopo l'avvio usare gli esiti reversibili
Non presentata/Squalificata. Non disponibili backup operativi, notifiche o gestione completa
delle partecipazioni per scuola. Solo dati dimostrativi; verifica visiva e di usabilità
con l'operatore ancora necessaria.
