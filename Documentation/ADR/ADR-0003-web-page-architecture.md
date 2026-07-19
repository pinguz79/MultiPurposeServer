# ADR-0003 — Le applicazioni Web adottano una Page Architecture solo quando necessaria

## Stato

Accettata

## Contesto

Le pagine Web possono avere complessità molto diverse.

Alcune recuperano dati da un singolo Service e renderizzano una View. Altre coordinano API remote, cache, routing, paginazione e composizione di più fonti dati.

Applicare lo stesso numero di layer a ogni pagina produrrebbe astrazioni artificiali. Lasciare tutta l'orchestrazione nei Controller renderebbe invece le pagine complesse difficili da comprendere e mantenere.

## Decisione

Per le pagine con logica non banale si adotta il flusso:

```text
Controller
    ↓
Page Service
    ↓
Page Model
    ↓
View
    ↓
Components
```

Il pattern non deve essere applicato meccanicamente.

Una pagina semplice può essere gestita direttamente dal Controller quando:

- recupera dati da un solo Service;
- non coordina più infrastrutture;
- non contiene logica significativa di composizione.

Il Page Service viene introdotto quando esiste una reale orchestrazione.

Il Page Model rappresenta lo stato applicativo completo richiesto dalla View e deve rimanere indipendente da HTTP e HTML.

Ogni componente riceve il modello più piccolo sufficiente alla propria responsabilità.

```text
AlbumCard    ← singolo Album
AlbumGrid    ← collezione di Album
PhotoBrowser ← AlbumPage
```

## Conseguenze

### Vantaggi

- I Controller complessi rimangono sottili.
- La composizione della pagina è isolata.
- Le View ricevono uno stato coerente.
- I componenti riutilizzabili non dipendono inutilmente dall'intera pagina.
- Le pagine semplici non vengono appesantite da classi senza valore reale.

### Costi

- La scelta di introdurre un Page Service richiede valutazione caso per caso.
- Il Page Model può diventare troppo ampio se non si distingue lo stato applicativo dai dettagli di presentazione.
