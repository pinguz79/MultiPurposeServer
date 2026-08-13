# Convenzioni di coding

## 1. Scopo e valore normativo

Questa guida definisce lo stile autorevole di MultiPurposeServer per sviluppatori, assistenti AI e generatori.
Le regole dei test sono approfondite in [Testing Conventions](TestingConventions.md); architettura e contratti
restano nei documenti proprietari. **Deve** indica un requisito; **dovrebbe** la scelta predefinita derogabile con
una motivazione concreta. Il codice generato non viene corretto manualmente.

## 2. Principi trasversali

Prima si identifica il risultato complessivo, poi si sceglie la forma piu semplice che lo rappresenta: le
convenzioni non sono trasformazioni testuali indipendenti.

Una riga dovrebbe restare entro 200 caratteri, con tolleranza naturale fino a circa 210. Non e un limite
meccanico: URL, stringhe indivisibili, firme e assertion possono superarlo quando la forma compatta e migliore.
Si va a capo soltanto nel primo punto necessario, conservando sulla riga corrente tutti gli elementi completi
che entrano. Sono vietati wrap preventivi, simmetrici o "un elemento per riga".

Le dichiarazioni di costruttori tradizionali e primary constructor che ricevono dipendenze tramite dependency
injection mantengono invece una soglia di 100 caratteri, con tolleranza naturale fino a circa 105. La deroga non
si applica a record, DTO, entity o value object che ricevono soltanto dati.

Una sola riga vuota separa membri, blocchi logici e fasi; mai righe vuote multiple. I commenti spiegano motivi e
vincoli, non traducono il codice. Sono in italiano, eccetto pattern come `Arrange`, `Act`, `Assert`, `TODO`,
`FIXME`, `HACK`; il testo successivo resta italiano.

Ogni file contiene un solo tipo e ne porta il nome; i tipi annidati sono vietati. File UTF-8 senza BOM, newline
finale, 4 spazi, niente tab o trailing whitespace. Nessun rename gratuito. Il namespace segue progetto e
alberatura logica.

## 3. C#

### 3.1 Struttura

- namespace sempre block-scoped; graffa C# sempre su nuova riga;
- top-level statements vietati: anche l'entry point dichiara esplicitamente namespace, classe `Program` e metodo `Main`;
- graffe obbligatorie anche per una sola istruzione;
- `using` in testa e fuori dal namespace: `System` prima, riga vuota, altri namespace; gruppi alfabetici;
- preferire `using` ai fully qualified name; per conflitti alias semantici (`DataAlbum = ...`), non abbreviazioni;
- `global using` soltanto centralmente per dipendenze realmente trasversali.

Ordine membri: costanti, campi statici, campi di istanza, costruttori, proprieta, eventi, metodi pubblici,
protetti, privati. Overload e membri correlati restano vicini; ordine logico prima dell'alfabetico.

Le region servono solo con almeno due gruppi sostanziali. Mai una region con un metodo, una region unica che
ingloba quasi tutto, o `Public methods`/`Private methods`. Per famiglie omogenee si raggruppa preferibilmente per
operazione (`Get`, `Set`, `Add`, `Update`, `Delete`, `Reset`), altrimenti per responsabilita funzionale.

### 3.2 Naming

- `PascalCase`: namespace, tipi, interfacce, enum/membri, metodi, proprieta, eventi, costanti e proprieta dei
  primary constructor di classi/record;
- interfacce con prefisso `I`;
- `camelCase`: parametri, locali e parametri non esposti dei primary constructor;
- `_camelCase`: campi privati dichiarati;
- acronimi come parole: `Id`, `Api`, `Dto`, `Url`, `Http`, `Json`;
- booleani semanticamente nominati (`Is`, `Has`, `Can`, `Should`, `Requires` quando appropriati), mai confrontati
  esplicitamente con `true`/`false`.

Non aggiungere `Async` solo perche un metodo restituisce `Task`/`ValueTask`: il suffisso distingue una variante
asincrona da una sincrona realmente esistente o deriva da un contratto esterno. Nessun suffisso `Sync`.

### 3.3 Dichiarazioni e stato

Accessibilita sempre esplicita, eccetto membri delle interfacce, e la piu restrittiva possibile. Ordine
modificatori: accessibilita, `static`, `abstract`/`virtual`/`override`/`sealed`, `readonly`, `async`, `unsafe`, tipo.
Le classi concrete non progettate per ereditarieta dovrebbero essere `sealed`, salvo entity/proxy/framework.

Preferire primary constructor per ricevere e conservare dipendenze/valori; costruttore tradizionale per logica,
validazione, trasformazione o framework. Nessuna conversione automatica di fixture o tipi generati.

