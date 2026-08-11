# Playbook di Ingegneria di MultiPurposeServer

## 1. Scopo

Questo Playbook definisce come viene condotto il lavoro su MultiPurposeServer.

Si applica a sviluppatori, assistenti AI e strumenti di generazione automatica. Stabilisce principi operativi, flusso di lavoro, gestione dei cambiamenti, debito tecnico, documentazione e criteri di completamento.

Non descrive l'architettura, lo stile C#, i livelli di testing o le procedure dettagliate di code review. Tali responsabilità appartengono ai documenti specialistici collegati.

In caso di contrasto, la documentazione architetturale autorevole e gli ADR prevalgono sul Playbook.

---

## 2. Valore normativo

Le indicazioni utilizzano una terminologia coerente:

- **deve / non deve** indica un requisito necessario per considerare il lavoro conforme;
- **dovrebbe / normalmente** indica la pratica predefinita, derogabile con una motivazione concreta;
- **può** indica un'opzione ammessa e non una preferenza.

Una deviazione da un requisito deve essere esplicita. Richiede un ADR soltanto quando introduce una scelta architetturale significativa, duratura o non ovvia.

---

## 3. Principi operativi

### 3.1 Leggibilità

Il risultato deve essere comprensibile senza ricostruire comportamenti impliciti o intenzioni non documentate.

- Preferire soluzioni semplici ed esplicite.
- Evitare complessità non necessaria.
- Privilegiare la manutenibilità rispetto all'ingegnosità.
- Conservare nomi e responsabilità riconoscibili.

### 3.2 Coerenza

La coerenza del progetto ha più valore dell'introduzione occasionale di un nuovo pattern.

- Estendere soluzioni consolidate quando rappresentano ancora correttamente il problema.
- Introdurre un nuovo approccio quando risolve un limite concreto.
- Non forzare uniformità quando i concetti hanno semantiche differenti.

### 3.3 Responsabilità

Ogni artefatto deve possedere una responsabilità chiara: codice, test, documento, progetto o servizio.

- Individuare il proprietario del problema prima di modificare il sistema.
- Evitare sovrapposizioni e scorciatoie tra confini.
- Introdurre un nuovo componente soltanto quando esiste una responsabilità reale.

### 3.4 Evoluzione incrementale

MPS evolve tramite passi piccoli e verificabili, non mediante riscritture prive di necessità concreta.

- Preferire evoluzione e refactoring incrementali.
- Accettare temporaneamente duplicazione quando il concetto non è ancora compreso.
- Estrarre astrazioni soltanto dopo che una responsabilità stabile è emersa.
- Considerare la sostituzione completa come alternativa da motivare.

### 3.5 Architettura e documentazione

L'implementazione deve seguire i confini architetturali consolidati. Una modifica che cambia conoscenza stabile deve aggiornare anche la documentazione proprietaria e, quando necessario, gli ADR.

---

## 4. Flusso di lavoro

### 4.1 Comprendere il problema

Prima di modificare il progetto è necessario:

- chiarire il risultato richiesto;
- identificare il perimetro autorizzato;
- leggere la documentazione autorevole pertinente;
- verificare lo stato reale di codice e repository;
- individuare responsabilità, vincoli e rischi coinvolti.

Le assunzioni che possono cambiare sostanzialmente il risultato devono essere rese esplicite.

### 4.2 Progettare la modifica

La progettazione deve precedere l'implementazione quando cambiano responsabilità, contratti o comportamento osservabile.

- Identificare il componente proprietario.
- Valutare l'impatto sui consumatori.
- Distinguere la modifica necessaria dalle evoluzioni future.
- Registrare le idee future senza ampliare automaticamente lo scope corrente.

### 4.3 Implementare per passi logici

Ogni passo dovrebbe avere un obiettivo riconoscibile e lasciare il lavoro in una condizione verificabile.

