# Backlog di MultiPurposeServer

## 1. Scopo

Questo documento è la fonte autorevole del lavoro funzionale noto ma non necessariamente pianificato.

Contiene Epic, Feature, Bug e Improvement. Il debito tecnico appartiene invece al registro [Technical Debt](../Engineering/TechnicalDebt.md).

La presenza nel Backlog non assegna automaticamente una milestone. La [Roadmap](Roadmap.md) stabilisce la sequenza intenzionale e `ProjectStatus.md` definisce l'attività corrente.

Gli identificatori `BL-XXXX` sono stabili e non vengono riutilizzati.

---

## 2. Tipi e stati

### Tipi

- **Epic**: risultato ampio da scomporre prima della pianificazione.
- **Feature**: nuova capacità osservabile da un utilizzatore.
- **Bug**: comportamento osservabile differente da quello atteso.
- **Improvement**: miglioramento funzionale di un comportamento esistente.

### Stati

- **Da definire**: intenzione nota ma non ancora sufficientemente specificata.
- **Aperto**: problema o risultato descritto e pronto per essere analizzato.
- **Pianificato**: assegnato a una milestone.
- **In corso**: lavoro attivo.
- **Completato**: risultato implementato e verificato.
- **Annullato**: non verrà realizzato; la motivazione viene conservata.

### Priorità

Le priorità sono `Critica`, `Alta`, `Media`, `Bassa` oppure `Non assegnata`.

La valutazione considera valore o impatto per l'utilizzatore, diffusione del problema, costo indicativo, urgenza, workaround e relazione con le milestone correnti.

---

## 3. Riepilogo attivo

| Tipo | Critica | Alta | Media | Bassa | Non assegnata |
|---|---:|---:|---:|---:|---:|
| Bug | 0 | 0 | 0 | 2 | 0 |
| Feature | 0 | 1 | 0 | 0 | 0 |
| Improvement | 0 | 1 | 0 | 2 | 1 |
| Epic | 0 | 0 | 0 | 0 | 3 |

---

## 4. Bug

### BL-0001 — Alcuni album di secondo livello non vengono caricati

