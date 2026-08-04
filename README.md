# MultiPurposeServer

MultiPurposeServer (MPS) è una piattaforma REST modulare progettata per ospitare più domini applicativi indipendenti all'interno di un unico host.

L'obiettivo della piattaforma è ridurre la complessità infrastrutturale condividendo esclusivamente i servizi tecnici comuni, mantenendo al tempo stesso l'autonomia dei singoli domini applicativi.

---

## Caratteristiche

- Architettura modulare.
- Host REST unico.
- Framework condiviso di servizi tecnici.
- Domini applicativi indipendenti.
- Persistenza indipendente per ciascun dominio.
- Piattaforma estendibile.

---

## Hosted Domains

Attualmente la piattaforma prevede i seguenti domini:

- Portfolio
- ModelBook
- Skating System
- BoardGameUniverse

L'architettura è progettata per consentire l'aggiunta di nuovi domini senza modificare i principi fondamentali della piattaforma.

---

## Documentazione

La documentazione completa è disponibile nella cartella `Documentation`.

Per iniziare si consiglia il seguente percorso:

1. [Home](Documentation/Home.md)
2. [Platform](Documentation/Platform.md)

Da questi documenti è possibile accedere a tutta la documentazione architetturale e di sviluppo.

---

## Struttura del repository

```text
Applications/
Documentation/
Domains/
Shared/
Tests/
```

---

## Stato del progetto

La piattaforma è in fase di sviluppo attivo.

Le decisioni architetturali vengono documentate tramite ADR (Architecture Decision Records) e consolidate progressivamente nella documentazione tecnica del progetto.

---

## Licenza

MIT License.