- Non combinare cambiamenti non correlati.
- Separare refactoring e modifica funzionale quando è possibile senza creare passaggi artificiali.
- Non costruire nuovo lavoro sopra errori non compresi.
- Non presentare come concluso un refactoring parziale.

### 4.4 Validare proporzionalmente al rischio

Ogni modifica deve essere verificata con controlli pertinenti alla sua natura e al suo impatto.

- Una modifica locale può richiedere test mirati.
- Una modifica trasversale richiede verifiche più ampie.
- Una modifica documentale richiede controlli di struttura, coerenza e collegamenti.
- Le verifiche non eseguite devono essere dichiarate esplicitamente.

I dettagli della strategia di testing appartengono a `TestingArchitecture.md`.

### 4.5 Chiudere il lavoro

Prima di passare a un nuovo obiettivo è necessario:

- completare lo scope concordato;
- verificare il risultato;
- aggiornare la documentazione pertinente;
- risolvere o registrare il debito emerso;
- rimuovere residui temporanei non necessari;
- comunicare con chiarezza risultato, verifiche e limiti residui.

### 4.6 Dichiarare gli artefatti di deployment di terze parti

Quando una modifica introduce o aggiorna un package, una libreria, un runtime nativo, un modello o un'altra risorsa di terze parti, il riepilogo di consegna deve elencare esplicitamente tutti i nuovi artefatti runtime da distribuire.

L'elenco deve essere ricavato dal contenuto effettivo del `publish` destinato all'ambiente di produzione e deve includere:

- assembly gestiti collocati nella root del publish, elencati singolarmente;
- modelli, file dati, DLL native e risorse caricati a runtime;
- eventuali sottocartelle come `runtimes` o `Models`, indicate come directory da copiare integralmente senza richiedere l'elenco dei singoli file contenuti;
- artefatti sostituiti quando viene aggiornata una dipendenza esistente;
- destinazione relativa di ciascun file quando non coincide con la root del deployment.

Il solo nome del package NuGet non è sufficiente. La data di ultima modifica non viene considerata un metodo affidabile per identificare dipendenze nuove o aggiornate. Se il publish non è stato generato o confrontato con la baseline precedente, il riepilogo deve dichiarare esplicitamente che l'elenco non è ancora verificato.

### 4.7 Preparare il publish Aruba

Quando una modifica deployabile coinvolge MPS o uno dei progetti server inclusi nel suo output, il workflow di consegna deve eseguire il publish dopo il completamento dei test pertinenti e prima di dichiarare la modifica pronta per il deploy.

Il publish usa il profilo versionato `MultiPurposeServer/Properties/PublishProfiles/Aruba.pubxml` tramite uno script che valida e ripulisce esclusivamente la cartella locale degli artefatti prima di rigenerarla:

```powershell
.\MultiPurposeServer\Publish-Aruba.ps1
```

Gli artefatti pronti per il trasferimento FTP si trovano in `MultiPurposeServer/bin/Publish/net10.0`.

Il riepilogo di consegna deve:

- dichiarare se il publish Aruba è riuscito;
- indicare il percorso completo della cartella prodotta;
- elencare le dipendenze esterne nuove o aggiornate secondo la sezione precedente;
- segnalare nuove sottocartelle runtime o modelli da trasferire integralmente;
- distinguere le modifiche a `appsettings.json` che devono essere integrate nella configurazione di produzione senza sovrascrivere valori locali o segreti;
- lasciare all'operatore il trasferimento FTP e l'eventuale riavvio dell'applicazione.

Il publish non è richiesto per modifiche esclusivamente documentali, per Portfolio.Web o quando il server non è coinvolto. In questi casi il riepilogo deve indicare che non esistono nuovi artefatti MPS da distribuire.

### 4.8 Preparare un deploy Aruba mirato

Il trasferimento FTPS non sincronizza l'intera root del server. Per ogni modifica server pronta al rilascio deve essere creato un piano in `Deployment/Aruba/Plans` che elenchi esplicitamente:

