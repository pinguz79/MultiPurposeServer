# Carta revolving AmEx — specifica consolidata

## Stato e perimetro

Decisioni approvate dall'utente, consolidate il 2026-09-11 per il prossimo vertical slice Finance.
Calcolo, persistenza e API di configurazione sono implementati; la UI revolving è implementata e verificata
localmente. Il conto AmEx non è stato creato in produzione. Rilascio server autorizzato il 14/09/2026 tramite
`Deployment/Aruba/Plans/finance-amex-revolving-server.json`; l'esito è tracciato dal workflow di deploy.
Il comportamento HelloCard esistente resta invariato.

Il perimetro comprende parametri, calcolo degli interessi, pianificazioni di oneri e rimborso, addebito su
HelloBank e rappresentazione del conto nella home desktop. Non comporta importazioni o bonifiche automatiche.

## Parametri e calendario

| Parametro logico | Valore iniziale |
|---|---|
| Plafond | 1.600,00 EUR |
| Scoperto ammesso | 0,10, cioè 10% del plafond |
| Quota rata | 0,10, cioè 10% del saldo di chiusura |
| Rata minima | 72,32 EUR |
| TAN acquisti | 0,12, cioè 12% annuo |
| Bollo | 2,00 EUR |
| Soglia bollo | 70,00 EUR, inclusiva |
| Chiusura ciclo | Giorno 6 |
| Addebito e rimborso | Giorno 19 del mese di chiusura |

Il ciclo comprende il giorno 7 del mese iniziale fino al giorno 6 del mese successivo, inclusi.
Esempio: 07/08–06/09, addebito HelloBank e rimborso AmEx il 19/09.
Per le previsioni il 19 resta fisso, senza slittamenti per weekend o festività, per decisione esplicita.
Per lo storico restano le date effettive già presenti, che possono differire dal 19.

I valori devono essere modificabili tramite la UI dei Parametri del Conto e seguire le convenzioni temporali
già esistenti. I nomi tecnici definitivi non sono fissati da questa tabella.
72,32 EUR è il minimo convenzionale scelto dall'utente, non una ricostruzione della rata minima contrattuale:
se cambia verrà aggiornato manualmente. I campioni non verificano l'intervento di tale soglia.
Per decisione dell'utente, il bollo si applica se il saldo alla chiusura, inclusi gli interessi ma escluso
il bollo corrente, è maggiore o uguale alla soglia. Soglia zero significa applicarlo sempre, anche a saldo
nullo o a credito. La soglia iniziale di 70 EUR è una regola previsionale scelta, non verificata sui PDF.

## Saldo, capitale e oneri

La home mostra un unico debito totale attuale, calcolato alla data odierna. Gli oneri previsti con data futura
non vi concorrono. Interessi e bollo vengono rappresentati come Movimenti alla chiusura del ciclo: dopo tale
data concorrono normalmente al saldo, e possono essere corretti esplicitamente sulla base dell'estratto conto.

Per il calcolo degli interessi distinguere internamente quota capitale e oneri. La quota capitale non include
interessi e bollo precedenti; il saldo totale invece li comprende. Non usare Categoria o Description per
riconoscere il ruolo contabile: la categoria rimane una classificazione indipendente della spesa.
Il rimborso resta un solo Movimento visibile, con ripartizione interna fra capitale e oneri.
La distinzione è persistita nella natura contabile del movimento, conservata indipendentemente dalla pianificazione.

Nei casi verificati il rimborso estingue integralmente interessi e bollo del ciclo precedente, destinando
il resto al capitale. Per esempio: 220,16 EUR = 16,19 EUR interessi + 2 EUR bollo + 201,97 EUR capitale.
L'ordine preciso fra singoli oneri in caso di pagamento insufficiente non è dimostrato dai campioni e resta aperto.

## Interessi e dipendenze

