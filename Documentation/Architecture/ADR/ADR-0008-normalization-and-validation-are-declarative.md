# ADR-0008 — La normalizzazione e la validazione dei Contracts sono dichiarative

## Stato

Accettato

---

## Contesto

Le Request devono dichiarare vincoli di presenza, relazioni tra proprietà, normalizzazione delle stringhe e trattamento ricorsivo degli oggetti figli.

Implementare questi controlli direttamente nei Controller o ripeterli in ogni endpoint produrrebbe:

- duplicazione;
- comportamenti incoerenti;
- Controller accoppiati ai dettagli dei Contracts;
- difficoltà nel verificare che tutti i DTO siano configurati correttamente;
- dipendenza implicita dall'ordine con cui le operazioni vengono eseguite.

Era inoltre necessario distinguere chiaramente due responsabilità:

- normalizzare un dato, portandolo in una forma canonica;
- validare un dato, verificando che rispetti il contratto.

---

## Decisione

La normalizzazione e la validazione dei Contracts vengono dichiarate tramite attributi applicati alle proprietà.

Il framework condiviso interpreta tali attributi e costruisce piani riutilizzabili per tipo.

Gli attributi di normalizzazione comprendono attualmente:

- `NormalizeAttribute`;
- `NormalizeChildrenAttribute`.

Gli attributi di validazione comprendono attualmente:

- `RequiredAttribute`;
- `RequiredAtLeastOneAttribute`;
- `RequiredAtLeastOneTrueAttribute`;
- `ValidateChildrenAttribute`.

La normalizzazione viene eseguita prima della validazione, ma ogni regola di validazione deve mantenere un contratto corretto anche quando utilizzata autonomamente.

Per esempio, `Required` applicato a una stringa considera mancanti:

- `null`;
- stringhe vuote;
- stringhe composte soltanto da whitespace.

Questa semantica non dipende dall'esecuzione preventiva del Normalizer.

Le regole che verificano la presenza condividono una definizione coerente di valore mancante.

Le regole specifiche, come `RequiredAtLeastOneTrue`, non modificano invece la semantica generale dei value type.

La reflection viene utilizzata durante la costruzione del piano relativo a un tipo.

I piani vengono memorizzati in cache e riutilizzati; l'esecuzione sulle singole istanze utilizza getter e setter già compilati.

I test sono separati per responsabilità:

- i test del Normalizer verificano il comportamento del motore di normalizzazione;
- i test del Validator verificano il comportamento del motore di validazione;
- i test dei DTO verificano esclusivamente che gli attributi richiesti siano presenti e configurati correttamente;
- i test contrattuali verificano anche la coerenza tra proprietà parent e child.

I test dei DTO non devono invocare `Normalize()` o `Validate()` per dimostrare la presenza di un attributo, perché ciò li renderebbe dipendenti dal comportamento dei motori e produrrebbe fallimenti ambigui.

---

## Conseguenze

### Vantaggi

- I Contracts rendono espliciti i propri vincoli.
- I Controller non contengono normalizzazione o validazione ripetitive.
- Normalizzazione e validazione rimangono responsabilità separate.
- Il comportamento è uniforme tra domini ed endpoint.
- I piani in cache limitano il costo della reflection.
- Le operazioni Bulk possono elaborare molti elementi riutilizzando lo stesso piano.
- Gli attributi rendono visibile la configurazione direttamente sul DTO.
- I test dei motori rimangono focalizzati sul comportamento.
- I test dei DTO rimangono focalizzati sulla configurazione del contratto.
- La separazione dei test consente di individuare rapidamente se una regressione appartiene al framework oppure al DTO.

### Costi

- Il comportamento completo di una Request non è visibile soltanto dal codice del Controller.
- Una configurazione errata degli attributi può emergere durante la costruzione del piano.
- Il framework deve produrre errori di configurazione chiari e specifici.
- L'aggiunta di nuovi attributi richiede una regola, il relativo wiring nel motore e test dedicati.
- Reflection e cache introducono complessità infrastrutturale.
- I test contrattuali devono essere aggiornati quando cambia intenzionalmente il contratto di un DTO.

---

## Vedi anche

- `Architecture.md`
- `SharedFramework.md`
- `DomainArchitecture.md`
- `InfrastructureArchitecture.md`
- `TestingArchitecture.md`
- `MpsPlaybook.md`
