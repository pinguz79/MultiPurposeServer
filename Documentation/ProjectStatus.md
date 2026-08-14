# Project Status

## Scopo

Questo documento rappresenta la fonte autorevole sullo stato corrente di MultiPurposeServer, sulla milestone attiva e sull'attività da riprendere in una nuova sessione.

Deve essere aggiornato al termine di ogni milestone o quando cambia formalmente la priorità del progetto.

---

## Livelli di stabilità della documentazione

### Bootstrap ufficiale

Stato: **Stable 1.0**

- `README.md`
- `Documentation/Home.md`
- `Documentation/Platform.md`
- `Documentation/ChatRecovery.md`
- `Documentation/ProjectStatus.md`

Questi documenti costituiscono il percorso minimo e ordinato di bootstrap del progetto. L'elenco non rappresenta il catalogo completo della documentazione ufficiale: altri documenti possono essere promossi a stabili senza entrare nel bootstrap, salvo che diventino necessari per ricostruire il contesto di ogni nuova sessione.

### Documentazione ufficiale consolidata

Stato: **Stable 1.0**

Home cataloga i documenti ufficiali relativi ad architettura, ADR, engineering, roadmap e dominio Portfolio. Questi documenti hanno completato revisione tematica e verifica globale e costituiscono fonti autorevoli nei rispettivi ambiti.

Catalogo e ordine di lettura: [Home](Home.md).

### Documentazione residua

Stato: **Alpha 0**

Home identifica esplicitamente i documenti ancora Alpha e i documenti temporanei di migrazione.

Questi documenti possono essere incompleti, incoerenti o non aggiornati. Devono essere verificati e consolidati prima di essere promossi a documentazione ufficiale.

---

## Stato attuale del progetto

MPS ha completato la milestone di consolidamento della documentazione.

Il codice ha recentemente completato una code review generale:

- build verde;
- test verdi;
- warning azzerati;
- pipeline, validazione e normalizzazione consolidate;
- debito tecnico residuo registrato nel registro consolidato.

Il secondo livello documentativo è diviso fra documenti ufficiali Stable 1.0 e materiale residuo Alpha esplicitamente non autorevole. Il materiale Alpha conserva lavoro futuro e non fa parte della documentazione promossa.

---

## Milestone corrente

**Consolidamento della pipeline Bulk**

La milestone completa l'evoluzione delle operazioni Bulk dalla strategia provvisoria `WarningAndContinue` al modello consolidato in [BulkOperations](Architecture/BulkOperations.md): persistenza e valutazione diventano dimensioni indipendenti, tutte le quattro combinazioni sono supportate e la relativa semantica viene verificata tramite Integration Test HTTP.

Ultima milestone conclusa: **Consolidamento della pipeline MVC**, completata il 13 agosto 2026.

La milestone ha completato `TD-0001` e `TD-0002`: Integration Test HTTP in memoria per Model Binding, normalizzazione, validazione ricorsiva e bulk, mancata invocazione dei Service e traduzione delle eccezioni; gestione centralizzata di `KeyNotFoundException` con mantenimento della semantica locale dei warning bulk.

Milestone precedente: **Consolidamento delle specifiche di coding**, completata il 13 agosto 2026.

La milestone ha completato `BL-0037`, `TD-0005` e `TD-0006`: convenzioni autorevoli per C#, test, PHP, JavaScript, CSS e SQL; enforcement deterministico riproducibile; baseline applicata a server, client, Shared Framework e test. La verifica finale ha superato build senza warning, 671 test non-production e revisione completa delle diff stilistiche separate dagli sviluppi funzionali.

Milestone precedente: **Automazione deploy**, completata il 12 agosto 2026.

Milestone precedente: **Affidabilità e gestione Portfolio**, completata l'11 agosto 2026.

La milestone ha completato `BL-0013`, `BL-0034` e `BL-0016`. `BL-0020` non è stato riprodotto: la situazione anomala è stata bonificata e il punto è stato trasferito al monitoraggio differito con test diagnostico e logging strutturato.

Milestone precedente: **Migliorie UI e UX**, completata l'11 agosto 2026.