Per ogni giorno del ciclo ricostruire il capitale di fine giornata, applicando acquisti e quota capitale dei
rimborsi dalla data operazione. Sommare gli interessi giornalieri con precisione decimale e arrotondare ai
centesimi solo il totale del ciclo, secondo la regola Finance concordata del mezzo centesimo verso l'alto
per importi positivi. Non arrotondare gli interessi di ogni giorno né usare il tasso giornaliero troncato stampato.

```text
InteressiCiclo = ArrotondaCentesimi(SommaGiornaliera(CapitaleFineGiorno * TAN / GiorniAnno))
```

Il denominatore è 365, oppure 366 per anno bisestile secondo le note degli estratti. L'applicazione precisa
al cambio anno, specialmente in presenza di anno bisestile, richiede un caso di regressione dedicato.
Le note AmEx indicano che per operazioni contabilizzate in un ciclo successivo la valuta è il primo giorno
del ciclo di contabilizzazione: questa casistica richiede verifica specifica e non è coperta dai due test.

Le dipendenze sono direzionali: capitale giornaliero e rimborsi di cicli precedenti -> interessi correnti ->
saldo di chiusura -> rata e rimborso successivo. Gli interessi correnti non leggono sé stessi né il saldo
comprensivo degli oneri come base capitale. Mantenere il rilevamento dei cicli diretti e indiretti nelle formule.
Consolidamento e correzioni manuali devono conservare le informazioni necessarie a ricostruire la quota capitale,
anche quando la formula diventa costante e il Movimento perde il collegamento alla Pianificazione.

## Rata, scoperto e rimborso

```text
RataBase = Max(ArrotondaCentesimi(SaldoChiusura / 10), RataMinima)
Scoperto = Max(0, SaldoChiusura - Plafond)
AddebitoPrevisto = SaldoChiusura <= 0 ? 0 : Min(SaldoChiusura, RataBase + Scoperto)
```

SaldoChiusura comprende già interessi e bollo del giorno 6: non aggiungerli una seconda volta alla rata.
Lo scoperto è rimborsato interamente, non soltanto nella misura del 10%. Il 10% di tolleranza del plafond
governa la disponibilità visualizzata, non esenta dal rimborso dell'eccedenza oltre 1.600 EUR.
Non emerge dai campioni un tasso aggiuntivo distinto per lo scoperto; non introdurlo senza ulteriori evidenze.
L'addebito non supera mai il debito effettivo: con 50 EUR di debito la rata è 50 EUR anche se il minimo è
72,32 EUR. A saldo nullo o a credito non si genera un addebito positivo.

Il 19 si prevede un solo addebito negativo su HelloBank e un solo rimborso negativo su AmEx, dove le spese
aumentano il debito con segno positivo. Il rimborso riduce il saldo totale e libera disponibilità per pari importo;
non azzera il debito residuo come il ripristino di una carta a saldo.
Lo storico HelloBank può contenere due Movimenti separati (rata e scoperto): conservarli e riconciliarne la somma,
senza duplicarli o estendere tale suddivisione alle pianificazioni future.

## Home desktop

Dato principale: debito totale attuale, non il solo speso nel ciclo. Mostrare inoltre disponibilità residua
rispetto al plafond e disponibilità comprensiva dello scoperto (massimo iniziale 1.760 EUR).
Segnalazione arancione oltre 1.600 EUR; rossa oltre 1.760 EUR. I limiti derivano dai Parametri, non sono cablati.
Gli oneri futuri non alterano il confronto ad oggi con l'app AmEx; quelli già scaduti devono essere riconciliati
con l'estratto, senza presumere una conferma bancaria automatica.

## Evidenze e regressioni

Analizzati 23 estratti consecutivi da ottobre 2024 ad agosto 2026: il 10% del saldo arrotondato più l'eccedenza
oltre plafond riproduce 23 importi dovuti su 23, di cui 10 con scoperto. Questa prova non copre la rata minima.
Fonti: estratti AmEx forniti dall'utente, identificati dalla data di chiusura; non archiviare in documentazione
pubblica numeri carta, indirizzi o altri dati identificativi non necessari.

