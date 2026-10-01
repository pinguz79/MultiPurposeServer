# Finance — Finanziamenti

## 1. Stato e perimetro

Decisioni funzionali approvate il 1 ottobre 2026. Specifica del prossimo vertical slice; implementazione,
migrazione, importazione dei dati e distribuzione non ancora eseguite.

Il Finanziamento è un'entità autonoma: non è un Conto e non possiede Movimenti contabili. Gli addebiti eventualmente
registrati su HelloBank o altri Conti continuano a esistere indipendentemente, senza collegamento obbligatorio,
generazione o modifica automatica da parte del Finanziamento.

Il primo rilascio supporta rate mensili costanti e TAN fisso. Consente di conoscere:

- capitale residuo stimato all'ultima rata considerata pagata;
- numero e totale delle rate contrattuali residue, inclusi assicurazione e altre spese;
- scadenza dell'ultima rata;
- ultimo capitale residuo verificato comunicato dal finanziatore e relativa rata.

Il capitale residuo non è un conteggio ufficiale di estinzione anticipata. Non si calcolano gli interessi tra
l'ultima rata e oggi, indennizzi, rimborsi assicurativi o altre rettifiche di estinzione. La UI non deve chiamarlo
«importo di estinzione oggi».

## 2. Persistenza e dati logici

Si persistono soltanto Finanziamento e RiallineamentoFinanziamento. Il piano rata per rata è una proiezione
calcolata al bisogno, non una collezione di entità persistite e non una Pianificazione Finance.

Il Finanziamento contiene logicamente:

- identificazione e nome visualizzato, finanziatore;
- capitale iniziale;
- TAN fisso;
- importo mensile destinato a capitale e interessi;
- assicurazione mensile e altre spese mensili fisse, con default zero;
- prima scadenza e numero contrattuale di rate, da cui derivare l'ultima scadenza;
- flag di chiusura esplicito.

L'elenco è funzionale, non un Contract definitivo. Naming tecnico, normalizzazione, vincoli numerici e di
unicità e rappresentazione del giorno contrattuale devono essere precisati prima dell'implementazione.
Il giorno contrattuale non deve essere perso quando una scadenza viene adattata a un mese corto.

Il RiallineamentoFinanziamento appartiene a un Finanziamento e identifica una specifica rata del piano,
non una data libera, con il capitale residuo verificato **dopo** quella rata. È ammesso un solo riallineamento
per rata, modificabile ed eliminabile. Si conservano i riallineamenti delle diverse rate; non è richiesto
uno storico delle revisioni del medesimo riallineamento.

## 3. Scadenze e pagamento presunto

Una rata è considerata pagata quando la sua scadenza è strettamente precedente alla data odierna di riferimento.
La rata di oggi è ancora residua. Non esiste una conferma esplicita del pagamento né un flag persistito per rata.
Si tratta di un'assunzione previsionale, non di una verifica dell'addebito bancario.

Se il giorno contrattuale non esiste nel mese, si usa l'ultimo giorno disponibile. Nessuno slittamento per
weekend o festività. Tutti i risultati di una risposta devono usare la stessa data di riferimento, resa
esplicita e controllabile nei test.

Il numero residuo conta le scadenze da oggi in avanti. Il totale residuo somma le rate contrattuali complete:
quota finanziamento + assicurazione + altre spese. Non coincide con il capitale residuo.

## 4. Piano stimato e arrotondamento

Per ciascuna rata, partendo dal capitale residuo precedente:

1. interessi = capitale residuo × TAN / 12;
2. arrotondamento degli interessi ai centesimi con `MidpointRounding.AwayFromZero`;
3. quota capitale teorica = rata finanziamento − interessi;
4. riduzione del capitale per la quota capitale; capitale residuo mai negativo.

Assicurazione e altre spese non riducono il capitale. Il TAN è usato come coefficiente nel calcolo (13,45%
corrisponde a 0,1345); il formato di trasmissione deve essere allineato alle convenzioni Finance nei Contract.

Quando la quota capitale teorica supera il debito disponibile, la riduzione effettiva si ferma a quel debito.
La differenza rispetto alla rata contrattuale viene segnalata, non nascosta. Dopo l'azzeramento del capitale
non si calcolano ulteriori interessi. Le rate contrattuali successive restano visibili e incluse nel totale
residuo previsto, senza riclassificarle falsamente come capitale rimborsato.

Anche un capitale positivo dopo l'ultima rata è uno scostamento da segnalare. Il calcolo e i riallineamenti
non cambiano automaticamente importi delle rate, numero di rate o scadenza finale. La presentazione deve
rendere leggibile la differenza fra totale contrattuale e componenti stimate in questi casi limite.

## 5. Riallineamenti

Il riallineamento sostituisce il capitale residuo dopo la rata di riferimento. Quella rata mantiene le quote
stimate a partire dal residuo precedente; il valore verificato è una correzione esplicita del saldo successivo,
non un pagamento aggiuntivo. Le rate seguenti vengono calcolate dal valore verificato.

Ogni calcolo usa l'ultimo riallineamento applicabile; un riallineamento successivo non modifica i risultati
precedenti. Modifica o eliminazione di un riallineamento ricalcolano le proiezioni dipendenti, fino al successivo
valore verificato. Per la GUI sono selezionabili soltanto rate precedenti a oggi, considerate pagate.

Il riepilogo distingue il residuo stimato corrente dall'ultimo valore verificato con rata di riferimento.
In assenza di riallineamenti il piano parte dal capitale iniziale e non inventa un dato «verificato».

## 6. Chiusura e GUI

La chiusura del Finanziamento è esplicita: né l'ultima scadenza né l'azzeramento del capitale lo chiudono
automaticamente. Un finanziamento aperto può quindi restare visibile per controllare scostamenti finali.

