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

Home cataloga i documenti ufficiali relativi ad architettura, ADR, engineering, roadmap e domini consolidati. Questi documenti hanno completato revisione tematica e verifica globale e costituiscono fonti autorevoli nei rispettivi ambiti.

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

**Avvio del dominio Finance**

La milestone introduce Finance come nuovo dominio autonomo di MultiPurposeServer, dedicato alla gestione e al monitoraggio delle finanze personali.

La progettazione funzionale e l'architettura iniziale di Finance sono state consolidate. I primi quattro vertical
slice, fino a Formule dinamiche, Pianificazioni, Preview e generazione atomica dei Movimenti, sono implementati,
distribuiti e collaudati in produzione. L'attività corrente è `BL-0055`, quinto vertical slice dedicato alle
Categorie e alla bonifica retroattiva dei dati esistenti. `BL-0041`, dedicato al raggruppamento gerarchico delle API
in Scalar, rimane in stand-by durante il completamento del flusso Finance.

### Milestone sospesa

**Consolidamento delle API pubbliche e dell'osservabilità**

La milestone è temporaneamente sospesa per dare priorità all'avvio del dominio Finance.

La milestone completa `TD-0003` e `TD-0004`: definisce responsabilità e granularità del logging fra Controller, pipeline e Service, completa la documentazione XML delle superfici pubbliche e ne verifica la resa nella documentazione OpenAPI esposta tramite Scalar. L'infrastruttura deve predisporre fondamenta coerenti per la futura consultazione centralizzata dei log senza anticipare l'implementazione di `BL-0035`.

Il punto esatto di avanzamento e le attività ancora da completare sono conservati nella sezione "Avanzamento della milestone".

Ultima milestone conclusa: **Consolidamento della pipeline Bulk**, completata il 14 agosto 2026.

La milestone ha introdotto strategie indipendenti di persistenza e valutazione, supporto delle quattro combinazioni ammesse, esiti condivisi, checkpoint applicativi e di persistenza e un esecutore comune al dominio Portfolio. Build, test, publish mirato e smoke test di produzione sono stati completati con successo.

Milestone precedente: **Consolidamento della pipeline MVC**, completata il 13 agosto 2026.

La milestone ha completato `TD-0001` e `TD-0002`: Integration Test HTTP in memoria per Model Binding, normalizzazione, validazione ricorsiva e bulk, mancata invocazione dei Service e traduzione delle eccezioni; gestione centralizzata di `KeyNotFoundException` con mantenimento della semantica locale dei warning bulk.

Milestone precedente: **Consolidamento delle specifiche di coding**, completata il 13 agosto 2026.

La milestone ha completato `BL-0037`, `TD-0005` e `TD-0006`: convenzioni autorevoli per C#, test, PHP, JavaScript, CSS e SQL; enforcement deterministico riproducibile; baseline applicata a server, client, Shared Framework e test. La verifica finale ha superato build senza warning, 671 test non-production e revisione completa delle diff stilistiche separate dagli sviluppi funzionali.

Milestone precedente: **Automazione deploy**, completata il 12 agosto 2026.

Milestone precedente: **Affidabilità e gestione Portfolio**, completata l'11 agosto 2026.

La milestone ha completato `BL-0013`, `BL-0034` e `BL-0016`. `BL-0020` non è stato riprodotto: la situazione anomala è stata bonificata e il punto è stato trasferito al monitoraggio differito con test diagnostico e logging strutturato.

Milestone precedente: **Migliorie UI e UX**, completata l'11 agosto 2026.

La milestone ha consegnato tutte le modifiche richieste a MPS, Portfolio.Api e Portfolio.Web. La revisione Google AdSense ha successivamente mantenuto il sito nello stato `Richiede attenzione` per `Contenuti di scarso valore`, pur confermando `ads.txt` autorizzato; resta inoltre da provare il flusso di selezione con una modella dopo la disponibilità di un nuovo shooting. Nessuna delle due verifica esterna modifica la chiusura tecnica della milestone.

Milestone ancora precedente: **preparazione di Portfolio.Web al traffico fotografico**, completata l'11 agosto 2026.

---

## Attività corrente

L'attività corrente è `BL-0053`, quarto vertical slice Finance dedicato a Formule dinamiche e Pianificazioni. Il
terzo vertical slice, `BL-0050`, e l'arricchimento della timeline `BL-0052` sono completati end-to-end. `BL-0041`
rimane in stand-by fino alla chiusura del nuovo vertical slice.

`TD-0011` è chiuso: la convenzione MPS per route e organizzazione dei Controller è applicata a Portfolio, Finance,
relativi client e test ed è stata collaudata mediante deploy coordinati Aruba e Altervista e smoke test di produzione.