Ricostruzione dettagliata da movimenti dei PDF `2025-08-06.pdf` e `2025-09-06.pdf`:

| Ciclo | Capitale iniziale | Acquisti | Rimborso capitale | Capitale finale | Interessi calcolati / effettivi |
|---|---:|---:|---:|---:|---:|
| 07/07–06/08/2025 | 1.591,59 | 197,38 | 152,47 | 1.636,50 | 16,19 / 16,19 |
| 07/08–06/09/2025 | 1.636,50 | 148,11 | 201,97 | 1.582,64 | 15,96 / 15,96 |

Importi in EUR. Nel primo ciclo il rimborso effettivo è 169,65 EUR il 21/07, composto da 152,47 capitale,
15,18 interessi e 2 bollo. Nel secondo è 220,16 EUR il 19/08, composto come sopra descritto.
Arrotondare quotidianamente dà 16,20 e 15,97 EUR; usare la data contabile con tasso esatto dà 16,11 e 15,89 EUR.
Le medie capitale ricostruite sono circa 1.588,779677 e 1.566,257419, mentre i PDF espongono 1.588,77 e 1.566,25:
gli interessi tornano al centesimo, ma non è dimostrata l'esatta convenzione di esposizione della media.

I due casi sono riprodotti sia nei test di `RevolvingCalculator` sia nei test di integrazione con SQLite e NCalc:
interessi di 16,19 e 15,96 EUR, rate di 220,16 e 160,66 EUR. Sono coperti ripartizione del rimborso, soglia del bollo,
override a zero, mezzo centesimo e rata limitata al debito. Le regressioni coprono inoltre cambio anno bisestile,
variazione temporale del TAN, oneri corretti manualmente, consolidamento, riferimenti circolari e parametri mancanti.
Le verifiche HelloCard esistenti restano parte della suite. Il profilo AmEx è configurabile dalla nuova API;
il flusso desktop e la visualizzazione specifica revolving sono implementati; l'estensione della risposta Conto
per gli indicatori revolving richiede il rilascio server insieme al client aggiornato.

## Proprietà calcolate e parametri del motore

Le formule mantengono la sintassi NCalc e il nome canonico del conto:

- `[AmEx.InteressiCiclo]`: capitale giornaliero del ciclo chiuso più recente alla data di valutazione;
- `[AmEx.BolloCiclo]`: saldo alla chiusura, inclusi i movimenti di interessi ed esclusi i movimenti di bollo della stessa chiusura;
- `[AmEx.RataUltimoCicloChiuso]`: rata sul saldo completo della chiusura più recente, comprensivo del bollo.

Il giorno di chiusura è incluso. Gli interessi escludono dalla propria ricostruzione gli oneri della chiusura corrente,
evitando autoreferenze; oneri precedenti e rimborsi sono invece ricostruiti cronologicamente. A parità di giorno gli
oneri precedono il rimborso. L'eventuale saldo iniziale è considerato capitale: oneri iniziali devono essere rappresentati
da movimenti con natura esplicita, non incorporati indistintamente nel saldo iniziale.

Nomi dei Parametri usati dal motore: `ChiusuraCiclo`, `Tan`, `Plafond`, `QuotaRata`, `RataMinima`, `Bollo`, `SogliaBollo`.
`Tan` è risolto per ogni giorno; soglia/importo del bollo e parametri della rata alla chiusura. Si applicano priorità e
intervalli inclusivi già previsti per i Parametri. La mancanza di un parametro necessario produce errore, non un valore zero.
Il motore legge i movimenti degli oneri realmente presenti: non aggiunge implicitamente interessi o bollo al saldo;
la configurazione crea tali movimenti e i rimborsi, assegnandone la natura corretta.

