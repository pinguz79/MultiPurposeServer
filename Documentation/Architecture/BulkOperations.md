# Operazioni Bulk

> **Stato: Stable 1.0 — autorevole.**

## 1. Scopo

Questo documento definisce la semantica condivisa delle Request Bulk di MultiPurposeServer.

I contratti tecnici correnti costituiscono una base ancora parziale. Le strategie descritte rappresentano il modello consolidato verso cui evolvere; nomi delle API e tipi concreti rimangono da progettare.

---

## 2. Contratto contenitore

Una Bulk Request contiene:

- options che selezionano la strategia;
- lista di item DTO;
- regole globali sulla struttura del payload;
- regole tecniche applicabili ai singoli item.

Prima di qualsiasi persistenza, il contenitore verifica options, presenza e struttura della lista e univocità degli item.

La presenza di duplicati invalida sempre l'intera Request. Nessun item viene elaborato o persistito.

La validazione distingue due livelli:

- la pipeline MVC valida il contenitore e produce `HTTP 400` per gli errori globali della Request;
- l'esecutore Bulk valida ciascun item durante l'elaborazione e applica la strategia di valutazione e persistenza selezionata.

Un item non valido non rende quindi automaticamente invalido l'intero contenitore. Con `PartialSuccess` gli altri item validi possono essere persistiti; con `EvaluateAll` la Response raccoglie tutti gli errori individuali rilevabili.

La normalizzazione può essere applicata ricorsivamente a tutti gli item prima dell'esecuzione perché non produce effetti persistenti. La validazione ricorsiva automatica della pipeline non deve invece anticipare la semantica propria dell'esecutore Bulk.

---

## 3. Dimensioni indipendenti della strategia

La strategia Bulk combina due dimensioni indipendenti.

### 3.1 Persistenza — `BulkPersistenceStrategy`

- **`AllOrNothing`:** tutti gli item devono riuscire; in caso contrario nessun effetto viene conservato.
- **`PartialSuccess`:** ogni item riuscito può essere conservato indipendentemente dall'esito degli altri.

### 3.2 Valutazione — `BulkEvaluationStrategy`

- **`StopOnFirstFailure`:** l'elaborazione si interrompe al primo fallimento.
- **`EvaluateAll`:** tutti gli item valutabili vengono processati per raccogliere l'insieme completo degli errori.

Tutte le combinazioni sono ammesse, compresa `PartialSuccess + StopOnFirstFailure`. Un client può preferire l'interruzione anticipata quando il costo del payload rende inaccettabile continuare dopo un errore.

`BulkOptions` espone le due dimensioni separatamente. I valori predefiniti sono `PartialSuccess` ed `EvaluateAll`, equivalenti al comportamento storico `WarningAndContinue`.

---

## 4. Atomicità

Ogni singolo item rimane atomico.

Con `All or Nothing`, il Controller governa rollback o compensazione dell'intera operazione. `Evaluate All` può completare la valutazione degli item, ma non autorizza persistenza parziale.

Con `Partial Success`, gli item riusciti possono essere conservati. Un fallimento non annulla automaticamente gli item precedenti, salvo che il dominio dichiari una dipendenza che lo richieda.

L'atomicità applicativa può coinvolgere database, filesystem o servizi esterni e non coincide necessariamente con una singola transazione database.

---

## 5. Identificazione e univocità

Ogni item deve essere identificabile nella Response mediante una chiave prevista dal relativo Contract.

La chiave può corrispondere:

- all'identificatore fisico già assegnato, tipicamente nelle operazioni di Update o Delete;
- a una chiave logica univoca, tipicamente durante la Create prima che il database generi l'identificatore fisico.

Il framework necessita del concetto di chiave, non della sua semantica. Il contratto opzionale, la nomenclatura degli attributi e la relazione fra chiave logica e vincolo di univocità rimangono da progettare.

Non è attualmente previsto il supporto a più chiavi logiche alternative per lo stesso tipo di item.

---

## 6. Ordinamento e dipendenze

Gli item sono normalmente indipendenti. Quando appartengono a una struttura autoreferenziale, il chiamante fornisce una lista in ordine logicamente valido.