## Attività sospesa

L'attività sospesa è completare l'applicazione della logging policy nell'host e nei domini, eliminando duplicazioni e lacune, e proseguire con la documentazione XML delle superfici pubbliche e la relativa verifica in Scalar. Il progetto autonomo `MultiPurposeServer.Shared.Logging` e le API diagnostiche di Portfolio sono già implementati.

Le quattro combinazioni fra persistenza e valutazione sono ora operative tramite `BulkOperationExecutor`. `PartialSuccess` usa un'operazione indipendente per item; `AllOrNothing` usa una sola operazione globale e checkpoint applicativi implementati tramite savepoint EF. La response e la tassonomia degli errori sono condivise, mentre l'esecutore resta per ora nel dominio Portfolio.

La procedura di automazione deploy è stata collaudata operativamente con release reali di MPS su Aruba e Portfolio.Web su Altervista. `BL-0020` resta sotto monitoraggio differito.

La revisione Google AdSense del 25 agosto 2026 ha nuovamente richiesto attenzione per `Contenuti di scarso valore`; `ads.txt` risulta autorizzato, quindi l'esito evidenzia un limite editoriale e non un malfunzionamento dell'integrazione tecnica. L'eventuale nuovo intervento editoriale e il flusso reale con una modella continuano a essere monitorati, ma non bloccano l'attività corrente.

---

## Avanzamento della milestone

### Avvio del dominio Finance - in corso

- [x] Formalizzare Finance nella Visione di MultiPurposeServer.
- [x] Registrare `BL-0039` nel Backlog e promuovere la milestone nella Roadmap.
- [x] Definire scopo e confini del dominio Finance.
- [x] Definire terminologia e concetti fondamentali del dominio.
- [x] Definire capacità funzionali e casi d'uso principali.
- [x] Consolidare il modello funzionale iniziale del dominio.
- [x] Definire l'architettura iniziale di Finance coerentemente con la Domain Architecture di MPS.
- [x] Estrarre e collaudare `MultiPurposeServer.Shared.Persistence` come comportamento trasversale preliminare.
- [x] Identificare il primo vertical slice implementativo.
- [x] Realizzare il primo vertical slice.
- [x] Verificare build, test e integrazione del nuovo dominio nell'host MPS.
- [x] Eseguire migrazione, deploy mirato e collaudo operativo del vertical slice in produzione.
- [x] Consolidare a livello MPS le convenzioni per route e organizzazione dei Controller.
- [x] Allineare localmente Portfolio, Finance, client e test alla convenzione API consolidata.
- [x] Eseguire il deploy coordinato e collaudare le nuove route in produzione, chiudendo `TD-0011`.
- [x] Definire il perimetro funzionale e tecnico di `BL-0046`, secondo vertical slice Finance.
- [x] Implementare la navigazione dai Conti all'elenco dei Movimenti.
- [x] Implementare le API Bulk di creazione e aggiornamento dei Movimenti, senza UI Desktop dedicata.
- [x] Verificare client Desktop, API, persistenza, test, migrazione e deploy del secondo vertical slice.
- [x] Definire il perimetro funzionale e tecnico di `BL-0050`, terzo vertical slice Finance.
- [x] Implementare modello, persistenza, API aggregate e Bulk Create delle Voci ricorrenti.
- [x] Implementare in Finance.Desktop la vista master-detail, i dialog e il grafico di copertura.
- [x] Verificare client Desktop, API, persistenza, test, migrazione e deploy del terzo vertical slice.
- [x] Completare `BL-0052` arricchendo la timeline con gruppi mensili, delta e saldi riepilogativi.
- [x] Definire il perimetro funzionale e tecnico di `BL-0053`, quarto vertical slice Finance.
- [x] Implementare motore Formule, Pianificazioni, Preview e generazione atomica dei Movimenti.
- [x] Implementare in Finance.Desktop il dialog mensile di creazione della Pianificazione.
- [x] Verificare test, migrazione, deploy e collaudo operativo del quarto vertical slice.
- [x] Definire il perimetro funzionale e tecnico di `BL-0055`, quinto vertical slice Finance.
- [ ] Implementare anagrafica, persistenza, API puntuali e Bulk delle Categorie.
- [ ] Integrare le Categorie in Voci ricorrenti, Pianificazioni, Preview e Movimenti.
- [ ] Implementare in Finance.Desktop la configurazione delle Categorie e i selettori previsti.
- [ ] Bonificare tramite Bulk Update le Categorie dei Movimenti esistenti.
- [ ] Verificare test, migrazione, deploy e collaudo operativo del quinto vertical slice.
- [ ] Riprendere `BL-0041` dopo il quinto vertical slice e consolidare il raggruppamento gerarchico delle API in Scalar.
- [x] Aggiornare la documentazione stabile con lo stato effettivamente implementato.