## API di configurazione

```text
POST /Finance/BackEnd/Conto/{contoName}/Configurazione/CartaRevolving
```

La carta e il conto di addebito devono già esistere ed essere distinti. L'autenticazione segue quella del BackEnd
Finance, senza deroghe ulteriori. Esempio di richiesta (periodo solo illustrativo, non autorizza inserimenti):

```json
{
  "plafond": 1600.00,
  "percentualeScoperto": 0.10,
  "quotaRata": 0.10,
  "rataMinima": 72.32,
  "tan": 0.12,
  "bollo": 2.00,
  "sogliaBollo": 70.00,
  "chiusuraCiclo": 6,
  "addebito": 19,
  "contoAddebitoName": "HelloBank",
  "validFrom": "2026-10-01",
  "validTo": "2036-12-31"
}
```

Il controller apre una `Service.Operation`, chiama la configurazione e completa l'operazione soltanto se riesce.
La transazione include nove Parametri, quattro Pianificazioni, relativi Movimenti e la correlazione fra addebito
e rimborso. Le scritture intermedie permettono alle formule di leggere tutte le dipendenze, ma non fanno commit.
Gli importi vengono verificati prima del commit; errori di calcolo o persistenza causano rollback integrale.
La cache viene riutilizzata soltanto nella fase finale senza scritture, scartandola anche in caso di errore.

| Serie | Conto | Data | Natura | Formula |
|---|---|---|---|---|
| Interessi | Carta | Chiusura | Interessi | `[AmEx.InteressiCiclo]` |
| Bollo | Carta | Chiusura | Bollo | `[AmEx.BolloCiclo]` |
| Rimborso | Carta | Addebito | Rimborso | `-[AmEx.RataUltimoCicloChiuso]` |
| Addebito | Conto collegato | Addebito | Ordinario | `-[AmEx.RataUltimoCicloChiuso]` |

Le categorie sono inizialmente assenti; non sono usate per riconoscere il ruolo contabile. I nomi delle Pianificazioni
usano il prefisso stabile `Revolving {Name}: `, mentre le descrizioni iniziali dei Movimenti usano il DisplayName.
La risposta restituisce il nome del conto, i quattro identificativi delle Pianificazioni e `Created`.

- `201 Created`: nuova configurazione completa.
- `200 OK`: configurazione già presente e compatibile; nessuna duplicazione di pianificazioni o movimenti.
- `400 Bad Request`: valori/calendario non validi o formule non valutabili.
- `404 Not Found`: carta o conto collegato non presenti.
- `409 Conflict`: profilo parziale/a saldo, pianificazioni o correlazione incomplete/incompatibili.
- `401 Unauthorized`: chiave assente o non valida fuori da Development.

La ripetizione aggiorna i valori permanenti dei Parametri mantenendo identità, priorità e override temporali,
senza ricreare i Movimenti già consolidati o modificati manualmente. Non cambia periodo, periodicità o conto
collegato di Pianificazioni esistenti: come per CartaASaldo, tali modifiche appartengono al flusso futuro di
modifica/rigenerazione delle Pianificazioni. Profili a saldo non vengono convertiti automaticamente.

In questo bootstrap addebito e rimborso coincidono e seguono la chiusura nello stesso mese. Si rifiutano calendari
che farebbero coincidere chiusura e addebito nei mesi corti, e override di calendario incompatibili con le date
generate. La modifica generica di un Parametro di calendario non ripianifica automaticamente movimenti esistenti.
Le modifiche economiche, invece, vengono considerate dalle formule non ancora consolidate.

## Stato della persistenza e prossimi blocchi

