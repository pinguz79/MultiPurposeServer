# Playbook di Ingegneria di MultiPurposeServer

## 1. Scopo del documento

Questo playbook raccoglie le pratiche di ingegneria adottate durante lo sviluppo di MultiPurposeServer.

Il suo obiettivo è definire un approccio comune alla progettazione, all'implementazione, al testing e alla documentazione del progetto, indipendentemente da chi contribuisce al suo sviluppo.

Le linee guida contenute in questo documento si applicano allo stesso modo agli sviluppatori, agli assistenti basati su intelligenza artificiale e agli strumenti di generazione automatica del codice. Il loro scopo è mantenere il progetto coerente nel tempo, ridurre i costi di manutenzione e favorirne un'evoluzione continua e incrementale.

Questo playbook non è una guida allo stile di programmazione né descrive l'architettura di MultiPurposeServer. Definisce invece i principi di ingegneria e le pratiche di sviluppo che dovrebbero guidare ogni decisione tecnica durante l'intero ciclo di vita del progetto.

Il documento integra la documentazione tecnica del progetto e deve essere letto insieme ad essa.

Per gli aspetti architetturali e per i dettagli implementativi fare riferimento a:

- `Architecture.md`
- `SharedFramework.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`

Qualora le indicazioni contenute in questo playbook risultassero in contrasto con la documentazione architetturale, quest'ultima ha sempre la precedenza.

L'ingegneria del software è un processo continuo di presa delle decisioni. Ogni scelta progettuale, ogni refactoring, ogni test e ogni evoluzione dell'architettura contribuiscono alla qualità del progetto nel lungo periodo.

Lo scopo di questo playbook è fornire un insieme condiviso di principi che permetta di mantenere tali decisioni coerenti nel tempo.

---

## 2. Principi di Ingegneria

I principi di ingegneria rappresentano il modello con cui vengono affrontate le decisioni tecniche all'interno di MultiPurposeServer.

Sono indipendenti dal linguaggio di programmazione, dai framework utilizzati e dagli strumenti di sviluppo, e rimangono validi anche quando il progetto evolve.

Quando esistono più soluzioni corrette allo stesso problema, questi principi aiutano a individuare quella maggiormente coerente con gli obiettivi del progetto.

### 2.1 La leggibilità prima dell'ingegnosità

Il codice viene letto molte più volte di quante ne venga scritto.

Per questo motivo la leggibilità deve sempre prevalere su soluzioni particolarmente ingegnose, eccessivamente compatte o difficili da comprendere.

Una soluzione semplice e immediatamente comprensibile è quasi sempre preferibile a una soluzione sofisticata che richiede uno sforzo aggiuntivo per essere interpretata.

#### Linee guida

- Preferire comportamenti espliciti a comportamenti impliciti.
- Mantenere ogni implementazione focalizzata su una singola responsabilità.
- Evitare complessità non necessarie.
- Privilegiare la manutenibilità nel lungo periodo rispetto all'eleganza di una soluzione nel breve termine.
- Scrivere codice comprensibile senza richiedere spiegazioni aggiuntive.

---

### 2.2 La coerenza prima della novità

La coerenza dell'intero progetto ha generalmente più valore dell'introduzione di un nuovo pattern o di una nuova soluzione.

Nuove idee dovrebbero essere adottate solo quando apportano un reale beneficio architetturale che non può essere ottenuto estendendo quanto già esiste.

#### Linee guida

- Riutilizzare, quando possibile, i pattern architetturali già presenti.
- Preferire l'estensione di una soluzione esistente all'introduzione di una nuova.
- Valutare sempre l'impatto nel lungo periodo prima di introdurre nuovi pattern.
- Privilegiare prevedibilità e uniformità rispetto all'originalità.
- Mantenere il progetto coerente durante la sua evoluzione.

---

### 2.3 Evoluzione incrementale

MultiPurposeServer evolve attraverso piccoli miglioramenti continui, non mediante grandi riscritture.

Ogni decisione tecnica dovrebbe preservare l'architettura esistente, consentendole di evolvere progressivamente senza interruzioni o cambiamenti radicali.

#### Linee guida

- Preferire piccoli refactoring incrementali.
- Verificare ogni modifica tramite compilazione e testing prima di procedere.
- Evitare refactoring estesi con molteplici obiettivi.
- Lasciare che le astrazioni emergano progressivamente.
- Migliorare i componenti esistenti quando possibile, invece di sostituirli.

---

### 2.4 L'architettura prima dell'implementazione

Prima di scrivere codice è necessario individuare quale sia la corretta collocazione architetturale di una responsabilità.

Una buona decisione architetturale porta naturalmente a implementazioni più semplici. Al contrario, una scelta architetturale errata genera complessità anche quando il codice è scritto correttamente.

#### Linee guida

- Individuare il corretto livello architetturale prima di implementare una nuova funzionalità.
- Preservare la separazione delle responsabilità.
- Evitare che una responsabilità oltrepassi i confini dell'architettura.
- Lasciare che sia l'architettura a guidare l'implementazione e non viceversa.

#### Vedi anche

- `Architecture.md`

