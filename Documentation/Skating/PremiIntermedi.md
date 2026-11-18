# Premi intermedi del prototipo

Blocco del 23 ottobre 2026, basato su DS-054/055/056/066. Dal 26 ottobre sono disponibili anche le [assolute](PremiAssoluti.md).

## Configurazione

Il file `skating-settings.json`, accanto all'eseguibile, viene letto una sola volta all'avvio:

```json
{
  "IntermediateAwardAggregation": "Discipline"
}
```

Valori ammessi: `Discipline` (per disciplina) oppure `Category` (per categoria, trasversalmente
alle discipline). Il file distribuito contiene `null`: nessun regolamento viene scelto al
posto dell'organizzatore. Questo parametro è solo il valore iniziale dei nuovi archivi.
Con `null`, file mancante o non valido il criterio si sceglie direttamente nella manifestazione.
Il menu **Configurazione → Impostazioni applicazione...** modifica questo JSON, conservando
eventuali altri campi. Mostra la scelta salvata, il criterio attivo e l'avviso di riavvio.
**Salva** sostituisce atomicamente il file solo dopo aver scritto la nuova versione;
un errore mantiene la finestra aperta e non sostituisce il file precedente. **Annulla** non scrive.
Il file mancante può essere creato; un JSON illeggibile viene segnalato senza sovrascriverlo.
Un aggiornamento del programma non deve sovrascrivere una configurazione personalizzata:
estrarre la nuova versione in una cartella distinta e reimpostare il criterio.

Dal 17 novembre 2026 **Configurazione → Premi della manifestazione...** permette di scegliere
o cambiare il criterio salvato nell'archivio, anche dopo l'avvio, senza riavviare l'app.
Gli archivi esistenti usano il proprio criterio indipendentemente dal JSON. Quelli mai
configurati richiedono una scelta esplicita, proposta anche accedendo ai premi.
Il cambio raggruppa diversamente i vincitori, ricalcolando intermedi e assoluto Open senza
modificare voti ufficiali, chiusure delle gare o percorso PRO. Se esistono convalide dei
premi interessati viene chiesto il consenso alla loro revoca; annullare conserva tutto.
La persistenza ricontrolla il consenso nella transazione. Scegliere lo stesso criterio non
revoca convalide. Non sono previsti ripescaggi di secondi/terzi per completare il podio:
con uno o due qualificati il premio conserva rispettivamente uno o due partecipanti.

## Consultazione e convalida

Il nuovo menu **Premi intermedi...** mostra i gruppi derivati dalle gare non PRO. Si vedono:

- vincitori attuali, coreografia, scuola, gara di origine, posizione e punteggio;
- stato provvisorio/definitivo, numero di gare di origine chiuse e parità;
- voti ufficiali originali della riga selezionata, con quelli esclusi in grigio;
- tutte le gare di origine e collegamento per aprirne classifica e scheda voti.

Entrano solo i primi. Finché una gara è provvisoria possono comparire più primi provvisori
a pari merito; non è una qualificazione definitiva. Incomplete e senza voti non producono
un vincitore. Gare con tutte le esibizioni escluse non contribuiscono partecipanti.
I PRO sono esclusi dai premi intermedi e accedono direttamente all'assoluta PRO.

La somma e le esclusioni automatiche vengono ricalcolate autonomamente fra i partecipanti
al premio, partendo sempre dai voti originali. Non si sommano i totali delle gare e non si
ereditano le loro esclusioni. Le parità sul podio impediscono la convalida, quelle successive
sono segnalate ma ammesse. Nessun bonus o incremento automatico.

**Convalida premio** richiede almeno un partecipante, tutte le gare di origine chiuse e
nessuna parità sul podio. La verifica viene ripetuta nella transazione di scrittura.
**Riapri premio** revoca soltanto la sua convalida, senza cambiare le gare o i voti.

Per correggere uno spareggio aprire la gara originale, riaprirla, riaprire la scheda e
modificare il voto indicato dai giudici. Confermare poi la scheda, richiudere la gara e
convalidare di nuovo il premio. La rettifica è ufficiale anche nella gara originaria.
La riapertura della gara invalida persistentemente il premio collegato: richiudere la gara
non riconvalida automaticamente il premio. Premi non collegati rimangono definitivi.
Anche nuove iscrizioni nello stesso gruppo richiedono una nuova verifica; i cambi del
percorso PRO richiedono prima la riapertura delle gare del livello interessato.

## Persistenza e limiti

Formato v7: `AwardSettings` memorizza il criterio; `IntermediateAward` la convalida per
criterio/gruppo. Nessuna copia di voti o coreografie. La lettura usa una transazione coerente
e non scrive. Aggiornamento v1–v6 esplicito, con scelta del backup già prevista.

Verifiche automatiche per entrambi gli aggregatori, esclusione PRO, fonti provvisorie,
parità, ricalcolo giurie, propagazione delle rettifiche, invalidazione selettiva, cambio
criterio, migrazione e schermata WinForms. Notifiche e catalogo/ripristino backup
non sono inclusi: il prototipo resta da usare con dati dimostrativi.