Una Request può dichiarare ordinabilità intrinseca. Il framework conosce che gli item sono ordinabili e invoca il relativo comportamento; il DTO definisce come calcolare l'ordine senza esporre al framework la semantica della gerarchia.

Sono ammesse relazioni tra istanze dello stesso tipo, ma non:

- auto-riferimenti di un'istanza a se stessa;
- cicli fra istanze, come `A → B → A`.

Se un item dipende da un parent fallito o assente, il suo esito distingue la dipendenza mancante dall'errore originario del parent.

---

## 7. Errori

La Response distingue:

- errori globali della Request;
- errori di validazione del singolo DTO;
- violazioni di persistenza, incluse foreign key e vincoli di univocità;
- dipendenze mancanti;
- item non processati a causa della strategia scelta.

Un errore globale produce una risposta HTTP di errore e nessuna persistenza. Sono errori globali, fra gli altri, options non valide, lista assente o vuota, struttura del contenitore non valida e chiavi duplicate.

Gli errori individuali ammessi dalla strategia non trasformano automaticamente l'intera risposta in un errore HTTP.

---

## 8. Response

Una Request elaborata validamente secondo la strategia selezionata restituisce `200 OK`, anche quando la strategia ammette fallimenti individuali.

Il body contiene:

- esito aggregato;
- strategia applicata;
- esito di ogni item;
- chiave o posizione necessaria a correlare item e risultato;
- errori formalizzati.

Il contratto condiviso è composto da:

- `BulkResponse<TKey, TValue>`, che espone options, `BulkOutcome` e risultati ordinati;
- `BulkItemResult<TKey, TValue>`, che espone indice, chiave, `BulkItemOutcome`, stato di persistenza, valore ed errori;
- `BulkError`, classificato tramite `BulkErrorKind` e dettagliato da codice e messaggio.

`BulkOutcome` distingue `Succeeded`, `PartiallySucceeded` e `Failed`. `BulkItemOutcome` distingue `Succeeded`, `Failed` e `NotProcessed`.

`Persisted` è indipendente dall'esito di elaborazione: con `AllOrNothing` un item può risultare `Succeeded` ma `Persisted = false` quando un altro item causa il rollback dell'intero payload.

Gli errori individuali sono divisi nelle sole famiglie `Validation` e `Persistence`. Il codice distingue i casi concreti, per esempio item inesistente, foreign key violata o parent mancante, senza moltiplicare le categorie principali.

Gli esiti individuali distinguono almeno:

- `Succeeded`;
- `Failed`;
- `NotProcessed`.

La Response non nasconde che un'operazione `All or Nothing` ha effettuato soltanto valutazione senza conservare item formalmente validi.

---

## 9. Stato delle capacità

### Attuali

- `IBulk<TItem>`;
- `BulkRequest<TItem>`;
- `BulkOptions` comuni;
- strategie `BulkPersistenceStrategy` e `BulkEvaluationStrategy`;
- comportamento predefinito `PartialSuccess + EvaluateAll`.
- response generiche `BulkResponse<TKey, TValue>` e `BulkItemResult<TKey, TValue>`;
- esiti ed errori Bulk condivisi.
- esecutore comune alle API Bulk del dominio Portfolio;
- operazioni indipendenti per `PartialSuccess`;
- operazione globale con checkpoint per `AllOrNothing`;
- supporto indipendente di `StopOnFirstFailure` ed `EvaluateAll`.

### Pianificate

- identificazione opzionale tramite chiave;
- ordinabilità intrinseca opzionale;
- eventuale promozione dell'esecutore nel framework condiviso quando un secondo dominio ne confermerà la riusabilità.

Le API concrete saranno definite durante l'implementazione senza modificare la semantica consolidata, salvo nuova decisione esplicita.

---

## 10. Riferimenti

- [Shared Framework](SharedFramework.md)
- [Request Processing](RequestProcessing.md)
- [API Architecture](ApiArchitecture.md)
- [Domain Architecture](DomainArchitecture.md)
- [ADR-0006 — Le Request Bulk condividono contratti tecnici comuni](ADR/ADR-0006-bulk-requests-share-common-technical-contracts.md)