---

### 2.5 I concetti condivisi devono emergere naturalmente

Le astrazioni condivise devono nascere dall'osservazione di pattern ricorrenti e non dall'anticipazione di esigenze future.

Lo scopo di un framework condiviso è consolidare concetti che hanno già dimostrato di essere comuni, non prevedere ciò che potrebbe diventarlo.

> **Shared is Earned, not Planned.**

#### Linee guida

- Estrarre comportamenti condivisi solo dopo aver individuato una reale duplicazione.
- Evitare astrazioni speculative.
- Validare nuovi concetti condivisi in più domini prima di promuoverli nello Shared Framework.
- Mantenere i framework condivisi focalizzati su concetti realmente stabili.
- Preferire l'evoluzione delle astrazioni esistenti all'introduzione di astrazioni parallele.

#### Vedi anche

- `SharedFramework.md`

---

### 2.6 Una sola responsabilità

Ogni artefatto software dovrebbe avere una responsabilità chiara e ben definita.

Questo principio non riguarda soltanto classi e metodi, ma si applica anche a framework, documenti, test e componenti architetturali.

Responsabilità ben delimitate rendono il progetto più semplice da comprendere, da testare e da evolvere.

#### Linee guida

- Mantenere ogni responsabilità focalizzata e coesa.
- Evitare componenti che risolvono problemi non correlati tra loro.
- Preferire la composizione all'ereditarietà quando non strettamente necessaria.
- Effettuare refactoring quando una responsabilità inizia a divergere.
- Lasciare che le responsabilità emergano naturalmente dal dominio del problema.

---

### 2.7 Manutenibilità nel lungo periodo

Le decisioni tecniche devono essere valutate in funzione del loro impatto nel lungo periodo e non della loro comodità immediata.

Una soluzione che facilita la manutenzione futura è generalmente preferibile a una che riduce soltanto lo sforzo iniziale di sviluppo.

#### Linee guida

- Privilegiare la manutenibilità rispetto alle ottimizzazioni premature.
- Preferire soluzioni semplici e ben strutturate.
- Considerare sempre come il codice verrà compreso ed esteso in futuro.
- Ridurre il debito tecnico ogni volta che può essere affrontato in modo incrementale.
- Adottare decisioni sostenibili per la crescita del progetto.

---

### 2.8 La documentazione fa parte dell'ingegneria

La documentazione evolve insieme al software ed è parte integrante del processo di sviluppo.

Ogni volta che l'architettura o le pratiche di ingegneria cambiano, è necessario valutare se anche la documentazione debba essere aggiornata.

#### Linee guida

- Mantenere la documentazione allineata all'implementazione.
- Aggiornare la documentazione in modo incrementale.
- Evitare duplicazioni tra documenti diversi.
- Documentare le decisioni architetturali nel documento appropriato.
- Garantire che ogni concetto abbia un unico documento di riferimento.

#### Vedi anche

- `Architecture.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`

---

## 3. Flusso di Sviluppo

Il flusso di sviluppo definisce il processo con cui MultiPurposeServer evolve in modo controllato, incrementale e verificabile.

Il suo scopo è ridurre il rischio di regressioni, preservare la coerenza dell'architettura e favorire una validazione continua durante tutte le attività di sviluppo.

Le stesse pratiche si applicano allo sviluppo di nuove funzionalità, alla correzione di bug, ai refactoring e alle evoluzioni dell'architettura.

### 3.1 Un passo logico alla volta

Ogni modifica significativa dovrebbe essere suddivisa in una sequenza di passi piccoli, indipendenti e facilmente verificabili.

Ogni passo deve avere un unico obiettivo e lasciare il progetto in uno stato coerente prima di procedere con quello successivo.

#### Linee guida

- Affrontare un solo problema alla volta.
- Evitare di combinare refactoring non correlati.
- Completare l'obiettivo corrente prima di iniziarne uno nuovo.
- Preferire molti piccoli miglioramenti a un'unica modifica di grandi dimensioni.
- Rendere ogni passo facilmente comprensibile e revisionabile.

---

### 3.2 Validare continuamente

Ogni modifica significativa dovrebbe essere verificata prima di procedere con ulteriori sviluppi.

Una validazione continua permette di individuare rapidamente gli errori, semplifica il debugging e riduce il costo delle regressioni.

#### Linee guida

- Compilare il progetto dopo ogni modifica significativa.
- Eseguire i test pertinenti prima di procedere.
- Risolvere immediatamente eventuali errori.
- Non costruire nuove funzionalità su una soluzione non funzionante.
- Mantenere il progetto sempre in uno stato stabile.

---

### 3.3 Completare il traguardo corrente

Lo sviluppo dovrebbe procedere in modo sequenziale.

Prima di affrontare un nuovo obiettivo è necessario portare quello corrente a uno stato stabile e verificabile.

Le idee per sviluppi futuri devono essere annotate, ma non devono interrompere il completamento dell'attività in corso.

#### Linee guida

