# Deploy mirato di Portfolio.Web su Altervista

Il deploy Altervista trasferisce soltanto i file elencati in un piano revisionato. Non sincronizza l'intero progetto e non distribuisce artefatti di sviluppo, script database o log runtime.

## Preparazione

Configurare l'environment GitHub `Altervista` con i secret:

- `ALTERVISTA_FTP_SERVER`;
- `ALTERVISTA_FTP_USERNAME`;
- `ALTERVISTA_FTP_PASSWORD`.

Configurare inoltre la variabile di environment non segreta:

- `ALTERVISTA_FTP_CERTIFICATE_SHA256`.

Il collegamento usa FTPS esplicito sulla porta 21 e l'host ufficiale `ftp.nomeutente.altervista.org`. Poiché il certificato Altervista è emesso per `altervista.org` e `*.altervista.org`, lo script ammette la sola mancata corrispondenza del nome quando la fingerprint SHA-256 coincide esattamente con quella revisionata nell'environment. Qualsiasi altro errore TLS o fingerprint differente interrompe il collegamento. Credenziali e fingerprint corrente non devono essere inserite nei piani.

## Piano di deploy

Ogni piano contiene:

- `deployable`: deve essere `true` per autorizzare operazioni remote; i template mantengono `false`;
- `uploadFiles`: file singoli, relativi alla root di `Applications/Portfolio/Portfolio.Web`, da caricare mantenendo lo stesso percorso remoto;
- `deleteFiles`: soli file remoti da eliminare esplicitamente;
- `smokeUrls`: URL pubblici coperti dai test di produzione eseguiti dopo il deploy.

Il workflow rifiuta percorsi assoluti, attraversamenti `..`, artefatti riservati allo sviluppo e contenuti runtime dentro `portfolio/internal/logs`. La sola infrastruttura versionata ammessa in quella cartella è `.htaccess`.

## Esecuzione

Da GitHub Actions selezionare `Deploy Portfolio.Web to Altervista`, indicare il percorso del piano revisionato e scegliere:

- `test_connection = true` per verificare credenziali e accesso FTPS tramite il comando di sola lettura `PWD`, senza aprire il canale dati;
- `test_transfer = true` per caricare, riscaricare, confrontare ed eliminare un file sentinella temporaneo nella root FTP;
- `execute = false` per validare sintassi PHP e piano senza operazioni remote;
- `execute = true` per trasferire esclusivamente i file revisionati ed eseguire i test di produzione in sola lettura.

Il test di connessione non crea, modifica o elimina file remoti. Il test di trasferimento usa esclusivamente `.codex-altervista-ftps-transfer-test.txt` e lo elimina nel blocco di pulizia dopo averne verificato il contenuto. Il deploy applica retry a ogni operazione, ma Altervista non offre un equivalente automatico di `app_offline.htm`: in caso di errore dopo un trasferimento parziale, il workflow segnala quante operazioni sono state completate e richiede una verifica esplicita prima del retry.
