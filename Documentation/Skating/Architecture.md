# Skating System - architettura concordata

Data: 21 ottobre 2026. Stato: concordata con Marco, non ancora implementata.

## Collocazione e indipendenza

- Nuovo sviluppo nel repository MultiPurposeServer, sotto `Applications/Skating`.
- Applicazione desktop autonoma, non client del server MPS: preparazione, gara, modifiche,
  voti, classifiche, notifiche e backup devono funzionare senza Internet.
- Logica della manifestazione e persistenza locale appartengono all'area Skating; separazione
  in progetti secondo necessità, senza anticipare progetti vuoti. Nessun modulo server
  `Domains/Skating` introdotto solo per simmetria.
- Dipendenze condivise solo per componenti tecnici effettivamente riutilizzati, in `Shared`.
  Niente dipendenze dall'host, da Finance o dalle implementazioni interne degli altri domini.
- Soluzione o filtro dedicato, build, test e publish propri; rilascio Skating non richiede
  deploy MPS/Aruba né il rilascio dei client Finance. File di soluzione e pipeline ancora da creare.
- Demo nel repository separato `SketingSystem` mantenuta come riferimento. Nessuna migrazione,
  reset o conversione dei suoi database implicita in questa decisione.

## Configurazione

- Aggregatori per disciplina e per categoria entrambi implementati; parametro JSON letto
  all'avvio stabilisce quale usare. Non modificabile a runtime. Scelta finale di regolamento
  non bloccante per costruire il prototipo; non inventare un valore predefinito approvato.
- Aspetto griglia e navigazione Invio sono preferenze modificabili a runtime dalla maschera
  di configurazione e ricordate fra sessioni, senza modificare dati o regole di valutazione.
- La semantica dell'aggregatore non deve cambiare silenziosamente su risultati già convalidati:
  l'eventuale gestione del cambio JSON al riavvio con manifestazione in corso richiede una
  protezione tecnica coerente con il blocco della regola all'avvio della manifestazione.
- Database della manifestazione separato dal database di servizio backup/notifiche (DT-001).

## Primo prototipo operativo

Obiettivo: verificare velocità di trascrizione, leggibilità e corrispondenza dei blocchi
con le schede cartacee, usando dati dimostrativi separati dalla demo e dai dati reali.

- Preparare manifestazione dimostrativa, gare e blocchi.
- Griglia paginata, salvataggio locale, presenza/assenza giudici e sblocchi per blocchi misti.
- Conferma/riapertura esibizioni, chiusura esplicita gare e classifiche con parità evidenziate.
- Entrambe le grafiche e modalità Invio, entrambe le aggregazioni.
- Backup e notifiche restano nel prodotto concordato; possono seguire nel blocco successivo,
  ma devono essere completati e verificati prima dell'uso con dati reali.
- Nessuna produzione/stampa delle schede in questo perimetro: futura change request.

Prima di scrivere codice: verificare il riuso effettivo della demo e delle librerie condivise,
leggere le convenzioni di engineering MPS e definire confini e verifiche del primo blocco.
Questo documento non autorizza a dichiarare già migrato, compilato o rilasciato il prototipo.
