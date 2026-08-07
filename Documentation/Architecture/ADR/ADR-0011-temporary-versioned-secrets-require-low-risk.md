# ADR-0011 — I segreti versionati temporaneamente richiedono rischio basso

## Stato

Accettato

## Ambito

Sicurezza e infrastruttura di deployment

## Data della decisione

2026-08-07

## Origine

`ADR-ALPHA-0011`, rivalutato durante il consolidamento di `SecurityArchitecture.md`.

## Contesto

Il deployment corrente su Aruba e Altervista privilegia semplicità, ripetibilità e disaster recovery immediato. Alcuni valori sensibili sono quindi presenti nei file di configurazione distribuiti e nella history del repository privato.

La separazione immediata richiederebbe un processo di distribuzione e ripristino più articolato. I valori attualmente esposti sono stati censiti considerando massimo danno, difficoltà di recovery e probabilità concreta di attacco; il rischio residuo è stato classificato basso o molto basso nel contesto corrente.

## Decisione

Lo stato architetturale obiettivo mantiene i segreti separati dal codice e dalla configurazione pubblica versionata.

Durante il bootstrap è tuttavia accettata temporaneamente l'esposizione o il versionamento di un segreto soltanto quando una valutazione preventiva e documentata conclude che il rischio residuo è basso o molto basso.

La valutazione considera almeno:

- massimo danno ottenibile;
- difficoltà, costo e tempo di recovery;
- probabilità concreta di attacco;
- possibilità di revoca o rotazione;
- condizioni che richiedono una rivalutazione.

Ogni nuovo segreto segue lo stesso processo prima dell'esposizione. L'accettazione delle categorie attuali non costituisce precedente automatico. Un rischio medio o superiore non può beneficiare dell'eccezione.

Le valutazioni sono registrate senza valori sensibili nel `SecretRiskRegister.md`. La migrazione futura verso la separazione e la rotazione rimane debito tecnico.

## Vincoli

- Il repository rimane privato e con accesso limitato.
- Segreti, password e token non vengono registrati nei log o restituiti nelle risposte.
- Ogni valore deve poter essere sostituito senza modificare il codice.
- Una credenziale compromessa viene ruotata.
- Cambiamenti di esposizione, impatto, recovery o probabilità provocano una nuova valutazione.
- La migrazione comprende la rotazione dei valori già presenti nella history.

## Conseguenze

### Positive

- Il deployment corrente rimane semplice e ripetibile.
- Il disaster recovery non dipende da modifiche manuali non documentate sugli host.
- Le eccezioni future richiedono una decisione esplicita e confrontabile.
- Il costo della mitigazione rimane proporzionato al rischio concreto.

### Negative

- Valori sensibili rimangono presenti in history, clone e backup.
- Il repository deve restare privato finché persiste l'eccezione.
- Ogni variazione del contesto richiede rivalutazione.
- La migrazione finale richiederà separazione, aggiornamento degli host e rotazione completa.

## Condizioni di revisione

- repository pubblico o condiviso più ampiamente;
- ingresso di altri sviluppatori;
- introduzione di dati o servizi più critici;
- segreto con danno massimo significativo o recovery complesso;
- aumento dell'esposizione pubblica;
- evidenza di abuso o compromissione;
- processo di deployment capace di distribuire segreti separatamente con costo sostenibile.

## Alternative considerate

### Separazione immediata

Valida come stato obiettivo, ma rinviata perché il beneficio corrente non è proporzionato al costo operativo e al rischio valutato.

### Accettazione generale dei segreti versionati

Scartata perché eliminerebbe la valutazione preventiva e trasformerebbe un'eccezione contestuale in una pratica ordinaria.

### Divieto assoluto senza valutazione del contesto

Scartato durante il bootstrap perché non considera impatto, recovery, probabilità e costo della mitigazione.

## Riferimenti

- [Security Architecture](../SecurityArchitecture.md)
- [Secret Risk Register](../../Security/SecretRiskRegister.md)
- [Technical Debt](../../Engineering/TechnicalDebt.md)
