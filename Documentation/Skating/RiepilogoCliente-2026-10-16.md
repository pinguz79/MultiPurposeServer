# Skating System — riepilogo per il confronto con il cliente

16 ottobre 2026

Questo documento riassume la revisione delle ultime risposte ricevute. Distingue le
esigenze espresse dal cliente dalle soluzioni elaborate con Marco, ancora da validare.
Non descrive funzionalità già realizzate e non considera approvati i punti precedenti
rimasti senza risposta. Le decisioni tecniche sono escluse da questo riepilogo.

## 1. Chiarimenti ricevuti

- **Assolo/Duo:** oggi costituisce una specialità unica, con categorie e livelli;
  in futuro potrebbero servire specialità distinte per stile o per assolo e duo.
- **Schede dei giudici:** possono contenere più coreografie in ordine di scaletta.
  La raccolta delle schede può avvenire a blocchi, senza seguire l'ordine delle gare.
- **Iscrizioni:** gli stessi atleti possono presentare più coreografie nella stessa
  gara, purché abbiano nomi differenti.
- **Atleti senza coreografie:** restano nell'elenco completo, riconoscibili, ma non
  vengono conteggiati fra i partecipanti finché non hanno almeno un'iscrizione.
- **Livelli:** le denominazioni devono poter cambiare, ad esempio PRO/SemiPro.
- **Giuria:** normalmente tutti i giudici valutano tutte le esibizioni; occorre però
  affrontare assenze, abbandoni e sostituzioni durante la manifestazione.
- **Correzioni:** sono casi reali lo spostamento di una coreografia già valutata
  in un'altra gara, la sostituzione di un atleta e, raramente, la squalifica.
- **Chiusure:** una gara può concludersi quando tutte le sue esibizioni sono state
  gestite, anche se la scaletta alterna gare diverse.

## 2. Soluzioni proposte

### Organizzazione e inserimento voti

Una gara è identificata da **specialità, categoria e livello**. Assolo, duo e gruppo
descrivono la composizione della coreografia e non separano automaticamente le gare.
Le eventuali future divisioni si ottengono configurando le specialità desiderate.

L'operatore inserisce i voti in una **scheda dedicata a una sola esibizione**:
il formato cartaceo con più coreografie non impone una schermata collettiva.
Non occorre registrare la fine fisica dell'esibizione per accedere alla scheda.
La chiusura della scheda protegge i dati già verificati da modifiche accidentali.

Cambiare il nome di un livello non cambia le sue regole né il percorso dei premi.
La chiusura della singola gara dovrà essere rapida; valuteremo l'utilità della
chiusura di più gare insieme dopo aver verificato il flusso operativo.

### Una classifica sempre consultabile

La stessa vista è disponibile prima, durante e dopo la gara. È indicata come
**Provvisoria** fino alla chiusura, poi come **Definitiva**. Una riapertura la rende
nuovamente provvisoria.

Le schede complete sono ordinate per punteggio, segnalando quelle ancora aperte.
Le esibizioni senza voti seguono la scaletta, senza piazzamenti; quelle con voti
incompleti sono riconoscibili e non ricevono ancora una posizione. Un voto mancante
non viene sostituito da zero. Non presentate e squalificate rimangono visibili con
il proprio stato, fuori dai piazzamenti.

### Punteggi e parità

Proponiamo di mantenere voti da **0 a 10 con un decimale**, usando la somma dei voti
validi. Il decimale è parte della normale valutazione, non uno spareggio aggiuntivo.

Le parità sono accettate e quelle sul podio evidenziate. **Tutti i primi a pari
merito** accedono alla classifica successiva del rispettivo percorso. Si riutilizzano
le valutazioni già espresse, senza nuove esibizioni.

La numerazione proposta, ancora da confermare, è **1-1-3**: due primi sono seguiti
dal terzo; analogamente due secondi sono seguiti dal quarto, cioè 1-2-2-4.
Questo non stabilisce automaticamente quanti premi materiali consegnare.

### Giudici assenti: proposta da verificare con particolare attenzione

Il giudice assente e tutti i suoi voti restano registrati. L'assenza può essere
temporanea o definitiva; non occorre cancellare il giudice.

Per ogni classifica confrontiamo il numero di valutazioni delle esibizioni ammesse.
Se è uguale, non permettiamo esclusioni, anche quando hanno votato persone diverse.
Se è diverso, proponiamo che l'operatore scelga quali giudici escludere dal conteggio:

