# Finance — Domain Model

## 1. Scopo

Questo documento descrive il modello di dettaglio del dominio Finance e approfondisce i concetti definiti in [Finance — Dominio](Domain.md).

Il documento raccoglie le decisioni consolidate relative alle entità persistite, alle loro relazioni, ai contratti trasversali e alla valutazione delle Formule. I dettagli puramente implementativi non ancora decisi restano esplicitamente aperti.

## 2. Inventario

Il modello comprende dodici entità persistite:

1. `Conto`
2. `Movimento`
3. `Categoria`
4. `ParametroTemporale`
5. `Configurazione`
6. `Pianificazione`
7. `Periodicita`
8. `CorrelazioneMovimento`
9. `CorrelazionePianificazione`
10. `Casello`
11. `TariffaTratta`
12. `Pedaggio`

Sono inoltre previsti due contratti trasversali:

- `IEvaluable`;
- `IOverrideable`.

Gli identificatori delle entità sono previsti come `Guid`.

## 3. Entità

### 3.1 Conto

```text
Conto
├── Id
├── Nome
├── DisplayName
├── SaldoIniziale
├── Movimenti
├── Configurazioni
├── SaldoAllaData(data)       [calcolato]
└── Saldo                     [calcolato]
```

`Nome` costituisce l'identificatore logico del Conto. È stabile e non modificabile; viene mantenuto in PascalCase nei dati persistiti e può essere rappresentato in camelCase nelle Formule.

`DisplayName` è il nome human-readable destinato alla UI e può essere modificato senza alterare l'identità logica del Conto.

Non è prevista una `DataSaldoIniziale`: il saldo iniziale costituisce la base matematica del calcolo indipendentemente dalla data richiesta.

`SaldoAllaData(data)` restituisce il saldo iniziale sommato ai valori dei Movimenti con `Data <= data`.

`Saldo` equivale a `SaldoAllaData(Oggi())`.

### 3.2 Movimento

```text
Movimento : IEvaluable
├── Id
├── Data
├── Descrizione
├── Formula
├── ContoId
├── Conto
├── CategoriaId?
├── Categoria?
├── PianificazioneId?
├── Pianificazione?
├── CorrelazioniComeA
├── CorrelazioniComeB
├── Valore                    [calcolato]
└── Variabili                 [calcolabile]
```

Il valore economico del Movimento è determinato dalla `Formula`.

`PianificazioneId` rappresenta il legame operativo con la Pianificazione che gestisce il Movimento. Tale legame può essere rimosso dal consolidamento.

Le dipendenze contenute nella Formula possono essere ricavate a runtime e utilizzate per navigazione e impact analysis. Non è necessario persisterle nella prima versione.

### 3.3 Categoria

```text
Categoria
├── Id
├── Nome
├── DisplayName
├── Movimenti
├── ParametriTemporali
└── Pianificazioni
```

Le Categorie sono piatte e non introducono una gerarchia.

Le navigation inverse possono essere caricate lazy.

### 3.4 ParametroTemporale

```text
ParametroTemporale : IEvaluable, IOverrideable
├── Id
├── Nome
├── DisplayName
├── CategoriaId?
├── Categoria?
├── Formula
├── ValidoDa?
├── ValidoA?
├── Indice
├── ValoreAllaData(data)
├── Valore                    [calcolato]
└── Variabili                 [calcolabile]
```

Un ParametroTemporale rappresenta una voce economica utilizzabile direttamente nella generazione dei Movimenti, il cui valore può cambiare nel tempo.

Sono esempi di Parametri il canone di affitto, il valore ordinario dello stipendio, il canone periodico di un servizio o l'importo ordinario di una spesa ricorrente.

L'identità logica del gruppo di override è `Nome`.

`DisplayName` è obbligatorio ma può differire fra override dello stesso Parametro. Questo permette alla UI di rendere comprensibile il significato delle differenti definizioni temporali.

`Categoria` è opzionale.

`DisplayName` e `Categoria` costituiscono valori di default per l'authoring di una Pianificazione basata sul Parametro. Il valore effettivamente scelto viene memorizzato nella Pianificazione e non mantiene una dipendenza dinamica dal default del Parametro.

### 3.5 Configurazione

```text
Configurazione : IEvaluable, IOverrideable
├── Id
├── ContoId
├── Conto
├── Nome
├── Formula
├── ValidoDa?
├── ValidoA?
├── Indice
├── ValoreAllaData(data)
├── Valore                    [calcolato]
└── Variabili                 [calcolabile]
```

