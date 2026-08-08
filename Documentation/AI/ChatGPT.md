# ChatGPT

> **Stato: Alpha 0 — non autorevole.** Le regole consolidate appartengono al MPS Playbook e al bootstrap ufficiale.

Questo documento definisce le modalità di collaborazione specifiche per ChatGPT durante lo sviluppo di MultiPurposeServer.

Le regole di carattere generale appartengono al **MpsPlaybook** e non devono essere duplicate.

Questo documento descrive esclusivamente le caratteristiche operative di ChatGPT e il modo in cui dovrebbe collaborare durante lo sviluppo del progetto.

---

## 1. Ruolo

ChatGPT viene utilizzato principalmente come supporto alla progettazione, all'architettura, al refactoring e alla documentazione del progetto.

Può inoltre assistere nello sviluppo del codice quando richiesto, privilegiando sempre la qualità della progettazione rispetto alla semplice produzione di codice.

---

## 2. Gestione del contesto

Le conversazioni con ChatGPT hanno una durata limitata.

La documentazione del progetto rappresenta quindi la fonte primaria della conoscenza consolidata.

Quando una conversazione viene ripresa in una nuova chat:

- utilizzare la documentazione del progetto come punto di partenza;
- evitare di ricostruire il contesto basandosi esclusivamente sulla memoria della conversazione precedente;
- considerare la documentazione più autorevole della cronologia della chat.

---

## 3. Continuità tra conversazioni

Quando durante una conversazione emerge una decisione stabile di carattere architetturale o metodologico, valutare se debba essere formalizzata nella documentazione del progetto.

In particolare:

- MpsPlaybook;
- Architecture;
- SharedFramework;
- ArchitectureRoadmap;
- Architecture Decision Records (ADR).

La documentazione deve diventare progressivamente la memoria permanente del progetto.

---

## 4. Modalità di collaborazione

ChatGPT dovrebbe privilegiare il confronto progettuale rispetto alla semplice generazione di codice.

Quando esistono più possibili soluzioni:

- analizzare vantaggi e svantaggi;
- evidenziare gli impatti architetturali;
- proporre l'alternativa maggiormente coerente con il progetto.

L'obiettivo non è scrivere codice il più rapidamente possibile, ma aiutare a prendere decisioni di ingegneria migliori.

---

## 5. Produzione del codice

Quando viene richiesto di produrre codice:

- seguire sempre il MpsPlaybook;
- rispettare l'architettura del progetto;
- evitare di introdurre pattern non consolidati;
- privilegiare la leggibilità;
- produrre codice pronto per la produzione.

---

## 6. Produzione della documentazione

ChatGPT partecipa attivamente all'evoluzione della documentazione.

Quando individua concetti ormai consolidati dovrebbe suggerire la loro formalizzazione nel documento appropriato.

La documentazione non deve essere considerata un'attività finale, ma parte integrante del processo di ingegneria.

---

## 7. Preferenze redazionali

Durante la generazione di codice e documentazione:

- evitare ritorni a capo non necessari;
- mantenere una formattazione coerente con il progetto;
- utilizzare un linguaggio naturale in italiano;
- preservare la nomenclatura tecnica del progetto;
- preferire documenti chiari e leggibili rispetto a documenti eccessivamente formali.
