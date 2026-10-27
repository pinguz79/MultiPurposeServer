# Anagrafiche - primo blocco del prototipo

Stato: implementazione iniziale richiesta da Marco. Questa pagina descrive il comportamento
da provare, non attribuisce al cliente nuove approvazioni sulle scelte di dettaglio.

## Creazione, consultazione e modifica

Il menu Anagrafiche si abilita solo dopo aver aperto una manifestazione compatibile.
Quattro schermate indipendenti consentono di consultare le voci e aggiungerne altre senza
chiudere la schermata. I dati vengono salvati subito nell'archivio selezionato e riappaiono
alla riapertura dell'app. È disponibile anche la modifica, non ancora l'eliminazione.

Il pulsante **Modifica** o il doppio clic su una riga precompilano i campi della voce selezionata.
**Salva modifiche** aggiorna la voce senza cambiarne l'identificativo; **Annulla modifica**
scarta i cambiamenti. Durante la modifica selezione e aggiornamento dell'elenco sono disabilitati.
La chiusura con dati non salvati richiede conferma. Valgono gli stessi controlli di creazione:
un errore mantiene i dati nell'editor e non modifica la voce salvata. L'aggiornamento di una voce
non più esistente viene rifiutato, senza ricrearla. Non serve una nuova versione dell'archivio.

| Anagrafica | Campi iniziali | Comportamento |
| --- | --- | --- |
| Atleta | Nome, cognome | Obbligatori; omonimi ammessi con identificativi diversi |
| Disciplina | Nome | Obbligatorio e univoco nell'elenco discipline |
| Categoria | Nome | Obbligatorio e univoco nell'elenco categorie |
| Livello | Nome, percorso PRO | Nome obbligatorio e univoco nell'elenco livelli; PRO esplicito |

Scelte iniziali del prototipo da verificare nell'uso: nome/cognome separati e obbligatori,
nessuna deduplicazione automatica degli omonimi, univocità dei nomi dei tre cataloghi.
Gli spazi esterni vengono rimossi; il confronto dei nomi ignora le maiuscole ma non gli accenti.
Lo stesso nome può comparire in cataloghi diversi. Il flag PRO inizialmente è disattivato:
scrivere un livello chiamato "PRO" non lo attiva automaticamente; l'operatore sceglie il percorso.
Non si precompilano i cataloghi con nomi o regole presunte del regolamento.

## Regole già consolidate rispettate

- Atleti senza coreografia conservati nell'anagrafica completa (DS-006).
- Nessuna scuola obbligatoria sull'atleta: sarà rappresentata dalla coreografia (DS-053).
- Categorie e livelli condivisi fra tutte le discipline, senza matrice preventiva (DS-031).
- Nessuna classificazione anagrafica automatica o vincolo d'età introdotto.
- Il significato del percorso PRO resta separato dalla denominazione del livello.
- Nessuna gara o partecipazione viene generata inserendo semplicemente un'anagrafica.

## Limiti e verifiche

Scuole e iscrizioni sono ora introdotte nel blocco [Scuole e coreografie](ScuoleECoreografie.md).
Giudici, criteri e gare derivate sono introdotti in [Gare e giurie](GareEGiurie.md).
Ancora esclusi: eliminazione generale delle anagrafiche e voti.
Inserimenti e modifiche non generano backup operativi; l'aggiornamento del formato precedente
propone invece una copia tecnica preventiva, descritta in DT-004.

Test automatici: campi obbligatori, identificativi validi, percorso PRO esplicito,
salvataggio/riapertura, omonimi, duplicati, cataloghi condivisi e isolamento degli archivi;
test dei controlli WinForms per inserimento, modifica, annullamento e aggiornamento della griglia.
La modifica verifica anche conservazione dell'identificativo, duplicati e voci non più presenti.
La verifica visiva con l'operatore resta da effettuare.
