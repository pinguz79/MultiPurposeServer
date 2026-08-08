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

Nessuna nuova milestone è stata ancora selezionata.

Ultima milestone conclusa: consolidamento della documentazione draft e promozione della documentazione verificata a Stable 1.0.

---

## Attività corrente

Selezionare e formalizzare la prossima milestone confrontando backlog funzionale, debito tecnico e roadmap architetturale.

Non avviare una nuova attività implementativa prima che la milestone scelta sia registrata in questo documento e nella Roadmap.

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

Stato massimo attivo: **1 alto**

### BL-0001 — Alcuni album di secondo livello non vengono caricati

Portfolio.Web restituisce un errore aprendo alcuni album annidati; il caso noto è `Modelle e Modelli / Annalisa L.`.

Altri elementi: **1 medio, 2 non prioritizzati**.

Backlog completo: [Backlog](Roadmap/Backlog.md).

Il bug è registrato ma non sostituisce l'attività corrente finché non viene esplicitamente pianificato o riclassificato come interruzione urgente.

---

## Istruzione per una nuova sessione

Alla domanda "A che punto siamo su MPS?", rispondere che:

> MPS ha completato la code review generale e la milestone di consolidamento della documentazione. Il bootstrap e il secondo livello documentativo verificato sono Stable 1.0; Home ne rappresenta l'indice ordinato. Il materiale residuo Alpha è esplicitamente non autorevole e conserva lavoro futuro. Non è ancora stata selezionata una nuova milestone; l'attività corrente è confrontare backlog funzionale, debito tecnico e roadmap architetturale e formalizzare la prossima priorità.

Prima di iniziare il lavoro, verificare l'attività corrente e il prossimo elemento non completato della checklist.

---

## Ultimo aggiornamento

- Data: 2026-08-08
- Milestone: Nessuna milestone attiva; consolidamento della documentazione completato
- Attività corrente: Selezione e formalizzazione della prossima milestone
