# Finance expression engine spike

## Obiettivo

Lo spike confronta NCalc e Dynamic Expresso sul vocabolario minimo ricavato dalle formule realmente necessarie a Finance:

- costanti e Voci ricorrenti;
- proprietà e Parametri dei Conti;
- operatori `+`, `-`, `*`, `/` e meno unario;
- funzioni `min` e `max`;
- estrazione case-insensitive delle dipendenze.

## Corpus

Il corpus comprende:

- addebito della carta principale;
- rata Amex con minimo contrattuale;
- rata Agos limitata al debito residuo;
- scoperto Amex;
- interessi Amex e Agos.

Finance adotta direttamente la sintassi NCalc senza esporre oggetti del dominio:

```text
[amex.saldoCicloPrecedente]                              (NCalc e Finance V1)
[amex.saldoCicloPrecedente] -> amex_saldoCicloPrecedente (solo adapter dello spike Dynamic Expresso)
```

## Risultati

Entrambi i motori valutano correttamente il corpus e richiedono un adapter per la sintassi Finance.

Nessuno dei due applica autonomamente l'arrotondamento monetario dopo ciascuna operazione. La formula di controllo
`[primoImporto] / 2 + [secondoImporto] / 2`, con entrambi gli importi pari a `1,01`, restituisce `1,01`; applicando
`AwayFromZero` a ogni divisione Finance deve invece ottenere `1,02`. La policy deve quindi appartenere a un evaluator
Finance posto sopra il motore scelto.

Dynamic Expresso offre una sintassi C# più ricca del necessario. Questa capacità non porta benefici al corpus Finance
e aumenta il lavoro necessario per impedire costrutti non appartenenti al vocabolario autorizzato.

NCalc nasce come expression evaluator, espone un linguaggio più ristretto e rende naturale trattare i riferimenti
Finance come parametri opachi. La differenza di case delle funzioni native (`Min` e `Max`) viene assorbita dall'adapter.

## Esito

Lo spike raccomanda **NCalc**.

L'adozione produttiva deve mantenere un Evaluator Finance responsabile di:

- parsing e normalizzazione dei riferimenti `[...]`;
- risoluzione temporale;
- vocabolario autorizzato;
- dependency discovery e cycle detection;
- aritmetica e arrotondamento monetari;
- traduzione degli errori in errori di dominio.

NCalc definisce la sintassi persistita V1 ma non la semantica Finance. Un eventuale cambio futuro di motore verrà
gestito tramite migrazione delle Formule o adapter dal formato V1, senza introdurre oggi un translator preventivo.
