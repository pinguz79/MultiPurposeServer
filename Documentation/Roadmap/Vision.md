# Visione di lungo periodo di MultiPurposeServer

## 1. Scopo del documento

Questo documento conserva la visione di lungo periodo di MultiPurposeServer.

Non rappresenta un piano di sviluppo, una lista di attività o un backlog.

La Visione raccoglie idee, obiettivi, direzioni evolutive e concetti che potrebbero influenzare l'architettura e l'evoluzione del progetto negli anni.

Una voce presente in questo documento non implica necessariamente che verrà implementata né definisce una priorità temporale.

L'obiettivo è mantenere una visione complessiva del progetto evitando di perdere idee che potrebbero diventare importanti in futuro.

---

# 2. Visione

MultiPurposeServer nasce come piattaforma modulare per la gestione di contenuti, persone, media ed eventi.

Portfolio rappresenta il primo dominio implementato, ma il progetto è concepito per ospitare domini differenti che condividono infrastruttura, servizi e concetti comuni senza perdere la propria autonomia.

La crescita del progetto dovrà avvenire in modo incrementale, lasciando emergere i concetti condivisi dall'esperienza maturata nei vari domini, evitando astrazioni premature.

---

# 3. Domini

## Portfolio

Portfolio rappresenta il dominio dedicato alla pubblicazione di contenuti fotografici professionali.

Obiettivi di lungo periodo:

- gestione completa di shooting fotografici;
- gestione album e fotografie;
- pubblicazione online;
- download controllato delle immagini;
- gestione watermark;
- workflow professionale per fotografi;
- integrazione con Lightroom;
- pubblicazione automatica sui social network;
- gestione licensing;
- gestione fotografie HD a pagamento.

---

## ModelBook

ModelBook rappresenterà il portale dedicato a modelle, modelli, fotografi ed agenzie.

Possibili funzionalità:

- portfolio personale;
- profili pubblici;
- casting;
- gestione agenzie;
- eventi;
- concorsi;
- networking;
- applicazione mobile dedicata.

---

## Skating

Dominio dedicato alla gestione di gare ed eventi di danza.

Obiettivi:

- gestione competizioni;
- iscrizioni;
- risultati;
- classifiche;
- gestione società sportive.

---

# 4. Workflow

## Workflow fotografico

Visione di lungo periodo:

```
Lightroom
        ↓
Portfolio.Api
        ↓
Portfolio.Web
        ↓
Pubblicazione Social
```

L'intero processo di pubblicazione dovrebbe poter essere eseguito senza interventi manuali ripetitivi.

---

## Workflow amministrativo

Le operazioni amministrative dovranno poter essere eseguite tramite uno o più client dedicati.

Le modalità di amministrazione (Desktop, Web oppure entrambe) rappresentano una decisione ancora aperta.

Tutti i client amministrativi dovranno utilizzare Portfolio.Api come unico punto di accesso ai dati.

---

# 5. Pubblicazione

La semplice modifica di un album dovrà rimanere distinta dalla sua pubblicazione.

In futuro la pubblicazione di un album potrà comprendere:

- validazione finale;
- sincronizzazione della cache;
- pubblicazione sul sito;
- pubblicazione Facebook;
- pubblicazione Instagram;
- eventuale pubblicazione su altri social;
- registrazione dello stato di pubblicazione.

La pubblicazione dovrà essere concepita come un workflow applicativo e non come un semplice aggiornamento CRUD.

---

# 6. Collezioni e navigazione

L'organizzazione fisica degli album nel filesystem rappresenta la loro posizione principale.

In futuro dovrà essere possibile creare percorsi di navigazione alternativi senza duplicare gli album.

Esempi:

- raccolta personale di una modella;
- raccolte di agenzia;
- raccolte tematiche;
- raccolte editoriali;
- best of;
- portfolio personali.

Queste raccolte dovranno poter referenziare album esistenti mantenendo un'unica copia fisica dei contenuti.

La forma definitiva di questo modello dovrà emergere durante l'evoluzione del progetto.

Portfolio ha consolidato semanticamente il concetto di Album virtuale: una Collection priva di folder che costruisce percorsi alternativi tramite link persistiti verso Album virtuali o fisici, senza modificare la gerarchia filesystem.

L'archiviazione potrà utilizzare un Album virtuale con funzione `Archive` per escludere Album datati dalla navigazione ordinaria mantenendoli raggiungibili direttamente, fisicamente invariati e soggetti alla policy di accesso già prevista.

---

# 7. Social

In futuro MPS dovrà poter gestire l'integrazione con i principali social network.

Possibili funzionalità:

- pubblicazione automatica;
- gestione hashtag;
- tag delle modelle;
- tag delle agenzie;
- programmazione dei post;
- sincronizzazione dello stato di pubblicazione.

---

# 8. AI

Possibili evoluzioni:

- suggerimento parole chiave;
- riconoscimento automatico persone;
- suggerimento copertina album;
- individuazione duplicati;
- ricerca semantica;
- generazione descrizioni.

Queste funzionalità verranno valutate quando il progetto avrà raggiunto una maggiore maturità.

---

# 9. Concetti emergenti

Alcuni concetti stanno iniziando ad emergere in più domini ma non devono ancora essere generalizzati.

Esempi:

- Person
- Media
- Collections
- References
- Social Identity
- Event
- Location

Per il momento tali concetti devono continuare ad evolvere all'interno del proprio dominio.

Quando almeno due domini avranno maturato esigenze realmente comuni si valuterà la loro estrazione in `Shared`.

---

# 10. Decisioni aperte

Argomenti ancora da consolidare:

- amministrazione Desktop o Web;
- plugin Lightroom;
- modello definitivo delle Collections;
- pubblicazione asincrona;
- gestione job in background;
- strategia di integrazione con i social;
- modello condiviso Person;
- modello condiviso Media.

---

# 11. Principi

La Visione non descrive come implementare una funzionalità.

Descrive dove il progetto vuole arrivare.

Le decisioni implementative appartengono ad `Architecture.md`.

Le attività operative appartengono al [Backlog](Backlog.md) e agli eventuali strumenti di pianificazione collegati.

Quando un'idea influenza l'evoluzione di MultiPurposeServer ma non rappresenta ancora un'attività concreta, il suo posto è questo documento.

---

# 12. Riferimenti

- [Roadmap](Roadmap.md)
- [Backlog](Backlog.md)
- [Project Status](../ProjectStatus.md)