Preferire auto-property; backing field solo per comportamento reale. `init`, `private set`, `set` esprimono il
lifecycle. Campi non riassegnabili `readonly`; costanti compile-time `const`; valori runtime condivisi
`static readonly`. Metodo privato indipendente dallo stato `static` quando semanticamente corretto. Omettere
`this.` salvo ambiguita o requisito tecnico.

### 3.4 Null, var e inizializzazioni

La nullability esprime il contratto, non silenzia warning. `= null!;` e ammesso per navigation EF obbligatorie
inizializzate dal framework. Controlli con `is null`/`is not null`; `?.`, `??`, `??=` per casi semplici, guard
clause quando null richiede una decisione. Mai `!` al posto di validazione.

Usare `var` se il tipo e evidente; tipo esplicito se chiarisce astrazione o risultato. Preferire target-typed
`new` e collection expressions (`[]`) se il contesto mantiene evidente il tipo. Negli elenchi multilinea
mantenere la trailing comma quando consentita; non serve inline.

### 3.5 Espressioni e controllo di flusso

Preferire expression-bodied member per una singola espressione. Incorporare una variabile assegnata e subito
restituita, salvo dia nome a un concetto, sia riusata o separi passaggi significativi.

Preferire guard clause. Estrarre condizioni che combinano piu concetti; non rinominare una singola proprieta.
Usare ternari solo per scegliere tra due valori, mai annidati. Restano inline finche entrano e, quando spezzati,
mantengono su ogni riga il massimo numero di elementi completi:

```csharp
return shortCondition ? LongButStillFittingTrueValue()
    : FalseValueThatWouldExceedTheRecommendedLineLength();

return thisIsAVeryLongConditionThatStillRemainsReadable()
    ? ShortTrueValue() : ShortFalseValue();
```

Preferire switch expression se ogni ramo produce un valore; switch tradizionale per piu istruzioni. Pattern
matching preferito a verifiche manuali equivalenti finche leggibile.

### 3.6 Stringhe, valori e tipi

String interpolation per messaggi; concatenazione/API dedicate per segmenti tecnici, path e URL; raw literals
per template multilinea. Confronti espliciti: `Ordinal`/`OrdinalIgnoreCase` per identificatori, chiavi e path,
cultura solo per testo utente. Non usare case conversion solo per confrontare. Structured logging rinviato al BL
dedicato ai log.

Valori significativi/ripetuti diventano costanti, enum o options; letterali autoesplicativi (`Count == 0`)
restano inline; valori di ambiente in configurazione. Enum singolari, membri PascalCase, nessun suffisso `Enum`;
numeri espliciti se persistiti/interoperabili; `[Flags]` solo per combinazioni, potenze di due e `None = 0`.

Record per dati con uguaglianza strutturale (DTO, options, risultati, value object); classi per identita, lifecycle,
stato o comportamento; entity EF normalmente classi. Tuple nominate solo per risultati locali semplici; record
per contratti pubblici o trasversali. Tipi anonimi solo locali; evitare `dynamic`, circoscriverlo ai confini.

Usare `nameof` per simboli e `typeof` per tipi; stringhe per contratti esterni stabili.

### 3.7 Collezioni, LINQ e lambda

Restituire il contratto meno specifico corretto: `IEnumerable<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`.
Collezioni vuote mai null; materializzare se il risultato deve essere stabile o dipende da risorse in chiusura.
Usare `Count` se disponibile, `Any()` per sequenze/predicati. `Single`, `First` e `OrDefault` esprimono contratti
diversi.

LINQ per trasformazioni, filtri, ordinamenti e aggregazioni leggibili; ciclo esplicito per flussi complessi.
`List<T>.ForEach` e ammesso per una singola operazione sincrona, preferendo method group se inoltra gli stessi
argomenti. Non usare `ToList()` solo per `ForEach`. Per async sequenziale usare `foreach` con `await`;
`Parallel.ForEachAsync` solo per concorrenza intenzionale e indipendente; mai lambda async in `List<T>.ForEach`.

Lambda a espressione per callback brevi, blocco per piu istruzioni; estrarre se lunga, riusata o semanticamente
autonoma. Method group se chiaro e non ambiguo.

### 3.8 Metodi, dipendenze, async e risorse

Un metodo opera a un livello di astrazione. Estrarre blocchi con nome significativo, riuso o dettagli distraenti,
non wrapper minuscoli. Nessun limite arbitrario di righe. Evitare `ref`/`out` per risultati multipli. Gruppi
stabili di parametri diventano request/options; niente wrapper solo per abbassare il conteggio. Evitare booleani
ambigui nelle firme.