- Completare l'obiettivo corrente prima di discutere sviluppi futuri.
- Evitare di anticipare refactoring non ancora necessari.
- Non introdurre infrastrutture per problemi che ancora non esistono.
- Lasciare che il lavoro futuro emerga naturalmente dall'implementazione corrente.
- Mantenere ogni milestone focalizzata sul proprio obiettivo.

---

### 3.4 Progettare prima di implementare

Quando una modifica coinvolge l'architettura, la progettazione dovrebbe precedere l'implementazione.

Il codice deve essere la conseguenza di una decisione architetturale, non il punto di partenza.

Una progettazione chiara conduce generalmente a implementazioni più semplici e riduce la necessità di successivi refactoring.

#### Linee guida

- Chiarire le responsabilità prima di introdurre nuovi componenti.
- Validare le ipotesi architetturali prima di iniziare l'implementazione.
- Separare, quando possibile, la progettazione dall'attività di sviluppo.
- Discutere la soluzione prima di scrivere il codice.

---

### 3.5 La documentazione evolve insieme al progetto

La documentazione è parte integrante del processo di sviluppo e cresce insieme al software.

Ogni volta che emerge un nuovo concetto stabile è opportuno valutare se debba essere documentato.

La documentazione deve descrivere conoscenza consolidata, non attività ancora in corso.

#### Linee guida

- Aggiornare la documentazione solo quando un concetto si è stabilizzato.
- Documentare i concetti, non i dettagli implementativi.
- Inserire ogni concetto nel documento che ne è proprietario.
- Evitare duplicazioni tra documenti.
- Valutare se ogni modifica architetturale richieda anche un aggiornamento della documentazione.

---

### 3.6 Lasciare il progetto migliore di come lo si è trovato

Ogni attività completata dovrebbe lasciare il progetto in uno stato più pulito, più coerente e più semplice da mantenere.

Anche piccoli miglioramenti, accumulandosi nel tempo, contribuiscono in modo significativo alla qualità complessiva del progetto.

#### Linee guida

- Lasciare sempre la soluzione compilabile.
- Lasciare sempre superati i test pertinenti.
- Rimuovere il codice obsoleto quando è possibile farlo in sicurezza.
- Eliminare il piccolo debito tecnico quando si integra naturalmente con l'attività in corso.
- Segnalare il debito tecnico che non può essere affrontato immediatamente.
- Non lasciare mai refactoring incompleti.

## 4. Linee Guida Architetturali

Le linee guida architetturali definiscono i principi da seguire ogni volta che un'attività di sviluppo modifica la struttura del sistema.

Questo capitolo integra la documentazione architetturale concentrandosi sulle decisioni di ingegneria, senza sostituirsi alla descrizione dell'architettura stessa.

Per una descrizione completa dell'architettura del sistema fare riferimento a:

- `Architecture.md`
- `SharedFramework.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`

### 4.1 Rispettare le responsabilità esistenti

Prima di introdurre nuovo codice è necessario individuare quale componente sia già responsabile del problema da risolvere.

Una nuova responsabilità non dovrebbe essere introdotta quando un componente esistente può evolvere naturalmente per accoglierla.

#### Linee guida

- Preservare l'attuale separazione delle responsabilità.
- Estendere i componenti esistenti prima di crearne di nuovi.
- Evitare sovrapposizioni di responsabilità.
- Mantenere espliciti i confini architetturali.
- Preferire il refactoring dei componenti esistenti all'introduzione di soluzioni parallele.

---

### 4.2 Preferire l'evoluzione alla sostituzione

Quando possibile, i componenti architetturali dovrebbero evolvere in modo incrementale.

La sostituzione di un componente consolidato dovrebbe rappresentare l'ultima opzione disponibile, da prendere in considerazione solo quando l'evoluzione progressiva non è più praticabile.

L'evoluzione preserva la conoscenza acquisita, riduce il rischio di regressioni e mantiene l'architettura stabile nel tempo.

#### Linee guida

- Preferire l'estensione delle soluzioni esistenti.
- Evitare riscritture non necessarie.
- Preservare la compatibilità quando possibile.
- Considerare la sostituzione come ultima alternativa.
- Privilegiare il refactoring evolutivo rispetto a riprogettazioni radicali.

---

### 4.3 Shared is Earned, not Planned

Un concetto appartiene allo Shared Framework solo dopo aver dimostrato il proprio valore in più domini applicativi.

Le astrazioni condivise devono emergere naturalmente da esigenze ricorrenti e non essere introdotte preventivamente.

Lo Shared Framework ha lo scopo di raccogliere concetti stabili, non di anticipare esigenze future.

#### Linee guida

- Promuovere un concetto nello Shared Framework solo dopo averne dimostrato la natura generica.
- Evitare astrazioni condivise speculative.
- Mantenere lo Shared Framework focalizzato su concetti realmente stabili.
- Lasciare che i domini maturino prima di estrarne il comportamento comune.
- Preferire l'evoluzione delle astrazioni condivise esistenti all'introduzione di nuove astrazioni parallele.

#### Vedi anche

- `SharedFramework.md`

---

### 4.4 Preservare i confini tra i livelli architetturali

I livelli dell'architettura esistono per separare le responsabilità e ridurre l'accoppiamento tra le diverse parti del sistema.