`Movimento.Natura` e `Pianificazione.MovimentoNatura` distinguono `Ordinario`, `Interessi`, `Bollo` e `Rimborso`.
Sono metadati contabili indipendenti da descrizione e categoria: nel profilo revolving i movimenti ordinari
variano il capitale, gli oneri restano esclusi dalla base interessi e il rimborso viene ripartito fra oneri e capitale.
La natura è memorizzata sul movimento stesso e sopravvive a consolidamento e scollegamento dalla pianificazione.
La migrazione `AddNaturaMovimento` inizializza i dati esistenti a `Ordinario`, senza riclassificare automaticamente
i rimborsi storici o alterare formule e importi. La migrazione è verificata su SQLite in memoria ed è inclusa
nel server distribuito il 14/09/2026. L'estensione delle API dei movimenti espone questi metadati per importare
lo storico senza perdere la distinzione contabile (richiede il relativo rilascio server).

### Natura contabile nelle API dei movimenti

Il campo JSON `natura` usa i valori numerici `0` Ordinario, `1` Interessi, `2` Bollo, `3` Rimborso.
Non dipende dalla categoria o dalla descrizione e non viene dedotto automaticamente dal segno dell'importo.

- Bulk Create: campo facoltativo per ogni item, con default `0` per compatibilità con i payload esistenti.
- PATCH puntuale e Bulk Update: campo nullable; assente/null conserva il valore persistito, `0` lo riporta
  esplicitamente a Ordinario. È ammessa la modifica della sola natura.
- La risposta amministrativa `MovimentoConfigurationDto`, incluse le risposte bulk, restituisce `natura`.
- Valori numerici non definiti sono rifiutati prima della scrittura. Nel flusso bulk l'errore è `InvalidNatura`,
  nel PATCH puntuale la risposta è HTTP 400. Restano valide le strategie AllOrNothing/PartialSuccess esistenti.
- Modificare importo, categoria o descrizione senza specificare natura non la altera. Consolidamento e
  scollegamento dalla pianificazione la preservano; la cache di calcolo viene invalidata dalle modifiche.

Non sono necessarie nuove migrazioni: i campi sono già presenti. Non vengono riclassificati automaticamente
i movimenti esistenti né modificate le pianificazioni. Sulla carta le spese aumentano il debito e i rimborsi
lo riducono; sull'altro conto l'addebito corrispondente resta un movimento Ordinario negativo.

## Configurazione desktop

Da Configurazione → Parametri conti, selezionare la carta e scegliere «Configura carta revolving...». La dialog
propone i valori AmEx sopra indicati; il conto di addebito esclude la carta stessa. Percentuali espresse in UI
da 0 a 100 vengono inviate come coefficienti da 0 a 1. Date e importi usano controlli dedicati.
La conferma invia una sola richiesta atomica; gli errori restano nella dialog senza perdere i valori inseriti.
Durante l'invio non sono possibili ulteriori conferme o chiusure della dialog. Il periodo deve essere scelto
evitando duplicazioni con lo storico: non vengono eseguite riconciliazioni automatiche.

La scorciatoia è disabilitata per profili già configurati a saldo o revolving: le variazioni economiche passano
dalla griglia Parametri, che conserva intervalli e priorità. Il calendario delle pianificazioni non si rigenera
modificando i parametri; tale gestione resta fuori perimetro.

La risposta frontend Conto include `RevolvingIndicators`, separato dagli indicatori della carta a saldo:
plafond, scoperto assoluto e due disponibilità residue calcolate con i parametri alla data odierna.
La home usa `Balance` come debito attuale e mostra le disponibilità; arancione e rosso scattano soltanto oltre
le rispettive soglie, non all'uguaglianza. Non effettua chiamate aggiuntive per ogni card né include oneri futuri
nel debito attuale. La navigazione delle carte a saldo e revolving usa la vista per cicli: all'apertura il server
sceglie il ciclo corrente, mentre la navigazione successiva seleziona mese e anno di chiusura. Il parametro
`ChiusuraCiclo` governa i confini (per AmEx 7-6), senza deduzioni dal nome del conto. Oneri e rimborsi restano
visibili e contribuiscono ai totali. Un conto senza profilo carta resta nella vista mensile; l'import storico
dei movimenti non configura automaticamente parametri o pianificazioni.