Collaboratori con I/O, configurazione, lifecycle o sostituibilita arrivano via DI; dati/value object si creano
direttamente. Nessun obbligo preventivo interfaccia+implementazione.

Delega async pura restituisce il task; `async`/`await` per composizione, risultato, eccezioni/finally o logica
successiva. `CancellationToken` ultimo, nome `cancellationToken`, sempre propagato; opzionale solo al confine dove
lo e davvero. Niente `.Wait()`/`.Result`. Niente `ConfigureAwait(false)` nel codice applicativo; eventuale scelta
uniforme e documentata solo per librerie riutilizzabili.

`using var` fino a fine scope, blocco `using` per durata ridotta, `await using` per risorse async; niente Dispose
manuale se il lifecycle e esprimibile dal linguaggio.

### 3.9 Errori, validazione, attributi e direttive

Lanciare l'eccezione piu specifica, messaggi italiani, `nameof` per parametri. Catturare solo per traduzione,
contesto, compensazione, cleanup o recovery; `throw;` preserva lo stack. Validazioni standard con API framework,
regole specifiche con guard clause; niente duplicazione difensiva meccanica in ogni livello.

Attributi immediatamente sopra la dichiarazione, senza riga vuota. Una riga per attributo con argomenti o
responsabilita diverse; semplici e correlati possono condividere la riga.

Evitare `#if` nel codice applicativo; usarlo per reali differenze compile-time/piattaforma. `#pragma warning`
circoscritto e motivato in italiano. `partial` solo per generatori/framework, non per spezzare classi grandi.

Extension method in classi statiche dedicate, per operazioni naturali sul tipo; non nascondono dipendenze o
logica applicativa e restano circoscritti al dominio appropriato.

### 3.10 XML documentation

Usare `///` per API pubbliche consumate fuori dal progetto, soprattutto Shared Framework, Contracts e interfacce.
Descrivere contratto, semantica, vincoli ed effetti, in italiano; non ripetere il nome. `<exception>` solo per
eccezioni contrattuali; `<inheritdoc />` nelle implementazioni. Non e obbligatoria per ogni public interno.
I warning XML si riattivano selettivamente dopo l'applicazione della baseline.

## 4. PHP

File applicativi: `<?php`, riga vuota, `declare(strict_types=1);`. Graffe obbligatorie sulla stessa riga.
`PascalCase` per tipi, `camelCase` per metodi/proprieta/parametri/locali, `UPPER_SNAKE_CASE` per costanti. Tipi di
parametri, proprieta e ritorni quando rappresentabili; null solo se semantico; confronti `===`/`!==`.

Finche manca autoload, `require_once` subito dopo strict types, path da `__DIR__`, ordine logico/alfabetico.
Composer/autoload e un'evoluzione eventuale di TD-0006 solo a fronte di vantaggi reali. DI per collaboratori con
I/O/configurazione, ma il refactoring esistente si valuta separatamente dal puro stile.

Sempre `[]`, mai `array()`. Associativi solo per strutture locali semplici; DTO/classi per contratti stabili.
Array multilinea: una voce per riga e trailing comma. Apici singoli senza interpolazione, doppi con
interpolazione; graffe sulle variabili se chiariscono. Concatenazione per segmenti tecnici/path.

`isset` per esistenza e non-null; `array_key_exists` se la chiave conta anche con null; `empty` solo se tutti i
suoi valori "vuoti" hanno la stessa semantica; altrimenti confronto esplicito. `??` per fallback. Mai `@` per
sopprimere errori.

Eccezioni per errori eccezionali, null/false/risultati tipizzati per esiti previsti; convenzione coerente nello
stesso livello. Catturare solo per contesto/traduzione/recovery, preservando `previous`. Messaggi italiani.

Le view contengono markup e decisioni semplici; dati/API/query/logica in controller o service. Nel markup usare
`if ... endif`/`foreach ... endforeach`. Escaping contestuale obbligatorio (`htmlspecialchars`, `rawurlencode`).
Niente logica complessa in `<?= ... ?>`; variabili preparatorie ammesse per separare rendering e preparazione.

## 5. JavaScript

`const` per default, `let` per riassegnazione, mai `var`. `camelCase` per variabili/funzioni, `PascalCase` classi,
`UPPER_SNAKE_CASE` vere costanti globali. Apici singoli e template literal per interpolazione. Graffe obbligatorie
sulla stessa riga, `===`/`!==`. Arrow function per callback/locali brevi; `function` se hoisting/leggibilita aiuta.

