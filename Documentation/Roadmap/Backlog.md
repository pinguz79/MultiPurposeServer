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
| Feature | 0 | 3 | 0 | 1 | 0 |
| Improvement | 0 | 0 | 0 | 2 | 2 |
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
- **Riaperto:** 2026-08-08, dopo la riproduzione su [GitHub #1](https://github.com/pinguz79/MultiPurposeServer/issues/1) durante la creazione di `Modelle-Modelli/Cecilia-B/sunset-at-paraggi`. Su cache vuota, l'accesso diretto usava `path` invece di `fullPath` nella scrittura della route; il vincolo univoco su `album_id` impediva poi all'upsert gerarchico di correggere l'associazione locale. La correzione deve essere verificata prima con accesso diretto a cache fredda e poi con navigazione completa cold/warm.
- **Richiuso:** 2026-08-08. La verifica finale ha rilevato una route storica corrotta nella baseline, eliminato 107 route album e 171 risposte API, aperto direttamente `sunset-at-paraggi` con HTTP 200 sulla cache vuota e completato senza errori sia la navigazione cold sia quella warm. `AlbumPageService` valida e persiste ora il `fullPath` restituito dalla risoluzione API.
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
- **Stato:** Completato
- **Priorità corrente:** Bassa
- **Classe di blocco:** Blocking condizionale prima del prossimo deploy o debug server che richieda la sincronizzazione
- **Segnalato:** 2026-08-08
- **Issue:** [GitHub #4](https://github.com/pinguz79/MultiPurposeServer/issues/4)

`Calendari / 2021 / FairyTales 2021` è una `Collection` con due sottoalbum e 14 fotografie dirette. La struttura viola i vincoli di `AlbumKind` e impedisce la sincronizzazione completa fra database e filesystem.

- **Impatto:** basso finché non è richiesta una nuova release del server; diventa bloccante prima di un deploy su Aruba o di un'attività diagnostica che esegua la sincronizzazione.
- **Correzione proposta:** creare `FairyTales 2021 / Impaginato` e spostarvi le 14 fotografie oggi presenti direttamente nella collection, aggiornando coerentemente filesystem e database.
- **Strategia di riconciliazione approvata:** la sincronizzazione tratta le fotografie presenti nel database ma mancanti sul filesystem secondo una configurazione esplicita. `KeepAndReport` conserva l'entità e produce una segnalazione diagnostica; `DeleteDatabaseEntity` elimina l'entità soltanto dopo un preflight globale e nel rispetto di una soglia massima configurata. Il report strutturato viene persistito e alimenta l'health check `portfolio-album-sync`.
- **Procedura operativa per la bonifica:** creare sul filesystem `FairyTales 2021 / Impaginato`, spostarvi le 14 copie JPEG, avviare una sola sincronizzazione con `DeleteDatabaseEntity` e soglia `20`, verificare report e alberatura, quindi ripristinare `KeepAndReport` come comportamento ordinario.
- **Audit API:** il controllo del 2026-08-08 su 106 album non ha rilevato altre violazioni strutturali osservabili tramite le API correnti.
- **Criteri di accettazione:** `FairyTales 2021` contiene soltanto sottoalbum; le fotografie sono mappate in `Impaginato`; la sincronizzazione completa termina senza errori; un nuovo audit non rileva violazioni residue oppure le traccia separatamente.
- **Completato:** 2026-08-09. Le 14 fotografie sono state spostate in `Impaginato`; la riconciliazione controllata ha eliminato 14 entità obsolete e ricreato album e fotografie nella nuova posizione. Il report ha restituito `Healthy`, così come l'health check di produzione. La configurazione ordinaria è stata ripristinata a `KeepAndReport`.

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
- **Stato:** Completato
- **Priorità:** Alta
- **Registrato:** 2026-08-08

Integrare in Portfolio.Web i codici pubblicitari forniti da Altervista per valorizzare il traffico generato dalla selezione fotografica e dalla successiva pubblicazione sui social.

- **Criteri di accettazione:** almeno una posizione pubblicitaria è attiva nelle pagine interessate dal nuovo traffico; il layout rimane fruibile su desktop e mobile; l'integrazione usa i codici e gli strumenti previsti da Altervista; sono verificate le conseguenze applicabili su privacy e consenso.
- **Prerequisito privacy:** il pannello Altervista richiede Iubenda o un'altra CMP certificata Google aggiornata. Il 2026-08-08 la CMP Iubenda TCF v2 è stata integrata e verificata in produzione: accettazione, rifiuto, persistenza, modifica delle preferenze e resa mobile hanno dato esito positivo. La policy corrente dichiara `Altervista Advertising` e `Altervista Platform`; dovrà essere rivalutata prima dell'eventuale attivazione futura di Google AdSense. La regressione e la relativa risoluzione sono tracciate in [GitHub #5](https://github.com/pinguz79/MultiPurposeServer/issues/5).
- **Test di non regressione:** uno smoke test di produzione read-only verifica su home, collection e photo album la presenza del bootstrap Iubenda, della configurazione TCF, del footer condiviso, della Privacy Policy e del controllo per riaprire le preferenze. Il test è opt-in e indipendente dallo scenario che rigenera le cache.
- **Completato:** 2026-08-09. Banner verificato su desktop e mobile, con e senza consenso preesistente; layout, CMP e collegamenti privacy hanno superato i controlli tecnici. Il successivo feedback della modella ha confermato una resa pubblicitaria positiva senza segnalare problemi di fruibilità.

### BL-0007 — Curare la presentazione dei link album sui social

- **Tipo:** Improvement
- **Area:** Portfolio.Web
- **Stato:** Completato
- **Priorità:** Alta
- **Registrato:** 2026-08-08

Rendere stabile e riconoscibile la presentazione di un album quando il relativo URL viene inserito manualmente nella descrizione di un contenuto social.

- **Criteri di accettazione:** l'album ha URL pubblico e stabile, titolo e descrizione coerenti, URL canonica e metadati Open Graph essenziali; l'anteprima del link viene verificata almeno sul canale social scelto per la pubblicazione imminente.
- **Nota:** il risultato non richiede un comando di condivisione integrato in Portfolio.Web.
- **Completato:** 2026-08-08
- **Esito:** Portfolio.Web espone titolo, descrizione, canonical, Open Graph e Twitter Card coerenti, utilizzando come immagine la prima fotografia o la cover disponibile senza richieste API aggiuntive. Il componente di sharing usa la stessa fonte editoriale. Lo smoke test di produzione è superato e l'anteprima dell'album è stata verificata su Facebook.

### BL-0008 — Completare lo sharing automatico da Portfolio.Web

- **Tipo:** Improvement
- **Area:** Portfolio.Web
- **Stato:** Aperto
- **Priorità:** Bassa
- **Milestone:** Migliorie UI e UX
- **Registrato:** 2026-08-08

Completare e rendere uniforme il meccanismo, oggi parziale, di condivisione diretta di album e fotografie da Portfolio.Web.

- **Workaround:** la persona riceve la fotografia, la pubblica autonomamente e inserisce manualmente nella descrizione il link pubblico dell'album.
- **Motivazione della priorità:** il workaround è macchinoso ma consente comunque il flusso editoriale previsto; lo sharing automatico non è necessario per la milestone corrente.
- **Verifica 2026-08-08:** condividendo su Facebook una singola fotografia, l'URL conserva `photoId` ma l'anteprima risultante coincide con quella dell'album: titolo, descrizione e immagine Open Graph non rappresentano ancora la fotografia selezionata. La futura implementazione deve distinguere l'URL dell'oggetto social dalla canonical SEO dell'album e produrre metadati specifici per la fotografia.

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

### BL-0014 — Valorizzare ModelBook.Cloud nel footer di Portfolio.Web

- **Tipo:** Improvement
- **Area:** Portfolio.Web / ModelBook
- **Stato:** Aperto
- **Priorità:** Non assegnata
- **Milestone:** Migliorie UI e UX
- **Registrato:** 2026-08-08

Sostituire il footer tecnico `Portfolio.Web` con il messaggio editoriale `Powered by ModelBook.Cloud`, rendendolo in futuro un collegamento attivo al frontend ModelBook quando sarà disponibile.

- **Obiettivo:** rendere visibile la relazione fra i progetti e creare una sinergia editoriale fra Portfolio e ModelBook senza introdurre dipendenze applicative tra i domini.
- **Criteri di accettazione:** il footer è coerente con l'identità visiva di Portfolio.Web; il testo è presente su tutte le pagine; il link viene attivato soltanto quando esiste una destinazione ModelBook pubblica e stabile.

### BL-0015 — Attivare Google AdSense su Portfolio.Web

- **Tipo:** Feature
- **Area:** Portfolio.Web / Monetizzazione
- **Stato:** Completato — revisione Google pendente
- **Priorità:** Alta
- **Registrato:** 2026-08-08

Integrare Google AdSense attraverso il collegamento già predisposto nel pannello Altervista, tentando di valorizzare il traffico imminente oltre al formato Altervista 300x250 già attivo.

- **Prerequisiti verificabili:** completare le attività richieste dall'account AdSense, verificare proprietà e collegamento del sito, stato `ads.txt`, accessibilità del crawler e configurazione della CMP certificata. La policy Iubenda deve essere rivalutata includendo gli eventuali nuovi servizi effettivamente attivati.
- **Criteri di accettazione tecnici:** il codice o componente previsto dal flusso Altervista/AdSense è presente nelle pagine stabilite; non duplica né rende ingannevoli i banner esistenti; layout, consenso e navigazione restano corretti su desktop e mobile; uno smoke test protegge gli elementi applicativi sotto il controllo del progetto.
- **Criterio esterno:** l'erogazione effettiva richiede che Google completi la revisione e assegni al sito lo stato `Ready`. La documentazione ufficiale indica normalmente alcuni giorni, con possibili tempi di 2–4 settimane.
- **Condizione di milestone:** prima della chiusura viene registrato uno dei due esiti: AdSense attivo e verificato, oppure preparazione completata ma approvazione Google ancora pendente, con rinvio esplicito dell'attivazione senza bloccare indefinitamente la milestone.
- **Esito:** l'11 agosto 2026 la preparazione tecnica e editoriale è stata completata e la nuova revisione AdSense è stata richiesta. Il sito espone contenuti editoriali sostanziali, sitemap e metadati coerenti, CMP e policy verificate e annunci esclusivamente sulle pagine classificate `Standard`. L'assegnazione dello stato `Ready` rimane un esito esterno pendente e non blocca la chiusura della milestone tecnica.

### BL-0016 — Sostituire Swagger UI con Scalar

- **Tipo:** Improvement
- **Area:** MPS / API documentation
- **Stato:** Aperto
- **Priorità:** Bassa
- **Registrato:** 2026-08-09

Sostituire l'attuale esposizione interattiva basata su Swagger UI e Swashbuckle con Scalar, mantenendo una specifica OpenAPI valida e un'esperienza di consultazione e prova delle API adatta allo sviluppo e alla diagnostica.

- **Motivazione:** ridurre l'accoppiamento con la generazione Swagger corrente e gli attriti di compatibilità fra `Swashbuckle.AspNetCore` e `Microsoft.OpenApi`, emersi durante l'introduzione degli health check.
- **Vincolo:** la migrazione non deve modificare route, contratti o comportamento delle API e deve rispettare `EnableSwagger` o una configurazione equivalente per l'esposizione della documentazione.
- **Criteri di accettazione:** Scalar espone correttamente tutti gli endpoint documentabili; la specifica OpenAPI viene generata senza errori; autenticazione tramite API key e prova delle chiamate restano disponibili; health check ed endpoint infrastrutturali non pertinenti sono esclusi dalla documentazione applicativa; i riferimenti operativi a `/swagger` vengono aggiornati.

### BL-0017 — Valutare la condivisione degli album su Instagram

- **Tipo:** Improvement
- **Area:** Portfolio.Web / Condivisione social
- **Stato:** Aperto
- **Priorità:** Media
- **Milestone:** Migliorie UI e UX
- **Registrato:** 2026-08-09
- **Origine:** feedback utente sulla milestone del traffico fotografico imminente

Valutare e rendere esplicito il flusso di condivisione di un album verso Instagram, oggi assente dalle opzioni presentate da Portfolio.Web.

- **Nota:** Instagram non offre necessariamente un equivalente Web diretto dei normali share endpoint; l'analisi deve distinguere condivisione nativa tramite Web Share API, apertura dell'app, copia del link e pubblicazione manuale.
- **Criteri di accettazione:** il flusso scelto funziona sui dispositivi supportati, non presenta azioni ingannevoli quando Instagram non è disponibile e conserva il workaround di copia del link.

### BL-0018 — Evitare il taglio dei volti nelle thumbnail

- **Tipo:** Bug UX
- **Area:** Portfolio.Api / Media, Portfolio.Web
- **Stato:** Completato
- **Priorità:** Alta
- **Registrato:** 2026-08-09
- **Origine:** feedback utente sulla milestone del traffico fotografico imminente

Le thumbnail di numerose fotografie applicano un ritaglio che tronca la testa del soggetto. Verificare le impostazioni ImageMagick usate nella generazione delle miniature e il rapporto fra dimensioni server e contenitore CSS lato Portfolio.Web.

- **Ipotesi iniziali:** crop centrale non adatto ai ritratti verticali, geometria `cover` applicata durante il resize oppure ulteriore ritaglio CSS tramite `object-fit: cover`.
- **Criteri di accettazione:** i volti restano visibili nelle thumbnail rappresentative orizzontali e verticali; il layout delle card rimane uniforme; cache e rigenerazione delle miniature sono gestite esplicitamente; test mirati proteggono dimensioni e modalità di resize concordate.
- **Implementazione:** le cover generate da sorgenti verticali usano un crop ancorato in alto; le sorgenti orizzontali mantengono il crop centrale. La cache cover è versionata nella cartella `covers-top-v1`, così il primo accesso successivo al deploy rigenera automaticamente le immagini senza richiedere la cancellazione manuale della cache precedente. BL-0019 conserva separatamente l'eventuale smart crop basato su riconoscimento del soggetto.
- **Esito:** deploy e verifica visiva completati l'11 agosto 2026. Il crop alto rende correttamente circa il 90% delle cover precedentemente problematiche; i casi residui dipendono dalla composizione specifica della fotografia e vengono demandati a BL-0019. Test ImageMagick e suite Portfolio.Api verdi.

### BL-0019 — Introdurre uno smart crop locale per le cover

- **Tipo:** Improvement
- **Area:** Portfolio.Api / Media
- **Stato:** Aperto
- **Priorità:** Media
- **Milestone:** Migliorie UI e UX
- **Registrato:** 2026-08-09
- **Origine:** approfondimento di `BL-0018`

Evolvere la generazione delle cover con un algoritmo locale capace di individuare automaticamente il ritaglio più significativo per ogni fotografia, privilegiando i volti e, in loro assenza, il soggetto o le aree visivamente salienti.

- **Strategia candidata:** rilevare localmente bounding box e landmark dei volti tramite un modello ONNX/OpenCV, calcolare un rettangolo compatibile con il rapporto richiesto e demandare a ImageMagick il resize e il crop effettivi.
- **Fallback:** quando non viene rilevato un soggetto affidabile, applicare la regola geometrica deterministica prevista da `BL-0018`, con gravity differenziata per orientamento.
- **Vincoli:** nessun invio delle fotografie a servizi cloud; elaborazione compatibile con l'infrastruttura di MPS; risultato memorizzabile nella cache; possibilità futura di salvare un punto focale manuale senza renderlo requisito iniziale.
- **Criteri di accettazione:** il crop conserva correttamente i volti nei casi rappresentativi con uno o più soggetti; gestisce in modo prevedibile immagini prive di volti; il fallback è verificato; prestazioni e dipendenze del modello sono misurate; la rigenerazione delle cover rimane esplicita e controllabile.

### BL-0020 — Correggere la creazione duplicata di album nella root

- **Tipo:** Bug
- **Area:** Portfolio.Api / Sincronizzazione album
- **Stato:** Aperto
- **Priorità:** Media
- **Registrato:** 2026-08-09
- **Origine:** creazione dell'album `Sunset @ Paraggi`

Durante la creazione di `sunset-at-paraggi` sotto l'album corretto, il sistema ha creato anche una gallery omonima nella root del Portfolio.

- **Indagine:** ricostruire il flusso completo fra API, filesystem e sincronizzazione; stabilire in quale passaggio venga creata la directory o l'entità root; verificare l'eventuale relazione con path esplicito, cache o riavvio di MPS.
- **Riproduzione:** costruire un caso automatizzato che crei un album annidato e dimostri la comparsa indesiderata del duplicato root prima della correzione.
- **Correzione:** garantire che la creazione e la successiva sincronizzazione conservino esclusivamente il parent richiesto e non interpretino lo slug annidato come directory root autonoma.
- **Bonifica:** definire ed eseguire una rimozione sicura della gallery root errata, verificando preventivamente entità database, directory fisica, eventuali fotografie, route e cache coinvolte.
- **Strumento di bonifica:** è stata implementata una cancellazione amministrativa limitata agli Album completamente vuoti. L'operazione rifiuta Album con children, Photo o contenuti sul filesystem e ripristina la directory se la cancellazione persistente fallisce. Questo consente di rimuovere in sicurezza la Gallery root errata e la Gallery inutilizzata `Temporary`, ma non corregge la causa che ha generato il duplicato.
- **Bonifica eseguita:** il 2026-08-09 le Gallery root vuote `Temporary` e `sunset-at-paraggi` sono state eliminate tramite l'API amministrativa. Dopo l'invalidazione delle cache, entrambi i vecchi URL restituiscono `404`, la home non le espone, la sitemap contiene 110 URL e gli audit di discovery e metadati risultano positivi. `BL-0020` rimane aperto fino alla riproduzione e correzione della causa originaria.
- **Criteri di accettazione:** il difetto è riprodotto da un test di non regressione; la causa è documentata; la creazione annidata non genera elementi root; la gallery errata viene eliminata senza coinvolgere l'album corretto; audit e health check finali risultano sani.

### BL-0021 — Valutare un modulo contatti interno per Portfolio.Web

- **Tipo:** Improvement
- **Area:** Portfolio.Web / Contatti
- **Stato:** Da definire
- **Priorità:** Bassa
- **Registrato:** 2026-08-09
- **Origine:** progettazione della pagina `Chi sono`

Valutare l'introduzione di un modulo interno con cui modelle, modelli e collaboratori possano proporre commissioni o progetti creativi direttamente da Portfolio.Web.

- **Situazione iniziale:** la pagina `Chi sono` privilegia collegamenti diretti a Instagram, Facebook e WhatsApp.
- **Aspetti da definire:** campi e categorie della richiesta, gestione e destinazione dei messaggi, protezione antispam, rate limiting, trattamento dei dati personali, aggiornamento della privacy policy e conferma di ricezione.
- **Criteri di accettazione preliminari:** il modulo viene introdotto soltanto se offre un vantaggio concreto rispetto ai canali esterni; non espone indirizzi o servizi a spam; presenta informative e consenso adeguati; restituisce un esito affidabile senza perdere le richieste.

### BL-0022 — Verificare gli Album virtuali per la doppia navigazione persona e shooting

- **Tipo:** Architectural investigation
- **Area:** Portfolio / Album virtuali
- **Stato:** Da definire
- **Priorità:** Bassa
- **Registrato:** 2026-08-09
- **Origine:** audit dei metadati duplicati degli Album

Verificare durante la modellazione degli Album virtuali che lo stesso insieme di fotografie possa essere raggiunto attraverso due tassonomie complementari senza duplicare Photo o Album fisici:

- `Modella → Shooting`, per consultare tutti i lavori della stessa persona;
- `Shooting → Modelle`, per consultare tutte le persone coinvolte nello stesso evento o progetto, come una sfilata.

L'ipotesi candidata mantiene l'Album fisico nel percorso canonico `Modella → Shooting` e costruisce il secondo percorso mediante Album virtuali, per esempio `Gallery → Sfilata Katana (virtuale) → Modella (virtuale) → Album fisico`. Questa struttura sembra compatibile con le regole attuali, che consentono più navigation path verso lo stesso Album fisico e vietano soltanto collegamenti alternativi diretti `Fisico → Fisico`.

- **Aspetti da verificare:** identità condivisa dell'Album fisico; breadcrumb dipendente dal percorso richiesto; policy di accesso lungo la catena virtuale; cover e conteggi; comportamento delle route; assenza di duplicazione delle fotografie; gestione del caso in cui uno shooting coinvolga molte persone.
- **Evidenza storica disponibile:** l'export del vecchio Portfolio ZenPhoto conserva file `.alb` usati per costruire raccolte e percorsi virtuali, tra cui collegamenti relativi a `Miss Villetta 2023`, `Sfilata Katana` e `RS Fashion Group`. Questi file devono essere preservati e analizzati come fixture e casi d'uso reali durante la modellazione; non sono automaticamente autorevoli per il nuovo formato dati o per le regole di dominio.
- **Vincolo documentale:** questa voce non modifica le regole autorevoli correnti di `Portfolio/Domain.md`. Eventuali variazioni verranno decise e documentate soltanto durante la modellazione tecnica degli Album virtuali.
- **Criteri di accettazione preliminari:** dimostrare con esempi e modello dati che entrambe le navigazioni conducono alle stesse risorse fisiche; stabilire se le regole esistenti siano sufficienti o richiedano un ADR; definire route, breadcrumb, accesso e lifecycle dei link prima dell'implementazione.

### BL-0023 — Raccontare la storia del progetto FairyTales 2021

- **Tipo:** Content
- **Area:** Portfolio.Web / Contenuti editoriali
- **Stato:** Completato
- **Priorità:** Alta
- **Registrato:** 2026-08-10
- **Origine:** redazione delle descrizioni degli Album

Realizzare una pagina editoriale dedicata alla storia di FairyTales 2021, capace di raccontare il progetto oltre le brevi descrizioni della galleria: l'ideazione delle tredici protagoniste, la lunga preparazione di set e costumi, la versione speciale interpretata interamente da Camilla, le locandine pubblicate progressivamente per creare attesa e la trasformazione manuale delle fotografie in cartoline in stile cartoon.

La pagina costituisce il primo articolo di una sezione editoriale `Dietro le quinte`, realizzata inizialmente come mini-CMS file-based: un unico motore gestisce indice, route, template, metadati e sitemap, mentre ogni nuovo articolo aggiunge contenuto senza richiedere lo sviluppo di una pagina applicativa dedicata.

Il racconto dovrà conservare anche il contesto storico delle presentazioni programmate nei locali, con la partecipazione delle modelle e le cartoline autografate, interrotte dopo il primo appuntamento dall'entrata in vigore del secondo lockdown per COVID-19.

- **Criteri di accettazione preliminari:** testo concordato con l'autore; collegamento dalla galleria FairyTales senza appesantire le descrizioni degli Album; uso di fotografie e materiali promozionali coerenti; metadati editoriali e resa responsive; distinzione chiara tra la versione corale e quella dedicata a Camilla.
- **Esito:** il mini-CMS file-based è stato pubblicato con indice, route, template, metadati SEO/social, dati strutturati, sitemap e backlink dagli Album. Sono online gli articoli dedicati a FairyTales 2021, PhotographerSharing, Sfilata Katana e progetto Calendari; RS Fashion Group e Mermaid in the Night restano bozze non raggiungibili. Smoke test di produzione superati l'11 agosto 2026.

### BL-0024 — Correggere Blue de Genes in Bleu de Genes

- **Tipo:** Data correction
- **Area:** Portfolio / Album e filesystem
- **Stato:** Da pianificare
- **Priorità:** Medio-bassa
- **Registrato:** 2026-08-10
- **Origine:** redazione delle descrizioni dei calendari 2024

Correggere da `Blue de Genes` a `Bleu de Genes` il nome e il path dei due Album del calendario 2024, ripristinando la grafia francese corretta del nome storico attribuito al tessuto denim genovese.

La bonifica deve essere pianificata come modifica strutturale coordinata e non come semplice aggiornamento editoriale: coinvolge le folder sul filesystem, i nomi e i path persistiti nel database, le route pubbliche e le cache di Portfolio.Web.

- **Criteri di accettazione preliminari:** individuare tutti i riferimenti alla grafia errata; definire l'ordine sicuro di rename tra filesystem e database; valutare redirect o compatibilità per gli URL precedenti; rigenerare le cache; verificare navigazione, sitemap, canonical e assenza di duplicati o route residue.

### BL-0025 — Classificare le fotografie per la compatibilità pubblicitaria

- **Tipo:** Feature
- **Area:** Portfolio.Api / Portfolio.Web / Monetizzazione
- **Stato:** Completato
- **Priorità:** Alta
- **Registrato:** 2026-08-10
- **Origine:** audit visivo preliminare per `BL-0015`

Persistire sulla singola Photo una classificazione editoriale che distingua i contenuti compatibili con la pubblicità da quelli soggetti a restrizioni. La classificazione degli Album non viene persistita: è derivata dai contenuti visivi direttamente esposti e può essere memorizzata nelle rappresentazioni di cache.

Gli stati derivati degli Album sono:

- `Standard`: tutti i contenuti direttamente esposti sono standard;
- `PartiallyRestricted`: sono presenti contenuti standard e restricted;
- `Restricted`: tutti i contenuti direttamente esposti sono restricted.

Per un PhotoAlbum i contenuti direttamente esposti sono le Photo figlie. Per Collection e Gallery sono le cover dei figli diretti. Un figlio `PartiallyRestricted` fornisce al parent una cover standard; un figlio `Restricted`, non avendo alternative standard, fornisce necessariamente una cover restricted. La propagazione verso l'alto avviene quindi soltanto quando il contenuto restricted viene effettivamente esposto dalla cover, non per la sola presenza in un discendente.

- **Regola cover:** scegliere casualmente soltanto fra candidati standard quando ne esiste almeno uno; usare candidati restricted esclusivamente come fallback quando l'Album non possiede alternative standard; un Album vuoto rimane `Standard` e usa il placeholder.
- **Regola pubblicitaria:** consentire annunci esclusivamente nelle pagine classificate `Standard`; `PartiallyRestricted` e `Restricted` rimangono pubbliche e navigabili ma non espongono pubblicità.
- **Audit iniziale:** classificare almeno le Photo degli Album emersi dal controllo AdSense, con priorità per `Calendari/2022/Christal2022`; verificare poi `Fiore2022`, `GraceCats2022`, `Germana-2023`, `Annalisa-s-Secrets-2025`, `SexySunset` e gli Album della sfilata `Dolcenera & Il Sogno`.
- **Criteri di accettazione:** classificazione Photo persistita e gestibile; stato derivato restituito dalle API per Photo e Album; cover conformi alla regola di fallback; annunci assenti nelle pagine non standard; cache invalidata quando cambia una classificazione; test unitari e di integrazione coprono derivazione, propagazione, Album vuoti e rendering pubblicitario.
- **Relazione con AdSense:** la feature è un prerequisito applicativo di `BL-0015` prima della nuova richiesta di revisione.
- **Esito:** il 2026-08-10 sono state classificate 62 Photo in produzione; gli otto PhotoAlbum coinvolti risultano `PartiallyRestricted`, usano cover standard e non espongono pubblicità. Update puntuale e bulk, invalidazione cache e smoke test desktop/mobile hanno dato esito positivo.

### BL-0026 — Revisionare la classificazione fotografica di un sottoalbero Portfolio

- **Tipo:** Feature
- **Area:** Portfolio.Admin / Moderazione editoriale
- **Stato:** Da definire
- **Priorità:** Bassa
- **Registrato:** 2026-08-10
- **Origine:** evoluzione operativa di `BL-0025`

Realizzare uno strumento amministrativo che permetta di revisionare sistematicamente la classificazione delle Photo partendo dall'intero Portfolio oppure da qualunque Gallery, Collection o PhotoAlbum scelto come radice dell'attività.

Il processo deve attraversare tutte le Photo raggiungibili nel sottoalbero selezionato, mostrare il contenuto necessario alla valutazione e consentire di confermare o modificare esplicitamente la classificazione. La revisione deve poter essere interrotta e ripresa senza perdere l'avanzamento e deve distinguere Photo ancora da esaminare, già confermate e modificate durante la sessione.

- **Vincolo:** l'eventuale supporto automatico o AI può suggerire una classificazione e ordinare i casi dubbi, ma non sostituisce la decisione editoriale esplicita dell'amministratore.
- **Effetti:** ogni modifica applicata deve usare il normale flusso di aggiornamento previsto da `BL-0025`, invalidando stati Album, cover e cache interessati.
- **Aspetti da definire:** client amministrativo iniziale; persistenza o ricostruzione dell'avanzamento; filtri per stato corrente e livello di confidenza; modalità bulk; audit delle modifiche; gestione di nuove Photo aggiunte dopo una revisione conclusa.
- **Criteri di accettazione preliminari:** selezione di una radice arbitraria; enumerazione completa e senza duplicati delle Photo discendenti; classificazione manuale verificabile; avanzamento riprendibile; riepilogo finale del sottoalbero; ricalcolo coerente degli Album e delle cover coinvolte.

### BL-0027 — Migrare il mini-CMS editoriale di Portfolio.Web su database

- **Tipo:** Feature
- **Area:** Portfolio.Web / Contenuti editoriali
- **Stato:** Da definire
- **Priorità:** Bassa
- **Registrato:** 2026-08-10
- **Origine:** progettazione della sezione editoriale `Dietro le quinte`

Evolvere il mini-CMS inizialmente file-based verso una persistenza su database, mantenendo stabili le route pubbliche, il template degli articoli e i metadati SEO/social.

- **Situazione iniziale prevista:** gli articoli sono contenuti versionati nel repository e renderizzati da un unico motore; `FairyTales 2021` costituisce il primo contenuto editoriale.
- **Obiettivo futuro:** consentire creazione, modifica, anteprima, pubblicazione e archiviazione senza distribuire nuovamente Portfolio.Web.
- **Aspetti da definire:** modello dati; versionamento e revisioni; gestione delle immagini; bozze e pubblicazione programmata; autorizzazioni editoriali; sanitizzazione del contenuto; importazione degli articoli file-based esistenti; backup e disaster recovery.
- **Criteri di accettazione preliminari:** migrazione senza variazione degli URL pubblici; contenuti e metadati preservati; bozze non raggiungibili pubblicamente; operazioni editoriali protette; rollback e backup verificati.

### BL-0028 — Gestire selezione, crediti e acquisto di fotografie e calendari

- **Tipo:** Feature / Product discovery
- **Area:** Portfolio / Account / E-commerce
- **Stato:** Da definire
- **Priorità:** Media
- **Registrato:** 2026-08-10
- **Origine:** evoluzione del flusso di selezione delle Photo

Consentire alle persone ritratte di scegliere le fotografie comprese nello shooting e acquistare eventuali fotografie aggiuntive. Il profilo può ricevere un credito iniziale, espresso come numero di Photo selezionabili, derivante dal servizio acquistato o dagli accordi TF.

- **Casi d'uso iniziali:** PhotographerSharing con un minimo di Photo comprese; TF con quantità concordata inferiore agli scatti disponibili; acquisto di Photo oltre il credito residuo; consultazione di selezioni, credito utilizzato e credito disponibile.
- **Calendari:** valutare la vendita del calendario dell'anno corrente e delle sole copie residue realmente disponibili degli anni precedenti; evitare ristampe antieconomiche dei calendari storici.
- **Aspetti da definire:** titolarità del credito; listini e promozioni; prenotazione o consumo atomico del credito; pagamenti, rimborsi e ricevute; disponibilità di magazzino; consegna digitale o fisica; autorizzazioni sulle Photo; privacy; fiscalità; scadenza dei crediti; gestione amministrativa delle eccezioni.
- **Vincolo architetturale:** selezione, credito, ordine, pagamento e consegna sono concetti distinti; il pagamento esterno deve essere trattato come attore dell'operazione applicativa atomica e non come semplice transazione database.
- **Criteri di accettazione preliminari:** il credito non può essere consumato due volte; la selezione distingue elementi compresi ed eccedenti; prezzo e disponibilità sono confermati prima del pagamento; ordini e consegne sono auditabili; un errore non produce addebiti o diritti di download incoerenti.

### BL-0029 — Introdurre piani, abbonamenti e token in ModelBook

- **Tipo:** Feature / Product discovery
- **Area:** ModelBook / Monetizzazione
- **Stato:** Da definire
- **Priorità:** Media
- **Registrato:** 2026-08-10
- **Origine:** definizione del modello commerciale ModelBook

Mantenere ModelBook pienamente utilizzabile senza pagamento, affiancando al piano gratuito due o tre piani tariffari con limiti progressivamente più permissivi e funzionalità premium.

- **Possibili differenziatori:** ricerca avanzata; quantità di Photo pubblicabili; numero di messaggi mensili; ulteriori capacità da definire durante la progettazione del dominio.
- **Token:** messaggistica e annunci in bacheca possono consumare token mensili inclusi nel piano. I token aggiuntivi devono poter essere acquistati indipendentemente dal piano sottoscritto.
- **Piano lifetime:** valutare un piano a pagamento una tantum privo dei normali vincoli ricorrenti, definendone con precisione sostenibilità, limiti e condizioni nel tempo.
- **Aspetti da definire:** matrice piani/feature; rinnovi; upgrade e downgrade; rollover e scadenza dei token; acquisti una tantum; rimborsi; abuso e spam; sospensione account; evoluzione dei prezzi; trattamento degli utenti lifetime quando nascono nuove feature o costi operativi.
- **Criteri di accettazione preliminari:** il piano gratuito resta concretamente fruibile; limiti e consumi sono trasparenti; token e diritti non possono essere duplicati; cambio piano e rinnovo hanno comportamento deterministico; la sicurezza non dipende dal client; pagamenti e abilitazioni sono auditabili.

### BL-0030 — Bonificare il titolo Mermaid in the Night prima della pubblicazione

- **Tipo:** Data correction / Editorial workflow
- **Area:** Catalogo Lightroom / Portfolio / Mini-CMS
- **Stato:** Da pianificare
- **Priorità:** Media
- **Registrato:** 2026-08-10
- **Origine:** preparazione della bozza editoriale `Mermaid in the Night`

Correggere la grafia storica errata `Marmaid in the Night` adottando ovunque il titolo inglese corretto `Mermaid in the Night` prima di pubblicare articolo e Album.

- **Ordine vincolante:** rinominare per primo il progetto, le folder e gli eventuali file nel catalogo Lightroom; verificare che il catalogo non riporti elementi mancanti; aggiornare successivamente export e folder destinate a Portfolio; creare o rinominare l'Album con path `mermaid-in-the-night`; completare infine collegamenti editoriali, route e cache.
- **Vincolo:** non pubblicare la bozza e non creare collegamenti pubblici finché catalogo Lightroom, filesystem esportato e Portfolio non usano la stessa denominazione.
- **Criteri di accettazione preliminari:** nessun riferimento residuo a `Marmaid`; catalogo Lightroom integro; file e folder raggiungibili; Album navigabile con titolo e path corretti; articolo collegato all'Album; sitemap, canonical e cache coerenti.

### BL-0031 — Migliorare il ritaglio delle copertine nell'elenco degli articoli

- **Tipo:** UX improvement
- **Area:** Portfolio.Web / Mini-CMS / Responsive design
- **Stato:** Da pianificare
- **Priorità:** Media
- **Milestone:** Migliorie UI e UX
- **Registrato:** 2026-08-10
- **Origine:** verifica visiva dell'indice della sezione editoriale

Rivedere il rendering delle immagini di copertina nelle card dell'elenco articoli: il ritaglio attuale può escludere porzioni troppo importanti della fotografia e produrre anteprime poco rappresentative del contenuto.

La soluzione candidata consiste nell'adottare un comportamento analogo a quello già utilizzato per le card degli Album, preservando il più possibile la composizione originale e mantenendo una resa coerente su desktop e dispositivi mobili. Prima dell'implementazione verificare se il comportamento possa essere condiviso a livello CSS oppure se articoli e Album richiedano proporzioni distinte.

- **Aspetti da verificare:** `object-fit` e `object-position`; rapporto d'aspetto delle card; fotografie verticali e orizzontali; altezza uniforme dell'elenco; comportamento responsive; assenza di layout shift; eventuale futura focal area configurabile per singola copertina.
- **Criteri di accettazione preliminari:** volto o soggetto principale non tagliato nei casi rappresentativi; card ordinate e uniformi; resa verificata con copertine verticali e orizzontali su desktop e mobile; nessuna regressione sulle card degli Album.

### BL-0032 — Raccontare la nascita del calendario Germana 2023

- **Tipo:** Content
- **Area:** Portfolio.Web / Contenuti editoriali
- **Stato:** Da pianificare
- **Priorità:** Bassa
- **Milestone:** Migliorie UI e UX
- **Registrato:** 2026-08-11
- **Origine:** intervista editoriale sulla storia del progetto Calendari

Realizzare un articolo dedicato a Germana 2023, ampliando la breve descrizione dell'Album con la storia dei tre set confluiti nel calendario e, in particolare, della sessione nell'abbazia abbandonata nei boschi vicino a Scarpino.

Il racconto dovrà includere la ricerca della location, la scelta del sentiero più lungo ma tracciato, le difficoltà del rientro al buio con attrezzatura e valigia degli outfit, i telefoni quasi scarichi e la successiva decisione di completare il materiale con un set alla Torretta di Quezzi e uno proveniente da un precedente PhotographerSharing.

- **Vincolo editoriale:** spiegare con discrezione che il calendario non fu promosso né stampato per ragioni personali e professionali della protagonista, senza pubblicare dettagli identificativi relativi al suo contesto lavorativo.
- **Criteri di accettazione preliminari:** testo concordato con l'autore; collegamento a `Calendari/2023/Germana-2023`; distinzione chiara fra i tre set; copertina reale; metadati SEO/social; stato `draft` fino alla revisione editoriale finale.

### Promemoria — Idea futura da recuperare

Il 2026-08-10, insieme alle idee su e-commerce Portfolio e monetizzazione ModelBook, era emersa una terza idea che non è stato possibile ricostruire. Il promemoria rimane intenzionalmente visibile finché l'idea non viene ricordata e trasformata in una voce di backlog completa oppure esplicitamente eliminata.

---

## 7. Elementi completati o annullati

`BL-0001`, `BL-0002`, `BL-0006`, `BL-0007`, `BL-0009` e `BL-0012` sono completati e rimangono nelle rispettive sezioni per conservarne contesto, verifiche ed esito.

Gli elementi completati o annullati conservano identificatore ed esito. Se il documento diventerà troppo esteso potranno essere trasferiti in un archivio senza riutilizzarne gli ID.

---

## Riferimenti

- [Roadmap](Roadmap.md)
- [Visione](Vision.md)
- [Project Status](../ProjectStatus.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
- [Portfolio Domain](../Portfolio/Domain.md)
