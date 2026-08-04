# Platform

## Overview

MultiPurposeServer (MPS) è una piattaforma server progettata per ospitare più domini applicativi indipendenti all'interno di un unico host REST.

La piattaforma nasce con l'obiettivo di ridurre la complessità infrastrutturale e favorire il riutilizzo delle funzionalità tecniche comuni, mantenendo al tempo stesso una netta separazione tra i domini applicativi.

MPS non rappresenta un'applicazione specifica, ma un'infrastruttura sulla quale possono essere sviluppati ed eseguiti sistemi client/server eterogenei, indipendenti tra loro e accomunati esclusivamente dai servizi tecnici forniti dalla piattaforma.

---

# Objectives

La piattaforma persegue i seguenti obiettivi:

- utilizzare un unico host HTTPS per ridurre costi di infrastruttura e complessità operativa;
- ospitare domini applicativi indipendenti all'interno dello stesso processo server;
- centralizzare esclusivamente le funzionalità tecniche realmente condivisibili;
- consentire ai domini di evolvere in modo indipendente;
- favorire il riutilizzo del codice senza aumentare l'accoppiamento tra i domini.

---

# Core Principles

## Single Host

MPS utilizza un unico host REST come contenitore dei domini applicativi.

Questa scelta è motivata principalmente da esigenze operative ed economiche:

- semplificazione della pubblicazione;
- riduzione dei costi di hosting;
- utilizzo di un unico certificato HTTPS;
- centralizzazione della gestione dell'infrastruttura.

La condivisione dell'host non implica la condivisione dei domini.

---

## Modular Architecture

Ogni dominio rappresenta un sistema autonomo.

Ogni dominio possiede:

- API dedicate;
- modello dati indipendente;
- configurazione indipendente;
- persistenza indipendente;
- logica di business indipendente;
- ciclo evolutivo indipendente.

L'host rappresenta esclusivamente il punto di composizione della piattaforma.

---

## Shared Framework

La piattaforma mette a disposizione un framework comune che fornisce servizi tecnici riutilizzabili dai domini.

Il framework può offrire, ad esempio:

- accesso alla persistenza;
- validazione;
- normalizzazione;
- elaborazioni bulk;
- gestione delle transazioni;
- accesso al filesystem;
- elaborazione immagini;
- invio email;
- altri servizi infrastrutturali condivisi.

Il framework non contiene logica di business.

Ogni dominio utilizza esclusivamente i servizi necessari e mantiene il pieno controllo delle proprie regole applicative.

---

## Domain Autonomy

Ogni dominio deve poter evolvere indipendentemente dagli altri.

La condivisione dell'host e del framework non deve introdurre dipendenze funzionali tra domini.

Ogni dominio deve poter essere estratto e distribuito separatamente con modifiche minime.

---

## Extensibility

I servizi del framework devono essere progettati per essere:

- configurabili;
- estendibili;
- sostituibili quando necessario.

Il framework fornisce implementazioni predefinite per i casi comuni e punti di estensione espliciti quando il comportamento dipende dal dominio applicativo.

I domini descrivono il proprio comportamento; il framework fornisce i meccanismi tecnici necessari alla sua esecuzione.

---

# Hosted Domains

La piattaforma è progettata per ospitare domini applicativi indipendenti.

Attualmente sono previsti i seguenti domini.

| Domain | Purpose |
|----------|---------|
| Portfolio | Gestione di portfolio fotografici professionali. |
| ModelBook | Gestione di fotografi, modelle, collaborazioni e produzioni fotografiche. |
| Skating System | Gestione di competizioni sportive, classifiche e risultati. |
| BoardGameUniverse | Piattaforma multiplayer per giochi da tavolo, giochi di carte e giochi di ruolo online. |

L'architettura della piattaforma è progettata per consentire l'aggiunta di nuovi domini senza modificare i principi fondamentali del sistema.

---

# Long-term Vision

MultiPurposeServer non nasce per risolvere uno specifico problema di business.

Nasce per fornire una piattaforma modulare sulla quale costruire, ospitare ed evolvere sistemi client/server indipendenti, condividendo esclusivamente le capacità tecniche comuni e preservando l'autonomia dei singoli domini.

L'evoluzione della piattaforma è guidata dai principi architetturali documentati e dagli Architecture Decision Record (ADR), con l'obiettivo di mantenere nel tempo coerenza, semplicità ed elevata manutenibilità dell'intero ecosistema.