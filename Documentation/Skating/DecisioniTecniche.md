# Registro interno delle decisioni tecniche

## Regola di tracciamento

Dal 9 ottobre 2026 le decisioni puramente tecnico-implementative vengono tracciate separatamente
dalle [decisioni funzionali](DecisioniSemantiche.md). Non vengono incluse nel materiale da
sottoporre al cliente per approvazione.

Ogni futura voce deve indicare ID DT, data, stato, contesto, scelta, alternative considerate,
conseguenze e collegamenti alle decisioni funzionali pertinenti. Le scelte superate restano
rintracciabili. Una limitazione funzionale non diventa approvata classificandola come tecnica.

## Decisioni registrate

## DT-010 - Assolute separate e invalidazione dei percorsi, formato v8

- Data: 26 ottobre 2026. Implementato nel prototipo offline.
- `IAbsoluteAwardStore` espone lettura, convalida e riapertura PRO/non PRO.
- Voti originali condivisi, nessuna duplicazione: ogni assoluta ricalcola autonomamente
  totali ed esclusioni. La lettura delle fonti e la convalida usano uno snapshot coerente.
- `AbsoluteAward` conserva solo la convalida per percorso; la riapertura di una fonte
  invalida persistentemente il relativo percorso, senza toccare quello indipendente.
- Migrazione v8 esplicita e transazionale, con backup facoltativo già previsto; conserva
  criterio e convalide intermedie precedenti. Nessuna dipendenza server o nuovo pacchetto.
- Dettagli funzionali e prova manuale in [Premi assoluti](PremiAssoluti.md).

## DT-009 - Premi intermedi e criterio persistito, formato v7

- Data: 23 ottobre 2026. Stato: implementato nel prototipo.
- Contesto: DS-054/055/056/066, aggregazione configurata all'avvio e voti ufficiali condivisi.
- Scelta: JSON letto una volta, criterio associato all'archivio e confrontato a ogni accesso;
  convalida persistita per gruppo, calcolo dai voti originali nello snapshot delle gare.
  Riapertura di una fonte invalida il solo premio collegato nella stessa transazione.
- Alternative scartate: copie dei voti; semplice confronto di hash dei risultati che potrebbe
  far tornare una convalida valida dopo una modifica e successivo ripristino del valore.
- Conseguenze: migrazione v7, nuova configurazione locale da preservare nei publish,
  nessun criterio predefinito di regolamento. Assolute e notifiche restano successive.
- Dettagli e verifiche: [Premi intermedi](PremiIntermedi.md).

## DT-008 - Classifiche calcolate e chiusura persistita, formato v6

- Data: 23 ottobre 2026. Stato: implementato nel prototipo.
- Contesto: DS-055/056/057/062; classifica sempre consultabile e correzioni protette.
- Scelta: calcolatore puro nel Core, lettura coerente SQLite nella stessa transazione,
  stato chiusura sulla gara ed esito sulla coreografia. Nessuna copia dei voti; la presenza
  storica delle schede isola il calcolo dalle assenze globali successive.
- Alternativa scartata: inizializzare le presenze durante la lettura della classifica,
  che avrebbe scritto dati e fissato assenze prima dell'apertura delle schede.
- Conseguenze: migrazione v1–v5 -> v6; chiusura ricontrollata transazionalmente, protezioni
  anche nei metodi di modifica voti e iscrizioni. Le revoche conservano i voti e richiedono
  nuova conferma. Premi derivati e notifiche restano nel prossimo perimetro.
- Dettagli, verifiche e limiti: [Classifiche](Classifiche.md).

## DT-007 - Schede voti persistite e scaletta nel formato v5

- Data: 22 ottobre 2026. Stato: implementato per il terzo blocco dimostrativo.
- Contesto: DS-056/057/058/059/066; inserimento offline per blocco con protezione delle righe confermate.
- Scelta: contratto IPerformanceStore, voti interi in decimi, identità coreografia come esibizione,
  numero stabile distinto dall'ordine operativo. Presenze fotografate alla prima apertura del blocco.
- Alternativa scartata: dedurre continuamente le presenze dall'anagrafica, che modificherebbe
  retroattivamente schede già predisposte. Non si duplicano i voti per calcolare classifiche.
- Migrazione transazionale v1–v4 -> v5 con copia preventiva facoltativa esplicita. I voti validi
  si salvano singolarmente; la conferma crea una copia consistente prima della transazione.
- Conseguenze: criteri protetti dopo avvio; giurie e struttura coreografie protette dopo predisposizione
  scheda. Preferenze grafiche e Invio in JSON locale fuori dall'archivio, modificabili sulla scheda.
- Limite esplicito: copia tecnica senza catalogo/ripristino; nessuna promessa di gestione completa
  dei backup. Orari, pause, classifiche e notifiche restano nei prossimi blocchi.
- Percorso e verifiche: [Scaletta e voti](ScalettaEVoti.md).