Una Configurazione rappresenta un'informazione funzionale associata a uno specifico Conto e necessaria a determinarne il comportamento o a calcolare valori economici. A differenza di un ParametroTemporale, non rappresenta direttamente una voce economica destinata a generare Movimenti.

Sono esempi di Configurazioni il plafond di una carta, il giorno di chiusura del ciclo di fatturazione, la rata ordinaria, la percentuale utilizzata per determinare la rata e il valore minimo previsto per la rata.

L'identità logica del gruppo di override è `ContoId + Nome`.

Il `Nome` non può collidere con proprietà persistite o calcolate di `Conto`.

Il modello temporale viene mantenuto anche per Configurazioni che nella pratica potrebbero non cambiare mai.

### 3.6 Pianificazione

```text
Pianificazione
├── Id
├── Descrizione
├── FormulaMovimento
├── DescrizioneMovimento
├── CategoriaId?
├── Categoria?
├── ValidoDa
├── ValidoA
├── PeriodicitaId
├── Periodicita
├── ContoId
├── Conto
├── Movimenti
├── CorrelazioniComeA
└── CorrelazioniComeB
```

`DescrizioneMovimento` è obbligatoria.

Quando una Pianificazione viene creata a partire da un ParametroTemporale, `DisplayName` e `Categoria` del Parametro possono essere utilizzati come valori di default. A livello di DTO possono quindi essere omessi per accettare i default oppure valorizzati per effettuare un override.

La Pianificazione memorizza il valore effettivo scelto: successive modifiche ai default del Parametro non modificano automaticamente la Pianificazione.

Una Pianificazione gestisce i Movimenti ad essa collegati finché il legame operativo non viene rimosso.

L'aggiornamento propaga selettivamente le proprietà corrispondenti:

```text
FormulaMovimento      -> Formula
DescrizioneMovimento  -> Descrizione
Categoria             -> Categoria
Periodicita           -> Data
ValidoDa / ValidoA    -> insieme dei Movimenti gestiti
```

Le modifiche manuali apportate ai Movimenti devono essere preservate quando non sono direttamente coinvolte dalla modifica della Pianificazione.

Le operazioni che comportano eliminazioni implicite, perdita di modifiche manuali o altri effetti potenzialmente distruttivi richiedono warning e approvazione dell'utente.

### 3.7 Periodicita

L'ordine previsto delle proprietà è:

```text
Periodicita
├── Id
├── Frequenza
├── Intervallo
├── GiornoSettimana?
├── SettimanaMese?
├── GiornoMese?
├── MeseAnno?
├── FineMese
└── Pianificazioni
```

La Periodicita è immutabile e condivisibile.

L'ancoraggio della ricorrenza appartiene alla Pianificazione ed è determinato da `ValidoDa`.

Per una ricorrenza ogni due venerdì:

```text
ValidoDa 20/08/2026 -> 21/08/2026, 04/09/2026, ...
ValidoDa 22/08/2026 -> 28/08/2026, 11/09/2026, ...
```

Vengono persistite solo Periodicita effettivamente utilizzate. La rimozione delle Periodicita non più referenziate è un problema di housekeeping e non modifica la semantica del dominio.

### 3.8 CorrelazioneMovimento

```text
CorrelazioneMovimento
├── Id
├── MovimentoAId
├── MovimentoA
├── MovimentoBId
└── MovimentoB
```

La relazione è simmetrica.

La coppia viene canonicalizzata in modo da rappresentare una sola volta la correlazione indipendentemente dall'ordine degli estremi.

Non sono consentite self-reference né duplicati A/B e B/A.

### 3.9 CorrelazionePianificazione

```text
CorrelazionePianificazione
├── Id
├── PianificazioneAId
├── PianificazioneA
├── PianificazioneBId
└── PianificazioneB
```

La relazione segue le stesse regole strutturali di `CorrelazioneMovimento`, ma rimane volutamente un'entità distinta.

Le correlazioni supportano l'impact analysis. Una modifica alla Pianificazione A può rendere necessario valutare una modifica alla Pianificazione B correlata; qualora B venga modificata, i Movimenti direttamente gestiti da B possono quindi risultare indirettamente impattati dalla modifica originaria di A.

La correlazione non implica propagazione automatica delle modifiche.

### 3.10 Casello

```text
Casello
├── Id
├── Nome
├── DisplayName
├── TariffeComeA
├── TariffeComeB
└── Tariffe                  [calcolata]
```

La distinzione fra `TariffeComeA` e `TariffeComeB` è necessaria alla relazione persistita.

