# Portfolio Domain

## 1. Scopo del documento

Questo documento descrive il dominio funzionale di Portfolio.

Non rappresenta lo schema del database, la struttura delle API o il dettaglio delle classi attualmente implementate. Definisce invece il linguaggio del dominio, le entità consolidate, le invarianti e i concetti ancora emergenti.

---

## 2. Visione del dominio

Portfolio gestisce contenuti fotografici professionali organizzati in una gerarchia navigabile.

Ogni nodo corrisponde fisicamente a una cartella del filesystem e assume logicamente uno dei seguenti ruoli:

- Gallery;
- Collection;
- Photo Album.

Il dominio deve mantenere una collocazione principale e autorevole per ogni contenuto, consentendo in futuro percorsi di navigazione alternativi senza duplicare album o fotografie.

---

## 3. Linguaggio del dominio

### Portfolio Node

Un Portfolio Node rappresenta un nodo della gerarchia.

Nel modello attuale può continuare a essere rappresentato dall'entità `Album`, purché il suo ruolo logico sia esposto in modo esplicito.

### Gallery

Una Gallery è un nodo senza padre, direttamente sotto la root.

Esempi:

- Modelle e Modelli;
- Calendari;
- Eventi;
- Editoriali.

Rappresenta una grande area di navigazione.

### Collection

Una Collection è un nodo intermedio con uno o più figli.

Può rappresentare:

- una persona;
- un evento;
- un'agenzia;
- un anno;
- una categoria;
- un progetto;
- un raggruppamento tematico.

Non rappresenta necessariamente uno shooting.

### Photo Album

Un Photo Album è un nodo foglia con un padre e senza figli.

Rappresenta un insieme fotografico coerente, per esempio:

- uno shooting;
- le fotografie di una singola modella durante una sfilata;
- le fotografie di una concorrente durante un concorso;
- un calendario;
- un progetto editoriale;
- un backstage specifico.

### Photo

Una Photo appartiene a un Photo Album.

La fotografia originale è il contenuto autorevole. Thumbnail, preview, cover e altre varianti ridimensionate sono contenuti derivati e ricostruibili.

---

## 4. Classificazione dei nodi

Il ruolo logico di un nodo deriva dalla posizione e dalla presenza di figli.

```text
Parent assente
    → Gallery

Almeno un figlio
    → Collection

Parent presente e nessun figlio
    → Photo Album
```

Questa classificazione può essere esposta tramite una proprietà calcolata `Kind`.

Esempio concettuale:

```csharp
public AlbumKind Kind =>
    ParentId is null
        ? AlbumKind.Gallery
        : Children.Any()
            ? AlbumKind.Collection
            : AlbumKind.PhotoAlbum;
```

L'implementazione dovrà evitare accessi ripetuti o inefficienti alla collezione dei figli.

---

## 5. Invarianti

### 5.1 Esclusività tra figli e fotografie

Una Collection contiene nodi figli e non contiene fotografie.

Un Photo Album contiene fotografie e non contiene nodi figli.

```text
Collection
    → figli
    → nessuna fotografia

Photo Album
    → fotografie
    → nessun figlio
```

### 5.2 Collocazione principale

Ogni Photo Album possiede una sola collocazione principale e autorevole nella gerarchia.

Questa collocazione rappresenta il contesto proprietario e il motivo principale per cui il contenuto esiste.

### 5.3 Path stabile

Il path rappresenta l'identità pubblica e navigabile del nodo.

Nel funzionamento ordinario non cambia. Un eventuale cambio di path è una bonifica eccezionale che può essere gestita manualmente.

### 5.4 Filesystem e gerarchia

Ogni nodo principale corrisponde a una cartella fisica.

La gerarchia logica principale e la struttura del filesystem rimangono allineate.

### 5.5 Contenuti derivati

Cache, mapping e varianti ridimensionate devono poter essere invalidati e ricostruiti senza perdita di dati reali.

---

## 6. Collection Kind

In futuro potrà essere utile classificare le Collection tramite un `CollectionKind`.

Possibili valori:

- Person;
- Event;
- Agency;
- Year;
- Category;
- Project;
- Other.

Il tipo potrà influenzare comportamento, visibilità, layout e relazioni.

Esempi:

- una Collection `Person` potrà essere associata a un profilo utente;
- una Collection `Event` non sarà associabile a un singolo profilo;
- una Collection `Person` potrà avere visibilità completa riservata alla persona associata;
- una Collection `Event` potrà essere condivisa con le persone rappresentate nelle Collection sottostanti.

`CollectionKind` è un concetto emergente e non deve essere implementato prima che le regole siano sufficientemente stabili.

---

## 7. Navigazione tra nodi fratelli

In futuro potrà essere introdotta una navigazione Previous / Next tra nodi fratelli dello stesso livello.

La funzionalità potrà applicarsi a:

- Gallery;
- Collection;
- Photo Album.

Esempi:

```text
← Gallery precedente
Gallery corrente
Gallery successiva →
```

```text
← Collection precedente
Collection corrente
Collection successiva →
```

```text
← Photo Album precedente
Photo Album corrente
Photo Album successivo →
```

La funzionalità non è necessaria nella prima fase, ma può migliorare la navigazione sequenziale di eventi, concorsi, sfilate, calendari e raccolte personali.

---

## 8. Linked Albums e percorsi alternativi

In futuro una Collection potrà mostrare riferimenti ad album collocati in altri rami.

Esempio:

```text
Modelle e Modelli/
└── Annalisa Larosa/
    ├── Urban Style
    ├── → Annalisa's Secrets
    └── → Villetta Dinegro
```