- file del publish da caricare o sostituire;
- sottocartelle del publish da trasferire integralmente;
- file applicativi remoti da eliminare;
- smoke test pubblici da eseguire dopo il deploy.

Il piano deve riflettere esclusivamente l'impatto della modifica consegnata ed essere revisionato prima dell'esecuzione. Database, log e media runtime non possono comparire nel piano. Configurazione di produzione, segreti e modelli possono invece essere sostituiti quando la modifica lo richiede.

La GitHub Action `Deploy MPS to Aruba` viene avviata manualmente. Prima valida sempre il piano in modalità non distruttiva; il trasferimento remoto avviene soltanto quando l'input `execute` è esplicitamente abilitato. La procedura completa e le protezioni applicate sono descritte in `Deployment/Aruba/README.md`.

### 4.9 Preparare un deploy Altervista mirato

Le modifiche deployabili di Portfolio.Web usano un piano versionato in `Deployment/Altervista/Plans`. Ogni piano elenca file singoli da caricare o eliminare mantenendo la corrispondenza fra la root locale `Applications/Portfolio/Portfolio.Web` e la root FTP di Altervista.

La GitHub Action `Deploy Portfolio.Web to Altervista` valida sempre la sintassi di tutti i file PHP e il contenuto del piano. Il trasferimento FTPS avviene soltanto con `execute` esplicitamente abilitato ed è seguito dai test di produzione in sola lettura. Artefatti di build, script database e log runtime non sono distribuibili. La procedura completa è descritta in `Deployment/Altervista/README.md`.

---

## 5. Commit

Ogni commit deve rappresentare un singolo cambiamento logico coerente.

- Il messaggio deve descrivere l'intento del cambiamento.
- Non devono essere incluse modifiche estranee.
- Il repository committato deve rimanere in uno stato coerente e comprensibile.
- Le verifiche pertinenti devono essere eseguite prima del commit.
- I commit WIP non appartengono al normale flusso del branch principale.

Durante il lavoro locale sono ammessi stati intermedi non compilabili, purché non vengano committati o presentati come completati.

Refactoring e comportamento funzionale dovrebbero essere separati quando la distinzione rende più chiara la revisione e la history.

---

## 6. Cambiamenti e refactoring

Un refactoring migliora la struttura senza cambiare intenzionalmente il comportamento osservabile.

- Deve preservare il contratto interessato.
- Deve essere sostenuto dalle verifiche appropriate.
- Dovrebbe procedere attraverso passi piccoli e reversibili.
- Non deve spostare responsabilità tra layer senza una motivazione architetturale.
- Deve aggiornare la documentazione quando consolida o modifica un concetto stabile.

Quando refactoring e modifica funzionale non sono separabili in modo sensato, la relazione deve essere evidente nella descrizione del lavoro e nelle verifiche.

Una riscrittura completa è ammessa quando l'evoluzione incrementale non è praticabile o avrebbe costi e rischi superiori. La motivazione deve essere esplicita.

---

## 7. Debito tecnico

Il debito tecnico è un compromesso o una carenza che aumenta rischio, costo di manutenzione o difficoltà evolutiva.

La fonte autorevole delle singole voci è [Technical Debt](TechnicalDebt.md).

### 7.1 Debito incontrato durante un'attività

Il debito direttamente adiacente può essere risolto nello scope corrente quando è chiaro, a basso rischio e non altera l'obiettivo.

Deve invece essere registrato e rinviato quando:

- richiede una nuova decisione;
- coinvolge componenti non pertinenti;
- necessita verifiche estese;
- ritarda il risultato concordato senza bloccarne la correttezza.

Il debito che impedisce di completare correttamente l'attività entra nello scope corrente. Una correzione laterale non deve essere nascosta in un commit con significato differente.

### 7.2 Fattori di priorità

La priorità deriva dalla valutazione congiunta di tre fattori:

