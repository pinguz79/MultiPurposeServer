# Dominio Portfolio

## 1. Scopo

Questo documento descrive identità, linguaggio, concetti e invarianti funzionali del dominio Portfolio.

Non definisce API, persistenza, struttura del codice o tecnologie dei client. Distingue il modello corrente dalle evoluzioni già consolidate semanticamente e dalle decisioni ancora aperte.

---

## 2. Identità e confine

Portfolio gestisce l'archivio fotografico professionale di un unico owner, il fotografo, e ne governa organizzazione, consultazione e condivisione controllata.

L'owner rappresenta l'autorità editoriale e amministrativa del dominio. Album e Photo appartengono al suo Portfolio.

Le altre persone possono essere coinvolte nella produzione dei contenuti, essere citate nei profili pubblici e ottenere accesso limitato alle risorse che le riguardano. Non possiedono un Portfolio autonomo e non amministrano una propria galleria indipendente.

Portfolio non è un portale multi-tenant. Profili e gallerie autonome per ciascun utente, messaggistica e interazione fra utenti appartengono al dominio ModelBook.

Client Web, Mobile, Desktop, amministrativi o integrati in strumenti fotografici rappresentano modalità differenti di accesso allo stesso dominio e utilizzano Portfolio.Api come fonte autorevole.

---

## 3. Attori

### 3.1 Owner

Esiste un solo owner del Portfolio. Governa contenuti, organizzazione, accessi e configurazione editoriale.

L'owner può essere rappresentato anche come Person, ma il ruolo di owner rimane unico e appartiene al dominio, non alla natura biologica della persona.

### 3.2 Person

Una Person rappresenta una persona biologica coinvolta nella produzione, rappresentazione o consultazione dei contenuti.

Può esistere senza account. Quando ottiene accesso al portale, un Account del dominio può essere collegato alla Person senza far coincidere identità editoriale e identità di sicurezza.

### 3.3 Account

L'Account autentica un soggetto nel dominio Portfolio. Credenziali, sessioni e permessi appartengono all'Account e non alla Person.

Il collegamento a una Person permette alle policy di attribuire accessi relativi ai contenuti che la riguardano. Il profilo pubblico e i dati privati dell'Account rimangono separati.

---

## 4. Album e gerarchia fisica

### 4.1 Album

Album è il nodo gerarchico fondamentale del Portfolio. Un Album fisico corrisponde a una folder e possiede un solo parent fisico autorevole, salvo le Gallery che non hanno parent.

La gerarchia fisica forma un albero e determina la collocazione canonica dei contenuti.

### 4.2 AlbumKind

`AlbumKind` è una classificazione derivata destinata ai client. Non determina identità, lifecycle o tipi differenti di entità.

La regola corrente è:

```text
Parent assente      → Gallery
Children presenti   → Collection
altrimenti          → PhotoAlbum
```

Un Album vuoto viene convenzionalmente classificato come PhotoAlbum. Se riceve un sottoalbum diventa Collection; il cambiamento è il ricalcolo di una proprietà di rendering e non una transizione di dominio.

- Una Gallery è un punto di ingresso radice della navigazione fisica.
- Una Collection organizza altri Album.
- Un PhotoAlbum è un nodo fisico senza figli e può contenere Photo.

### 4.3 Invarianti strutturali

- Una Gallery è una radice fisica e non può contenere direttamente Photo.
- Un Album con sottoalbum non può contenere Photo.
- Un Album con Photo non può ricevere sottoalbum.
- Un Album non può essere parent fisico di se stesso.
- La gerarchia fisica non ammette cicli.

Queste regole vengono applicate dal server indipendentemente dal comportamento dei client.

---

## 5. Photo

Una Photo rappresenta un asset fotografico identificato dal dominio e appartiene sempre a un solo Album fisico.

Il file originale costituisce il contenuto binario autorevole. La Photo conserva identità, mapping fisico, metadati editoriali, associazioni e stato di disponibilità.