Le collocazioni principali rimangono:

```text
Calendari/2025/Annalisa's Secrets
```

```text
Modelle e Modelli/RS Fashion Group/Annalisa L Guest/Villetta Dinegro
```

Il riferimento alternativo:

- non duplica fotografie;
- non modifica il parent reale;
- non modifica il path reale;
- permette un secondo percorso di scoperta;
- punta all'identità del nodo target;
- può avere un look and feel differente nel frontend.

Il nome definitivo del concetto è ancora aperto. Possibili termini:

- Linked Album;
- Album Link;
- Collection Entry;
- Reference.

---

## 9. Concetti emergenti

### Person

Può rappresentare modella, modello, fotografo, ballerino, atleta o altro professionista.

In Portfolio potrà essere collegata a Collection, Photo Album, Photo e profili social.

In futuro potrà diventare un concetto condiviso con ModelBook e Skating.

### Event

Può rappresentare concorso, sfilata, gara, spettacolo, workshop o evento editoriale.

### Agency

Può rappresentare un'agenzia o un'organizzazione proprietaria o promotrice di un progetto.

### Social Profile

Può rappresentare l'identità social di una persona, agenzia o progetto.

### Location

Può rappresentare il luogo di uno shooting o di un evento.

Questi concetti devono maturare nel dominio Portfolio prima di essere eventualmente estratti in `Shared`.

---

## 10. Tag e profili social

In futuro sarà possibile associare persone e profili social a:

- Collection;
- Photo Album;
- singole Photo.

Possibili utilizzi:

- mostrare crediti;
- taggare modelle, agenzie e collaboratori;
- generare automaticamente menzioni per Facebook e Instagram;
- collegare album e fotografie a profili ModelBook;
- controllare visibilità e accesso.

Il modello dovrà distinguere tra identità della persona, ruolo nel contenuto, profilo social e handle da usare in pubblicazione.

---

## 11. Ciclo di vita e pubblicazione

La modifica di un album deve rimanere distinta dalla sua pubblicazione.

```text
Update Album
    ≠
Publish Album
```

In futuro il ciclo di vita potrà includere:

```text
Draft
    ↓
Ready
    ↓
Published
    ↓
Archived
```

La pubblicazione potrà coordinare:

- validazione;
- controllo dei contenuti;
- generazione delle varianti;
- sincronizzazione della cache di Portfolio.Web;
- aggiornamento dei mapping;
- pubblicazione sul sito;
- pubblicazione social;
- registrazione degli esiti;
- retry.

Un errore nella pubblicazione social non dovrà annullare la pubblicazione sul sito.

---

## 12. Workflow futuri

### 12.1 Amministrazione

Le operazioni amministrative potranno essere eseguite tramite:

- applicazione Desktop;
- sezione Admin Web;
- entrambi i client;
- altri client futuri.

Tutti i client dovranno utilizzare Portfolio.Api come fonte autorevole.

### 12.2 Lightroom

In futuro potrà essere sviluppato un plugin Lightroom dedicato per:

- creare un Photo Album;
- aggiornare un album esistente;
- caricare fotografie e metadati;
- scegliere la copertina;
- attivare il workflow di pubblicazione;
- aggiornare incrementalmente un album.

### 12.3 Sincronizzazione Portfolio.Web

Portfolio.Api dovrà poter notificare Portfolio.Web dopo modifiche riuscite.

La sincronizzazione potrà comprendere:

- upsert puntuale dei mapping;
- eliminazione puntuale dei mapping;
- invalidazione selettiva delle risposte API;
- clear completo come meccanismo di recupero.

Il client amministrativo non deve conoscere direttamente gli endpoint interni di Portfolio.Web.

---

## 13. Operazioni bulk

Portfolio.Api possiede già operazioni di bonifica bulk.

Le operazioni bulk devono:

- mantenere un comportamento transazionale coerente;
- evitare aggiornamenti parziali non desiderati;
- produrre risultati espliciti;
- permettere la sincronizzazione successiva delle cache;
- restare indipendenti dal client amministrativo.

---

## 14. Modello attuale e modello futuro

Il modello di dominio e il modello di persistenza non devono necessariamente coincidere.

Oggi una sola entità `Album` può rappresentare Gallery, Collection e Photo Album.

In futuro si potrà valutare se mantenere:

```text
Album
└── Kind
```

oppure introdurre concetti espliciti:

```text
PortfolioNode
├── Gallery
├── Collection
└── PhotoAlbum
```

La scelta dovrà essere guidata da esigenze reali e non dalla sola eleganza teorica.

---

## 15. Decisioni aperte

Restano da consolidare:

- implementazione effettiva di `AlbumKind`;
- opportunità e tempi di introduzione di `CollectionKind`;
- modello definitivo dei Linked Albums;
- relazione tra Collection `Person` e profilo utente;
- regole di visibilità per persone ed eventi;
- amministrazione Desktop, Web o entrambe;
- modello di pubblicazione;
- job asincroni;
- integrazione social;
- modello condiviso `Person`;
- modello condiviso `Media`;
- navigazione Previous / Next tra nodi fratelli.

---

## 16. Regola finale

Portfolio deve continuare a evolvere partendo da esigenze reali.

I concetti devono essere nominati e documentati appena diventano visibili, ma non devono essere generalizzati o estratti prematuramente.

Portfolio rappresenta il primo laboratorio in cui MPS sta facendo emergere concetti più generali come Person, Media, Collection, Reference, Event e Social Identity.

Questi concetti potranno diventare condivisi soltanto quando almeno due domini avranno dimostrato di averne realmente bisogno.