La proprietà calcolata `Tariffe` espone invece semanticamente tutte le tariffe del Casello:

```csharp
TariffeComeA.Union(TariffeComeB)
```

### 3.11 TariffaTratta

```text
TariffaTratta : IEvaluable, IOverrideable
├── Id
├── CaselloAId
├── CaselloA
├── CaselloBId
├── CaselloB
├── Formula
├── ValidoDa?
├── ValidoA?
└── Indice
```

La coppia di Caselli è non orientata: la tariffa fra Genova Est e Genova Ovest è la stessa entità logica indipendentemente dalla direzione di percorrenza.

La coppia viene canonicalizzata per la persistenza.

L'identità logica del gruppo di override è `CaselloA + CaselloB`.

La tariffa è rappresentata mediante `Formula` e non mediante un semplice valore `decimal`, così da utilizzare lo stesso meccanismo di valutazione temporale degli altri concetti valutabili.

### 3.12 Pedaggio

```text
Pedaggio
├── Id
├── MovimentoId
└── Movimento
```

`Pedaggio` è una entity-marker associata a un Movimento.

La navigabilità è intenzionalmente disponibile soltanto da `Pedaggio` verso `Movimento`. `Movimento` non espone una navigation verso `Pedaggio`.

Data, descrizione, valore e Categoria appartengono al Movimento e non vengono duplicati.

La presenza di `Pedaggio` esprime semanticamente che il Movimento appartiene anche all'insieme dei pedaggi. La Categoria del Movimento può quindi continuare a rappresentare una dimensione differente, per esempio Lavoro o Svago.

Questo pattern può essere riutilizzato quando emerge la necessità di attribuire a un Movimento una seconda classificazione che rappresenta in realtà un concetto autonomo del dominio, prima di introdurre genericamente una relazione N:N con Categoria.

## 4. IEvaluable

Contratto concettuale:

```csharp
public interface IEvaluable
{
    string Formula { get; }

    object ValoreAllaData(DateOnly data)
        => Evaluator.Evaluate(this, data);
}
```

La firma concreta potrà essere adeguata durante l'implementazione, in particolare qualora l'Evaluator richieda un context.

Il contratto deve supportare almeno valori `decimal` per gli importi monetari e `int` per informazioni come il giorno di chiusura del ciclo di fatturazione.

Le implementazioni previste sono:

- `Movimento`;
- `ParametroTemporale`;
- `Configurazione`;
- `TariffaTratta`.

## 5. IOverrideable

Contratto concettuale:

```csharp
public interface IOverrideable
{
    DateOnly? ValidoDa { get; }
    DateOnly? ValidoA { get; }
    int Indice { get; }
}
```

Le implementazioni previste sono:

- `ParametroTemporale`;
- `Configurazione`;
- `TariffaTratta`.

`IOverrideable` definisce esclusivamente le informazioni comuni necessarie alla risoluzione temporale. Non conosce l'identità logica del gruppo di override, che dipende dall'entità:

```text
ParametroTemporale -> Nome
Configurazione     -> Conto + Nome
TariffaTratta      -> CaselloA + CaselloB
```

## 6. Formula ed Evaluator

### 6.1 Sintassi Finance

Finance definisce una sintassi di riferimento propria, indipendente dall'expression engine concreto.

La sintassi V1 è JS-like:

```text
letterali          21, 123.45
variabili          $affitto
member access      $helloCard.Plafond
self               $this.Plafond
operatori          + - * /
unario             -
raggruppamento     ( )
funzioni           min(...) max(...)
argomenti          ,
```

Il simbolo `$` identifica una variabile Finance.

L'operatore `.` consente l'accesso a un membro dell'oggetto risolto.

### 6.2 Risoluzione delle variabili

Una variabile semplice, per esempio `$affitto`, viene risolta individuando il ParametroTemporale corrispondente e selezionandone la definizione applicabile alla data di valutazione.

Per una variabile che identifica un Conto, per esempio `$helloCard.Plafond`:

1. viene risolto il Conto `HelloCard`;
2. `Plafond` viene cercato fra le proprietà persistite o calcolate del Conto;
3. se la proprietà non esiste, viene cercata una Configurazione `Plafond` associata al Conto;
4. la Configurazione applicabile viene valutata alla data richiesta.

`$this` rappresenta l'oggetto `IEvaluable` corrente.

### 6.3 Dipendenze

È utile poter ricavare le variabili dalle quali dipende una Formula.

Questa informazione può essere utilizzata per:

