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
posto dell'organizzatore. Impostare il valore prima della prova e riavviare. Con `null`, file
mancante o non valido le altre funzionalità restano disponibili, ma non i premi intermedi.
Un aggiornamento del programma non deve sovrascrivere una configurazione personalizzata.

Il criterio viene associato all'archivio all'avvio della manifestazione oppure, previa
conferma, aprendo **Premi intermedi...**. Una scelta diversa può essere confermata soltanto
prima dell'avvio; dopo, la discordanza col JSON blocca l'accesso ai premi e richiede di
ripristinare il parametro originale. Non si altera il regolamento al semplice riavvio.
Archivi precedenti già avviati ma mai configurati richiedono una prima associazione esplicita.
Il criterio resta fisso durante l'esecuzione: non è una preferenza grafica a runtime.

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