## DT-006 - Gare derivate e configurazione giurie nel formato v4

- Data: 22 ottobre 2026. Stato: implementato per il secondo blocco dimostrativo.
- Contesto: configurare giurie e criteri senza anticipare data entry, avvio o classifiche.
- Scelta: chiave composta disciplina/categoria/livello; configurazione giuria persistita
  separatamente dalle coreografie. Sincronizzazione delle combinazioni nella stessa
  transazione di salvataggio della coreografia, non durante la lettura.
- Gare vuote di preparazione rimosse insieme alle sole assegnazioni locali dei giudici;
  cataloghi e atleti non eliminati. Prima dei voti dovrà essere estesa la protezione
  delle gare iniziate/concluse, che oggi non esistono.
- Giuria predefinita risolta sui giudici correnti; salvataggio esplicito materializza
  la selezione. Contratto IRaceStore separato da anagrafiche e iscrizioni.
- Alternativa scartata: generazione in lettura con scritture implicite o prodotto cartesiano.
- Migrazione v1/v2/v3 -> v4 conserva dati e introduce tre criteri iniziali; ricava le
  gare dalle iscrizioni già presenti. Backup .before-v4-*.bak salvo rinuncia esplicita.
- Regole del prototipo e verifica con Marco: [Gare e giurie](GareEGiurie.md).

## DT-005 - Scuole e coreografie nel formato v3

- Data: 22 ottobre 2026. Stato: implementato per il primo blocco del percorso dimostrativo.
- Contesto: iscrizioni locali modificabili senza anticipare voti, gare e backend.
- Scelta: School, Choreography e tabella associativa ChoreographyAthlete; riferimenti Guid
  stabili, foreign key SQLite abilitate e aggiornamento atomico dei componenti.
- Il modello Core valida composizione, durata non negativa e identità; il repository
  controlla riferimenti esistenti e duplicati di nome/composizione nella stessa gara sotto
  transazione. Nessun duplicato di atleti anagrafici per rappresentare scuole differenti.
- Alternativa scartata: scuola sull'atleta, incompatibile con DS-053.
- Migrazione incrementale v1/v2 -> v3, senza ricreare tabelle anagrafiche esistenti.
  Backup predefinito .before-v3-*.bak; opzione esplicita senza backup invariata.
- Nessuna eliminazione o migrazione di dati reali eseguita durante lo sviluppo.
- Regole e limiti del prototipo: [Scuole e coreografie](ScuoleECoreografie.md);
  riferimenti funzionali DS-040, DS-053 e durata DS-059.

## DT-004 - Anagrafiche e aggiornamento esplicito del formato v1

Aggiornamento del 22 ottobre 2026, richiesto da Marco: la dialog offre anche "Continua senza
backup" oltre ad aggiornamento con backup (predefinito) e annullamento. La rinuncia è esplicita
per la singola operazione; non modifica validazione, transazione o copie già esistenti.
Il contratto `Upgrade` conserva la copia come default. Questa eccezione riguarda solo la
migrazione tecnica dell'archivio, non i backup operativi delle gare.

- Data: 21 ottobre 2026. Stato: implementato per il blocco di creazione anagrafiche.
- Contesto: mantenere utilizzabili i primi file del prototipo senza convertire archivi estranei.
- Scelta: formato v2 con tabelle distinte Athlete, Discipline, Category e Level, identità Guid
  e contratto `IRegistryStore` separato dalla gestione della manifestazione. Cataloghi condivisi
  senza chiavi verso la disciplina. Nessuna dipendenza Core dal motore SQLite.
- I nomi dei cataloghi usano una collation SQLite registrata su ogni connessione, basata
  su `OrdinalIgnoreCase`, con vincolo UNIQUE anche contro inserimenti concorrenti.
  Non si usano nomi normalizzati duplicati o il solo NOCASE ASCII di SQLite.
- `Load` continua a leggere v1 senza scrivere. La GUI richiede consenso per `Upgrade`,
  che verifica il formato, crea una copia tramite SQLite BackupDatabase accanto al file
  e solo successivamente esegue DDL e aggiornamento versione in transazione.
- Fallimento backup: nessuna migrazione; fallimento DDL: rollback con copia conservata.
  Il rifiuto del consenso non cambia la manifestazione già visualizzata.
- Alternativa scartata: ricreare i file del primo prototipo o migrarli silenziosamente.
- La copia tecnica non è il catalogo DT-001 e non ne anticipa ripristino o notifiche.
  Il comportamento funzionale delle anagrafiche è descritto in Anagrafiche.md.

## DT-003 - Primo archivio locale della manifestazione

- Data: 21 ottobre 2026. Stato: implementato nel primo blocco applicativo.
- Contesto: verificare creazione e riapertura offline senza anticipare il modello completo della gara.
- Scelta: `Skating.Core` per identità e contratto `IManifestationStore`, `Skating.Persistence`
  per SQLite e `Skating.Desktop` per UI/composizione. Nessuna dipendenza Core da SQLite o WinForms.
