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

### Documentazione consolidata candidata alla promozione

Stato: **Release Candidate**

Home cataloga i documenti già consolidati relativi ad architettura, ADR, engineering, roadmap e dominio Portfolio. Questi documenti hanno completato la revisione tematica ma diventeranno ufficialmente **Stable 1.0** soltanto al termine della verifica finale.

Catalogo e ordine di lettura: [Home](Home.md).

### Documentazione residua

Stato: **Alpha 0**

Home identifica esplicitamente i documenti ancora Alpha e i documenti temporanei di migrazione.

Questi documenti possono essere incompleti, incoerenti o non aggiornati. Devono essere verificati e consolidati prima di essere promossi a documentazione ufficiale.

---

## Stato attuale del progetto

MPS si trova in una fase di consolidamento della documentazione.

Il codice ha recentemente completato una code review generale:

- build verde;
- test verdi;
- warning azzerati;
- pipeline, validazione e normalizzazione consolidate;
- debito tecnico residuo registrato nel registro consolidato.

Il secondo livello documentativo è diviso fra documenti consolidati candidati alla promozione e materiale residuo Alpha ancora da verificare o distribuire.

---

## Milestone corrente

Consolidamento della documentazione draft e sua progressiva promozione a documentazione ufficiale.

---

## Attività corrente

Eseguire la verifica finale di coerenza e promuovere la documentazione consolidata.

Non iniziare nuova documentazione implementativa finché la struttura architetturale e le responsabilità dei documenti non sono consolidate.

---

## Avanzamento della milestone

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
- [ ] Verifica finale di coerenza e promozione della documentazione

### Rilievi della verifica finale

- [x] VF-01 — Allineare l'ordine del bootstrap fra Home e Chat Recovery.
- [x] VF-02 — Correggere il catalogo di stabilità nel Project Status.
- [x] VF-03 — Completare le valutazioni puntuali del Secret Risk Register.
- [x] VF-04 — Separare in Architecture gli approfondimenti consolidati dal materiale Alpha.
- [x] VF-05 — Uniformare l'ordine di lettura di Domain Architecture e Shared Framework.
- [x] VF-06 — Rendere autonomamente riconoscibili i documenti Alpha.
- [x] VF-07 — Riallineare il Glossary alla terminologia consolidata.
- [ ] VF-08 — Distribuire il residuo di Architecture Consolidation e definirne la chiusura.

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

Stato massimo attivo: **1 alto**

### BL-0001 — Alcuni album di secondo livello non vengono caricati

Portfolio.Web restituisce un errore aprendo alcuni album annidati; il caso noto è `Modelle e Modelli / Annalisa L.`.

Altri elementi: **1 medio, 2 non prioritizzati**.

Backlog completo: [Backlog](Roadmap/Backlog.md).

Il bug è registrato ma non sostituisce l'attività corrente finché non viene esplicitamente pianificato o riclassificato come interruzione urgente.

---

## Istruzione per una nuova sessione

Alla domanda "A che punto siamo su MPS?", rispondere che:

> MPS ha completato una fase importante di code review e si trova ora nella milestone di consolidamento della documentazione. La documentazione ufficiale di bootstrap è disponibile e Home collega i documenti già consolidati; il materiale residuo ancora Alpha deve essere verificato prima della promozione. L'architettura generale, lo Shared Framework, l'architettura dei domini e delle Web Application, l'architettura di testing, l'architettura di sicurezza, le specifiche del dominio Portfolio, il processo ADR e le pratiche di engineering sono stati consolidati. Il reset del catalogo ADR Alpha è completato; l'attività corrente è la verifica finale di coerenza e promozione della documentazione.

Prima di iniziare il lavoro, verificare l'attività corrente e il prossimo elemento non completato della checklist.

---

## Ultimo aggiornamento

- Data: 2026-08-08
- Milestone: Consolidamento della documentazione
- Attività corrente: Verifica finale di coerenza e promozione della documentazione