Quando una modifica attraversa un confine architetturale è opportuno verificare che la responsabilità appartenga realmente al livello in cui si intende collocarla.

Confini chiari rendono l'architettura più comprensibile, più semplice da testare e più facile da evolvere.

#### Linee guida

- Mantenere la logica di business all'interno dell'Application Layer.
- Mantenere le responsabilità di persistenza all'interno del Persistence Layer.
- Isolare l'infrastruttura dalle regole di business.
- Mantenere i Controller focalizzati esclusivamente sull'orchestrazione HTTP.
- Evitare che le responsabilità attraversino i livelli architetturali.

#### Vedi anche

- `Architecture.md`

---

### 4.5 Preferire astrazioni stabili

Le astrazioni dovrebbero rappresentare concetti stabili e non implementazioni temporanee.

Una buona astrazione continua ad avere significato anche quando la sua implementazione evolve.

Lo scopo di un'astrazione è rendere più chiaro il dominio, non semplicemente eliminare codice duplicato.

#### Linee guida

- Estrarre le astrazioni dai concetti, non dalle implementazioni.
- Evitare astrazioni create esclusivamente per eliminare duplicazioni.
- Preferire astrazioni guidate dal dominio.
- Mantenere i contratti indipendenti dai dettagli implementativi.
- Progettare astrazioni che rimangano stabili nel tempo.

---

### 4.6 L'architettura evolve consapevolmente

L'architettura dovrebbe evolvere attraverso decisioni di ingegneria consapevoli e non come conseguenza accidentale dell'implementazione.

Le modifiche architetturali significative dovrebbero essere valutate, documentate e introdotte in modo incrementale.

#### Linee guida

- Discutere le modifiche architetturali significative prima di implementarle.
- Valutare sempre il loro impatto nel lungo periodo.
- Mantenere sincronizzata la documentazione architetturale con l'implementazione.
- Documentare le decisioni che definiscono una direzione stabile del progetto.
- Preferire un'evoluzione graduale dell'architettura rispetto a riprogettazioni radicali.

#### Vedi anche

- `Architecture.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`

---

## 5. Linee Guida di Implementazione

Le linee guida di implementazione definiscono le convenzioni adottate durante lo sviluppo di MultiPurposeServer.

Il loro obiettivo è favorire coerenza, leggibilità e manutenibilità dell'intera codebase.

Se i capitoli precedenti descrivono come vengono prese le decisioni di ingegneria, questo capitolo definisce come tali decisioni debbano tradursi in codice di produzione.

### 5.1 Preferire le funzionalità moderne del linguaggio

Le funzionalità più recenti del linguaggio dovrebbero essere adottate quando migliorano la leggibilità, riducono il codice ripetitivo o esprimono con maggiore chiarezza l'intento dell'implementazione.

Non dovrebbero invece essere introdotte semplicemente perché disponibili.

#### Linee guida

- Utilizzare le funzionalità moderne quando migliorano la leggibilità.
- Ridurre il codice ripetitivo quando ciò migliora la manutenibilità.
- Mantenere il codice facilmente comprensibile per chi conosce il linguaggio.
- Preferire una sintassi espressiva a implementazioni verbose.
- Evitare l'introduzione di nuove sintassi senza un reale beneficio.

---

### 5.2 Esprimere chiaramente le intenzioni

Il codice dovrebbe comunicare il proprio scopo nel modo più chiaro possibile.

Chi legge un'implementazione dovrebbe comprenderne l'obiettivo senza dover ricostruire mentalmente comportamenti impliciti.

#### Linee guida

- Scegliere nomi significativi.
- Preferire comportamenti espliciti a comportamenti impliciti.
- Mantenere le implementazioni lineari e facilmente comprensibili.
- Evitare effetti collaterali inattesi.
- Rendere immediatamente evidente lo scopo di ogni metodo.

---

### 5.3 Mantenere i metodi focalizzati

Ogni metodo dovrebbe svolgere un'unica attività ben definita.

Metodi piccoli e coesi sono più semplici da comprendere, testare e mantenere.

#### Linee guida

- Assegnare a ogni metodo una sola responsabilità.
- Evitare di mescolare responsabilità differenti.
- Estrarre metodi privati quando migliorano la leggibilità.
- Limitare i livelli di annidamento.
- Preferire gli early return quando rendono il flusso più semplice.

---

### 5.4 Convenzioni di sviluppo C#

MultiPurposeServer adotta un insieme condiviso di convenzioni C# per massimizzare la leggibilità e ridurre differenze stilistiche non necessarie.

Queste convenzioni rappresentano lo stile corrente del progetto e devono essere applicate in modo coerente al nuovo codice. Possono evolvere insieme al linguaggio e alla codebase, ma ogni modifica deve essere introdotta consapevolmente e applicata in modo uniforme.

#### Linee guida

