# Chat Recovery

## Scopo

Questo documento descrive la procedura che un assistente AI dovrebbe seguire quando inizia una nuova conversazione relativa al progetto MultiPurposeServer.

L'obiettivo è ricostruire rapidamente il contesto del progetto utilizzando il repository come fonte autorevole, riducendo la dipendenza dalla memoria della conversazione precedente.

---

## Principio fondamentale

Il repository rappresenta la fonte autorevole del progetto.

La memoria della conversazione può aiutare a comprendere il contesto, ma non deve mai prevalere sul contenuto effettivo del repository.

In caso di conflitto, prevale sempre il repository.

---

## Procedura di bootstrap

Prima di analizzare il codice, eseguire code review o proporre modifiche, l'assistente dovrebbe seguire il seguente ordine.

### 1. Leggere il README

Leggere il file `README.md` del repository.

Il README descrive:

- l'identità della piattaforma;
- l'organizzazione del repository;
- il percorso di documentazione consigliato.

---

### 2. Leggere la documentazione architetturale

Seguire il percorso indicato nel README.

In particolare:

1. Home
2. Platform

ed eventuali documenti richiamati da essi.

Lo scopo è comprendere:

- gli obiettivi della piattaforma;
- i principi architetturali;
- l'organizzazione dei domini;
- le convenzioni adottate.

---

### 3. Leggere lo stato del progetto

Prima di analizzare il codice, leggere [Project Status](ProjectStatus.md).

Il documento descrive:

- i livelli di stabilità della documentazione;
- lo stato corrente del progetto;
- la milestone attiva;
- l'attività da riprendere;
- l'avanzamento della milestone;
- il debito tecnico noto.

Per milestone, priorità e attività corrente, `ProjectStatus.md` prevale sulla documentazione draft e sulla memoria della conversazione.

---

### 4. Ricostruire il contesto tecnico

Solo dopo aver compreso l'architettura generale è opportuno iniziare l'analisi del codice.

Quando possibile, è preferibile leggere direttamente i file coinvolti anziché basarsi su descrizioni presenti nella conversazione.

---

### 5. Utilizzare la memoria della conversazione

La memoria della conversazione dovrebbe essere utilizzata esclusivamente per recuperare:

- preferenze dell'utente;
- decisioni progettuali non ancora documentate;
- attività in corso;
- TODO ancora aperti.

La memoria non dovrebbe sostituire la lettura del repository.

---

### 6. Valutare la completezza della documentazione

Al termine del bootstrap l'assistente dovrebbe valutare se il repository contiene tutte le informazioni necessarie per comprendere il progetto.

Se durante il bootstrap è stato necessario ricorrere alla memoria della conversazione per comprendere aspetti strutturali, architetturali o procedurali del progetto, tali informazioni dovrebbero essere considerate candidate a entrare nella documentazione del repository.

L'obiettivo è ridurre progressivamente la dipendenza dalla memoria delle conversazioni e rendere il repository sempre più autosufficiente.

---

## Code Review

Prima di eseguire una code review l'assistente dovrebbe verificare di avere accesso alla versione aggiornata dei file interessati.

Non dovrebbe basare l'analisi su file parziali, versioni obsolete o ricostruzioni effettuate esclusivamente tramite memoria.

---

## Proposte di modifica

Ogni proposta dovrebbe essere coerente con:

- l'architettura del progetto;
- le convenzioni documentate;
- il coding style adottato;
- le decisioni progettuali già consolidate.

L'assistente dovrebbe evitare modifiche che introducano convenzioni differenti senza una motivazione esplicita.

---

## Cosa non fare

Durante la fase di bootstrap l'assistente non dovrebbe:

- iniziare una code review senza aver letto la documentazione minima del progetto;
- considerare la memoria della conversazione come fonte autorevole;
- assumere che il codice discusso in una chat precedente coincida con quello presente nel repository;
- proporre modifiche senza aver letto i file interessati;
- basare conclusioni su file incompleti o versioni obsolete;
- introdurre convenzioni architetturali o di coding style differenti da quelle già adottate;
- dedurre il comportamento del sistema senza verificarlo nel codice o nella documentazione.

---

## Checklist

Prima di iniziare qualsiasi attività verificare di aver completato tutti i seguenti passaggi.

- [ ] README letto.
- [ ] Home letta.
- [ ] Platform letta.
- [ ] Chat Recovery letta.
- [ ] Project Status letto.
- [ ] Documentazione collegata letta (quando necessaria).
- [ ] Architettura generale compresa.
- [ ] File aggiornati recuperati dal repository.
- [ ] Contesto della conversazione recuperato (se necessario).
- [ ] Completezza della documentazione valutata.
- [ ] Bootstrap completato.

---

## Obiettivo

Seguendo questa procedura, ogni nuova conversazione può ricostruire rapidamente il contesto del progetto mantenendo il repository come unica fonte autorevole della documentazione tecnica.

Parallelamente, ogni bootstrap rappresenta un'opportunità per migliorare progressivamente la documentazione del progetto, rendendo il repository sempre più completo e autosufficiente.
