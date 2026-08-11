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

**Affidabilità e gestione Portfolio.**

La milestone comprende quattro interventi dedicati alla creazione degli Album, al routing legacy e alla documentazione interattiva delle API.

Ultima milestone conclusa: **Migliorie UI e UX**, completata l'11 agosto 2026.

La milestone ha consegnato tutte le modifiche richieste a MPS, Portfolio.Api e Portfolio.Web. Restano due verifiche dipendenti da processi esterni: l'esito della revisione Google AdSense e la prova del flusso di selezione con una modella dopo la disponibilità di un nuovo shooting. Nessuna delle due richiede ulteriore implementazione per considerare conclusa la milestone.

Milestone precedente: **preparazione di Portfolio.Web al traffico fotografico**, completata l'11 agosto 2026.

---

## Attività corrente

La milestone **Affidabilità e gestione Portfolio** è attiva. L'attività corrente è `BL-0020`: riprodurre e correggere la creazione duplicata di Album nella root.

Gli esiti esterni della revisione Google AdSense e del flusso reale con una modella continuano a essere monitorati, ma non bloccano la nuova milestone.

---

## Avanzamento della milestone

### Affidabilità e gestione Portfolio

- [ ] `BL-0020` — Correggere la creazione duplicata di Album nella root — in corso.
- [ ] `BL-0013` — Consentire un path esplicito nella creazione degli Album.
- [ ] `BL-0034` — Intercettare i percorsi legacy di ZenPhoto in Portfolio.Web.
- [ ] `BL-0016` — Sostituire Swagger UI con Scalar.

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

Stato: **2 alti**

### TD-0001 — Integration Test della pipeline MVC

Manca la verifica integrata del flusso HTTP completo di normalizzazione, validazione e gestione degli errori.

### TD-0002 — Gestione centralizzata di `KeyNotFoundException`

La traduzione in `404 Not Found` è ancora duplicata nei Controller e deve confluire nella pipeline MVC.

Altri debiti: **2 medi, 5 bassi**.

Registro completo: [Technical Debt](Engineering/TechnicalDebt.md).

Queste attività non costituiscono la priorità corrente, salvo diversa indicazione in questo documento.

---

## Backlog funzionale

Stato massimo attivo: **Medio**

### Milestone Affidabilità e gestione Portfolio

La milestone comprende `BL-0020`, `BL-0013`, `BL-0034` e `BL-0016`. Priorità, criteri di accettazione e dettagli sono riportati nel registro completo.

### Milestone Migliorie UI e UX — completata

La milestone comprende `BL-0008`, `BL-0014`, `BL-0017`, `BL-0019`, `BL-0031`, `BL-0032` e `BL-0033`. Priorità, criteri di accettazione e dettagli sono riportati nel registro completo.

`BL-0015` e `BL-0023` sono completati; l'esito della revisione Google viene monitorato come dipendenza esterna.

Backlog completo: [Backlog](Roadmap/Backlog.md).

Tutti e sette gli elementi della milestone precedente sono completati.

---

## Istruzione per una nuova sessione

Alla domanda "A che punto siamo su MPS?", rispondere che:

> MPS ha completato la code review generale, il consolidamento della documentazione, la preparazione di Portfolio.Web al traffico fotografico e la milestone Migliorie UI e UX. La milestone attiva è Affidabilità e gestione Portfolio; l'attività corrente è BL-0020, dedicata alla riproduzione e correzione della creazione duplicata di Album nella root. La revisione AdSense e la prova con una modella restano verifiche esterne pendenti e non bloccanti.

Prima di iniziare il lavoro, verificare l'attività corrente e il prossimo elemento non completato della checklist.

---

## Ultimo aggiornamento

- Data: 2026-08-11
- Milestone: Affidabilità e gestione Portfolio — attiva
- Attività corrente: `BL-0020` — creazione duplicata di Album nella root
