# Roadmap di MultiPurposeServer

## 1. Scopo

Questo documento descrive la sequenza intenzionale di evoluzione di MultiPurposeServer.

La [Visione](Vision.md) conserva le direzioni di lungo periodo. Il [Backlog](Backlog.md) registra tutto il lavoro funzionale noto. `ProjectStatus.md` rappresenta invece la fonte autorevole sull'attività corrente e prevale in caso di divergenza.

La Roadmap non duplica le singole attività: stabilisce quali risultati appartengono a `Now`, `Next` e `Later` e rimanda alle relative fonti.

---

## 2. Now

### Migliorie UI e UX

La milestone raccoglie interventi mirati a rendere Portfolio.Web più curato, riconoscibile e semplice da usare, migliorando la resa delle immagini, i flussi di condivisione, l'identità editoriale e i contenuti proposti agli utenti.

La milestone comprende:

- `BL-0008`, completare lo sharing automatico da Portfolio.Web;
- `BL-0014`, valorizzare ModelBook.Cloud nel footer di Portfolio.Web;
- `BL-0017`, valutare la condivisione degli album su Instagram;
- `BL-0019`, introdurre uno smart crop locale per le cover;
- `BL-0031`, migliorare il ritaglio delle copertine nell'elenco degli articoli;
- `BL-0032`, raccontare la nascita del calendario Germana 2023.

La milestone è conclusa quando i sei elementi sono completati oppure quando un'eventuale impossibilità tecnica emersa durante l'analisi è documentata e determina una decisione esplicita sul relativo elemento. Priorità e stato iniziale restano quelli registrati nel Backlog; l'ordine operativo viene definito all'avvio dei lavori.

---

## 3. Next

Risultati candidati successivi alla milestone corrente:

- definizione di una Engineering Baseline minima prima di nuovi sviluppi estesi: convenzioni C#, architettura di persistenza, guida editoriale e regole operative per il codice generato con AI;
- debito tecnico di priorità Alta, a partire da `TD-0001` e `TD-0002`;
- SEO e contenuti editoriali estesi di Portfolio.Web;
- evoluzioni funzionali di Portfolio non necessarie al traffico imminente, incluso lo sharing automatico.

---

## 4. Later

Direzioni già riconosciute ma non pianificate:

- evoluzione dei workflow fotografici e amministrativi di Portfolio;
- diagnostica amministrativa delle cache di Portfolio, da integrare nel futuro Portfolio.Admin;
- avvio del dominio ModelBook;
- avvio del dominio Skating;
- avvio del dominio BoardGameUniverse;
- avvio degli altri domini descritti nella Visione;
- client Web, Mobile, Desktop e amministrativi dei domini;
- integrazioni social e workflow di pubblicazione;
- valutazione delle capacità AI descritte nella Visione.

Queste direzioni diventano milestone soltanto quando vengono selezionate e definite con un risultato verificabile.

---

## 5. Regole di aggiornamento

- `Now` contiene una sola milestone principale, salvo interruzioni esplicite.
- `Next` contiene risultati candidati già sufficientemente concreti per essere valutati.
- `Later` contiene direzioni riconosciute ma non pianificate.
- Il Backlog conserva le singole attività senza attribuire loro automaticamente una posizione nella Roadmap.
- Il debito tecnico rimane nel registro dedicato e viene richiamato qui soltanto quando influenza la sequenza delle milestone.
- Ogni cambio di `Now` deve aggiornare anche `ProjectStatus.md`.

---

## Riferimenti

- [Visione](Vision.md)
- [Backlog](Backlog.md)
- [Project Status](../ProjectStatus.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
- [Architecture Roadmap](../Architecture/ArchitectureRoadmap.md)