Preferire async/await a catene lunghe; delega pura restituisce promise. `fetch` controlla `response.ok`.
`Promise.all` per concorrenza intenzionale; `for...of` con await per sequenzialita; mai `forEach(async ...)`.

Query DOM una volta per elementi riusati, null-check per opzionali. `data-*` per comportamento, non classi CSS.
Eventi con `addEventListener`, non inline. `textContent` per testo; `innerHTML` solo per markup controllato.

## 6. CSS

Classi kebab-case orientate a componente/ruolo (`album-card__title`, `album-card--restricted`). Ordine proprieta:
layout/posizione, dimensioni/spazi, tipografia, colori/sfondi/bordi, effetti/transizioni, specifiche/responsive.
Custom property per valori condivisi/semantici; selettori poco specifici; `!important` solo per override esterni
documentati.

Separare file quando emerge componente, pagina o responsabilita autonoma: `base.css`, `layout.css`,
`components/<nome>.css`, `<pagina>.css`; `components.css` non diventa un contenitore indistinto. File kebab-case,
con varianti e media query del componente. Nessun file per poche regole senza identita.

## 7. SQL

Keyword maiuscole; nomi coerenti col database esistente. Colonne esplicite, niente `SELECT *` salvo diagnostica;
valori sempre parametrizzati. `FROM`, `WHERE`, `GROUP BY`, `HAVING`, `ORDER BY` sempre su nuova riga. `JOIN`
indentati rispetto a `FROM`; `ON` inline finche entra, poi ulteriormente indentato.

Le condizioni formano un albero visivo: si spezza prima dell'operatore del livello corrente (`AND` o `OR`), ogni
ramo ha una riga; gruppi annidati dell'altro operatore sono parentetizzati e indentati ulteriormente.

```sql
SELECT album.id, album.name, photo.id
FROM albums AS album
    INNER JOIN photos AS photo ON photo.album_id = album.id
WHERE album.is_active = 1
    AND (
        album.kind = 'PhotoAlbum'
            OR (
                album.kind = 'Collection'
                AND album.child_count > 0
            )
    )
    AND album.is_deleted = 0
ORDER BY album.name;
```

Alias semantici e `AS` esplicito; con piu tabelle qualificare tutte le colonne; evitare alias a una lettera salvo
query locali inequivocabili. Con una tabella l'alias e facoltativo. Script versionati hanno nome/scopo/ordine
chiari; modifiche distruttive richiedono strategia di ripristino.

## 8. Enforcement e quality gate

`.editorconfig`, formatter, analyzer e gli script in `Tools/CodeStyle` applicano solo regole deterministiche:
encoding, newline, spazi/tab, graffe, namespace, using, un solo tipo per file e corrispondenza tra tipo, nome del
file, progetto e gerarchia delle cartelle. I sorgenti generati, incluse migration EF e file `*.Designer.cs`, sono
esclusi dai controlli strutturali. Nessun wrap automatico a 100-105. Region, classe/record, estrazione metodi,
LINQ/ciclo, builder, AAA e commenti richiedono review umana. Le correzioni automatiche non alterano comportamento,
non convertono indiscriminatamente costruttori e non riorganizzano semanticamente membri.

Prima di ogni commit:

1. identificare aggiunte, modifiche, rename ed eliminazioni rispetto a `HEAD`;
2. revisionare integralmente tutti i file coinvolti, non solo l'ultima modifica incrementale;
3. applicare fix automatici deterministici e sicuri;
4. eseguire formatter, analyzer, build e test pertinenti;
5. correggere le violazioni certe;
6. esaminare e correggere o accettare motivatamente i warning semantici;
7. riesaminare la diff completa finale;
8. escludere file generati, segreti, database, log e publish non previsti.

La parte deterministica C# si esegue dalla root del repository con:

```powershell
./Tools/CodeStyle/Invoke-CodeStyle.ps1
```

L'opzione `-Fix` applica esclusivamente whitespace e diagnostiche esplicitamente ammesse dallo script. Le migrazioni
EF generate sono escluse. Il comando senza opzioni non modifica i sorgenti e fallisce se trova scostamenti. Fino al
completamento della bonifica della baseline, l'esito fotografa il debito residuo e non viene usato come quality gate
globale bloccante.

**Error** blocca il commit; **Warning** richiede disposizione; **Info** non blocca. Il riepilogo descrive la diff
completa da `HEAD`; per file nuovi l'intero contenuto rilevante, essenziale anche per deploy manuali Portfolio.Web.

## 9. Riferimenti

- [MPS Playbook](MpsPlaybook.md)
- [Testing Conventions](TestingConventions.md)
- [Code Review](CodeReview.md)
- [Testing Architecture](../Architecture/TestingArchitecture.md)