Thumbnail, preview, versione Web, watermark e altri profili sono rappresentazioni derivate e ricostruibili dall'originale. Non possiedono identità fotografica autonoma e possono avere classificazioni di accesso differenti.

Una Photo censita il cui originale non è disponibile rimane nel dominio come contenuto non disponibile o corrotto. Una variante residua non rende integro l'asset e non permette di ricostruire l'originale.

---

## 6. Identità, nomi e path

### 6.1 Album

- `Id` è l'identità fisica stabile dell'entità.
- `Path` è il segmento tecnico usato per filesystem, riconciliazione e routing.
- Il full path fisico è la chiave logica e il locator canonico dell'Album.
- `Name` è il nome editoriale mostrato all'utente.

Rinominare `Name` non cambia automaticamente folder, path o URL. Spostamento e modifica del path sono operazioni esplicite e straordinarie che devono coordinare database, filesystem, routing e cache.

`Path` deve essere univoco fra Album fratelli. Un eventuale vecchio path pubblico può essere conservato come alias o redirect.

### 6.2 Photo

`Id` identifica stabilmente la Photo. La coppia Album fisico e filename costituisce il mapping logico verso il file originale.

---

## 7. Coerenza fra database e filesystem

Database e filesystem rappresentano aspetti differenti dello stesso modello e devono mantenere una corrispondenza obbligatoria.

```text
Folder sul filesystem
    → deve esistere Album nel database

Album nel database
    → deve esistere Folder sul filesystem

File immagine sul filesystem
    → deve esistere Photo nel database

Photo nel database senza file
    → contenuto non disponibile o corrotto
```

Il database conserva identità, relazioni e metadati. Il filesystem conserva path e contenuto binario.

La riconciliazione è conservativa:

- una folder mancante può essere ricreata dall'Album;
- un record Album mancante può essere ricostruito dalla folder;
- un record Photo mancante può essere ricostruito dal file;
- un originale mancante non può essere ricostruito dalla Photo;
- un'assenza unilaterale non viene interpretata automaticamente come intenzione di cancellazione.

### 7.1 Cancellazione

La cancellazione è un'operazione esplicita del dominio che rimuove coerentemente mapping e contenuto fisico.

Eliminare soltanto un lato provocherebbe la ricostruzione del lato mancante oppure uno stato di contenuto non disponibile. Le operazioni devono quindi governare gli effetti su database e filesystem mediante atomicità applicativa, compensazione e riconciliazione.

Poiché un originale non è ricostruibile, la cancellazione fisica può applicare conferma, trash o retention secondo la futura policy operativa.

---

## 8. Cover

La cover è il risultato di una regola di dominio stocastica e non una Photo selezionata e persistita.

Per ogni Album fisico viene scelta casualmente una Photo fra quelle presenti nel sottoalbero fisico canonico:

```text
Album fisico
    ↓
Photo del sottoalbero fisico
    ↓
selezione casuale
    ↓
variante Cover della Photo estratta
```

Un Album privo di Photo nel proprio sottoalbero non possiede cover. Seed, caching e algoritmo sono dettagli tecnici. Una futura evoluzione può ponderare maggiormente le Photo recenti, mantenendo casuale la selezione.

La cover di un Album fisico ignora i link virtuali e considera soltanto il sottoalbero filesystem canonico.

La cover di un Album virtuale viene invece scelta fra l'insieme distinto delle Photo appartenenti agli Album fisici raggiungibili attraverso i suoi link, anche quando esistono nodi virtuali intermedi. Lo stesso contenuto non viene ponderato più volte soltanto perché è raggiungibile tramite più rami.

Un Album virtuale WIP privo di Album fisici raggiungibili non possiede cover.

---

## 9. Visibilità

Portfolio non adotta attualmente un workflow draft/pubblicato. Una modifica valida diventa immediatamente parte dello stato corrente, ma la sua visibilità dipende dalla policy di accesso della risorsa e della rappresentazione.

