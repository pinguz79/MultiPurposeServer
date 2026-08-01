# Processo di Code Review di MultiPurposeServer

## 1. Scopo del documento

Questo documento descrive come eseguire una code review completa della solution MultiPurposeServer.

La revisione descritta in questo documento non coincide con la normale verifica di una singola pull request. È una revisione periodica e trasversale dell'intera solution, finalizzata a verificare che codice, test, architettura e documentazione siano ancora coerenti dopo una fase significativa di evoluzione.

La checklist operativa utilizzata durante la revisione è definita in:

- `CodeReviewChecklist.md`

---

## 2. Quando eseguire una code review completa

Una revisione completa della solution dovrebbe essere avviata quando si verifica almeno una delle seguenti condizioni:

- è stato completato un refactoring architetturale significativo;
- sono stati introdotti o modificati componenti condivisi;
- più attività successive hanno modificato responsabilità e confini del sistema;
- la struttura dei test non riflette più chiaramente la struttura del codice produttivo;
- la documentazione è stata aggiornata e deve essere confrontata con l'implementazione reale;
- sono emersi più segnali di debito tecnico collegati tra loro;
- una milestone architetturale sta per essere dichiarata conclusa;
- il team ritiene necessario fermare l'evoluzione funzionale per verificare lo stato complessivo della solution.

La code review completa non deve essere eseguita dopo ogni piccola modifica. Deve essere utilizzata quando il valore della verifica trasversale supera il costo dell'interruzione.

---

## 3. Obiettivi

La code review completa deve verificare che:

- la solution compili senza errori;
- i test siano verdi e organizzati nel livello corretto;
- le responsabilità architetturali siano rispettate;
- le dipendenze puntino nella direzione prevista;
- il codice non contenga residui di architetture o convenzioni superate;
- le API espongano contratti coerenti;
- sicurezza, configurazione e infrastruttura siano gestite correttamente;
- la documentazione descriva il sistema reale;
- il debito tecnico emerso venga classificato e tracciato;
- la revisione termini con una decisione esplicita sullo stato della milestone analizzata.

La revisione non ha lo scopo di rendere perfetto ogni file. Ha lo scopo di rendere visibili i problemi, correggere quelli necessari e registrare consapevolmente quelli rinviati.

---

## 4. Principi della revisione

### 4.1 Verificare il comportamento prima della forma

La revisione parte da build, test e comportamento osservabile.

La pulizia formale del codice viene affrontata soltanto dopo aver verificato che il sistema funzioni e che le responsabilità siano corrette.

### 4.2 Procedere per aree

La solution viene analizzata un'area alla volta.

Ogni area deve essere completata o sospesa con un TODO esplicito prima di passare alla successiva.

### 4.3 Non mescolare refactoring indipendenti

Una revisione può far emergere più interventi, ma non tutti devono essere eseguiti immediatamente.

Modifiche funzionali, refactoring architetturali, riorganizzazione dei test e pulizia della formattazione devono rimanere attività distinguibili.

### 4.4 Preservare il comportamento

Ogni modifica introdotta durante la revisione deve mantenere il comportamento atteso, salvo quando viene identificato e corretto un difetto reale.

I test non devono essere modificati soltanto per farli tornare verdi.

### 4.5 Fermarsi quando lo scopo è raggiunto

La review termina quando:

- tutti i punti applicabili della checklist sono stati valutati;
- i problemi bloccanti sono stati risolti;
- i problemi rinviati sono stati registrati;
- build e test finali sono verdi;
- il risultato della revisione è stato documentato.

La review non deve trasformarsi in un refactoring senza fine.

---

## 5. Preparazione

Prima di iniziare:

1. assicurarsi che il working tree sia pulito oppure che le modifiche presenti siano chiaramente comprese;
2. eseguire build e test completi;
3. registrare warning, test falliti e problemi già noti;
4. identificare la milestone o l'area che ha motivato la revisione;
5. aprire `CodeReviewChecklist.md`;
6. creare un registro dei rilievi.

La revisione deve partire da uno stato noto. Se build o test sono già falliti, il problema deve essere registrato prima di iniziare le modifiche.

---

## 6. Perimetro

Il perimetro predefinito comprende:

- host MultiPurposeServer;
- Applications;
- Domains;
- Shared Framework;
- Contracts;
- persistenza e infrastruttura;
- sicurezza;
- test;
- configurazione della solution;
- documentazione tecnica;
- script e strumenti inclusi nella solution.

