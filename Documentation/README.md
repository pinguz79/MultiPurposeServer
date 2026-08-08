# Organizzazione della documentazione e del repository

> **Stato: Stable 1.0 — autorevole.**

## 1. Scopo

Questo documento descrive come il repository e la documentazione di MultiPurposeServer sono organizzati per responsabilità.

L'indice navigabile e l'ordine di lettura appartengono a [Home](Home.md). Stato corrente, milestone e livelli di stabilità appartengono a [Project Status](ProjectStatus.md).

---

## 2. Struttura del repository

| Area | Responsabilità |
|---|---|
| `Applications` | Client Web, Mobile, Desktop o amministrativi dei domini. |
| `Domains` | Moduli server autonomi che implementano i contesti funzionali. |
| `Shared` | Capacità tecniche condivise e indipendenti dal business. |
| `Tests` | Suite organizzate secondo le responsabilità dei componenti produttivi. |
| `tools` | Script e strumenti di supporto allo sviluppo. |
| `Documentation` | Documentazione architetturale, funzionale, operativa e di engineering. |
| `MultiPurposeServer` | Host e Composition Root correnti della piattaforma. |

La struttura cresce soltanto quando emerge una responsabilità reale. Non vengono introdotte cartelle, progetti o classificazioni vuote per anticipare esigenze future.

I confini logici precedono quelli fisici: più responsabilità possono convivere nello stesso progetto o assembly quando rimangono distinguibili e non introducono accoppiamenti impropri.

---

## 3. Struttura della documentazione

| Area | Responsabilità |
|---|---|
| Root `README.md` | Punto di ingresso e ordine minimo del bootstrap. |
| `Home.md` | Indice generale ordinato dal quadro generale agli approfondimenti. |
| `Platform.md` | Identità, obiettivi e principi della piattaforma. |
| `Architecture` | Overview, architetture specialistiche, Glossary e ADR. |
| `Engineering` | Workflow, pratiche, code review, convenzioni e debito tecnico. |
| `Portfolio` | Specifiche e documenti relativi al dominio Portfolio e alle sue Application. |
| `Roadmap` | Vision, sequenza delle milestone e backlog funzionale. |
| `Security` | Registri e documenti operativi specialistici di sicurezza. |
| `AI` | Indicazioni ancora Alpha per strumenti specifici di assistenza. |
| `ChatRecovery.md` | Procedura di recupero del contesto in una nuova sessione. |
| `ProjectStatus.md` | Stato, milestone e attività corrente autorevoli. |

---

## 4. Profondità progressiva

La lettura procede dal generale al particolare:

```text
README della root
    ↓
Bootstrap
    ↓
Platform e Architecture
    ↓
Documenti specialistici
    ↓
ADR, convenzioni e documentazione implementativa
```

`Architecture.md` rappresenta l'interfaccia architetturale generale. I documenti specialistici approfondiscono i singoli sottosistemi. Gli ADR spiegano motivazioni, alternative e conseguenze senza sostituire la descrizione dello stato corrente.

---

## 5. Ownership dei concetti

Ogni concetto possiede un documento proprietario. Gli altri documenti possono descriverlo dalla propria prospettiva o collegarlo, ma non ne duplicano la responsabilità.

In caso di conflitto:

- `ProjectStatus.md` prevale per stato e attività corrente;
- il documento specialistico consolidato prevale nel proprio ambito;
- un ADR conserva la motivazione della decisione, mentre l'architettura descrive lo stato corrente;
- una fonte Stable prevale su Release Candidate e Alpha;
- il repository effettivo prevale sulle ricostruzioni presenti nelle conversazioni.

---

## 6. Livelli di stabilità

### Stable 1.0

Documento ufficiale e autorevole nel proprio ambito.

### Release Candidate

Documento tematicamente consolidato che attende la verifica finale prima della promozione.

### Alpha 0

Documento non autorevole, potenzialmente incompleto, incoerente o superato. Deve dichiarare esplicitamente il proprio stato e venire confrontato con fonti consolidate e codice.

I documenti temporanei dichiarano scopo, durata e condizione di eliminazione.

---

## 7. Evoluzione

Documentazione e architettura evolvono insieme.

Una modifica aggiorna contestualmente il documento proprietario quando cambia un contratto, una responsabilità, una procedura o altra conoscenza stabile. Le nuove decisioni significative vengono registrate tramite ADR quando necessario.

History, discussioni e documenti temporanei possono aiutare la migrazione, ma non sostituiscono la documentazione corrente.

---

## 8. Riferimenti

- [Home](Home.md)
- [Project Status](ProjectStatus.md)
- [Architecture](Architecture/Architecture.md)
- [Glossary](Architecture/Glossary.md)
- [MPS Playbook](Engineering/MpsPlaybook.md)
- [Architecture Decision Records](Architecture/ADR/README.md)
