# Roadmap di MultiPurposeServer

## 1. Scopo

Questo documento descrive la sequenza intenzionale di evoluzione di MultiPurposeServer.

La [Visione](Vision.md) conserva le direzioni di lungo periodo. Il [Backlog](Backlog.md) registra tutto il lavoro funzionale noto. `ProjectStatus.md` rappresenta invece la fonte autorevole sull'attività corrente e prevale in caso di divergenza.

La Roadmap non duplica le singole attività: stabilisce quali risultati appartengono a `Now`, `Next` e `Later` e rimanda alle relative fonti.

---

## 2. Now

### Selezione della prossima milestone

La milestone di consolidamento della documentazione è stata completata il 2026-08-08. Il bootstrap e il secondo livello verificato sono stati promossi a Stable 1.0; il materiale residuo Alpha rimane esplicitamente non autorevole.

Non è ancora stata selezionata una nuova milestone implementativa. L'attività corrente consiste nel confrontare i candidati descritti in `Next` e formalizzare la scelta in [Project Status](../ProjectStatus.md).

Durante questa selezione:

- non viene avviata implicitamente alcuna attività candidata;
- backlog, debito tecnico e milestone architetturali vengono confrontati per valore, rischio e urgenza;
- la decisione aggiorna contestualmente Roadmap e Project Status.

---

## 3. Next

La milestone successiva non è ancora stata selezionata.

Al termine del consolidamento documentale verranno rivalutati congiuntamente:

- i bug funzionali aperti nel Backlog, a partire da `BL-0001`;
- il debito tecnico di priorità Alta;
- le milestone tecniche residue;
- la prossima evoluzione funzionale di Portfolio;
- l'eventuale avvio di un nuovo dominio.

La presenza in questo elenco non costituisce ancora pianificazione. La scelta verrà registrata aggiornando questa sezione e `ProjectStatus.md`.

---

## 4. Later

Direzioni già riconosciute ma non pianificate:

- evoluzione dei workflow fotografici e amministrativi di Portfolio;
- avvio del dominio ModelBook;
- avvio del dominio Skating;
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