È possibile limitare la revisione a un sottoinsieme, purché il perimetro venga dichiarato esplicitamente.

Esempio:

```text
Perimetro:
- Shared Validation Framework
- Portfolio.Contracts
- pipeline MVC di Portfolio.Api
- test correlati
- documentazione e ADR associati
```

---

## 7. Registro dei rilievi

Ogni problema emerso deve essere classificato.

Formato consigliato:

| Campo | Descrizione |
|---|---|
| Area | Progetto, layer o documento interessato |
| Rilievo | Problema osservato |
| Gravità | Bloccante, alta, media, bassa |
| Tipo | Difetto, debito tecnico, incoerenza architetturale, test, documentazione, pulizia |
| Decisione | Correggere ora, registrare come TODO, nessun intervento |
| Destinazione | Commit, issue, roadmap, ADR o documento da aggiornare |
| Stato | Aperto, in corso, risolto, rinviato |

Le tre decisioni ammesse sono:

### Correggere ora

Il problema:

- impedisce la build o l'esecuzione dei test;
- viola un confine architetturale importante;
- introduce un comportamento errato;
- rende falsa o incoerente la documentazione corrente;
- impedisce di dichiarare conclusa la milestone.

### Registrare come TODO

Il problema è reale, ma:

- non è bloccante;
- richiede un refactoring separato;
- non appartiene al perimetro corrente;
- deve essere pianificato in una milestone successiva.

### Nessun intervento

Il comportamento è intenzionale, coerente e adeguatamente documentato.

La decisione deve essere esplicita per evitare che lo stesso punto venga riaperto senza nuove informazioni.

---

## 8. Ordine di esecuzione

L'ordine consigliato è:

1. stato iniziale;
2. struttura della solution;
3. architettura e dipendenze;
4. Contracts e API;
5. Application e dominio;
6. infrastruttura e persistenza;
7. sicurezza;
8. codice condiviso;
9. testing;
10. qualità e organizzazione del codice;
11. documentazione;
12. pulizia e verifica finale.

L'ordine può essere adattato, ma è preferibile verificare prima le responsabilità e soltanto dopo la forma interna dei file.

---

## 9. Modalità di lavoro

Per ogni area:

1. leggere la documentazione autorevole;
2. identificare i progetti e i file interessati;
3. confrontare responsabilità dichiarate e implementazione;
4. eseguire i test pertinenti;
5. registrare i rilievi;
6. applicare una modifica alla volta;
7. compilare ed eseguire i test dopo ogni passo significativo;
8. aggiornare documentazione e TODO quando necessario;
9. marcare la sezione della checklist come completata.

Quando emerge un problema fuori perimetro, non bisogna interrompere automaticamente la review corrente. Il problema va registrato e classificato.

---

## 10. Evidenze richieste

Una review completa dovrebbe produrre almeno:

- checklist compilata;
- elenco dei rilievi;
- elenco delle modifiche effettuate;
- TODO o issue creati;
- documenti aggiornati;
- risultato della build finale;
- risultato dei test finali;
- eventuali warning residui;
- decisione finale sulla milestone.

---

## 11. Esito della revisione

La review può concludersi con uno dei seguenti esiti.

### Approvata

- nessun problema bloccante;
- build verde;
- test verdi;
- documentazione coerente;
- debito residuo tracciato.

### Approvata con TODO

- nessun problema bloccante;
- build e test verdi;
- rimangono interventi non urgenti registrati esplicitamente.

### Non approvata

- esistono difetti o incoerenze che impediscono di considerare stabile l'area revisionata;
- build o test non sono affidabili;
- la documentazione non descrive lo stato reale;
- rimangono problemi bloccanti non risolti.

---

## 12. Chiusura

Al termine:

1. eseguire build completa;
2. eseguire tutti i test;
3. verificare il working tree;
4. controllare che non siano stati aggiunti artefatti generati;
5. aggiornare roadmap e documentazione;
6. registrare l'esito;
7. creare un commit dedicato quando appropriato.

La code review è conclusa soltanto quando il risultato è verificabile e riproducibile.

---

## 13. Vedi anche

- `MpsPlaybook.md`
- `CodeReviewChecklist.md`
- `../Architecture/Architecture.md`
- `../Architecture/ArchitectureRoadmap.md`
- `../Architecture/TestingArchitecture.md`