- Utilizzare namespace a blocco.
- Utilizzare i primary constructor quando appropriato.
- Preferire le collection expression quando migliorano la leggibilità.
- Utilizzare il target-typed `new` quando il tipo è già evidente.
- Preferire gli expression-bodied member per implementazioni semplici.
- Utilizzare il pattern matching quando rende il codice più leggibile.
- Preferire `nameof()` alle stringhe letterali.
- Utilizzare i membri `required` quando esprimono meglio i requisiti di inizializzazione.
- Utilizzare `var` quando il tipo è immediatamente evidente; negli altri casi preferire il tipo esplicito.
- Privilegiare strutture immutabili quando appropriate.

---

### 5.5 Formattazione e impaginazione del codice

La formattazione serve a migliorare la leggibilità del codice, non a esprimere preferenze personali.

La coerenza dell'intera codebase è più importante dello stile individuale di ciascun sviluppatore.

#### Linee guida

- Seguire la formattazione già adottata dal codice circostante.
- Evitare righe vuote non necessarie.
- Evitare ritorni a capo inutili.
- Raggruppare visivamente le istruzioni correlate.
- Mantenere le istruzioni su una sola riga quando la leggibilità lo consente.
- Spezzare le istruzioni lunghe solo quando migliora realmente la leggibilità.
- Mantenere un'indentazione semplice e coerente.

---

### 5.6 Commenti

Un buon codice dovrebbe spiegare da solo cosa fa.

I commenti dovrebbero quindi chiarire il motivo di una scelta progettuale, non descrivere ciò che l'implementazione rende già evidente.

#### Linee guida

- Preferire codice autoesplicativo.
- Eliminare i commenti che duplicano l'implementazione.
- Documentare le decisioni progettuali non immediatamente evidenti.
- Mantenere i commenti sincronizzati con il codice.
- Utilizzare commenti di sezione solo quando migliorano la leggibilità.
- Nei test unitari utilizzare `// Arrange`, `// Act` e `// Assert`.

---

### 5.7 Immutabilità ed effetti collaterali

Ridurre lo stato mutabile rende il codice più semplice da comprendere e limita il rischio di effetti collaterali indesiderati.

Quando il dominio lo consente, è preferibile adottare strutture immutabili.

#### Linee guida

- Preferire oggetti immutabili quando appropriato.
- Limitare lo stato mutabile.
- Rendere chiaro il ciclo di vita degli oggetti.
- Esplicitare gli effetti collaterali.
- Evitare modifiche di stato nascoste.

---

### 5.8 Scrivere codice pensando alla manutenzione

Il codice di produzione verrà mantenuto molto più a lungo di quanto sia stato necessario per svilupparlo.

Le decisioni implementative dovrebbero quindi privilegiare la futura evoluzione del progetto rispetto alla sola comodità immediata.

#### Linee guida

- Privilegiare la leggibilità rispetto alle ottimizzazioni premature.
- Mantenere le implementazioni semplici.
- Evitare astrazioni non necessarie.
- Preferire soluzioni facilmente manutenibili a soluzioni particolarmente ingegnose.
- Lasciare il codice più semplice da comprendere di come lo si è trovato.

#### Vedi anche

- `Principi di Ingegneria` (§2)

---

## 6. Strategia di Testing

Il testing è parte integrante del processo di ingegneria del software.

Il suo scopo non è soltanto verificare la correttezza del codice, ma anche proteggere l'architettura, documentare il comportamento atteso e consentire un refactoring sicuro durante l'intero ciclo di vita del progetto.

Poiché componenti differenti richiedono strategie differenti, MultiPurposeServer adotta più livelli di testing, ciascuno con una responsabilità ben definita.

### 6.1 Testare la responsabilità corretta

Ogni test dovrebbe verificare esclusivamente la responsabilità del componente sottoposto a verifica.

Un test non dovrebbe ripetere controlli già garantiti da livelli architetturali inferiori o superiori.

Una buona suite di test riduce le duplicazioni aumentando al tempo stesso il livello di affidabilità.

#### Linee guida

- Verificare una sola responsabilità per ogni test.
- Evitare test ridondanti.
- Verificare il comportamento, non l'implementazione.
- Mantenere i test focalizzati sul contratto pubblico.
- Assicurarsi che ogni test abbia uno scopo preciso e ben definito.

---

### 6.2 Privilegiare i test comportamentali

I test dovrebbero verificare il comportamento osservabile del sistema e non i dettagli della sua implementazione.

L'implementazione può evolvere liberamente, purché il comportamento esterno rimanga invariato.

Questo permette di effettuare refactoring senza dover riscrivere inutilmente la suite di test.

#### Linee guida

- Verificare il risultato, non il modo in cui viene ottenuto.
- Evitare dipendenze da metodi privati.
- Rendere i test resistenti ai refactoring interni.
- Concentrarsi esclusivamente sul comportamento osservabile.
- Considerare privati i dettagli implementativi che non fanno parte del contratto pubblico.

---

### 6.3 Componenti dichiarativi richiedono test dichiarativi

Quando il comportamento è guidato da una configurazione dichiarativa, i test dovrebbero verificare la configurazione e non il funzionamento del framework.

Il framework è responsabile dell'esecuzione della configurazione.

I Request Contract sono responsabili esclusivamente della sua corretta descrizione.

#### Linee guida

