# Roadmap di MultiPurposeServer

## 1. Scopo

Questo documento descrive la sequenza intenzionale di evoluzione di MultiPurposeServer.

La [Visione](Vision.md) conserva le direzioni di lungo periodo. Il [Backlog](Backlog.md) registra tutto il lavoro funzionale noto. `ProjectStatus.md` rappresenta invece la fonte autorevole sull'attività corrente e prevale in caso di divergenza.

La Roadmap non duplica le singole attività: stabilisce quali risultati appartengono a `Now`, `Next` e `Later` e rimanda alle relative fonti.

---

## 2. Now

### Consolidamento delle specifiche di coding

La milestone consolida `BL-0037` e chiude `TD-0005` e `TD-0006`. Il lavoro parte dalla rilevazione delle convenzioni prevalenti, definisce una baseline autorevole e riproducibile e la applica successivamente a server, client, Shared Framework e test senza introdurre modifiche funzionali.

`TD-0001` e `TD-0002` non appartengono a questa milestone: restano candidati per quella successiva perché riguardano la pipeline MVC e comportamenti applicativi.

### Ultima milestone completata: Automazione deploy

La milestone ha introdotto publish e deploy mirati tramite GitHub Actions per MPS su Aruba e Portfolio.Web su Altervista. Connessione e trasferimento FTPS sono stati verificati con sentinelle temporanee; il primo deploy applicativo reale rimane un collaudo operativo differito.

### Milestone precedente: Affidabilità e gestione Portfolio

La milestone ha compreso, nell'ordine operativo approvato:

- `BL-0020`, riprodurre e correggere la creazione duplicata di Album nella root — non riproducibile, bonificato e trasferito al monitoraggio differito;
- `BL-0013`, consentire un path esplicito nella creazione degli Album — completato;
- `BL-0034`, intercettare i percorsi legacy di ZenPhoto in Portfolio.Web — completato;
- `BL-0016`, sostituire Swagger UI con Scalar — completato e verificato in produzione.

La milestone è stata chiusa dopo aver completato gli altri tre elementi e aver dotato `BL-0020` di bonifica, test diagnostico e logging strutturato. Una nuova evidenza riattiverà il bug senza riaprire retroattivamente la milestone.

### Milestone precedente: Migliorie UI e UX

La milestone raccoglie interventi mirati a rendere Portfolio.Web più curato, riconoscibile e semplice da usare, migliorando la resa delle immagini, i flussi di condivisione, l'identità editoriale e i contenuti proposti agli utenti.

La milestone comprende:

- `BL-0031`, migliorare il ritaglio delle copertine nell'elenco degli articoli — completato;
- `BL-0014`, valorizzare ModelBook.Cloud nel footer di Portfolio.Web — completato;
- `BL-0017`, valutare la condivisione degli album su Instagram — completato;
- `BL-0008`, completare lo sharing automatico da Portfolio.Web — completato;
- `BL-0032`, raccontare la nascita del calendario Germana 2023 — completato;
- `BL-0033`, generare cover editoriali ad alta risoluzione — completato;
- `BL-0019`, introdurre uno smart crop locale per le cover — completato.

La milestone è stata completata l'11 agosto 2026. L'elenco conserva l'ordine operativo seguito; priorità, criteri di accettazione ed esiti restano registrati nel Backlog.

---

## 3. Next

Risultati candidati successivi alla milestone corrente:

- debito tecnico di priorità Alta, a partire da `TD-0001` e `TD-0002`, da affrontare nella milestone successiva al consolidamento delle specifiche di coding;
- completamento delle altre componenti della Engineering Baseline: architettura di persistenza, guida editoriale e regole operative per il codice generato con AI non già coperte da `BL-0037`;
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