La milestone ha consegnato tutte le modifiche richieste a MPS, Portfolio.Api e Portfolio.Web. Restano due verifiche dipendenti da processi esterni: l'esito della revisione Google AdSense e la prova del flusso di selezione con una modella dopo la disponibilità di un nuovo shooting. Nessuna delle due richiede ulteriore implementazione per considerare conclusa la milestone.

Milestone ancora precedente: **preparazione di Portfolio.Web al traffico fotografico**, completata l'11 agosto 2026.

---

## Attività corrente

La milestone di consolidamento della pipeline Bulk è completata. La prossima attività verrà selezionata dal backlog e dalla roadmap.

Le quattro combinazioni fra persistenza e valutazione sono ora operative tramite `BulkOperationExecutor`. `PartialSuccess` usa un'operazione indipendente per item; `AllOrNothing` usa una sola operazione globale e checkpoint applicativi implementati tramite savepoint EF. La response e la tassonomia degli errori sono condivise, mentre l'esecutore resta per ora nel dominio Portfolio.

La procedura di automazione deploy è stata collaudata operativamente con release reali di MPS su Aruba e Portfolio.Web su Altervista. `BL-0020` resta sotto monitoraggio differito.

Gli esiti esterni della revisione Google AdSense e del flusso reale con una modella continuano a essere monitorati, ma non bloccano la scelta della prossima milestone.

---

## Avanzamento della milestone

### Consolidamento della pipeline Bulk — completata

- [x] Verificare l'implementazione corrente delle Bulk API Album e Foto.
- [x] Definire nomenclatura e contratti concreti delle strategie di persistenza e valutazione.
- [x] Distinguere la validazione globale del contenitore dalla validazione dei singoli item.
- [x] Definire esiti aggregati, risultati per item e tassonomia degli errori.
- [x] Progettare l'esecuzione condivisa senza sottrarre ai Controller la responsabilità dell'atomicità applicativa.
- [x] Implementare `AllOrNothing` e `PartialSuccess`.
- [x] Implementare `StopOnFirstFailure` ed `EvaluateAll` in combinazione indipendente.
- [x] Rifattorizzare i Controller Bulk Album e Foto eliminando duplicazioni e controlli manuali residui.
- [x] Verificare tutte le combinazioni con Unit Test e Integration Test HTTP.
- [x] Eseguire build e test completi.
- [x] Eseguire publish e smoke test proporzionati alle modifiche.

### Consolidamento della pipeline MVC — completata

- [x] Definire il perimetro degli Integration Test della pipeline HTTP.
- [x] Verificare Model Binding, normalizzazione e validazione attraverso richieste HTTP reali in memoria.
- [x] Verificare che Request non valide non invochino i Service.
- [x] Verificare la traduzione delle eccezioni applicative nelle risposte HTTP.
- [x] Centralizzare la gestione di `KeyNotFoundException`.
- [x] Rimuovere i `try/catch` duplicati dai Controller interessati e riallocare i test al livello corretto.
- [x] Eseguire build, test completi e revisione finale della milestone.

### Consolidamento delle specifiche di coding — completata

- [x] Rilevare le convenzioni prevalenti e le divergenze attuali nella codebase server, client, Shared Framework e test.
- [x] Definire e approvare la struttura della documentazione autorevole di code style.
- [x] Consolidare le convenzioni C# e le regole applicabili ai progetti server e client.
- [x] Consolidare le convenzioni specifiche dei test in coordinamento con `TestingConventions.md`.
- [x] Definire le convenzioni PHP e frontend realmente necessarie a Portfolio.Web.
- [x] Distinguere formatter, analyzer e regole editoriali, includendo la quality gate pre-commit.
- [x] Aggiornare le istruzioni destinate allo sviluppo assistito da AI.
- [x] Applicare la baseline alla codebase con diff separata da modifiche funzionali.
- [x] Chiudere `TD-0005` — conversione dei namespace residui.
- [x] Chiudere `TD-0006` — uniformazione della formattazione interna.
- [x] Verificare build, 671 test non-production e assenza di variazioni funzionali.

### Automazione deploy — completata