- si parte dalle giurie più numerose;
- sono candidati soltanto giudici con valutazioni mancanti in altre esibizioni;
- non si riducono le giurie già meno numerose;
- dopo ogni scelta si ricalcolano le opzioni;
- raggiunta la stessa numerosità, ulteriori esclusioni sono impedite. Per cambiare
  scelta occorre prima reincludere un giudice escluso.

Esempi:

| Giudici della prima esibizione | Giudici della seconda | Comportamento proposto |
|---|---|---|
| A, B, C | A, B, D | Tre valutazioni ciascuna: nessuna esclusione. |
| A, B, C, D | A, B, E | Si può escludere C oppure D. Dopo una scelta si torna a tre e ci si ferma. |

I voti esclusi sono ingrigiti, **non cancellati**. Non introduciamo una dichiarazione
obbligatoria di chi sostituisce chi. Resta il requisito di almeno tre giudici:
non prevediamo una deroga per accettare una valutazione con meno di tre.

Le scelte valgono per la specifica classifica: gara, disciplina o assoluta.
Un voto può quindi essere valido nella gara e non nel premio successivo;
il totale può cambiare, pur usando sempre i voti originali. Una successiva assenza
non riscrive automaticamente una gara già chiusa.

Questa proposta riguarda il numero di valutazioni dei giudici, non autorizza a
ignorare criteri lasciati incompleti per errore. Le combinazioni più complesse di
assenze richiedono ancora verifica: non assumiamo che ogni situazione sia risolvibile
con una semplice esclusione.

### Spostamenti, squalifiche e recupero dagli errori

Lo spostamento di una coreografia già valutata richiede un avviso e una conferma.
I voti e i giudici che li hanno espressi vengono conservati. Le gare chiuse coinvolte
devono essere riaperte; risultati e qualificazioni interessati vengono ricalcolati
e richiedono una nuova conferma. Le classifiche estranee non cambiano.

La squalifica è possibile anche durante un'esibizione, prima dei voti, oppure dopo
la premiazione. Se la gara è chiusa occorre prima riaprirla e poi richiuderla.
Gli eventuali voti restano conservati ma non concorrono a piazzamenti e qualificazioni.
Il podio e gli accessi ai premi successivi vengono ricalcolati.

**Non presentata** e **Squalificata** hanno indicazioni distinte ma sono entrambe
escluse dalla classifica. Non presentata si applica in assenza di voti; la squalifica
può intervenire anche con voti già registrati.

Entrambi gli stati sono revocabili, come le chiusure, per rimediare a errori umani.
La revoca recupera i dati conservati e riapre la scheda da verificare; non genera
voti e non ripristina automaticamente un vecchio podio o una classifica definitiva.

### Atleti e scuola

La sostituzione di un atleta mostra chiaramente uscente ed entrante prima della
conferma. Nella stessa gara conserva i voti della coreografia; se la scheda è chiusa
occorre prima riaprirla. Non modifica le altre iscrizioni dell'atleta.

Proponiamo di consentire il cambio della scuola rappresentata solo prima di qualsiasi
voto, anche parziale o pari a zero. Riaprire la gara non aggira questa condizione.
Correggere il semplice nome di una scuola è invece distinto dal cambiare scuola.

## 3. Conferme ancora necessarie

1. **Giudici:** è accettabile confrontare esibizioni valutate dallo stesso numero
   di giudici ma non necessariamente dalle stesse persone? È corretta la procedura
   manuale di esclusione illustrata, applicata separatamente a ogni classifica?
2. **Premi successivi:** confermate che l'eventuale esclusione di voti possa cambiare
   il totale rispetto alla gara d'origine, senza cancellare le valutazioni originali?
3. **Ex aequo:** confermate parità senza spareggio, avanzamento di tutti i primi
   e numerazione 1-1-3? Come vengono gestiti i premi materiali in caso di parità sul podio?
4. **Correzioni:** confermate le procedure di conservazione dei voti, riapertura e
   nuova convalida per spostamenti, sostituzioni, squalifiche e relative revoche?
5. **Operatività:** una chiusura rapida della singola gara è sufficiente oppure
   rimane necessaria una chiusura collettiva? Il cambio scuola limitato al periodo
   precedente ai voti copre le esigenze reali?

Le altre soluzioni della sezione 2 sono anch'esse sottoposte a verifica: la loro
presenza nel riepilogo non equivale a un'approvazione già ricevuta.