### Consolidamento delle API pubbliche e dell'osservabilità — sospesa

- [x] Rilevare il logging corrente in Controller, pipeline, Service e componenti infrastrutturali.
- [x] Definire responsabilità, categorie, livelli e granularità della logging policy.
- [x] Definire la gestione dei log strutturati e della correlazione delle operazioni, comprese le operazioni Bulk.
- [ ] Applicare la policy eliminando duplicazioni e lacune rilevate.
- [ ] Definire il perimetro delle superfici pubbliche soggette a documentazione XML.
- [ ] Abilitare e completare la documentazione XML prevista da `TD-0004`.
- [ ] Verificare la resa della documentazione OpenAPI in Scalar.
- [ ] Aggiungere controlli automatici proporzionati su logging e documentazione pubblica.
- [ ] Eseguire build, test completi, publish mirato e smoke test di produzione.

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
- [x] `BL-0015` — Completare la preparazione AdSense e richiedere la nuova revisione del sito; revisione conclusa il 25 agosto 2026 con richiesta di attenzione per `Contenuti di scarso valore` e `ads.txt` autorizzato.
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

Altri debiti: **5 bassi**.

Registro completo: [Technical Debt](Engineering/TechnicalDebt.md).

Queste attività non costituiscono la priorità corrente, salvo diversa indicazione in questo documento.

---

## Backlog funzionale

Stato massimo attivo: **Medio**

### Milestone Affidabilità e gestione Portfolio — completata

La milestone comprende `BL-0020`, `BL-0013`, `BL-0034` e `BL-0016`. Priorità, criteri di accettazione e dettagli sono riportati nel registro completo.

### Milestone Migliorie UI e UX — completata

La milestone comprende `BL-0008`, `BL-0014`, `BL-0017`, `BL-0019`, `BL-0031`, `BL-0032` e `BL-0033`. Priorità, criteri di accettazione e dettagli sono riportati nel registro completo.

`BL-0015` e `BL-0023` sono completati. La revisione Google del 25 agosto 2026 ha confermato la correttezza di `ads.txt`, ma ha richiesto ulteriore valore editoriale prima dell'approvazione AdSense.

Backlog completo: [Backlog](Roadmap/Backlog.md).

Tutti e sette gli elementi della milestone precedente sono completati.

---

## Istruzione per una nuova sessione

Alla domanda "A che punto siamo su MPS?", rispondere che:

> MPS ha temporaneamente sospeso la milestone di consolidamento delle API pubbliche e dell'osservabilità per dare
> priorità all'avvio del nuovo dominio Finance. I primi tre vertical slice Finance, dedicati rispettivamente ai
> Conti, alla navigazione Conti → Movimenti con operazioni Bulk e alle Voci ricorrenti, sono implementati e
> verificati end-to-end.
> Portfolio, Finance, client e test sono allineati alla convenzione MPS per route e organizzazione dei Controller;
> il deploy coordinato e il collaudo di produzione hanno chiuso `TD-0011`. L'attività corrente è `BL-0053`, quarto
> vertical slice Finance; `BL-0041` rimane in stand-by fino alla sua chiusura.

La milestone sospesa ha già consolidato la logging policy e implementato `MultiPurposeServer.Shared.Logging` e le API diagnostiche di Portfolio. Alla ripresa, il prossimo passo sarà completare l'applicazione della policy nell'host e nei domini, quindi affrontare la documentazione XML e verificarne la resa in Scalar. `TD-0003` e `TD-0004` restano aperti. `BL-0020` resta in monitoraggio differito e le verifiche esterne non sono bloccanti.

La milestone Automazione deploy è completata anche sul piano operativo: release reali mirate di MPS e Portfolio.Web sono state trasferite e verificate in produzione. La baseline di coding è autorevole e applicata: ogni nuovo intervento deve rispettarne la quality gate.

Prima di iniziare il lavoro, verificare l'attività corrente e il prossimo elemento non completato della checklist.

---

## Ultimo aggiornamento

- Data: 2026-08-28
- Milestone: Avvio del dominio Finance
- Attività corrente: `BL-0053`, quarto vertical slice Finance per Formule dinamiche e Pianificazioni.
- Attività sospesa: completamento dell'applicazione della logging policy, documentazione XML delle superfici pubbliche e verifica in Scalar.