- I test dei DTO verificano gli attributi.
- I test del framework verificano il comportamento.
- Evitare di duplicare i test del framework nei singoli Request Contract.
- Mantenere i test dichiarativi semplici e leggeri.
- Verificare la configurazione, non la sua esecuzione.

---

### 6.4 Livelli di testing

MultiPurposeServer organizza i test in funzione delle responsabilità architetturali e non dei dettagli implementativi.

Ogni livello di testing protegge un diverso aspetto dell'architettura.

I principali livelli di testing sono:

- Unit Test
- Framework Test
- Request Contract Test
- Integration Test

Ogni livello dovrebbe verificare esclusivamente le responsabilità che gli appartengono.

---

### 6.5 I test sono documentazione eseguibile

I test dovrebbero comunicare chiaramente lo scenario che verificano.

Chi li legge dovrebbe comprenderne immediatamente l'obiettivo, senza necessità di ulteriori spiegazioni.

Una buona suite di test rappresenta una documentazione sempre aggiornata del comportamento atteso del sistema.

#### Linee guida

- Seguire il pattern Arrange–Act–Assert.
- Utilizzare nomi descrittivi.
- Verificare un solo scenario per ogni test.
- Evitare setup non necessari.
- Privilegiare la leggibilità rispetto alla compattezza.

---

### 6.6 Test stabili favoriscono l'evoluzione

I test dovrebbero rimanere stabili anche quando l'implementazione evolve.

Una buona suite di test permette di effettuare refactoring con fiducia, riducendo il rischio di introdurre regressioni.

Al contrario, test fragili diventano rapidamente un ostacolo all'evoluzione del progetto.

#### Linee guida

- Evitare test che dipendono dai dettagli implementativi.
- Preferire test deterministici.
- Eliminare immediatamente i test instabili.
- Rendere esplicite tutte le dipendenze.
- Evitare dipendenze dall'ordine di esecuzione.

---

### 6.7 Il testing rende sicuro il refactoring

Uno degli obiettivi principali della suite di test è consentire l'evoluzione del progetto in sicurezza.

Il refactoring dovrebbe poter migliorare l'implementazione senza modificare il comportamento osservabile, affidandosi ai test per individuare eventuali regressioni.

Per questo motivo la suite di test rappresenta un patrimonio architetturale del progetto e non una semplice attività di verifica.

#### Linee guida

- Mantenere la suite di test affidabile.
- Aggiornare i test solo quando cambia il comportamento previsto.
- Non indebolire mai un test per adattarlo a un'implementazione errata.
- Considerare ogni test fallito come un'informazione utile.
- Applicare ai test lo stesso livello di cura riservato al codice di produzione.

#### Vedi anche

- `SharedFramework.md`
- `ADR-0008 – Normalizzazione e Validazione Dichiarative`
 
## 7. Strategia di Refactoring

Il refactoring è un'attività continua di ingegneria finalizzata a migliorare la struttura interna del software senza modificarne il comportamento osservabile.

Non rappresenta una fase separata dello sviluppo, ma una pratica quotidiana che accompagna l'evoluzione del progetto.

MultiPurposeServer privilegia un refactoring continuo e incrementale rispetto a riprogettazioni estese e invasive.

### 7.1 Preservare il comportamento

L'obiettivo principale di ogni refactoring è migliorare la struttura interna del codice mantenendone invariato il comportamento osservabile.

Dal punto di vista dei suoi utilizzatori, il sistema deve rimanere funzionalmente equivalente prima e dopo la modifica.

#### Linee guida

- Preservare il contratto pubblico.
- Preservare il comportamento osservabile dall'esterno.
- Evitare di combinare modifiche funzionali e refactoring nella stessa attività.
- Verificare il risultato attraverso la suite di test esistente.

---

### 7.2 Procedere in modo incrementale

I refactoring estesi dovrebbero essere suddivisi in una sequenza di passi piccoli e verificabili.

Ogni passo deve lasciare il progetto in uno stato coerente e funzionante prima di procedere con quello successivo.

Un approccio incrementale riduce il rischio, semplifica la revisione e rende più semplice individuare eventuali regressioni.

#### Linee guida

- Preferire molti piccoli refactoring a un'unica grande riscrittura.
- Rendere ogni passo comprensibile e verificabile in modo indipendente.
- Validare ogni modifica prima di procedere.
- Evitare refactoring lasciati parzialmente completati.

---

### 7.3 Lasciare emergere le astrazioni

Le astrazioni condivise devono nascere naturalmente da pattern ricorrenti e non essere introdotte in anticipo.

Durante la progettazione, la duplicazione può rappresentare una fonte preziosa di informazioni.

Un comportamento dovrebbe essere estratto in un'astrazione condivisa solo quando il concetto sottostante è diventato sufficientemente chiaro e stabile.

#### Linee guida

- Accettare temporaneamente la duplicazione mentre si comprende il problema.
- Estrarre un'astrazione solo dopo aver individuato un concetto stabile.
- Evitare generalizzazioni speculative.
- Preferire una progettazione evolutiva a una progettazione anticipatoria.

---

### 7.4 Lasciare che l'architettura guidi il refactoring

