# Deploy mirato di MPS su Aruba

Il deploy Aruba non sincronizza l'intera root FTP. Ogni rilascio usa un piano versionato che elenca esclusivamente file da caricare, directory generate da trasferire integralmente e file applicativi da eliminare.

## Preparazione

Configurare l'environment GitHub `Aruba` con i secret:

- `ARUBA_FTP_SERVER`;
- `ARUBA_FTP_USERNAME`;
- `ARUBA_FTP_PASSWORD`.

Il collegamento usa FTPS esplicito sulla porta 21. Le credenziali non devono essere inserite nei piani, nei workflow o nella documentazione.

## Piano di deploy

Ogni piano contiene:

- `deployable`: deve essere `true` per autorizzare operazioni remote; i template mantengono `false`;
- `uploadFiles`: file singoli provenienti dalla root del publish;
- `uploadDirectories`: sottocartelle del publish da espandere e trasferire;
- `deleteFiles`: soli file remoti da eliminare esplicitamente;
- `smokeUrls`: URL pubblici da verificare dopo il deploy.

I percorsi sono relativi alla root del publish e alla root FTP. Lo script rifiuta percorsi assoluti, attraversamenti `..` e operazioni dentro `mdb-database`, `logs` e `Portfolio`.

## Esecuzione

Da GitHub Actions selezionare `Deploy MPS to Aruba`, indicare il percorso del piano revisionato e scegliere:

- `test_connection = true` per verificare credenziali e accesso FTPS tramite la sola lettura della root remota;
- `test_transfer = true` per caricare una piccola sentinella temporanea, riscaricarla, verificarne il contenuto e cancellarla;
- `execute = false` per compilare, testare, pubblicare e validare soltanto il piano;
- `execute = true` per eseguire anche il trasferimento FTPS e gli smoke test.

Il test di connessione non crea, modifica o elimina file remoti. Il test di trasferimento usa `curl` sul runner e opera esclusivamente sul file riservato `codex-aruba-ftps-transfer-test.txt`, del quale verifica il contenuto prima di tentarne sempre la cancellazione. Entrambi possono essere eseguiti con il piano di esempio proposto dal workflow.

Durante il trasferimento viene caricato temporaneamente `app_offline.htm` e ogni operazione usa retry. Se il deploy fallisce prima di modificare artefatti remoti, il sito viene riattivato automaticamente. Se fallisce dopo una modifica parziale, `app_offline.htm` viene conservato intenzionalmente per non esporre una release incoerente e il ripristino richiede un intervento esplicito.