Ogni ciclo espone righe virtuali di saldo iniziale e saldo finale, senza persistere movimenti aggiuntivi.
Il ciclo aperto distingue il saldo ad oggi dalla previsione di chiusura, se contiene movimenti futuri;
i cicli futuri indicano esplicitamente il saldo previsto. I rimborsi contribuiscono solo al proprio ciclo.
Per il conto AmEx il riepilogo mostra il plafond residuo esatto alla data del saldo e il confronto con
l'estratto: arrotondamento per difetto all'euro, minimo zero. Il parametro Plafond viene risolto per
intervallo e priorita alla data del riepilogo, non usando il valore odierno per i cicli storici.
Questa rappresentazione non modifica gli importi, i saldi o gli arrotondamenti delle formule.
Anche la card AmEx in home mostra il disponibile arrotondato per difetto all'euro (minimo zero),
su una riga separata dal plafond residuo esatto e dal residuo comprensivo dello scoperto.
Usa gli indicatori gia restituiti dalla lista conti, senza ulteriori chiamate API.

## Estensione revolving a rata fissa (Carta Agos)

Il contratto di configurazione accetta l'importo opzionale `Rata`: se presente, `QuotaRata` e
`RataMinima` devono essere zero e non vengono persistiti. Il solo parametro `Rata`, con le consuete
definizioni temporali ordinate, determina il pagamento: `Min(Max(saldo a chiusura, 0), Rata)`.
Gli interessi e gli oneri gia contabilizzati nella chiusura contribuiscono al saldo, ma l'eccedenza
rispetto al plafond non si aggiunge alla rata. Senza `Rata` resta invariato il calcolo AmEx.
Non viene convertito automaticamente un profilo esistente da una modalita all'altra.

La chiusura al giorno 31 equivale all'ultimo giorno disponibile del mese, anche a febbraio bisestile.
L'addebito usa l'ultimo ciclo chiuso: il giorno 20 con chiusura 31 si riferisce al mese precedente.
Chiusura e addebito coincidenti (anche dopo l'adattamento al mese corto) restano vietati.
Le pianificazioni sono generate nel periodo richiesto; evitare sovrapposizioni con addebiti gia presenti.
Il desktop offre l'opzione `Rata fissa`, che sostituisce quota percentuale e minimo con la rata scelta.
La rata viene risolta alla data di chiusura: un override per un addebito di ottobre deve coprire
la chiusura di settembre. La home riconosce anche il profilo con `Rata` e mostra il residuo negativo.

Per Carta Agos sono concordati plafond 5.600 EUR, scoperto zero, rata scelta 500 EUR, chiusura a
fine mese e addebito/rimborso il 20 successivo. Nessun bollo. TAN provvisorio 12%, copiato dal valore
AmEx ma indipendente e da verificare sugli estratti conto, come il metodo di calcolo degli interessi.
Il minimo contrattuale 168 EUR limita la scelta della rata, non il pagamento finale del residuo:
non e un parametro di calcolo distinto. Il validatore generico richiede una rata positiva e non
codifica limiti contrattuali specifici di un emittente; nella configurazione Agos va rispettato 168 EUR.
Il passaggio futuro a carta a saldo richiede una scelta esplicita, non avviene automaticamente a debito zero.
Questa estensione non configura i dati Agos in produzione e non importa movimenti.

Restano da implementare/definire:

- Rilascio del client aggiornato e dell'estensione della risposta Conto; collaudo con il conto AmEx reale.
- Saldo iniziale e data iniziale dell'import AmEx; riconciliazione dei rimborsi HelloBank già presenti.
- Casi limite di valuta, bisestile e pagamenti insufficienti, mantenendo possibile la bonifica manuale.