- [x] Versionare il profilo e lo script di publish Aruba con pulizia locale e retry compatibili con la solution sincronizzata tramite Dropbox.
- [x] Introdurre piani di deploy mirati per trasferire o eliminare esclusivamente gli artefatti revisionati.
- [x] Automatizzare tramite GitHub Actions il deploy di MPS su Aruba e di Portfolio.Web su Altervista.
- [x] Proteggere dati runtime, database, log e artefatti non distribuibili dalla sincronizzazione remota.
- [x] Verificare le connessioni FTPS e il ciclo upload, download, controllo contenuto e cancellazione tramite sentinelle temporanee.
- [x] Consolidare per Aruba FTPS implicito su porta 990, PASV classico e root applicativa `modelbook.cloud/`.
- [x] Consolidare per Altervista il trasferimento dati tramite `curl`, con verifica preventiva del certificato e controllo post-upload.
- [x] Aggiornare le GitHub Actions alle versioni basate su Node.js 24, eliminando i warning di deprecazione.
- [x] Collaudare operativamente release applicative reali di MPS su Aruba e Portfolio.Web su Altervista, includendo trasferimento mirato, riattivazione e smoke test di produzione.

Il collaudo reale ha verificato su Aruba la pubblicazione atomica delle DLL proprietarie mediante `app_offline.htm` e su Altervista il trasferimento ASCII dei file applicativi con controllo del contenuto prima della sostituzione. Entrambi i workflow hanno completato i rispettivi smoke test di produzione.

### Affidabilità e gestione Portfolio — completata

- [x] `BL-0020` — Disposizione accettata: difetto non riproducibile, situazione bonificata e monitoraggio differito.
- [x] `BL-0013` — Consentire un path esplicito nella creazione degli Album.
- [x] `BL-0034` — Intercettare i percorsi legacy di ZenPhoto in Portfolio.Web.
- [x] `BL-0016` — Sostituire Swagger UI con Scalar — completato e verificato in produzione.

### Migliorie UI e UX — completata

- [x] `BL-0031` — Migliorare il ritaglio delle copertine nell'elenco degli articoli.
- [x] `BL-0014` — Valorizzare ModelBook.Cloud nel footer di Portfolio.Web.
- [x] `BL-0017` — Valutare la condivisione degli album su Instagram.
- [x] `BL-0008` — Completare lo sharing automatico da Portfolio.Web.
- [x] `BL-0032` — Raccontare la nascita del calendario Germana 2023.
- [x] `BL-0033` — Generare cover editoriali ad alta risoluzione.
- [x] `BL-0019` — Introdurre uno smart crop locale per le cover.

### Milestone precedente: preparazione al traffico fotografico imminente

- [x] `BL-0001` — Correggere la variante cold-cache dell'accesso diretto agli album annidati.
- [x] Estendere i test di non regressione di `BL-0001` con accesso diretto prima della navigazione gerarchica.
- [x] Creare e verificare l'album `Modelle-Modelli/Cecilia-B/sunset-at-paraggi`, con nome visualizzato `Sunset @ Paraggi`.
- [x] `BL-0002` — Mostrare il codice foto insieme a `X di Y` nella preview.
- [x] Verificare tramite emulazione mobile leggibilità, navigazione e utilità del codice nelle schermate.
- [x] `BL-0006` — Integrare e verificare la pubblicità Altervista nelle pagine interessate.
- [x] Verificare gli eventuali adempimenti di privacy e consenso introdotti dalla pubblicità.
- [x] `BL-0018` — Evitare il taglio dei volti nelle cover degli album; eccezioni compositive demandate allo smart crop BL-0019.
- [x] `BL-0025` — Classificare le Photo e impedire annunci nelle pagine con contenuti restricted.
- [x] `BL-0023` — Pubblicare il mini-CMS editoriale e il primo articolo dedicato a FairyTales 2021.
- [x] `BL-0015` — Completare la preparazione AdSense e richiedere la nuova revisione del sito; approvazione Google pendente.
- [x] `BL-0007` — Rendere stabile e curata la presentazione manuale del link album sui social.
- [x] `BL-0009` — Verificare ricorsivamente la navigabilità della gerarchia pubblica su Portfolio.Api e Portfolio.Web.
- [x] Registrare come verifica operativa esterna il percorso selezione → comunicazione codici → pubblicazione → accesso all'album, da eseguire quando saranno disponibili shooting, fotografie e partecipante reale.