Le responsabilità architetturali devono guidare ogni attività di refactoring.

Il refactoring dovrebbe rafforzare l'architettura, non adattarla ai limiti dell'implementazione corrente.

Quando una responsabilità non è chiara, è necessario chiarire prima l'architettura e solo successivamente modificare il codice.

#### Linee guida

- Rifattorizzare verso responsabilità più chiare.
- Preservare i confini architetturali.
- Evitare di spostare responsabilità tra livelli senza una motivazione architetturale.
- Lasciare che sia l'architettura a guidare le decisioni implementative.

---

### 7.5 Affrontare consapevolmente il debito tecnico

Il debito tecnico deve essere gestito in modo consapevole e non occasionale.

I piccoli miglioramenti che si integrano naturalmente con l'attività in corso sono incoraggiati, mentre il debito architetturale più ampio deve essere pianificato e tracciato esplicitamente.

#### Linee guida

- Eliminare il piccolo debito tecnico quando è opportuno.
- Evitare di introdurre nuovo debito tecnico.
- Tracciare esplicitamente il debito significativo.
- Distinguere i compromessi temporanei dalle scelte architetturali definitive.

---

### 7.6 I test rendono possibile il refactoring

Una suite di test affidabile è il fondamento di ogni refactoring sicuro.

Il refactoring dovrebbe affidarsi ai test esistenti per rilevare modifiche involontarie del comportamento, evitando di dipendere esclusivamente da verifiche manuali.

Quando i test evidenziano debolezze architetturali, è opportuno migliorare sia il codice di produzione sia la suite di test.

#### Linee guida

- Non affrontare refactoring privi di una copertura di test adeguata.
- Considerare i test falliti come informazioni utili.
- Migliorare i test insieme al codice quando necessario.
- Preservare l'intento dei test esistenti.

---

### 7.7 La documentazione segue il refactoring

I refactoring significativi possono introdurre nuovi concetti architetturali o modificare quelli esistenti.

Quando un refactoring consolida un nuovo concetto, è necessario valutare se anche la documentazione del progetto debba evolvere.

La documentazione deve descrivere l'architettura risultante, non la storia delle implementazioni precedenti.

#### Linee guida

- Aggiornare la documentazione dopo i refactoring architetturali.
- Documentare concetti stabili, non dettagli implementativi.
- Rimuovere tempestivamente la documentazione obsoleta.
- Mantenere la documentazione sincronizzata con l'architettura.

#### Vedi anche

- `Principi di Ingegneria` (§2)
- `Flusso di Sviluppo` (§3)
- `Linee Guida Architetturali` (§4)
- `Strategia della Documentazione` (§8)

---

## 8. Strategia della Documentazione

La documentazione è parte integrante del processo di ingegneria.

Il suo scopo è preservare la conoscenza architetturale, comunicare le decisioni progettuali e mantenere il progetto comprensibile durante la sua evoluzione.

Deve crescere insieme al software ed essere trattata con la stessa cura riservata al codice di produzione.

### 8.1 Documentare i concetti, non il codice

La documentazione dovrebbe descrivere concetti stabili e non dettagli implementativi.

Le implementazioni cambiano naturalmente nel tempo, mentre i concetti architetturali tendono a rimanere più stabili.

#### Linee guida

- Documentare i concetti, non le implementazioni.
- Spiegare le responsabilità prima di descrivere i componenti.
- Evitare di documentare dettagli implementativi temporanei.
- Mantenere la documentazione focalizzata sulla conoscenza architetturale.

---

### 8.2 Ogni concetto ha un documento proprietario

Ogni concetto architetturale appartiene a un solo documento, che ne rappresenta la fonte autorevole e ne descrive il significato dal punto di vista della propria responsabilità.

Lo stesso argomento può comparire anche in altri documenti quando viene osservato da prospettive differenti.

Ad esempio, `Architecture.md` può descrivere come il testing è organizzato all'interno del sistema, mentre il Playbook definisce i principi e le pratiche da seguire nella scrittura dei test.

Questa sovrapposizione non costituisce una duplicazione, purché ogni documento rimanga focalizzato sul proprio scopo e non ripeta lo stesso contenuto.

Gli altri documenti possono quindi richiamare o approfondire il concetto dal proprio punto di vista, ma non devono sostituirsi al documento che ne è proprietario.

#### Linee guida

- Mantenere un'unica fonte autorevole per ogni concetto.
- Distinguere la condivisione di un argomento dalla duplicazione del suo contenuto.
- Trattare lo stesso argomento in più documenti solo quando cambiano prospettiva e responsabilità.
- Aggiornare il documento proprietario quando cambia il significato del concetto.
- Preferire i riferimenti alla ripetizione delle stesse informazioni.
- Mantenere ogni documento focalizzato sul proprio scopo.

---

### 8.3 Organizzare la documentazione per livelli

La documentazione dovrebbe essere organizzata secondo livelli di dettaglio progressivi.

Ogni documento deve concentrarsi sulla propria responsabilità e rimandare a documenti più specifici quando è necessario un approfondimento.

#### Linee guida