- Accesso iniziale mediante Microsoft.Data.Sqlite, senza ORM per la singola tabella di metadati.
  L'alternativa EF Core della demo resta valutabile quando cresce il modello; non si importa
  lo schema della demo né si espongono dettagli SQL nel contratto applicativo.
- File `.skating` scelto esplicitamente, identificato con `application_id` e `user_version`.
  Apertura in sola lettura; formati estranei o versioni non supportate rifiutati.
  Il supporto all'aggiornamento esplicito v1 -> v2 è introdotto da DT-004.
- Creazione esclusiva, schema e metadati in transazione. Nessuna sovrascrittura; in caso di
  errore il file riservato resta per diagnosi e non viene cancellato automaticamente.
- Connessioni brevi senza pooling; nessun catalogo di backup o notifica nell'archivio evento.
  Prima dell'uso reale servono anagrafiche, flussi di gara e protezioni/backup concordati.
- Test di integrazione su file temporanei: riapertura, protezione file esistenti e rifiuto
  senza mutazioni di file estranei, formati futuri e metadati invalidi.
- Bundle SQLitePCLRaw 2.1.14 esplicito per evitare il runtime 2.1.11 segnalato da NuGet:
  [avviso della dipendenza](https://github.com/advisories/GHSA-2m69-gcr7-jv3q).

## DT-002 - Applicazione autonoma nello stesso repository

- Data: 21 ottobre 2026. Stato: concordata con Marco; struttura desktop creata, funzionalità da implementare.
- Scelta: nuovo prototipo sotto `Applications/Skating` nel repository MultiPurposeServer,
  con logica e persistenza locali, senza dipendenza dall'host o dalle API MPS.
- Motivazione: riuso selettivo di librerie tecniche già condivise, senza dipendenza dalla rete.
- Alternativa non scelta: mantenere tutto il nuovo sviluppo nel repository separato oppure
  creare un modulo server in `Domains` per simmetria.
- Conseguenze: stessa solution MPS e inclusione nella build ordinaria; build mirata e pubblicazione
  del singolo progetto senza deploy server. Superata la precedente scelta di soluzione/filtro separato:
  l'autonomia dipende dai riferimenti, non dalla solution. Dipendenze `Shared` solo se realmente
  riutilizzate e senza logica Finance/di altri domini.
- Evoluzione possibile: backend amministrativo pre-manifestazione server-side. Separare UI,
  logica e persistenza per consentire questa evoluzione, senza introdurre ora API o sincronizzazione.
  La gara e le modifiche sul posto restano utilizzabili offline; il trasferimento dati futuro
  richiederà progettazione esplicita. Dettagli in Architecture.md.
- Il vecchio repository resta riferimento della demo. Database, codice e file esistenti
  non vengono spostati, cancellati o migrati; la documentazione è copiata qui come storico.
- Fonte corrente delle specifiche: questa cartella Documentation/Skating. Vedere Architecture.md.
- Configurazione aggregatore JSON letto all'avvio; grafica e navigazione Invio configurabili
  a runtime. Nomi delle proprietà e meccanismi di persistenza da definire nel primo blocco.
- Scelta interna non soggetta ad approvazione cliente; effetti funzionali in DS-066.

## DT-001 - Database di servizio separato per backup e notifiche

- Data: consolidamento del 21 ottobre 2026, confronto con Marco del 20 ottobre.
- Stato: scelta tecnica condivisa con Marco; non ancora implementata.
- Collegamenti: DS-060 e DS-061 nel [consolidamento](Consolidamento-2026-10-21.md).
- Contesto: ripristinare la manifestazione non deve annullare catalogo delle copie,
  notifiche o relativo stato letto/non letto.
- Scelta: database della manifestazione separato da un database di servizio contenente
  catalogo backup e notifiche. Dump salvati come file; catalogo con data/ora, descrizione
  dell'operazione pre-backup e nome file interno. La GUI espone soltanto data/ora e descrizione.
- Alternativa scartata: catalogo nel database ripristinato con esclusioni selettive al restore.
- Conseguenze: restore riguarda solo il database manifestazione; nessuna pulizia delle copie.
  Dump prima dell'operazione, verifica del completamento e registrazione nel catalogo;
  fallimento impedisce l'operazione. Validazione compatibilità prima del recupero.
- Da definire in implementazione: snapshot consistente, scritture atomiche, gestione di
  file orfani/errore catalogo, associazione alla corretta manifestazione e strategia sicura
  di sostituzione database. Non sono ancora specifiche approvate né funzionalità esistenti.
- Non includere la separazione fisica dei database nel PDF da approvare: descrivere al cliente
  soltanto gli effetti funzionali, la conservazione delle copie e il comportamento del ripristino.