- Un Album può essere pubblico, protetto o accessibile a specifici Account.
- Un nuovo Album fisico eredita normalmente la policy dal parent fisico.
- Le Gallery radice possiedono una policy esplicita.
- Una Photo eredita normalmente il contesto di accesso dell'Album fisico.
- Cover, thumbnail, preview, full-size e originale possono avere policy differenti.
- Archiviare un Album modifica la navigazione e non la relativa policy di accesso.
- Un Album archiviato rimane pubblico soltanto quando la sua policy lo rende pubblico.

Una modifica pubblica diventa immediatamente visibile; una modifica protetta diventa immediatamente visibile soltanto ai soggetti autorizzati.

---

## 10. Relazioni con le persone

Person, Participation, Appearance e Access Grant costituiscono un'evoluzione semanticamente consolidata ma non ancora modellata in dettaglio.

### 10.1 Participation

Participation descrive il coinvolgimento di una Person in un contenuto e il ruolo assunto in quel contesto, per esempio modella, makeup artist, stylist, fotografo o collaboratore.

Il ruolo non è una classificazione permanente della Person. La stessa persona può assumere ruoli differenti in Album o progetti differenti.

### 10.2 Appearance

Appearance indica che una Person è effettivamente rappresentata in una Photo. È distinta dalla partecipazione generale a uno shooting o Album.

### 10.3 Access Grant

Access Grant descrive quali risorse un Account può consultare o gestire. Non coincide automaticamente con Participation o Appearance.

Le policy potranno generare accessi a partire dalle relazioni editoriali, ma credito, presenza nella Photo e autorizzazione rimangono fatti distinti.

Le capacità future potranno comprendere:

- consultazione delle proprie Photo;
- accesso all'intero shooting quando autorizzato;
- selezione delle preferite;
- download di specifiche varianti;
- condivisione social;
- suggerimento di altri collaboratori;
- gestione limitata del proprio profilo.

---

## 11. Album virtuali

Gli Album virtuali costituiscono un'evoluzione semanticamente consolidata e permettono percorsi di navigazione alternativi senza duplicare o spostare contenuti fisici.

Un Album virtuale:

- è sempre una Collection;
- non possiede una folder;
- non contiene direttamente Photo;
- possiede nome, descrizione e segmento di path;
- può essere collegato a più parent;
- può contenere Album virtuali o riferimenti ad Album fisici;
- può essere vuoto come stato WIP.

Non esistono Gallery virtuali. Ogni percorso parte da una Gallery fisica e ogni ramo completo termina con un Album fisico.

### 11.1 Grafo di navigazione

La gerarchia fisica rimane l'albero canonico. I link virtuali formano con essa un grafo diretto aciclico.

Le relazioni alternative ammesse sono:

```text
Fisico   → Virtuale
Virtuale → Virtuale
Virtuale → Fisico
```

Un collegamento alternativo diretto `Fisico → Fisico` è vietato. La relazione fisica canonica rimane l'unico collegamento diretto fra due Album fisici.

Ogni coppia ordinata `(Parent, Child)` è persistita e univoca. Fra i children dello stesso parent, il segmento di path deve permettere una risoluzione univoca.

### 11.2 Routing

Album fisici e virtuali utilizzano lo stesso formato di route. Ogni path risolve univocamente un nodo, ma lo stesso Album può essere raggiunto attraverso più path.

Un Album fisico conserva un solo full path fisico canonico e può possedere più navigation path. Breadcrumb e navigazione contestuale seguono il percorso richiesto e non possono essere ricostruiti usando soltanto il parent fisico.

### 11.3 Visibilità

Un Album virtuale possiede una policy di accesso esplicita, perché la presenza di più parent rende ambigua l'ereditarietà dinamica.

La policy può essere inizializzata da quella del parent usato durante la creazione, ma rimane successivamente autonoma. Aggiungere o rimuovere parent non la modifica.

Un link virtuale non concede accesso alla destinazione. La navigazione espone soltanto children conoscibili dal chiamante e l'attraversamento di un path richiede accesso sia ai nodi del percorso sia alla risorsa finale.

