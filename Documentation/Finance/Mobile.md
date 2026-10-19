# Finance Mobile - primo blocco

Decisioni del 16 ottobre 2026. Prima piattaforma Android; UI MAUI/XAML,
con avvio iOS predisposto ma non ancora verificato. Distribuzione privata APK.

## Confini

- `Shared/MultiPurposeServer.Client`: trasporto HTTP, errori, configurazione HTTPS.
  Nessuna UI, dipendenza Finance o persistenza; nessun retry automatico.
- `Applications/Finance/Finance.Client`: endpoint e modelli di lettura Finance, senza EF.
  Per ora copre solo la lista conti. Non referenzia Finance.Contracts perché quel
  progetto dipende dal modello dati server. La futura migrazione del desktop va
  accompagnata da test di compatibilità, senza modificare il contratto JSON.
  La factory riceve il trasporto tramite DI e applica indirizzo e chiave per richiesta;
  non modifica gli header predefiniti dell'HttpClient condiviso.
- `Applications/Finance/Finance.Mobile`: configurazione e lista conti in XAML.
  Componenti grafici condivisi con ModelBook saranno estratti quando effettivamente
  riutilizzabili; non viene creato un framework grafico vuoto.

Desktop, server e SampleApp.Mobile rimangono invariati. Il progetto mobile è escluso
dalla build generale della soluzione, come SampleApp, per non imporre i workload
mobile a server e pipeline di deploy. Compilarlo esplicitamente.

## Connessione e dati

Primo avvio: Connessione, URL HTTPS di base di MPS (incluso l'eventuale percorso),
chiave API Finance già usata dal desktop. Nessuna credenziale è incorporata nell'APK.
L'indirizzo è nelle preferenze; la chiave in SecureStorage; backup Android disabilitato.
Redirect HTTP disabilitati per non inoltrare la chiave verso altre destinazioni.
L'autenticazione utente resta un'attività trasversale distinta dalla API key esistente.

Home: conti, saldo/speso/debito, valore mese/ciclo precedente, plafond e prossimo
addebito quando disponibili. Aggiornamento all'apertura o con il pulsante Aggiorna.
Errore, lista vuota e caricamento hanno stati distinti. Nessuna copia offline dei
movimenti; in caso di errore non si mantengono saldi vecchi come se fossero aggiornati.

Questo blocco non permette ancora inserimento, modifica o conferma di movimenti.
Per le future scritture: nessun retry o coda offline; esito incerto -> avviso e ritorno
alla home, verifica mediante nuova lettura. Errori certi di validazione -> correzione
nel form. La navigazione appartiene all'app, non alla libreria HTTP.

## Verifica

Test del client con server simulato: percorso base, API key, deserializzazione conti,
errori HTTP/non JSON, risposta nulla, interruzione, cancellazione, assenza di retry.

Comandi:

```powershell
dotnet test Tests/Finance/Finance.Client.Tests/Finance.Client.Tests.csproj
dotnet build Applications/Finance/Finance.Mobile/Finance.Mobile.csproj -f net10.0-android
```

Prima dell'uso reale: prova su telefono/emulatore di configurazione, chiave errata,
assenza di rete, rotazione, sospensione/ripresa e confronto dei saldi col desktop.
La compilazione non sostituisce questi controlli. Nessun test scrive in produzione.
APK Release firmato e chiave di firma stabile saranno predisposti in fase di
distribuzione; non distribuire i pacchetti Debug come release.

Verifica locale del 16 ottobre: 13 test client superati; build Android Debug riuscito.
Gli strumenti Android e testhost hanno richiesto esecuzione fuori dal sandbox per
problemi di comunicazione tra processi. Prova visuale e funzionale su dispositivo
ancora da eseguire; iOS non compilato. Desktop e produzione non toccati.

## Revisione delle convenzioni del primo blocco

Perimetro: intero contenuto dei quattro progetti nuovi, riferimenti di soluzione,
configurazione e documentazione mobile; non soltanto FinanceApiClient.
Fonti applicate: CodingConventions, TestingConventions, MpsPlaybook e .editorconfig.

Difformità corrette:

- Finance.Client spostato da Domains/Finance ad Applications/Finance;
- namespace block-scoped, allineati alle cartelle anche per gli avvii Android/iOS;
- un tipo per file, senza tipi annidati; separati DTO, contesto JSON, eccezione,
  applicazione, presentazione delle card e handler dei test;
- graffe, spaziatura, ordine dei membri, nomi dei campi privati e costanti;
- commenti italiani e documentazione dei contratti pubblici delle librerie;
- rimozione dei suffissi Async non richiesti da una variante sincrona o dal framework;
- dipendenze HTTP, preferenze e memoria protetta ricevute tramite DI;
- test con naming Metodo_WhenCondition_Risultato, sezioni AAA e assertion fluenti,
  senza assertion dentro il finto trasporto e senza null-forgiving;
- normalizzazione UTF-8 senza BOM, CRLF e newline finale dei file del blocco.

Non vengono modificati endpoint, payload, retry, navigazione o regole dei saldi.
Il nome dei metodi pubblici delle nuove librerie cambia, insieme a tutti i chiamanti;
non cambia il protocollo HTTP. Le librerie non erano ancora distribuite.
FluentAssertions usa la versione 8.10.0 già adottata nella suite: resta valido il
punto aperto sulla licenza descritto nelle Testing Conventions; non è una dipendenza
runtime dell'app o del server.

I controlli deterministici vengono applicati ai soli progetti del blocco, con lo
script strutturale del repository e gli stessi formatter/diagnostiche del suo wrapper.
Questo non certifica l'intera baseline del repository. La scelta di classe/record,
l'uso di DI, i test AAA e i commenti richiedono inoltre revisione manuale.

Esito della verifica del 19 ottobre: 14 test superati; verifica strutturale e
formatter whitespace/style senza scostamenti sui quattro progetti; controllo
encoding/CRLF/newline finale superato. Build Android Debug con controlli di stile
abilitati: nessun avviso né errore. Prova su dispositivo e build iOS restano escluse
da questa verifica. Nessun nuovo artefatto server da distribuire.
