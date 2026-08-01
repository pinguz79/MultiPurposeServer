# ADR-0008 — La normalizzazione e la validazione dei Contracts sono dichiarative

## Stato

Accettato

---

## Contesto

Con l'introduzione della pipeline condivisa per la normalizzazione e la validazione delle Request è emersa la necessità di semplificare ulteriormente la definizione dei Contracts.

Nella versione iniziale, ogni Request implementava direttamente la logica di:

- normalizzazione;
- validazione.

Questo approccio risultava corretto dal punto di vista funzionale, ma portava a distribuire la logica di validazione all'interno di numerosi Contracts, aumentando il codice ripetitivo e rendendo più difficile individuare a colpo d'occhio le regole applicate a una Request.

Le regole di validazione rappresentano infatti metadati dichiarativi e non comportamento applicativo.

---

## Decisione

Le Request definiscono le proprie regole mediante attributi dichiarativi.

Ad esempio:

- lunghezza minima e massima;
- campi obbligatori;
- intervalli numerici;
- espressioni regolari;
- altri vincoli dichiarativi.

La pipeline condivisa interpreta tali attributi durante la fase di validazione.

La normalizzazione continua invece a essere implementata solo quando realmente necessaria.

Le Request mantengono comunque la possibilità di implementare logica personalizzata attraverso `Validate()` qualora le regole richieste non siano esprimibili in forma dichiarativa.

Il modello risultante è quindi il seguente:

- validazione dichiarativa come comportamento predefinito;
- validazione procedurale solo quando strettamente necessaria.

---

## Conseguenze

### Vantaggi

- Le regole di validazione risultano immediatamente leggibili.
- I Contracts diventano prevalentemente dichiarativi.
- Viene ridotta la quantità di codice boilerplate.
- Le validazioni più comuni risultano uniformi tra tutti i domini.
- La pipeline condivisa può applicare automaticamente le regole definite dai Contracts.
- Le Request semplici richiedono pochissimo codice.

### Costi

- Alcune regole di business complesse non possono essere espresse esclusivamente tramite attributi.
- Gli sviluppatori devono distinguere chiaramente tra validazione dichiarativa e validazione procedurale.
- L'infrastruttura della pipeline deve supportare l'interpretazione coerente degli attributi utilizzati.

---

## Vedi anche

- `Architecture.md`
- `SharedFramework.md`
- `InfrastructureArchitecture.md`
- `TestingArchitecture.md`
- `MpsPlaybook.md`