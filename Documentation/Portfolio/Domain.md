# Portfolio Domain

## 1. Scopo del documento

Questo documento descrive il dominio funzionale **Portfolio** di MultiPurposeServer.

Non documenta l'implementazione delle API, la struttura del database o l'organizzazione del codice sorgente. Il suo scopo è descrivere il linguaggio del dominio, i concetti fondamentali, le regole che governano il modello e la direzione della sua evoluzione.

Portfolio rappresenta il dominio attraverso cui MPS gestisce contenuti fotografici professionali, la loro organizzazione, pubblicazione e consultazione.

Molti dei concetti che emergeranno in questo dominio potranno, in futuro, diventare patrimonio comune della piattaforma.

---

# 2. Visione del dominio

Portfolio gestisce raccolte fotografiche organizzate in una gerarchia navigabile.

L'obiettivo principale del dominio consiste nel preservare una struttura semplice e coerente, nella quale ogni contenuto possieda una collocazione principale e autorevole.

La navigazione deve risultare naturale per il visitatore, senza imporre la struttura interna utilizzata dal sistema.

Portfolio distingue chiaramente tra:

- organizzazione del contenuto;
- pubblicazione del contenuto;
- consultazione del contenuto.

Queste tre responsabilità possono evolvere indipendentemente.

---

# 3. Linguaggio del dominio

Portfolio utilizza un linguaggio specifico che descrive i concetti fondamentali del dominio.

## 3.1 Portfolio Node

Un Portfolio Node rappresenta un nodo della gerarchia.

Nel modello corrente continua a essere rappresentato dall'entità `Album`, ma il suo ruolo logico è espresso tramite la proprietà `Kind`.

In futuro il modello potrà evolvere introducendo entità più specializzate senza modificare il linguaggio del dominio.

---

## 3.2 Gallery

Una Gallery rappresenta il punto di ingresso principale della navigazione.

È sempre collocata direttamente sotto la radice del Portfolio.

Esempi:

- Modelle e Modelli
- Calendari
- Editoriali
- Eventi

Una Gallery identifica una grande area tematica.

---

## 3.3 Collection

Una Collection rappresenta un raggruppamento logico di altri Portfolio Node.

Può rappresentare, ad esempio:

- una persona;
- un evento;
- un'agenzia;
- un progetto;
- un anno;
- una categoria.

Una Collection organizza il contenuto ma non rappresenta necessariamente uno shooting fotografico.

---

## 3.4 Photo Album

Un Photo Album rappresenta una raccolta fotografica coerente.

Può contenere fotografie relative, ad esempio, a:

- uno shooting;
- una sfilata;
- un concorso;
- un backstage;
- un calendario;
- un progetto editoriale.

Il Photo Album costituisce il contenitore naturale delle fotografie.

---

## 3.5 Photo

Una Photo appartiene sempre a un Photo Album.

L'immagine originale rappresenta il contenuto autorevole.

Thumbnail, preview, cover e ogni altra variante sono contenuti derivati e ricostruibili.

---

# 4. Modello del dominio

Il dominio Portfolio adotta una struttura gerarchica.

Ogni Portfolio Node assume uno dei seguenti ruoli:

```text
Gallery
    ↓
Collection
    ↓
Photo Album
    ↓
Photo
```

Il ruolo logico di un nodo deriva dalla sua posizione nella gerarchia e dalla presenza di eventuali figli.

Nel modello attuale tale ruolo è rappresentato dalla proprietà `Kind`.

L'implementazione concreta rimane un dettaglio del modello di persistenza e non costituisce parte del linguaggio del dominio.

---

# 5. Invarianti del dominio

Le seguenti regole costituiscono invarianti del dominio Portfolio.

## 5.1 Un Photo Album appartiene a una sola collocazione principale

Ogni Photo Album possiede una sola posizione autorevole nella gerarchia.

Tale posizione rappresenta il contesto naturale del contenuto.

Eventuali percorsi alternativi non modificano questa relazione.

---

## 5.2 Una Photo appartiene sempre a un solo Photo Album

Le fotografie non vengono duplicate tra Album differenti.

L'identità della fotografia rimane unica.

---

## 5.3 Collection e Photo Album hanno responsabilità differenti

Una Collection organizza altri nodi.

Un Photo Album organizza fotografie.

Una Collection non contiene direttamente fotografie.

Un Photo Album non contiene altri Portfolio Node.

---

## 5.4 Il filesystem riflette la gerarchia principale

La struttura principale del Portfolio mantiene una corrispondenza con il filesystem.

La gerarchia logica rappresenta anche la collocazione fisica autorevole del contenuto.

---

## 5.5 Il path rappresenta l'identità pubblica

Il path identifica il contenuto all'interno del Portfolio.

Nel normale funzionamento deve essere considerato stabile.

Eventuali modifiche costituiscono attività straordinarie di manutenzione.

---

## 5.6 Le varianti sono contenuti derivati

Thumbnail, preview, cache, cover e ogni altra rappresentazione derivata non costituiscono il contenuto principale.

Devono poter essere eliminate e ricostruite senza perdita di informazioni.

# 6. Concetti emergenti

Portfolio rappresenta il primo dominio di MultiPurposeServer.

Per questo motivo alcuni concetti stanno emergendo progressivamente e potrebbero, in futuro, diventare patrimonio comune della piattaforma.

Tali concetti non devono essere generalizzati prematuramente.

Dovranno essere estratti nello Shared Framework soltanto quando almeno due domini avranno dimostrato di condividerne realmente il significato.

## 6.1 Person

Una Person rappresenta una persona coinvolta nella produzione o nella pubblicazione di contenuti.

Può identificare, ad esempio:

- una modella;
- un modello;
- un fotografo;
- un ballerino;
- un atleta;
- un collaboratore.

