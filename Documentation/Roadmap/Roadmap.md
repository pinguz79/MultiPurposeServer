# Roadmap di MultiPurposeServer

## 1. Scopo

Questo documento descrive la sequenza intenzionale di evoluzione di MultiPurposeServer.

La [Visione](Vision.md) conserva le direzioni di lungo periodo. Il [Backlog](Backlog.md) registra tutto il lavoro funzionale noto. `ProjectStatus.md` rappresenta invece la fonte autorevole sull'attività corrente e prevale in caso di divergenza.

La Roadmap non duplica le singole attività: stabilisce quali risultati appartengono a `Now`, `Next` e `Later` e rimanda alle relative fonti.

---

## 2. Now

### Preparazione di Portfolio.Web al traffico fotografico imminente

La milestone prepara Portfolio.Web al flusso reale di selezione delle fotografie di uno shooting del 2026-08-09 e al traffico prodotto dalla successiva pubblicazione social.

Il risultato atteso è rendere affidabile la consultazione dell'album, permettere l'identificazione non ambigua delle fotografie, valorizzare il traffico tramite Altervista e presentare correttamente il link dell'album quando viene condiviso manualmente.

La milestone comprende:

- `BL-0001`, caricamento affidabile degli album annidati — completato e verificato in produzione il 2026-08-08;
- test di non regressione costruiti sulla causa effettiva di `BL-0001` — completati;
- `BL-0002`, codice foto leggibile nella preview anche su mobile;
- verifica del percorso reale dell'album destinato alla selezione;
- `BL-0006`, integrazione pubblicitaria Altervista;
- `BL-0007`, URL e metadati essenziali per la condivisione manuale sui social;
- `BL-0009`, verifica ricorsiva della navigabilità pubblica attraverso API e sito in produzione — completata il 2026-08-08 e conservata come controllo ripetibile.

La scadenza operativa non coincide con lo shooting: le funzionalità devono essere disponibili prima dell'invio del link alla modella e, per pubblicità e presentazione social, non oltre la pubblicazione che produrrà il traffico aggiuntivo.

`BL-0008`, sharing automatico da Portfolio.Web, rimane fuori dal percorso critico perché esiste un workaround manuale.

---

## 3. Next

Risultati candidati successivi alla milestone corrente:

- definizione di una Engineering Baseline minima prima di nuovi sviluppi estesi: convenzioni C#, architettura di persistenza, guida editoriale e regole operative per il codice generato con AI;
- debito tecnico di priorità Alta, a partire da `TD-0001` e `TD-0002`;
- SEO e contenuti editoriali estesi di Portfolio.Web;
- valutazione e successiva integrazione di Google AdSense;
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
