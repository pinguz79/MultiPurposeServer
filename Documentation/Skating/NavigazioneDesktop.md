# Navigazione desktop e feedback demo

Decisioni concordate con Marco il 9 novembre 2026, a seguito del riscontro del cliente.

## Modalità operative

La modalità Anagrafica/Gara è una preferenza di navigazione della sessione, non uno stato
della manifestazione. Si parte in Anagrafica; il menu Modalità permette di cambiare liberamente
senza riavvio. Il cambio non avvia, chiude o riapre gare e non aggira i vincoli sui dati.
Per cambiare modalità si torna alla finestra principale chiudendo le finestre modali aperte.

ESC chiude le finestre popup, rispettando conferme di abbandono e validazioni di chiusura.
Le dialog con pulsante Annulla mantengono il relativo comportamento. Nei controlli che
usano ESC per annullare una modifica o chiudere una tendina, la prima pressione resta locale;
la pressione successiva chiude la finestra. ESC non chiude la finestra principale.

La cornice ambra identifica Anagrafica, quella azzurra Gara; una fascia testuale rende la
distinzione leggibile anche senza percepire i colori. I colori dei dati restano indipendenti.

| Elenco / doppio click | Anagrafica | Gara |
| --- | --- | --- |
| Gare | Configurazione giuria | Classifica della gara |
| Coreografie | Modifica coreografia | Scheda voti del blocco della coreografia |
| Scaletta | Modifica coreografia | Scheda voti del blocco dell'esibizione |

In Gara il menu delle anagrafiche e le azioni di creazione/modifica anagrafica delle
coreografie e della giuria non sono proposti: si torna in Anagrafica per usarli.
Le azioni esplicite di preparazione della scaletta e avvio restano disponibili e soggette
alle validazioni esistenti. Una coreografia senza blocco non viene aggiunta automaticamente:
la UI invita a preparare la scaletta.

## Menu classifiche

Un unico menu principale Classifiche raccoglie:

- Classifiche delle singole gare: classifica diretta, con tendina disciplina / categoria / livello
  per cambiare gara nella stessa finestra.
- Vincitori per disciplina / categoria: premi intermedi secondo aggregatore configurato.
- Vincitori assoluti Open / Pro: sottomenu con i due assoluti separati.

Il menu Classifiche è nascosto in modalità Anagrafica. Nei premi intermedi e assoluti la
gara di origine è un campo non modificabile, aggiornato dalla riga selezionata: il pulsante
di apertura porta alla gara di quell'esibizione. I nomi del menu non
modificano le regole: i PRO passano direttamente dalla gara all'assoluto, senza premio intermedio.

## Scheda voti di dettaglio

Le classifiche di gara, intermedie e assolute mostrano nel dettaglio una scheda grafica in
sola lettura: riquadri per giudice, criteri, voti originali e totale del giudice. Il totale
della classifica resta quello calcolato dal dominio. Voti esclusi in grigio con motivo;
schede incomplete mostrano soltanto il parziale inserito, senza attribuire un totale di classifica.

## Colori delle gare

Da Configurazione → Colori e legenda delle gare si associa un colore nominato alla combinazione
disciplina/categoria/livello. La stessa finestra è accessibile da Gare, dalla classifica di gara e dalla scheda voti.
La palette contiene 24 colori chiari per testo nero. Il colore assegnato non può essere riutilizzato
da un'altra combinazione nella stessa manifestazione; la tendina propone solo quelli liberi e il
colore corrente. "Senza colore" libera l'assegnazione e mantiene l'aspetto neutro. Non si assegnano
colori automaticamente; se sono esauriti occorre liberarne uno, senza bloccare la gara.

I colori sono salvati nell'archivio (schema 9), con aggiornamento e scelta del backup per gli archivi
precedenti. Eliminando una gara senza coreografie si libera anche il suo colore. I nomi di discipline,
categorie e livelli possono cambiare senza perdere l'associazione, basata sugli identificativi.
Le modifiche sono grafiche: non alterano voti o convalide e sono ammesse anche durante la manifestazione.

Il colore appare nell'elenco gare, nella scaletta, sulle coreografie delle classifiche di gara e dei
premi, e nella zona identificativa delle schede voti. La preferenza già esistente permette di scegliere
fra sfondo e indicatore nella scheda voti; resta compatibile la chiave JSON ColorDiscipline.
Avvisi di parità e stati dei voti conservano la propria evidenza, distinta dal colore della gara.

## Stampa dei risultati

Le tre classifiche offrono Stampa ed Esporta PDF. Si produce una fotografia della vista corrente
(completa o solo podio), con titolo gara/premio, stato PROVVISORIA/DEFINITIVA, colonne visibili,
intestazioni ripetute e numerazione delle pagine. Il documento usa testo nero su bianco,
indipendentemente dai colori personalizzati. I testi vanno a capo e le righe restano unite;
una riga più alta dell'intera pagina viene segnalata invece di troncarne il contenuto.

Stampa usa la selezione stampanti di Windows; Esporta PDF richiede Microsoft Print to PDF
abilitata nel sistema e chiede la destinazione con conferma di sovrascrittura. Non vengono
aggiunte dipendenze al pacchetto. Non equivale alla stampa delle schede cartacee dei giudici.

## Evidenza del podio

Gare, premi intermedi e assoluti offrono la vista Classifica completa / Solo podio. Il filtro include
tutte le esibizioni attive con posizione da 1 a 3, compresi eventuali parimerito; non tronca alle prime
tre righe e non attribuisce una posizione alle esibizioni senza voti. La classifica resta provvisoria
finché non convalidata e le regole di chiusura non dipendono dal filtro. La cella posizione distingue
oro, argento e bronzo, senza sovrascrivere il colore della gara o gli avvisi di parità.

## Scelte tecniche interne

Modalità confinata al desktop, passata alle finestre al momento dell'apertura. La sola
persistenza dei colori richiede la migrazione SQLite v9. Nessuna nuova dipendenza runtime
o modifica ai calcoli delle classifiche; le preferenze JSON esistenti restano compatibili.
