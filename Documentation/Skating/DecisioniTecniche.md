# Registro interno delle decisioni tecniche

## Regola di tracciamento

Dal 9 ottobre 2026 le decisioni puramente tecnico-implementative vengono tracciate separatamente
dalle [decisioni funzionali](DecisioniSemantiche.md). Non vengono incluse nel materiale da
sottoporre al cliente per approvazione.

Ogni futura voce deve indicare ID DT, data, stato, contesto, scelta, alternative considerate,
conseguenze e collegamenti alle decisioni funzionali pertinenti. Le scelte superate restano
rintracciabili. Una limitazione funzionale non diventa approvata classificandola come tecnica.

## Decisioni registrate

## DT-002 - Applicazione autonoma nello stesso repository

- Data: 21 ottobre 2026. Stato: concordata con Marco, non implementata.
- Scelta: nuovo prototipo sotto `Applications/Skating` nel repository MultiPurposeServer,
  con logica e persistenza locali, senza dipendenza dall'host o dalle API MPS.
- Motivazione: riuso selettivo di librerie tecniche già condivise, senza dipendenza dalla rete.
- Alternativa non scelta: mantenere tutto il nuovo sviluppo nel repository separato oppure
  creare un modulo server in `Domains` per simmetria.
- Conseguenze: soluzione/filtro, build/test e pubblicazione indipendenti; dipendenze `Shared`
  solo se realmente riutilizzate e senza logica Finance/di altri domini.
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