Una Collection virtuale pubblica non rende pubblico un Album fisico protetto. Navigation link e Access Grant rimangono concetti separati.

### 11.4 Popolamento

I link sono sempre persistiti e aggiunti intenzionalmente. Non esistono Album virtuali basati su query dinamiche permanenti.

Una funzionalità può cercare Album fisici o virtuali che soddisfano un criterio e materializzare i link risultanti, per esempio contenuti associati alla stessa Person.

### 11.5 Archiviazione

L'archiviazione viene rappresentata dall'appartenenza a un Album virtuale con funzione `Archive`.

Questa relazione costituisce l'unica fonte dello stato archiviato:

- l'Album fisico viene escluso dalla navigazione ordinaria;
- rimane raggiungibile tramite il path fisico diretto;
- compare nell'Album virtuale di archivio;
- filesystem, parent fisico, identità e contenuti non cambiano;
- la rimozione della relazione ripristina la navigazione ordinaria.

Lo stesso Album virtuale di archivio può essere collegato a più Gallery.

### 11.6 Cancellazione

La cancellazione di un Album fisico deve gestire esplicitamente tutti i link virtuali che lo referenziano e non può lasciare relazioni pendenti. Gli effetti su figli fisici, Photo e filesystem seguono la policy distruttiva del dominio.

Eliminare un Album virtuale non elimina mai gli Album fisici referenziati e non elimina implicitamente un Album virtuale child che possiede altri parent.

Inizialmente un Album virtuale può essere eliminato soltanto quando non possiede children e non è ancora referenziato da parent aggiuntivi. Il chiamante deve prima rimuovere o ricollocare intenzionalmente i link.

Ogni Album virtuale deve rimanere raggiungibile da almeno una Gallery fisica. La rimozione dell'ultimo link entrante viene rifiutata, salvo che appartenga alla stessa operazione atomica che elimina il nodo virtuale.

---

## 12. Stato delle funzionalità

### 12.1 Modello corrente

- gerarchia fisica di Album;
- classificazione derivata `AlbumKind`;
- Photo appartenenti a un solo Album;
- mapping database–filesystem e riconciliazione;
- varianti media ricostruibili;
- cover casuale dal sottoalbero fisico;
- modifica immediatamente efficace sullo stato corrente.

### 12.2 Evoluzioni consolidate ma non implementate

- Person e collegamento opzionale con Account;
- Participation, Appearance e Access Grant;
- profilo e capacità limitate delle persone autorizzate;
- Album virtuali e grafo di navigazione;
- archiviazione tramite Album virtuale con funzione `Archive`;
- protezione differenziata delle varianti media.

### 12.3 Evoluzioni candidate

Event, Agency, Social Profile, Location, plugin Lightroom, social publishing, job asincroni e altri workflow rimangono nella Vision finché non emergono comportamenti e requisiti sufficientemente concreti.

Non è pianificato un workflow draft/pubblicato. Un'eventuale necessità futura verrà rivalutata a partire da un caso d'uso reale.

---

## 13. Decisioni aperte

- Definire il modello implementativo di Person, Participation, Appearance e Access Grant.
- Definire il meccanismo di accesso alle varianti media protette.
- Valutare se il rendering debba separare figli fisici e Album virtuali correlati.
- Valutare se vietare più percorsi fra lo stesso Album fisico sorgente non Gallery e lo stesso Album fisico di destinazione.
- Definire lifecycle e vincoli operativi della cancellazione degli originali.
- Definire il modello tecnico e le API degli Album virtuali.

---

## 14. Riferimenti

- [Domain Architecture](../Architecture/DomainArchitecture.md)
- [Security Architecture](../Architecture/SecurityArchitecture.md)
- [Visione](../Roadmap/Vision.md)
- [Roadmap](../Roadmap/Roadmap.md)
- [Backlog](../Roadmap/Backlog.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
- [Payload di ripristino delle descrizioni Album](AlbumDescriptionsRecovery.json)