### Milestone precedente: consolidamento documentale

- [x] Definizione del README di bootstrap
- [x] Definizione di Home
- [x] Definizione di Platform
- [x] Definizione della procedura Chat Recovery
- [x] Definizione dello stato operativo del progetto
- [x] Consolidamento dell'architettura generale
- [x] Consolidamento dello Shared Framework
- [x] Consolidamento dell'architettura dei domini
- [x] Definizione del processo ADR e avvio del reset del catalogo Alpha
- [x] Consolidamento delle pratiche di engineering
- [x] Consolidamento dell'architettura di testing
- [x] Consolidamento dell'architettura di sicurezza
- [x] Consolidamento delle specifiche dei domini
- [x] Completamento del reset degli ADR Alpha
- [x] Aggiornamento di Home con i collegamenti ufficiali
- [x] Verifica finale di coerenza e promozione della documentazione

### Rilievi della verifica finale

- [x] VF-01 — Allineare l'ordine del bootstrap fra Home e Chat Recovery.
- [x] VF-02 — Correggere il catalogo di stabilità nel Project Status.
- [x] VF-03 — Completare le valutazioni puntuali del Secret Risk Register.
- [x] VF-04 — Separare in Architecture gli approfondimenti consolidati dal materiale Alpha.
- [x] VF-05 — Uniformare l'ordine di lettura di Domain Architecture e Shared Framework.
- [x] VF-06 — Rendere autonomamente riconoscibili i documenti Alpha.
- [x] VF-07 — Riallineare il Glossary alla terminologia consolidata.
- [x] VF-08 — Distribuire il residuo di Architecture Consolidation e definirne la chiusura.

---

## Debito tecnico

Stato: **2 medi**

### TD-0003 — Logging policy dei Controller

Deve essere consolidata la responsabilità di logging tra Controller, pipeline e Service.

### TD-0004 — Documentazione XML delle API pubbliche

La documentazione XML delle superfici pubbliche non è ancora completa.

Altri debiti: **3 bassi**.

Registro completo: [Technical Debt](Engineering/TechnicalDebt.md).

Queste attività non costituiscono la priorità corrente, salvo diversa indicazione in questo documento.

---

## Backlog funzionale

Stato massimo attivo: **Medio**

### Milestone Affidabilità e gestione Portfolio — completata

La milestone comprende `BL-0020`, `BL-0013`, `BL-0034` e `BL-0016`. Priorità, criteri di accettazione e dettagli sono riportati nel registro completo.

### Milestone Migliorie UI e UX — completata

La milestone comprende `BL-0008`, `BL-0014`, `BL-0017`, `BL-0019`, `BL-0031`, `BL-0032` e `BL-0033`. Priorità, criteri di accettazione e dettagli sono riportati nel registro completo.

`BL-0015` e `BL-0023` sono completati; l'esito della revisione Google viene monitorato come dipendenza esterna.

Backlog completo: [Backlog](Roadmap/Backlog.md).

Tutti e sette gli elementi della milestone precedente sono completati.

---

## Istruzione per una nuova sessione

Alla domanda "A che punto siamo su MPS?", rispondere che:

> MPS ha completato la code review generale, il consolidamento documentale, l'automazione deploy, il consolidamento delle specifiche di coding e il consolidamento della pipeline MVC. `TD-0001` e `TD-0002` sono risolti; la prossima milestone deve essere definita. BL-0020 resta in monitoraggio differito; revisione AdSense e prova con una modella restano verifiche esterne non bloccanti.

La milestone Automazione deploy è completata anche sul piano operativo: release reali mirate di MPS e Portfolio.Web sono state trasferite e verificate in produzione. La baseline di coding è ora autorevole e applicata: ogni nuovo intervento deve rispettarne la quality gate.

Prima di iniziare il lavoro, verificare l'attività corrente e il prossimo elemento non completato della checklist.

---

## Ultimo aggiornamento

- Data: 2026-08-13
- Milestone: Consolidamento della pipeline MVC completata
- Attività corrente: definizione della prossima milestone.
