# Glossario di MultiPurposeServer

## Scopo

Questo documento raccoglie il significato dei principali termini utilizzati nella documentazione di MultiPurposeServer.

Ogni termine possiede un significato preciso e dovrebbe essere utilizzato in modo coerente all'interno dell'intero progetto.

Il Glossario non sostituisce la documentazione architetturale o quella di dominio.

Il suo scopo è fornire un vocabolario comune.

---

# Architettura

## Application

Client che utilizza MultiPurposeServer.

Può essere Web, Mobile, Desktop o qualsiasi altro consumatore delle API.

---

## Domain

Modulo funzionale indipendente della piattaforma.

Ogni Domain possiede la propria logica di business, i propri Service, Repository, Contracts e persistenza.

---

## Shared Framework

Insieme dei componenti realmente condivisi tra più domini.

Uno Shared Framework nasce dall'evoluzione del progetto e non da generalizzazioni preventive.

---

## Contract

Oggetto che rappresenta il contratto pubblico delle API.

I Contracts appartengono al protocollo di comunicazione e non al modello interno del dominio.

---

## Service

Componente che implementa la logica applicativa del dominio.

Un Service non conosce HTTP né i Contracts.

---

## Repository

Componente responsabile della persistenza.

Non contiene logica di business.

---

## Composition Root

Punto in cui l'applicazione viene composta.

In MultiPurposeServer coincide con il progetto Host.

---

# Portfolio

## Portfolio Node

Nodo della gerarchia del Portfolio.

Può assumere differenti ruoli logici.

---

## Gallery

Nodo radice di una grande area tematica del Portfolio.

---

## Collection

Nodo utilizzato per organizzare altri Portfolio Node.

Non contiene direttamente fotografie.

---

## Photo Album

Contenitore di fotografie appartenenti a uno stesso servizio fotografico o progetto.

---

## Photo

Fotografia appartenente a un Photo Album.

L'immagine originale rappresenta il contenuto autorevole.

---

## Linked Album

Riferimento a un Photo Album appartenente a un diverso ramo della gerarchia.

Non crea una copia del contenuto.

---

## Publication

Processo mediante il quale un contenuto diventa pubblico.

È distinto dalla modifica del contenuto.

---

# Infrastruttura

## Media

Risorsa digitale gestita dalla piattaforma.

Può comprendere immagini, documenti, video o altri contenuti.

---

## Cache

Rappresentazione temporanea di dati derivati.

Può essere eliminata e ricostruita senza perdita del contenuto autorevole.

---

## Mapping

Associazione tra un'identità logica e una rappresentazione utilizzata da un componente esterno.

---

# Documentazione

## ADR

Architecture Decision Record.

Documento che descrive una decisione architetturale permanente.

---

## Architecture

Documentazione che descrive la struttura della piattaforma.

---

## Playbook

Documento che definisce il processo di sviluppo e le pratiche ingegneristiche adottate dal progetto.

---

## Roadmap

Documento che raccoglie le principali evoluzioni architetturali previste.

---

# Filosofia

## Shared is Earned, not Planned

Uno dei principi fondamentali di MultiPurposeServer.

Un concetto entra nello Shared Framework soltanto dopo aver dimostrato di essere realmente condiviso tra più domini.

---

## Keep It Simple

La semplicità rappresenta il principale criterio di progettazione.

La complessità viene introdotta soltanto quando giustificata da esigenze concrete.

---

## Refactor Continuously

L'architettura evolve attraverso piccoli miglioramenti continui, evitando grandi riscritture.

---

## Architecture First

Le decisioni architetturali guidano l'implementazione, non il contrario.