- navigazione;
- impact analysis;
- individuazione dei Movimenti influenzati da una modifica a un Parametro o a una Configurazione;
- warning prima di operazioni potenzialmente distruttive.

Le dipendenze non devono essere necessariamente persistite nella prima versione e possono essere ricavate dalla Formula.

La strategia per tradurre efficientemente queste ricerche in query database viene demandata all'implementazione.

### 6.4 Expression engine

La scelta dell'expression engine è demandata a uno spike tecnico comparativo fra NCalc e Dynamic Expresso.

Lo spike deve verificare almeno:

- operatori e funzioni richiesti;
- gestione dei tipi numerici;
- accesso ai membri;
- supporto del concetto di `this`;
- possibilità di individuare le variabili richieste dall'espressione;
- quantità di logica Finance necessaria per risolvere root e membri;
- possibilità di limitare il vocabolario ammesso;
- leggibilità della sintassi eventualmente persistita.

La sintassi Finance costituisce il riferimento. Se l'engine scelto supporta direttamente una sintassi equivalente e leggibile, Finance può allineare la propria sintassi prima che esistano Formule persistite. In caso contrario viene introdotto un adapter/translator.

## 7. Consolidamento

Il consolidamento ha lo scopo di rendere un Movimento indipendente dalle informazioni dinamiche utilizzate per calcolarlo.

Per un Movimento dinamico il risultato concettuale è:

```text
prima:
Formula = espressione dinamica
PianificazioneId = <id>

dopo:
Formula = valore costante valutato
PianificazioneId = null
```

Non è necessario uno stato persistito `Consolidato`.

Un Movimento con Formula già costante è già indipendente dalle condizioni al contorno, anche se la sua Data è futura.

Un Movimento futuro può essere consolidato anticipatamente quando si desidera congelarne esplicitamente il valore.

Il consolidamento non rende il Movimento immutabile e non impedisce successive operazioni di Bonifica.

## 8. Pianificazioni e impact analysis

Una Pianificazione gestisce i Movimenti ad essa collegati finché il relativo legame operativo rimane attivo.

La modifica di una proprietà della Pianificazione deve propagarsi esclusivamente alla proprietà corrispondente dei Movimenti gestiti e non deve sovrascrivere indiscriminatamente gli altri dati.

Gli override manuali devono essere preservati quando non sono direttamente coinvolti.

Le correlazioni fra Pianificazioni consentono di estendere transitivamente l'impact analysis. Una modifica non viene propagata automaticamente alle Pianificazioni correlate: Finance individua gli impatti e supporta l'utente nell'eventuale applicazione coordinata delle modifiche.

## 9. Telepass

### 9.1 Principi

Telepass non richiede un sottotipo di Conto.

I workaround utilizzati nel foglio Excel non vengono trasferiti nel modello:

- il parcheggio non è una tratta;
- il canone mensile non è una tratta;
- le due direzioni della stessa coppia di caselli non richiedono due tariffe.

Il canone può essere rappresentato come ParametroTemporale e utilizzato da una Pianificazione.

Un parcheggio è un normale Movimento.

### 9.2 Tariffario

`TariffaTratta` rappresenta il costo fra due Caselli indipendentemente dalla direzione.

Le variazioni nel tempo vengono rappresentate mediante override della stessa tariffa logica.

Un Movimento futuro relativo a un pedaggio può dipendere dinamicamente dalla TariffaTratta applicabile alla propria Data. L'introduzione di una nuova tariffa futura può quindi aggiornare le previsioni non consolidate senza modificare i Movimenti già congelati.

### 9.3 Pedaggi

`Pedaggio` identifica semanticamente un Movimento come pedaggio senza duplicarne i dati.

La Categoria del Movimento rimane libera di rappresentare un'altra dimensione:

```text
Movimento.Categoria = Lavoro
Pedaggio presente   = sì
```

È quindi possibile interrogare sia i Movimenti appartenenti alla Categoria sia l'insieme complessivo dei Pedaggi.

## 10. Decisioni volutamente aperte

Restano da definire durante gli spike o l'implementazione:

- firma definitiva dell'Evaluator e necessità di un context;
- tipo concreto di ritorno e conversioni di `IEvaluable`;
- expression engine;
- eventuale adapter/translator della sintassi;
- strategia efficiente per dependency e impact query lato database;
- dettagli EF, indici e constraint fisici;
- policy precisa per i valori mancanti durante la valutazione;
- strategia definitiva di cleanup delle Periodicita non più referenziate.

Questi punti non modificano le decisioni funzionali consolidate nel presente documento.