- Mantenere ogni documento focalizzato.
- Procedere dai concetti generali ai dettagli più specifici.
- Utilizzare riferimenti incrociati quando aggiungono valore.
- Evitare di mescolare livelli di astrazione differenti nello stesso documento.

---

### 8.4 Rendere ogni documento autosufficiente

Ogni documento dovrebbe poter essere compreso autonomamente.

I riferimenti incrociati servono ad approfondire un argomento, non a rendere comprensibile il documento corrente.

Il lettore non dovrebbe essere costretto a consultare più file per comprendere un singolo concetto.

#### Linee guida

- Scrivere documenti leggibili in modo indipendente.
- Utilizzare i riferimenti solo per ulteriori approfondimenti.
- Evitare dipendenze nascoste tra documenti.
- Rendere ogni documento completo rispetto al proprio ambito.

---

### 8.5 Utilizzare i riferimenti incrociati solo quando servono

I riferimenti incrociati migliorano la navigazione nella documentazione, ma non devono diventare obbligatori.

Devono essere introdotti solo quando offrono un reale valore aggiunto.

#### Linee guida

- Utilizzare le sezioni `Vedi anche` solo quando appropriate.
- Evitare sezioni di riferimento vuote.
- Fare riferimento ai concetti, non alle implementazioni.
- Preferire una navigazione sintetica a un collegamento esaustivo di ogni contenuto.

---

### 8.6 Far evolvere la documentazione in modo incrementale

La documentazione deve evolvere continuamente insieme al progetto.

Quando emerge un concetto stabile, è necessario valutare se debba evolvere anche il documento che ne è proprietario.

La documentazione non deve essere rimandata alla fine dello sviluppo.

#### Linee guida

- Aggiornare la documentazione insieme alle modifiche architetturali.
- Documentare i concetti dopo che si sono stabilizzati.
- Evitare documentazione speculativa.
- Mantenere la documentazione sincronizzata con l'implementazione.

---

### 8.7 Considerare la documentazione un patrimonio di ingegneria

La documentazione fa parte dell'architettura del progetto.

Una documentazione ben progettata riduce i tempi di inserimento di nuovi collaboratori, semplifica la manutenzione e preserva la conoscenza tecnica nel lungo periodo.

Deve quindi essere considerata un patrimonio di ingegneria e non un costo accessorio del progetto.

#### Linee guida

- Considerare la documentazione parte del risultato finale.
- Revisionarla con la stessa attenzione riservata al codice.
- Migliorarla in modo incrementale.
- Preservare la conoscenza architetturale per i contributori futuri.

---

## 9. Definizione di Completamento

Un'attività di sviluppo può essere considerata conclusa solo quando soddisfa sia i requisiti funzionali sia quelli di ingegneria.

Completare un'implementazione significa più che ottenere una compilazione corretta o superare una verifica manuale.

Ogni attività conclusa deve lasciare il progetto in uno stato stabile, manutenibile e correttamente documentato.

### 9.1 Completamento funzionale

La funzionalità implementata soddisfa i requisiti concordati e si comporta come previsto.

#### Checklist

- La funzionalità richiesta è completa.
- Il comportamento osservabile corrisponde al risultato atteso.
- Non sono state introdotte regressioni funzionali note.

---

### 9.2 Qualità del codice

L'implementazione rispetta i principi di ingegneria e le linee guida definite in questo playbook.

#### Checklist

- Il codice è leggibile e manutenibile.
- Le responsabilità sono chiaramente separate.
- L'implementazione segue i pattern architetturali consolidati.
- Non è stata introdotta complessità non necessaria.

---

### 9.3 Testing

L'implementazione è protetta dal livello di testing appropriato.

#### Checklist

- I test pertinenti sono stati aggiunti o aggiornati.
- I test esistenti continuano a essere superati.
- L'implementazione è coperta dalla strategia di testing corretta.
- Non rimangono test falliti o instabili.

---

### 9.4 Documentazione

La documentazione evolve insieme all'implementazione.

Quando un concetto stabile viene introdotto o modificato, il relativo documento viene aggiornato.

#### Checklist

- La documentazione riflette l'architettura corrente.
- I nuovi concetti sono stati documentati.
- La documentazione obsoleta è stata rimossa o aggiornata.
- I riferimenti incrociati rimangono coerenti.

---

### 9.5 Debito tecnico

Ogni attività conclusa dovrebbe lasciare il progetto in uno stato più pulito rispetto a quello iniziale.

L'eventuale debito tecnico residuo deve essere consapevole e tracciato esplicitamente.

#### Checklist

- Non rimangono refactoring incompleti.
- I workaround temporanei sono documentati.
- Il debito tecnico significativo è stato registrato.
- Il progetto rimane pulito e manutenibile.

---

### 9.6 Validazione finale

Prima di considerare conclusa un'attività è necessario verificare l'intera soluzione.

#### Checklist

- La soluzione compila correttamente.
- Tutti i test pertinenti vengono superati.
- L'implementazione è coerente con l'architettura del progetto.
- Codice e documentazione sono sincronizzati.
- Il lavoro è pronto per le evoluzioni future.
