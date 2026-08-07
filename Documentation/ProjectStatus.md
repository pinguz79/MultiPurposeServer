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

### Documentazione draft

Stato: **Alpha 0**

Il resto della cartella `Documentation` contiene una prima ricostruzione dell'architettura, delle pratiche di ingegneria, degli ADR, delle roadmap e dei domini.

Questi documenti possono essere incompleti, incoerenti o non aggiornati. Devono essere verificati e consolidati prima di essere promossi a documentazione ufficiale.

---

## Stato attuale del progetto

MPS si trova in una fase di consolidamento della documentazione.

Il codice ha recentemente completato una code review generale:

- build verde;
- test verdi;
- warning azzerati;
- pipeline, validazione e normalizzazione consolidate;
- debito tecnico residuo registrato nella documentazione draft.

Il secondo livello documentativo contiene una ricostruzione estesa del progetto, ma non è ancora considerato ufficiale.

---

## Milestone corrente

Consolidamento della documentazione draft e sua progressiva promozione a documentazione ufficiale.

---

## Attività corrente

Consolidare le pratiche di engineering.

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
- [ ] Consolidamento dell'architettura di testing
- [ ] Consolidamento dell'architettura di sicurezza
- [ ] Consolidamento delle specifiche dei domini
- [ ] Completamento del reset degli ADR Alpha
- [ ] Aggiornamento di Home con i collegamenti ufficiali
- [ ] Verifica finale di coerenza e promozione della documentazione

---

## Debito tecnico

Stato: **2 alti**

### TD-0001 — Integration Test della pipeline MVC

Manca la verifica integrata del flusso HTTP completo di normalizzazione, validazione e gestione degli errori.

### TD-0002 — Gestione centralizzata di `KeyNotFoundException`

La traduzione in `404 Not Found` è ancora duplicata nei Controller e deve confluire nella pipeline MVC.

Altri debiti: **2 medi, 2 bassi**.

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

> MPS ha completato una fase importante di code review e si trova ora nella milestone di consolidamento della documentazione. La documentazione ufficiale di bootstrap è disponibile, mentre il resto di `Documentation` è ancora in stato alpha e viene progressivamente verificato e promosso. L'architettura generale, lo Shared Framework, l'architettura dei domini, il processo ADR e le pratiche di engineering sono stati consolidati. Il reset del catalogo ADR Alpha proseguirà insieme ai documenti specialistici pertinenti; l'attività corrente è il consolidamento dell'architettura di testing.

Prima di iniziare il lavoro, verificare l'attività corrente e il prossimo elemento non completato della checklist.

---

## Ultimo aggiornamento

- Data: 2026-08-07
- Milestone: Consolidamento della documentazione
- Attività corrente: Consolidamento dell'architettura di testing
