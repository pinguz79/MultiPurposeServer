# Backlog di MultiPurposeServer

## 1. Scopo

Questo documento è la fonte autorevole del lavoro funzionale noto ma non necessariamente pianificato.

Contiene Epic, Feature, Bug e Improvement. Il debito tecnico appartiene invece al registro [Technical Debt](../Engineering/TechnicalDebt.md).

La presenza nel Backlog non assegna automaticamente una milestone. La [Roadmap](Roadmap.md) stabilisce la sequenza intenzionale e `ProjectStatus.md` definisce l'attività corrente.

Gli identificatori `BL-XXXX` sono stabili e non vengono riutilizzati.

---

## 2. Tipi e stati

### Tipi

- **Epic**: risultato ampio da scomporre prima della pianificazione.
- **Feature**: nuova capacità osservabile da un utilizzatore.
- **Bug**: comportamento osservabile differente da quello atteso.
- **Improvement**: miglioramento funzionale di un comportamento esistente.

### Stati

- **Da definire**: intenzione nota ma non ancora sufficientemente specificata.
- **Aperto**: problema o risultato descritto e pronto per essere analizzato.
- **Pianificato**: assegnato a una milestone.
- **In corso**: lavoro attivo.
- **Completato**: risultato implementato e verificato.
- **Annullato**: non verrà realizzato; la motivazione viene conservata.

### Priorità

Le priorità sono `Critica`, `Alta`, `Media`, `Bassa` oppure `Non assegnata`.

La valutazione considera valore o impatto per l'utilizzatore, diffusione del problema, costo indicativo, urgenza, workaround e relazione con le milestone correnti.

---

## 3. Riepilogo attivo

| Tipo | Critica | Alta | Media | Bassa | Non assegnata |
|---|---:|---:|---:|---:|---:|
| Bug | 0 | 1 | 1 | 0 | 0 |
| Epic | 0 | 0 | 0 | 0 | 2 |

---

## 4. Bug

### BL-0001 — Alcuni album di secondo livello non vengono caricati

- **Tipo:** Bug
- **Area:** Portfolio.Web
- **Stato:** Aperto
- **Priorità:** Alta
- **Segnalato:** 2026-08-07

Aprendo alcuni album di secondo livello, Portfolio.Web restituisce un errore invece di visualizzarne il contenuto.

Caso noto riproducibile:

```text
Modelle e Modelli / Annalisa L.
```

- **Impatto:** il contenuto dell'album non è fruibile.
- **Workaround:** non noto.
- **Criteri di accettazione:** l'album indicato e gli altri album validi di secondo livello vengono caricati senza errore; viene aggiunta la verifica appropriata per la causa individuata.
- **Note diagnostiche:** causa ed estensione del problema devono ancora essere determinate.

### BL-0002 — Nella preview fotografica manca il codice foto

- **Tipo:** Bug
- **Area:** Portfolio.Web
- **Stato:** Aperto
- **Priorità:** Media
- **Segnalato:** 2026-08-07

Negli album caricati correttamente, la preview mostra soltanto la posizione `X di Y` e non visualizza il codice della fotografia.

- **Impatto:** l'utente non può identificare la fotografia tramite il relativo codice dalla preview.
- **Workaround:** non noto nella preview corrente.
- **Criteri di accettazione:** la preview mostra sia il codice foto sia l'indicatore `X di Y`, mantenendo corretta la navigazione tra fotografie.
- **Note diagnostiche:** verificare disponibilità del dato nel payload e rendering del componente di preview.

---

## 5. Epic

### BL-0003 — Avvio del dominio ModelBook

- **Tipo:** Epic
- **Area:** ModelBook
- **Stato:** Da definire
- **Priorità:** Non assegnata
- **Registrato:** 2026-08-07

Progettare e implementare il dominio ModelBook e le relative Applications secondo la Visione e l'architettura dei domini.

Prima della pianificazione l'Epic deve essere scomposta in risultati funzionali verificabili, definendo primo rilascio, client iniziale, persistenza e modello di sicurezza.

### BL-0004 — Avvio del dominio Skating

- **Tipo:** Epic
- **Area:** Skating
- **Stato:** Da definire
- **Priorità:** Non assegnata
- **Registrato:** 2026-08-07

Progettare e implementare il dominio Skating per la gestione di competizioni, iscrizioni, risultati e classifiche.

Prima della pianificazione l'Epic deve essere scomposta in risultati funzionali verificabili e deve essere chiarito il perimetro del primo rilascio.

---

## 6. Elementi completati o annullati

Nessun elemento.

Gli elementi completati o annullati conservano identificatore ed esito. Se il documento diventerà troppo esteso potranno essere trasferiti in un archivio senza riutilizzarne gli ID.

---

## Riferimenti

- [Roadmap](Roadmap.md)
- [Visione](Vision.md)
- [Project Status](../ProjectStatus.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
- [Portfolio Domain](../Portfolio/Domain.md)
