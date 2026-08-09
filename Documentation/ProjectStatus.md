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

**Preparazione di Portfolio.Web al traffico fotografico imminente.**

La milestone prepara il flusso di selezione successivo allo shooting del 2026-08-09 e la successiva pubblicazione social. Deve concludersi prima dell'invio del link dell'album alla modella e, per gli aspetti pubblicitari e social, non oltre la pubblicazione che genererà il traffico aggiuntivo.

Ultima milestone conclusa: consolidamento della documentazione draft e promozione della documentazione verificata a Stable 1.0.

---

## Attività corrente

Preparare Portfolio.Web al flusso reale di consultazione e selezione delle fotografie. `BL-0001`, `BL-0002`, `BL-0006`, `BL-0007` e `BL-0012` sono completati. Il feedback reale sulla pubblicità è positivo; ha invece evidenziato il crop non adeguato delle cover degli album, ora tracciato da `BL-0018` come attività prioritaria.

Lo sharing automatico non è bloccante: la fotografia può essere inviata alla modella, pubblicata autonomamente e accompagnata dal link manuale all'album.

---

## Avanzamento della milestone

### Preparazione al traffico fotografico imminente

- [x] `BL-0001` — Correggere la variante cold-cache dell'accesso diretto agli album annidati.
- [x] Estendere i test di non regressione di `BL-0001` con accesso diretto prima della navigazione gerarchica.
- [x] Creare e verificare l'album `Modelle-Modelli/Cecilia-B/sunset-at-paraggi`, con nome visualizzato `Sunset @ Paraggi`.
- [x] `BL-0002` — Mostrare il codice foto insieme a `X di Y` nella preview.
- [x] Verificare tramite emulazione mobile leggibilità, navigazione e utilità del codice nelle schermate.
- [x] `BL-0006` — Integrare e verificare la pubblicità Altervista nelle pagine interessate.
- [x] Verificare gli eventuali adempimenti di privacy e consenso introdotti dalla pubblicità.
- [ ] `BL-0018` — Evitare il taglio dei volti nelle cover degli album.
- [ ] `BL-0015` — Completare la preparazione AdSense e richiedere/verificare l'approvazione del sito.
- [x] `BL-0007` — Rendere stabile e curata la presentazione manuale del link album sui social.
- [x] `BL-0009` — Verificare ricorsivamente la navigabilità della gerarchia pubblica su Portfolio.Api e Portfolio.Web.
- [ ] Eseguire una verifica end-to-end del percorso selezione → comunicazione codici → pubblicazione → accesso all'album.

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

Stato massimo attivo: **2 alti**

### BL-0015 — Attivare Google AdSense su Portfolio.Web

La preparazione tecnica e di conformità appartiene alla milestone; l'erogazione effettiva dipende dalla revisione esterna e dallo stato `Ready` assegnato da Google.

### BL-0018 — Evitare il taglio dei volti nelle cover degli album

Il crop centrale delle cover deve essere corretto prima dell'invio del prossimo album fotografico.

Altri elementi: **4 bassi, 5 non prioritizzati**.

Backlog completo: [Backlog](Roadmap/Backlog.md).

Questi elementi costituiscono il perimetro funzionale della milestone corrente.

---

## Istruzione per una nuova sessione

Alla domanda "A che punto siamo su MPS?", rispondere che:

> MPS ha completato la code review generale e il consolidamento della documentazione. La milestone corrente prepara Portfolio.Web al traffico collegato allo shooting del 2026-08-09: correzione degli album annidati, codice foto nelle preview, verifica mobile, pubblicità Altervista, tentativo di attivazione Google AdSense e presentazione corretta del link album condiviso manualmente. Lo sharing automatico non è bloccante e rimane pianificato con priorità bassa.

Prima di iniziare il lavoro, verificare l'attività corrente e il prossimo elemento non completato della checklist.

---

## Ultimo aggiornamento

- Data: 2026-08-08
- Milestone: Preparazione di Portfolio.Web al traffico fotografico imminente
- Attività corrente: `BL-0018`, correzione del crop delle cover degli album