- **Tipo:** Bug
- **Area:** Portfolio.Web
- **Stato:** Completato
- **Priorità:** Alta
- **Segnalato:** 2026-08-07
- **Issue:** [GitHub #1](https://github.com/pinguz79/MultiPurposeServer/issues/1)

Aprendo alcuni album di secondo livello, Portfolio.Web restituisce un errore invece di visualizzarne il contenuto.

Caso noto riproducibile:

```text
Modelle e Modelli / Annalisa L.
```

- **Impatto:** il contenuto dell'album non è fruibile.
- **Workaround:** non noto.
- **Criteri di accettazione:** l'album indicato e gli altri album validi di secondo livello vengono caricati senza errore; dopo avere identificato la causa viene aggiunta una batteria di test di non regressione che riproduca il difetto e ne verifichi la correzione a fronte degli sviluppi futuri.
- **Organizzazione dei test:** i test di non regressione devono rimanere chiaramente separati dagli Unit Test, tramite progetto dedicato oppure alberatura e namespace esplicitamente dedicati. Ogni scenario deve contenere un commento che richiami il bug storico e citi `BL-0001` e [GitHub #1](https://github.com/pinguz79/MultiPurposeServer/issues/1).
- **Note diagnostiche:** Portfolio.Api risolve correttamente l'album e il relativo figlio; Portfolio.Web fallisce perché il path corrente non è presente nella cache di routing dopo il tentativo di aggiornamento. Prima della correzione deve essere ricostruita e coperta da test anche la transizione che ha prodotto l'associazione obsoleta, non soltanto la successiva bonifica.
- **Verifica della rigenerazione:** il test di produzione eseguito il 2026-08-08 ha rilevato 18 pagine non raggiungibili sulla cache storica; dopo la cancellazione di 106 route album e 212 risposte API, l'intera gerarchia è risultata raggiungibile sia con cache fredda sia con cache calda. La generazione corrente non risulta sistematicamente difettosa.
- **Correzione difensiva:** `fullPath`, `id` e `kind` sono obbligatori per scrivere una route album. Portfolio.Web non ripiega più sul path locale: un payload incompleto viene rifiutato e registrato senza corrompere la cache. Rimane da valutare il recupero automatico di eventuali associazioni storiche obsolete.
- **Completato:** 2026-08-08
- **Esito finale:** dopo il deployment della correzione, baseline, rigenerazione a cache fredda e successiva verifica a cache calda hanno attraversato l'intera gerarchia senza errori. La prova conclusiva ha eliminato 106 route album, 0 route foto e 166 risposte API prima della rigenerazione. La bonifica automatica delle associazioni obsolete non è stata introdotta: una futura incoerenza deve rimanere osservabile e diagnosticabile.

### BL-0002 — Nella preview fotografica manca il codice foto

- **Tipo:** Bug
- **Area:** Portfolio.Web
- **Stato:** Completato
- **Priorità:** Alta
- **Segnalato:** 2026-08-07
- **Issue:** [GitHub #2](https://github.com/pinguz79/MultiPurposeServer/issues/2)

Negli album caricati correttamente, la preview mostra soltanto la posizione `X di Y` e non visualizza il codice della fotografia.

- **Impatto:** durante la selezione l'utente non può comunicare in modo rapido e non ambiguo quali fotografie ha scelto.
- **Workaround:** inviare schermate o descrizioni delle fotografie, con un processo manuale più lento e soggetto ad ambiguità.
- **Criteri di accettazione:** la preview mostra sia il codice foto sia l'indicatore `X di Y`, mantenendo corretta la navigazione tra fotografie; codice e indicatore restano leggibili anche da dispositivo mobile e nelle schermate condivise.
- **Note diagnostiche:** verificare disponibilità del dato nel payload e rendering del componente di preview.
- **Completato:** 2026-08-08
- **Esito:** Portfolio.Web mostra il codice disponibile insieme alla posizione `Foto X di Y`, lo aggiorna durante la navigazione e mantiene il solo indicatore di posizione quando l'API restituisce `selectionCode` nullo. I filename storici non conformi rilevati durante la verifica sono un problema dati indipendente, tracciato in `BL-0011` e [GitHub #3](https://github.com/pinguz79/MultiPurposeServer/issues/3).

### BL-0011 — Bonificare i filename senza SelectionCode di Miss Villetta 2023

- **Tipo:** Bug
- **Area:** Portfolio / Dati
- **Stato:** Aperto
- **Priorità:** Bassa
- **Segnalato:** 2026-08-08
- **Issue:** [GitHub #3](https://github.com/pinguz79/MultiPurposeServer/issues/3)

Le fotografie di quattro album `Miss Villetta 2023` restituiscono `selectionCode` nullo perché i filename non rispettano la naming convention prevista. Il caso verificato usa il formato `MissVilletta_Sel_Alessandra-004.jpg` invece di `MissVilletta_Sel_Alessandra_004.jpg`.

- **Impatto:** il codice non è disponibile nella preview, ma gli shooting risalgono al 2023 e le fotografie sono già state selezionate; la bonifica non è urgente.
- **Correzione proposta:** rinominare in modo coordinato i file fisici e i corrispondenti `Foto.FileName`, quindi invalidare la cache interessata. La naming convention non viene ampliata implicitamente.
- **Audit API:** il 2026-08-08 sono stati analizzati 106 album e 837 fotografie. I 76 codici nulli sono tutti negli album `Miss Villetta 2023` di Alessandra (8), Cecilia B. (16), Fiorella B. (24) e Monique R. (28).
- **Verifica residua:** controllare filesystem o database per confermare il formato puntuale dei 76 filename, dato che il DTO pubblico non espone `FileName`.
- **Criteri di accettazione:** tutti i filename censiti rispettano la convenzione; filesystem e database rimangono sincronizzati; Portfolio.Api restituisce il codice atteso; l'audit globale non rileva altri `selectionCode` nulli non giustificati.

### BL-0012 — Ripristinare la coerenza strutturale di FairyTales 2021

- **Tipo:** Bug
- **Area:** Portfolio / Sincronizzazione
- **Stato:** Aperto
- **Priorità corrente:** Bassa
- **Classe di blocco:** Blocking condizionale prima del prossimo deploy o debug server che richieda la sincronizzazione
- **Segnalato:** 2026-08-08
- **Issue:** [GitHub #4](https://github.com/pinguz79/MultiPurposeServer/issues/4)

`Calendari / 2021 / FairyTales 2021` è una `Collection` con due sottoalbum e 14 fotografie dirette. La struttura viola i vincoli di `AlbumKind` e impedisce la sincronizzazione completa fra database e filesystem.

- **Impatto:** basso finché non è richiesta una nuova release del server; diventa bloccante prima di un deploy su Aruba o di un'attività diagnostica che esegua la sincronizzazione.
- **Correzione proposta:** creare `FairyTales 2021 / Impaginato` e spostarvi le 14 fotografie oggi presenti direttamente nella collection, aggiornando coerentemente filesystem e database.
- **Audit API:** il controllo del 2026-08-08 su 106 album non ha rilevato altre violazioni strutturali osservabili tramite le API correnti.
- **Criteri di accettazione:** `FairyTales 2021` contiene soltanto sottoalbum; le fotografie sono mappate in `Impaginato`; la sincronizzazione completa termina senza errori; un nuovo audit non rileva violazioni residue oppure le traccia separatamente.

---

## 5. Epic

### BL-0003 — Avvio del dominio ModelBook

- **Tipo:** Epic
- **Area:** ModelBook
- **Stato:** Da definire
- **Priorità:** Non assegnata
- **Registrato:** 2026-08-07

Progettare e implementare il dominio ModelBook e le relative Applications secondo la Visione e l'architettura dei domini.

Prima della pianificazione l'Epic deve essere scomposta in risultati funzionali verificabili, definendo primo rilascio, client iniziale, persistenza e modello di sicurezza.

### BL-0004 — Avvio del dominio Skating

- **Tipo:** Epic
- **Area:** Skating
- **Stato:** Da definire
- **Priorità:** Non assegnata
- **Registrato:** 2026-08-07

Progettare e implementare il dominio Skating per la gestione di competizioni, iscrizioni, risultati e classifiche.

Prima della pianificazione l'Epic deve essere scomposta in risultati funzionali verificabili e deve essere chiarito il perimetro del primo rilascio.

### BL-0005 — Avvio del dominio BoardGameUniverse

- **Tipo:** Epic
- **Area:** BoardGameUniverse
- **Stato:** Da definire
- **Priorità:** Non assegnata
- **Registrato:** 2026-08-08

Progettare e implementare il dominio BoardGameUniverse secondo la Visione e l'architettura dei domini.

Prima della pianificazione l'Epic deve essere scomposta in risultati funzionali verificabili e deve essere chiarito il perimetro del primo rilascio.

---

## 6. Feature e Improvement

### BL-0006 — Integrare la pubblicità Altervista in Portfolio.Web

- **Tipo:** Feature
- **Area:** Portfolio.Web
- **Stato:** Pianificato
- **Priorità:** Alta
- **Registrato:** 2026-08-08

Integrare in Portfolio.Web i codici pubblicitari forniti da Altervista per valorizzare il traffico generato dalla selezione fotografica e dalla successiva pubblicazione sui social.

- **Criteri di accettazione:** almeno una posizione pubblicitaria è attiva nelle pagine interessate dal nuovo traffico; il layout rimane fruibile su desktop e mobile; l'integrazione usa i codici e gli strumenti previsti da Altervista; sono verificate le conseguenze applicabili su privacy e consenso.

### BL-0007 — Curare la presentazione dei link album sui social

- **Tipo:** Improvement
- **Area:** Portfolio.Web
- **Stato:** Pianificato
- **Priorità:** Alta
- **Registrato:** 2026-08-08

Rendere stabile e riconoscibile la presentazione di un album quando il relativo URL viene inserito manualmente nella descrizione di un contenuto social.

- **Criteri di accettazione:** l'album ha URL pubblico e stabile, titolo e descrizione coerenti, URL canonica e metadati Open Graph essenziali; l'anteprima del link viene verificata almeno sul canale social scelto per la pubblicazione imminente.
- **Nota:** il risultato non richiede un comando di condivisione integrato in Portfolio.Web.

### BL-0008 — Completare lo sharing automatico da Portfolio.Web

- **Tipo:** Improvement
- **Area:** Portfolio.Web
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-08

Completare e rendere uniforme il meccanismo, oggi parziale, di condivisione diretta di album e fotografie da Portfolio.Web.

- **Workaround:** la persona riceve la fotografia, la pubblica autonomamente e inserisce manualmente nella descrizione il link pubblico dell'album.
- **Motivazione della priorità:** il workaround è macchinoso ma consente comunque il flusso editoriale previsto; lo sharing automatico non è necessario per la milestone corrente.

### BL-0009 — Verificare automaticamente la navigabilità di Portfolio in produzione

- **Tipo:** Improvement
- **Area:** Portfolio / Testing
- **Stato:** Completato
- **Priorità:** Media
- **Registrato:** 2026-08-08

Realizzare un test di navigabilità che parta dalla root pubblica `https://marcolepriph.altervista.org`, scopra ricorsivamente la gerarchia degli album tramite le API pubbliche di Portfolio e verifichi tutte le richieste HTTP corrispondenti sia su Portfolio.Api sia su Portfolio.Web in produzione.

- **Obiettivo:** individuare link rotti, album non raggiungibili, divergenze fra gerarchia API e navigazione Web ed errori presenti soltanto nell'ambiente pubblicato.
- **Criteri di accettazione:** il test visita ogni album scoperto, registra URL e risultato di ciascuna richiesta, distingue gli errori API dagli errori Web e produce un esito complessivo ripetibile senza modificare dati applicativi.
- **Nota di pianificazione:** il test appartiene alla milestone corrente come verifica globale, ma la sua automazione può essere completata dopo la correzione urgente di `BL-0001` se non è necessaria per diagnosticare il difetto.
- **Completato:** 2026-08-08
- **Esito:** baseline storica con 18 pagine Portfolio.Web in errore; rigenerazione completa della cache e successivi passaggi cold/warm senza errori. Il test rimane disabilitato per default e richiede opt-in esplicito perché cancella cache di produzione ricostruibili.

### BL-0010 — Segnalare in Portfolio.Admin le incoerenze della cache

- **Tipo:** Improvement
- **Area:** Portfolio.Admin / Osservabilità
- **Stato:** Da definire
- **Priorità:** Non assegnata
- **Registrato:** 2026-08-08
- **Origine:** [GitHub #1](https://github.com/pinguz79/MultiPurposeServer/issues/1)

Quando verrà introdotto Portfolio.Admin, prevedere un controllo silenzioso delle invarianti della cache di Portfolio.Web e rendere visibili all'amministratore eventuali associazioni incoerenti, senza bonificarle automaticamente.

Possibili presentazioni:

- warning all'avvio di un client Desktop amministrativo;
- indicatore nella home dell'area Web amministrativa;
- pagina diagnostica con route coinvolte, tipo di incoerenza e azione esplicita disponibile.

Il controllo deve distinguere almeno path mancanti, `fullPath` non validi, associazioni non biunivoche fra route e ID e divergenze rispetto alla gerarchia autorevole esposta da Portfolio.Api. L'eventuale invalidazione o rigenerazione deve rimanere un'azione amministrativa esplicita e osservabile.

### BL-0013 — Consentire un path esplicito nella creazione degli album

- **Tipo:** Improvement
- **Area:** Portfolio.Api / Gestione album
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-08

Estendere il contratto di creazione album con un path alternativo opzionale. Quando il chiamante lo valorizza, Portfolio.Api deve usare il valore esplicito invece di dedurlo dal nome visualizzato; quando è assente, rimane valido il comportamento corrente.

- **Motivazione:** nome editoriale e slug possono avere rappresentazioni intenzionalmente diverse, per esempio `Sunset @ Paraggi` e `sunset-at-paraggi`.
- **Workaround corrente:** creare l'album usando inizialmente lo slug desiderato come nome, quindi aggiornare soltanto il nome visualizzato senza modificare il path.
- **Criteri di accettazione:** la request accetta un path opzionale; il valore esplicito viene normalizzato e validato secondo le regole delle route; unicità e coerenza gerarchica sono garantite; l'assenza del valore conserva la deduzione dal nome; sono coperti da test entrambi i flussi.

---

## 7. Elementi completati o annullati

`BL-0001`, `BL-0002` e `BL-0009` sono completati e rimangono nelle rispettive sezioni per conservarne contesto, verifiche ed esito.

Gli elementi completati o annullati conservano identificatore ed esito. Se il documento diventerà troppo esteso potranno essere trasferiti in un archivio senza riutilizzarne gli ID.

---

## Riferimenti

- [Roadmap](Roadmap.md)
- [Visione](Vision.md)
- [Project Status](../ProjectStatus.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
- [Portfolio Domain](../Portfolio/Domain.md)