In Portfolio una Person potrà essere associata a:

- Collection;
- Photo Album;
- Photo;
- profili social.

In futuro il concetto potrà essere condiviso con domini come ModelBook e Skating.

---

## 6.2 Event

Un Event rappresenta un avvenimento che produce contenuti fotografici.

Può identificare:

- concorsi;
- sfilate;
- workshop;
- spettacoli;
- gare;
- eventi editoriali.

Un Event può costituire il contesto di una o più Collection.

---

## 6.3 Agency

Una Agency rappresenta un'organizzazione coinvolta nella produzione dei contenuti.

Può rappresentare:

- agenzie di moda;
- organizzatori;
- scuole;
- associazioni;
- aziende.

---

## 6.4 Social Profile

Un Social Profile rappresenta un'identità pubblica associata a una Person, a una Agency o a un progetto.

Il dominio dovrà distinguere chiaramente tra:

- identità della persona;
- profilo social;
- handle utilizzato nella pubblicazione.

---

## 6.5 Location

Una Location rappresenta il luogo in cui vengono prodotti i contenuti.

Potrà essere associata a:

- Event;
- Photo Album;
- Photo.

---

# 7. Navigazione

La navigazione del Portfolio deve poter evolvere indipendentemente dalla struttura fisica del filesystem.

L'utente deve poter esplorare il contenuto secondo differenti prospettive senza duplicare fotografie o Album.

---

## 7.1 Navigazione tra nodi fratelli

In futuro il Portfolio potrà introdurre la navigazione sequenziale tra nodi dello stesso livello.

Ad esempio:

```text
← Album precedente

Album corrente

Album successivo →
```

Lo stesso principio potrà essere applicato a:

- Gallery;
- Collection;
- Photo Album.

---

## 7.2 Linked Album

Una Collection potrà contenere riferimenti a Photo Album appartenenti ad altri rami della gerarchia.

Lo scopo consiste nell'offrire percorsi alternativi di navigazione mantenendo una sola collocazione autorevole del contenuto.

Un Linked Album:

- non modifica il parent reale;
- non modifica il path principale;
- non duplica fotografie;
- non crea una nuova identità.

Rappresenta esclusivamente un differente punto di accesso allo stesso contenuto.

Il nome definitivo del concetto non è ancora consolidato.

---

# 8. Pubblicazione

La modifica del contenuto e la sua pubblicazione rappresentano due responsabilità distinte.

```text
Aggiornamento
        ≠
Pubblicazione
```

Un contenuto può essere modificato molte volte senza essere immediatamente pubblicato.

---

## 8.1 Ciclo di vita

In futuro il dominio potrà adottare un ciclo di vita simile al seguente.

```text
Draft
    ↓
Ready
    ↓
Published
    ↓
Archived
```

Gli stati definitivi verranno introdotti solo quando emergerà una reale necessità.

---

## 8.2 Processo di pubblicazione

La pubblicazione potrà coordinare attività differenti, tra cui:

- validazione;
- generazione delle varianti;
- aggiornamento dei mapping;
- sincronizzazione della cache;
- pubblicazione sul sito;
- pubblicazione sui social;
- registrazione degli esiti.

Le diverse attività dovranno poter fallire indipendentemente senza compromettere l'intero processo.

---

# 9. Workflow futuri

Portfolio dovrà poter essere amministrato tramite differenti client.

Ad esempio:

- Desktop Application;
- Web Administration;
- plugin Lightroom;
- futuri strumenti dedicati.

Tutti i client utilizzeranno Portfolio.Api come fonte autorevole.

---

## 9.1 Lightroom

In futuro potrà essere sviluppato un plugin dedicato per Adobe Lightroom.

Il plugin potrà consentire:

- creazione di Photo Album;
- caricamento delle fotografie;
- aggiornamento incrementale;
- scelta della cover;
- avvio del processo di pubblicazione.

---

## 9.2 Sincronizzazione

Portfolio.Api potrà notificare Portfolio.Web dopo modifiche rilevanti.

La sincronizzazione potrà comprendere:

- aggiornamento selettivo dei mapping;
- invalidazione delle cache;
- sincronizzazione incrementale;
- rigenerazione completa come procedura di recupero.

Le Applications amministrative non dovranno conoscere direttamente i meccanismi interni di sincronizzazione.

---

# 10. Evoluzione del dominio

Portfolio rappresenta il dominio nel quale stanno emergendo numerosi concetti destinati a evolvere.

L'obiettivo non consiste nel generalizzarli anticipatamente, ma nel permettere loro di maturare attraverso l'utilizzo reale.

Solo quando almeno due domini dimostreranno di condividere realmente uno stesso concetto, esso potrà essere estratto nello Shared Framework.

Questo principio vale, tra gli altri, per:

- Person;
- Event;
- Agency;
- Media;
- Collection;
- Social Identity.

Portfolio continua quindi a rappresentare il principale laboratorio evolutivo dell'intera piattaforma.

---

# 11. Decisioni aperte

Alcuni aspetti del dominio non sono ancora definitivamente consolidati.

Tra questi:

- evoluzione di `AlbumKind`;
- introduzione di `CollectionKind`;
- modello definitivo dei Linked Album;
- integrazione con ModelBook;
- modello di pubblicazione;
- gestione dei Job asincroni;
- integrazione Social;
- navigazione Previous / Next;
- evoluzione del modello Media.

Queste decisioni verranno prese quando emergeranno esigenze concrete.

---

# 12. Vedi anche

## Architettura

- `Architecture.md`
- `DomainArchitecture.md`
- `SharedFramework.md`

## Evoluzione

- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`

## Processo di sviluppo

- `Documentation/Engineering/MpsPlaybook.md`