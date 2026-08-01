# GitHub Copilot

Questo documento definisce le modalità di collaborazione specifiche per GitHub Copilot durante lo sviluppo di MultiPurposeServer.

Le regole di carattere generale appartengono al **MpsPlaybook** e non devono essere duplicate.

Questo documento descrive esclusivamente le caratteristiche operative di GitHub Copilot e il modo in cui dovrebbe assistere lo sviluppo del progetto.

---

## 1. Ruolo

GitHub Copilot viene utilizzato principalmente come supporto alla scrittura del codice.

Il suo obiettivo è velocizzare l'implementazione mantenendo piena coerenza con l'architettura e con le convenzioni del progetto.

---

## 2. Utilizzo del contesto

Prima di proporre nuovo codice è opportuno utilizzare il contesto disponibile nel repository.

In particolare:

- seguire i pattern già esistenti;
- privilegiare la coerenza rispetto all'originalità;
- riutilizzare le convenzioni adottate dal progetto.

---

## 3. Produzione del codice

Il codice prodotto dovrebbe:

- seguire il MpsPlaybook;
- rispettare l'architettura del progetto;
- utilizzare le convenzioni C# adottate;
- evitare codice duplicato non necessario;
- privilegiare la leggibilità.

---

## 4. Refactoring

Durante i refactoring:

- privilegiare modifiche incrementali;
- evitare modifiche funzionali non richieste;
- preservare il comportamento esistente;
- rispettare la suite di test.

---

## 5. Pattern architetturali

GitHub Copilot dovrebbe sempre preferire:

- pattern già presenti nel progetto;
- componenti esistenti;
- estensione dell'architettura corrente.

Nuove astrazioni dovrebbero essere introdotte solo quando realmente necessarie.

---

## 6. Limitazioni

GitHub Copilot produce suggerimenti di codice ma non possiede una visione completa del progetto.

Per decisioni architetturali, refactoring significativi o evoluzione del framework condiviso è opportuno fare riferimento al MpsPlaybook e alla documentazione del progetto.