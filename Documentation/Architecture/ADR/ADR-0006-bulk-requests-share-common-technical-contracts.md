# ADR-0006 — Le Request Bulk condividono contratti tecnici comuni

## Stato

Accettato

## Ambito

Shared Framework

## Data della decisione

2026-08-06

## Origine

`ADR-ALPHA-0007` e consolidamento di `SharedFramework.md`.

## Contesto

Più endpoint devono elaborare collezioni di Request. Lasciare a ogni dominio la definizione completa del contenitore, delle opzioni e del ciclo tecnico produrrebbe contratti incoerenti e duplicazione infrastrutturale.

Le operazioni bulk possono però avere dipendenze tra item, ordinamenti, strategie di persistenza ed errori differenti. Non devono essere ridotte a semplici collezioni di elementi sempre indipendenti.

## Decisione

Le Request Bulk adottano contratti tecnici comuni nello Shared Framework.

Il contenitore espone opzioni e collezione degli item; ogni item partecipa alle convenzioni di `IRequest`. Shared possiede normalizzazione, validazione e orchestrazione tecnica, mentre il dominio possiede la semantica dell'operazione e degli errori.

La request contenitore viene verificata prima di qualsiasi persistenza. Errori globali, opzioni non valide o duplicati invalidano l'intera richiesta.

Le strategie di persistenza e di valutazione degli errori sono concetti indipendenti. Gli item possono inoltre dichiarare capacità opzionali, come identificazione e ordinabilità intrinseca, senza esporre al framework la propria semantica.

Il contratto attuale implementa soltanto una parte di questa direzione. Le API future di strategie, chiavi, ordinamento e risultati non sono definite da questo ADR e richiedono progettazione dedicata.

## Conseguenze

### Positive

- Le API bulk condividono convenzioni tecniche coerenti.
- Normalizzazione e validazione comuni non vengono duplicate dai domini.
- L'evoluzione può supportare strategie e risultati uniformi.
- La semantica applicativa rimane nel dominio.

### Negative

- I domini devono rispettare il contenitore e il lifecycle tecnico comuni.
- L'evoluzione del contratto richiede attenzione alla compatibilità.
- Dipendenze, ordinamento e atomicità aumentano la complessità dell'orchestrazione Shared.

## Alternative considerate

### Contratti bulk indipendenti per dominio

Scartati perché duplicherebbero meccanismi tecnici e produrrebbero API incoerenti.

### Assumere item sempre indipendenti

Scartato perché gerarchie e riferimenti tra item possono richiedere ordine e propagazione degli errori.

### Fissare ora tutte le API future

Scartato per evitare contratti prematuri su capacità non ancora implementate.

## Riferimenti

- [Shared Framework](../SharedFramework.md)
- [Bulk Operations](../BulkOperations.md)