1. **Impatto**: failure, regressioni, sicurezza, integrità dei dati, costo di manutenzione e limitazioni architetturali.
2. **Rapporto costi/benefici**: sforzo richiesto, semplificazione ottenuta, lavoro futuro risparmiato e frequenza del beneficio.
3. **Urgenza strategica**: relazione con feature o milestone imminenti, opportunità di intervenire nell'area e costo crescente del rinvio.

La priorità non rappresenta una stima dello sforzo.

### 7.3 Livelli

- **Critica**: richiede intervento immediato e può bloccare completamento o rilascio.
- **Alta**: deve essere pianificata nella milestone corrente o nella successiva.
- **Media**: appartiene al backlog pianificato e viene rivalutata quando cambia il contesto.
- **Bassa**: viene affrontata opportunisticamente o quando il costo cumulativo lo giustifica.

Una miglioria può avere priorità Alta quando offre un beneficio elevato, costa poco o abilita una feature urgente anche in assenza di rischio operativo.

### 7.4 Stati

- **Aperto**: riconosciuto e non ancora pianificato.
- **Pianificato**: assegnato a una milestone o attività.
- **In corso**: intervento attivo.
- **Risolto**: causa rimossa e verificata.
- **Accettato**: compromesso consapevole la cui rimozione non è attualmente giustificata.

La priorità deve essere rivalutata quando cambiano impatto, costo, roadmap o feature collegate.

---

## 8. Documentazione

La documentazione è parte del risultato ingegneristico e viene revisionata con la stessa attenzione del codice.

### 8.1 Fonti autorevoli

Ogni concetto deve avere un documento proprietario. Altri documenti possono descriverlo da una prospettiva differente o rimandare alla fonte, ma non devono duplicarne la responsabilità.

### 8.2 Conoscenza consolidata e lavoro in corso

- La documentazione autorevole descrive decisioni e comportamenti consolidati.
- `ProjectStatus.md`, roadmap e registri temporanei possono descrivere attività, ipotesi e punti aperti.
- Un'idea non approvata non entra nella documentazione autorevole.
- Gli ADR preservano la motivazione delle decisioni significative.
- La history del lavoro non appartiene alla documentazione corrente, salvo registri esplicitamente storici o temporanei.

### 8.3 Aggiornamento

Una modifica deve aggiornare contestualmente la documentazione quando cambia un contratto, una responsabilità, una procedura o altra conoscenza stabile.

I documenti devono procedere dal generale al particolare, rimanere autosufficienti nel proprio ambito e usare collegamenti per gli approfondimenti.

Le convenzioni editoriali e Markdown appartengono alla guida documentale dedicata.

---

## 9. Definition of Done

Un'attività è completata quando tutti i requisiti pertinenti sono soddisfatti:

- il risultato concordato è stato ottenuto interamente;
- lo scope è stato rispettato;
- non sono incluse modifiche estranee;
- le responsabilità architetturali sono preservate;
- le verifiche proporzionate al rischio sono state eseguite;
- codice e test sono aggiornati quando pertinenti;
- documentazione e ADR sono aggiornati quando cambia conoscenza stabile;
- il debito introdotto o scoperto è stato risolto oppure registrato;
- non rimangono passaggi incompleti presentati come conclusi;
- verifiche non eseguite, limiti e rischi residui sono dichiarati.
- gli artefatti runtime di terze parti nuovi o aggiornati sono elencati quando la modifica influisce sul deployment.

La Definition of Done è obbligatoria ma non prescrive gli stessi comandi per ogni attività. I documenti specialistici definiscono le verifiche pertinenti per codice, test, sicurezza, documentazione e rilascio.

---

## 10. Procedure e approfondimenti

- [Architecture](../Architecture/Architecture.md)
- [Architecture Decision Records](../Architecture/ADR/README.md)
- [Shared Framework](../Architecture/SharedFramework.md)
- [Domain Architecture](../Architecture/DomainArchitecture.md)
- [Technical Debt](TechnicalDebt.md)
- [Code Review](CodeReview.md)
- [Code Review Checklist](CodeReviewChecklist.md)
- [Testing Architecture](../Architecture/TestingArchitecture.md)
