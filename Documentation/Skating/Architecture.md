# Skating System - architettura concordata

Aggiornamento: 26 ottobre 2026. Stato: anagrafiche, coreografie, giurie, scaletta, votazioni, classifiche delle singole gare, premi intermedi e assolute PRO/non PRO implementati.

## Collocazione e indipendenza

- Nuovo sviluppo nel repository MultiPurposeServer, sotto `Applications/Skating`.
- Applicazione desktop autonoma, non client del server MPS: preparazione, gara, modifiche,
  voti, classifiche, notifiche e backup devono funzionare senza Internet.
- Logica della manifestazione e persistenza locale appartengono all'area Skating; separazione
  in progetti secondo necessità, senza anticipare progetti vuoti. Nessun modulo server
  `Domains/Skating` introdotto solo per simmetria.
- Dipendenze condivise solo per componenti tecnici effettivamente riutilizzati, in `Shared`.
  Niente dipendenze dall'host, da Finance o dalle implementazioni interne degli altri domini.
- Solution unica `MultiPurposeServer.slnx`, con Skating incluso nella build ordinaria.
  La condivisione della solution non crea dipendenze applicative. Build mirata e publish
  restano eseguibili sul singolo progetto; rilascio Skating senza deploy MPS/Aruba o Finance.
  Pipeline ancora da creare; superata la precedente scelta di soluzione/filtro dedicato.
- Demo nel repository separato `SketingSystem` mantenuta come riferimento. Nessuna migrazione,
  reset o conversione dei suoi database implicita in questa decisione.

## Possibile backend amministrativo futuro

### Organizzazione del desktop

I sorgenti WinForms sono raggruppati per area funzionale sotto `Skating.Desktop`:
`Shell`, `Manifestations`, `Registry`, `Choreographies`, `Races`, `Scheduling`,
`Scoring`, `Rankings`, `Configuration` e `Common`. I namespace seguono le cartelle.
Ogni Form conserva accanto a sé il proprio Designer e le eventuali risorse; i tipi di
supporto specifici restano nell'area che li usa. `Common` contiene solo supporti condivisi.
Nella root restano `Program.cs`, `skating-settings.json` e i file di progetto.
Questa organizzazione non cambia formato o posizione degli archivi e delle preferenze,
né introduce nuove dipendenze tra progetti.

### Confini applicativi

Confini iniziali: `Skating.Core` contiene il modello, `IManifestationStore`, `IRegistryStore`, `IChoreographyStore`, `IRaceStore`, `IPerformanceStore`, `IIntermediateAwardStore` e `IAbsoluteAwardStore`;
`Skating.Persistence` implementa l'accesso SQLite; `Skating.Desktop` compone queste dipendenze
nel punto di avvio. La UI usa il contratto senza costruire connessioni o eseguire query.
L'archivio v8 contiene identificativo e nome della manifestazione, atleti, discipline,
categorie, livelli, scuole e coreografie con componenti, giudici, criteri e configurazione
delle giurie, scaletta, presenze, voti, chiusura gare, esiti reversibili, criterio e convalide premi intermedi e assoluti. Aggiornamento esplicito da v1–v7 con copia preventiva
predefinita o rinuncia esplicita al backup. Dettagli in [Classifiche](Classifiche.md).

La preparazione amministrativa pre-manifestazione potrà essere spostata sul server.
Per consentirlo, UI, logica applicativa e persistenza devono mantenere responsabilità separate:
la logica non deve dipendere da WinForms né direttamente dall'implementazione del database locale.
Progetti distinti vengono introdotti quando esiste una responsabilità concreta, non come contenitori vuoti.

Per ora tutte le funzionalità restano locali, comprese le modifiche sul posto durante la gara.
L'eventuale backend richiederà un flusso esplicito di trasferimento dei dati e decisioni su
conflitti e responsabilità, senza rendere la gara dipendente da rete o disponibilità del server.
API, sincronizzazione e modulo server non fanno parte del blocco iniziale.

## Configurazione

- Aggregatori per disciplina e per categoria entrambi implementati; parametro JSON letto
  all'avvio stabilisce quale usare. Modificabile dalla configurazione GUI, con effetto solo
  dopo il riavvio; il valore attivo non cambia durante la sessione. Scelta finale di regolamento
  non bloccante per costruire il prototipo; non inventare un valore predefinito approvato.
- Aspetto griglia e navigazione Invio sono preferenze modificabili a runtime dalla maschera
  di configurazione e ricordate fra sessioni, senza modificare dati o regole di valutazione.
- La semantica dell'aggregatore non deve cambiare silenziosamente su risultati già convalidati:
  il criterio persistito impedisce di usare un JSON diverso a manifestazione avviata.
  Procedura e configurazione in [Premi intermedi](PremiIntermedi.md).
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