Menu principale: `Conti`, `&Finanziamenti`, `Configurazione`. Finanziamenti è un menu gemello di Conti, con
l'elenco dei finanziamenti aperti. «Nuovo finanziamento…» e il separatore prima dell'elenco sono futuri,
non comandi disabilitati da introdurre nel primo rilascio.

Il click su un finanziamento apre il dettaglio nell'area principale del client. Il riepilogo resta fisso in alto:

- nome, capitale residuo stimato all'ultima rata considerata pagata;
- rata mensile complessiva;
- numero e totale delle rate residue;
- scadenza finale;
- ultimo riallineamento applicato, con importo e rata di riferimento;
- eventuale scostamento del piano rispetto al contratto.

Sotto scorre il piano calcolato, con scadenza, quota capitale, interessi, assicurazione, altre spese, totale rata
e capitale residuo. I riallineamenti sono evidenziati sulla rata interessata. Di default si mostrano soltanto
le rate residue; «Mostra anche le rate passate» rende consultabile il piano completo.

Un'azione sulla rata apre una dialog «Capitale residuo dopo questa rata», per inserire o modificare il
riallineamento; deve essere possibile anche eliminarlo. Non si introduce editing diretto della rata.

## 7. Superfici applicative e attività future

Primo rilascio: creazione, modifica e chiusura del finanziamento tramite API; consultazione del piano e
gestione dei riallineamenti anche nel desktop. Orchestrazione nei Controller, logica
di calcolo testabile nei livelli applicativi, persistenza secondo Shared.Persistence e lazy loading vigente.

Restano fuori perimetro:

- creazione/modifica anagrafica/chiusura da GUI;
- cambio rata, salto rata, estinzione parziale, TAN variabile e periodicità non mensili;
- collegamento automatico con Movimenti o Pianificazioni;
- conteggio ufficiale di estinzione e interessi inframensili;
- importatore generico di contratti o piani PDF.

In presenza di riallineamenti le modifiche ai dati contrattuali restituiscono `409`: occorre prima
eliminare esplicitamente i riallineamenti. Restano modificabili DisplayName, finanziatore e stato di chiusura.
Il nome logico è immutabile. La rata deve coprire gli interessi mensili e ridurre il capitale.

Superficie implementata:

- `GET /Finance/FrontEnd/Finanziamento/List`: finanziamenti aperti.
- `GET /Finance/FrontEnd/Finanziamento/{name}`: piano e riepilogo alla data del server.
- `GET /Finance/BackEnd/Finanziamento/List?includeClosed=true`: elenco anche dei chiusi.
- `GET /Finance/BackEnd/Finanziamento/{name}`: dati contrattuali.
- `POST /Finance/BackEnd/Finanziamento`: creazione.
- `PATCH /Finance/BackEnd/Finanziamento/{name}`: modifica e chiusura/riapertura tramite IsClosed.
- `POST`, `PATCH`, `DELETE /Finance/BackEnd/Finanziamento/{name}/Riallineamento/{number}`:
  creazione, modifica ed eliminazione del capitale verificato dopo la rata indicata.

Importi non negativi con massimo due decimali; capitale iniziale e rata strettamente positivi;
TAN espresso come coefficiente non negativo; da 1 a 1200 rate. DueDay omesso assume il giorno
della prima scadenza. La prima scadenza deve rispettare DueDay, limitato all'ultimo giorno del mese.
Le richieste non valide restituiscono `400`, le risorse assenti `404`, i duplicati `409`.

## 8. Caso di riferimento: Findomestic

Fonti fornite dall'utente: `Contratto_prestito.pdf` (condizioni economiche e allegato assicurativo) e
`Esportazione lista.pdf` del 01/10/2026. Non contengono la tabella ufficiale di ammortamento.
L'utente conferma prima rata a marzo 2026 e assenza di variazioni.

| Dato | Valore |
| --- | ---: |
| Capitale iniziale | 20.000,00 € |
| TAN | 13,45% |
| Rata finanziamento | 304,00 € |
| Assicurazione mensile | 25,50 € |
| Altre spese mensili | 0,00 € |
| Rata complessiva | 329,50 € |
| Prima rata | 20/03/2026 |
| Numero rate | 120 |
| Ultima rata | 20/02/2036 |
| Residuo verificato dopo la rata 7, 20/09/2026 | 19.422,49 € |
| Rate residue al 01/10/2026 | 113 |
| Totale rate residue al 01/10/2026 | 37.233,50 € |

Il residuo verificato è il dato di riconciliazione, non la prova che una ricostruzione da TAN/12 riproduca
esattamente le convenzioni del finanziatore. Nessun dato reale viene inserito per effetto di questa specifica.

## 9. Criteri di verifica

- Piano senza riallineamenti e separazione capitale/interessi/costi accessori.
- Arrotondamento al mezzo centesimo secondo la regola Finance.
- Confine temporale: ieri pagata, oggi e domani residue; mesi corti e anni bisestili.
- Riallineamento dopo rata, unicità per rata, modifica/eliminazione e più riallineamenti successivi.
- Valori precedenti a un riallineamento successivo invariati.
- Capitale mai negativo, stop interessi a zero, scostamenti finali positivi o azzeramento anticipato visibili.
- Rate e totale contrattuale non modificati implicitamente dalle correzioni del capitale.
- Chiusura esclusivamente esplicita ed elenco menu limitato agli aperti.
- GUI: riepilogo fisso, piano scorrevole, filtro passato e azioni consentite solo sulle rate passate.
- Caso Findomestic: 113 rate e 37.233,50 € al 01/10/2026, capitale corrente pari al riallineamento di 19.422,49 €.
