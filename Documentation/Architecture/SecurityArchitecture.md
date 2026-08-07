# Architettura della Sicurezza

## 1. Scopo del documento

Questo documento definisce i principi, i confini e le responsabilità che governano la sicurezza di MultiPurposeServer.

La sicurezza è una proprietà trasversale dell'intero sistema. Ogni dominio possiede la semantica e i dati di sicurezza del proprio contesto; lo Shared Framework fornisce meccanismi tecnici comuni; l'host compone e configura la pipeline; le Applications applicano i protocolli previsti senza diventare fonte autorevole delle decisioni.

Le tecnologie, le convenzioni implementative e le procedure operative appartengono ai documenti specialistici di livello inferiore. Le decisioni architetturali significative sono motivate dagli Architecture Decision Record.

---

## 2. Principi

### 2.1 Backend autorevole

Autenticazione e autorizzazione vengono applicate dal backend. Un client può adattare l'interfaccia alle capacità disponibili, ma l'assenza di una funzione nel frontend non costituisce un controllo di sicurezza.

### 2.2 Defense in Depth

Trasporto sicuro, autenticazione, autorizzazione, validazione, protezione dei dati, audit ed error handling riducono rischi differenti. Nessun singolo meccanismo è sufficiente da solo.

### 2.3 Least Privilege

Utenti, client, servizi, database, filesystem e provider esterni operano con i privilegi minimi necessari. Token, sessioni e credenziali devono essere limitati per ambito, destinatario e durata.

### 2.4 Default Deny

Una richiesta priva di una classificazione di accesso valida viene negata. L'accesso anonimo deve essere intenzionale ed esplicito.

In caso di configurazione mancante, stato ambiguo o fallimento non gestito della valutazione, il sistema deve fallire in modalità chiusa.

### 2.5 Trust Boundaries

Ogni attraversamento di un confine di fiducia richiede una verifica coerente con il rischio. La co-ubicazione nello stesso host, processo, repository o base URL non attribuisce fiducia fra domini o Applications.

---

## 3. Ownership della sicurezza

### 3.1 Shared Security Framework

Lo Shared Framework può fornire meccanismi tecnici riutilizzabili, come:

- integrazione con gli schemi di autenticazione;
- validazione tecnica di token e credenziali;
- astrazioni per policy e permission evaluation;
- costruzione del contesto di sicurezza;
- protezioni comuni e supporto al security audit.

Non possiede account, ruoli, permessi o regole applicative dei domini.

### 3.2 Domini

Ogni dominio possiede autonomamente:

- account e relativo lifecycle;
- collegamenti con identità esterne;
- client registrati e capacità applicative;
- ruoli e permessi;
- regole di autorizzazione sulle risorse;
- relazioni di accesso persistite;
- configurazione e persistenza della sicurezza.

Uno stesso essere umano registrato in più domini possiede account distinti e non correlati automaticamente. Ruoli, permessi, sessioni e autenticazioni non vengono propagati fra domini.

### 3.3 Host e Applications

L'host compone i meccanismi senza centralizzare identità o autorizzazioni. Le Applications utilizzano i protocolli pubblici del dominio e non deducono privilegi dalla propria implementazione.

---

## 4. Modello delle identità

### 4.1 Client e utente

Il contesto del client e l'identità dell'utente rappresentano dimensioni logicamente indipendenti:

```text
Security Context
    Client Context
        identificazione o autenticazione
        capacità riconosciute

    User Context
        assente oppure autenticato
        account del dominio
        permessi
```

Le due dimensioni non richiedono necessariamente due credenziali fisiche. Possono essere trasportate da credenziali separate, da un unico access token, da una sessione server-side o da altri meccanismi coerenti con il tipo di client.

Un client può operare senza utente quando la policy ammette operazioni anonymous o machine-to-machine. Un utente non acquisisce privilegi applicativi che il client non possiede.

### 4.2 Confidential e public client

Un confidential client può custodire credenziali e può quindi essere autenticato mediante un meccanismo adeguato. Un public client, come codice eseguito nel browser o un'applicazione Mobile o Desktop distribuita agli utenti, non può mantenere riservato un segreto statico incorporato.

Un identificatore o una chiave inclusa in un public client può fornire contesto, ma non deve essere considerata prova forte dell'identità del software. Le policy non attribuiscono garanzie superiori a quelle offerte dal meccanismo concretamente adottato.

L'eventuale protezione forte di API amministrative accessibili da public client richiederà una decisione specifica, per esempio tramite backend confidential, registrazione gestita dell'istanza, attestazione o altro meccanismo adeguato.

### 4.3 Identità esterne

Un Identity Provider fornisce una prova di identità e non un account globale MPS. Ogni dominio collega autonomamente l'identità esterna a un proprio account e decide registrazione, invito, approvazione e revoca.

L'identità tecnica esterna deve usare un identificatore stabile e scoped al provider, non un indirizzo email preso isolatamente.

### 4.4 Account applicativi

Un'identità non umana usata da un'integrazione protetta viene rappresentata come service account o application principal appartenente al dominio che espone il servizio. Non impersona automaticamente un utente umano.

---

## 5. Autorizzazione

### 5.1 Modello compositivo

La decisione finale deriva dall'intersezione di tre dimensioni:

```text
Capacità del client
        ∩
Permessi dell'utente
        ∩
Regole contestuali sulla risorsa
        =
Operazione autorizzata
```

I permessi rappresentano capacità atomiche. I ruoli sono aggregatori opzionali di permessi e non introducono logica di business.

Una regola che dipende dall'identità o dalla relazione con la risorsa appartiene all'autorizzazione. Una regola applicabile indipendentemente dal chiamante rimane business logic.

### 5.2 Classificazione degli endpoint

Ogni endpoint possiede una classificazione esplicita, composta quando necessario da:

- Anonymous;
- Client Context Required;
- Authenticated User Required;
- Permission Required;
- Resource Authorization Required.

Policy mancanti o inesistenti devono emergere all'avvio o mediante Contract Configuration Test. OpenAPI descrive la configurazione effettiva, comprese le eccezioni anonime.

### 5.3 Distribuzione delle responsabilità

La pipeline valuta autenticazione, capacità del client e permessi indipendenti dalla singola risorsa. Le policy contestuali vengono implementate dal dominio e orchestrate nel caso d'uso quando il contesto della risorsa è disponibile.

Il Service non legge claim o dettagli HTTP. Il Repository persiste e recupera dati senza prendere decisioni sull'identità. Verifica contestuale ed effetto devono appartenere alla stessa operazione atomica o la condizione deve essere rivalidata prima della persistenza.

Ogni entry point che invoca un'operazione protetta deve stabilire il contesto autorizzativo richiesto; la protezione di un Controller non rende intrinsecamente sicuro un Service invocabile altrove.

### 5.4 Access scope

Liste e ricerche applicano la visibilità prima di conteggio, ordinamento e paginazione. Il dominio deriva un access scope dal contesto di sicurezza e dai fatti autorizzativi persistiti; il Repository lo traduce in una query senza interpretarne la semantica.

Conteggi, pagine e ordinamenti non devono rivelare indirettamente risorse escluse.

Account, ownership, condivisioni, membership e altre relazioni di accesso possono essere persistiti nel database del dominio. Il Data Layer conserva i fatti; il dominio attribuisce loro significato.

### 5.5 Risposte HTTP

- `401 Unauthorized` indica un'identità richiesta assente o non valida.
- `403 Forbidden` indica identità valide ma capacità, permessi o policy insufficienti.
- `404 Not Found` può sostituire intenzionalmente `403` quando confermare l'esistenza della risorsa produrrebbe una divulgazione indesiderata.

Il mascheramento con `404` è una scelta esplicita del dominio, non una regola globale.

### 5.6 Bulk operations

L'autorizzazione globale di client, utente ed endpoint precede l'elaborazione degli item. Un fallimento globale produce `401` o `403` e impedisce qualsiasi elaborazione.

L'autorizzazione contestuale può invece fallire per singolo item. Questo esito costituisce una categoria di access control distinta da validation e database violation e partecipa alla strategia bulk selezionata.

La response può usare un esito neutro come `ResourceNotAccessible` per non distinguere una risorsa inesistente da una risorsa esistente ma vietata. La correlazione utilizza soltanto la chiave già fornita dal chiamante e non aggiunge informazioni sulla risorsa negata.

---

## 6. Lifecycle di identità e credenziali

Account, client, credenziali, sessioni e collegamenti con provider esterni possiedono lifecycle espliciti.

Ogni modifica di sicurezza deve diventare effettiva entro un intervallo massimo definito e proporzionato al rischio. Operazioni amministrative o risposta a una compromissione possono richiedere revoca immediata.

Token e sessioni devono avere scadenza, destinatario e privilegi limitati. Devono poter essere revocati o invalidati e non possono essere perpetui per sola comodità. Refresh e rinnovo rimangono distinti dall'access token.

Le modifiche rilevanti a account, ruoli, permessi, fattori e credenziali producono security audit e invalidano le cache autorizzative pertinenti.

### 6.1 User authentication e recovery

Eventuali password locali non vengono conservate in forma leggibile o reversibile. Token di registrazione, verifica e recupero sono temporanei, monouso e trattati come segreti.

Il recupero account non deve essere più debole dell'autenticazione ordinaria. Una compromissione o un recupero può richiedere la revoca delle sessioni esistenti.

L'enumerabilità di account e profili è una decisione del dominio basata sulla classificazione dei dati. I meccanismi di autenticazione non divulgano attributi o stati ulteriori rispetto alla superficie pubblica autorizzata.

L'architettura deve poter supportare MFA o step-up authentication per operazioni ad alto rischio, senza renderli obbligatori oggi per ogni dominio.

---

## 7. Classificazione e protezione dei dati

### 7.1 Classi

MPS utilizza quattro categorie generali:

- **Pubblico:** divulgazione intenzionale; accesso anonimo possibile.
- **Interno:** non destinato alla pubblicazione, con impatto limitato.
- **Protetto:** dato personale o applicativo accessibile soltanto a identità autorizzate.
- **Segreto:** credenziale o materiale che concede accesso e richiede protezione, rotazione e divieto di logging.

Lo Shared Framework può fornire meccanismi comuni; ogni dominio classifica concretamente dati e risorse.

### 7.2 Rappresentazioni e media

La classificazione appartiene alla singola rappresentazione. La derivazione tecnica non determina da sola il livello di protezione e ogni declassificazione deve essere deliberata e limitata alle informazioni necessarie.

Una fotografia può quindi avere originale e full-size protetti, ma preview, cover o thumbnail pubbliche o accessibili a un pubblico più ampio. Il dominio decide risoluzione, watermark, metadati e visibilità di ciascuna variante.

La modalità di trasporto non determina la protezione. Un media protetto non diventa anonimo per una limitazione del tag HTML `<img>`; deve usare una prova di autorizzazione compatibile con il canale, come sessione, proxy o capability temporanea. Il meccanismo concreto rimane da progettare.

### 7.3 Ciclo di vita del dato

La classificazione accompagna il dato in persistenza, cache, log, esportazioni, file temporanei e backup. Il dominio stabilisce finalità, visibilità, retention e cancellazione; l'infrastruttura applica le protezioni tecniche.

Devono essere raccolti e conservati soltanto i dati necessari. Copie derivate, cache e file temporanei possiedono lifecycle esplicito. I backup possono applicare una scadenza documentata anziché una cancellazione immediata.

---

## 8. Sicurezza delle API

HTTPS è obbligatorio per tutte le comunicazioni non locali o non appartenenti a un ambiente di sviluppo esplicitamente controllato.

CORS limita il comportamento degli script nel browser, ma non autentica il chiamante e non impedisce richieste dirette. Le policy CORS devono essere esplicite e minimali; autenticazione e autorizzazione vengono applicate indipendentemente.

Quando vengono usate credenziali ambientali, come cookie inviati automaticamente, deve essere prevista una protezione CSRF.

Normalizzazione e validazione proteggono il confine applicativo, ma non sostituiscono autorizzazione o business logic.

Il rate limiting protegge disponibilità e abuso senza attribuire fiducia. Può considerare IP, client, account, endpoint e costo dell'operazione.

Errori e documentazione API non devono divulgare stack trace, filesystem, database, configurazioni, credenziali o altri dettagli interni.

---

## 9. Gestione dei segreti

I segreti devono essere separati dal codice e dalla configurazione pubblica versionata e devono poter essere ruotati senza modificare il codice applicativo.

### 9.1 Eccezione temporanea valutata

Durante il bootstrap può essere accettata temporaneamente l'esposizione o il versionamento di un segreto soltanto dopo una valutazione esplicita il cui rischio residuo risulti basso o molto basso.

La valutazione precede l'esposizione e considera almeno:

- massimo danno ottenibile;
- difficoltà, costo e tempo di recovery;
- probabilità concreta di attacco;
- possibilità di revoca o rotazione;
- condizioni che richiedono una rivalutazione.

L'accettazione dei segreti attuali non costituisce precedente automatico per valori futuri. Un rischio medio o superiore non può beneficiare dell'eccezione.

Le categorie valutate vengono registrate senza valori sensibili nel `Documentation/Security/SecretRiskRegister.md`. L'ADR-0011 documenta contesto e motivazione della deviazione temporanea.

### 9.2 Logging ed errori

Password, token, API key, URL firmati completi e altri segreti non vengono registrati nei log né restituiti nelle risposte. I dettagli diagnostici rimangono accessibili soltanto attraverso canali protetti.

---

## 10. Security audit

Il security audit è distinto dal logging operativo. Serve a ricostruire azioni rilevanti per la sicurezza e può includere dominio, client, account, operazione, identificatore non sensibile della risorsa, esito, motivazione classificata, timestamp e correlation ID.

Devono essere valutati per l'audit almeno:

- modifiche a ruoli, permessi e configurazioni;
- revoche e rotazioni di credenziali;
- operazioni amministrative;
- accessi o dinieghi rilevanti rispetto al rischio.

Il dominio dichiara gli eventi applicativi; lo Shared Framework può fornire formato e meccanismo; l'infrastruttura governa destinazione, accesso, retention e protezione da alterazioni.

Ogni categoria di operazione stabilisce se l'indisponibilità dell'audit debba impedire l'operazione o produrre un allarme con prosecuzione controllata.

---

## 11. Threat model e gestione del rischio

Ogni dominio e Application possiede un threat model proporzionato alle proprie superfici e ai dati trattati. Il modello descrive almeno asset, attori, entry point, trust boundary, scenari di abuso, contromisure, rischio residuo, assunzioni e condizioni di revisione.

Il threat model viene rivalutato quando cambiano dati, classificazione, autenticazione, provider, superfici pubbliche, privilegi, infrastruttura o deployment.

Gli esiti vengono gestiti in base alla loro natura:

- mitigazioni consolidate nella specifica pertinente;
- rischi accettati in ADR o registri dedicati;
- interventi concreti in Technical Debt o Backlog;
- vulnerabilità attive e urgenti mediante escalation immediata.

Non viene imposto un metodo specifico finché non emerge una necessità concreta.

---

## 12. Verifica della sicurezza

I test funzionali verificano gli use case autorizzati. I test dei componenti che possiedono una regola di autorizzazione ne coprono sia gli esiti positivi sia quelli negativi.

Gli **Authorization Boundary Test** costituiscono una finalità trasversale distinta dai test funzionali e verificano dall'esterno che funzionalità e risorse non siano accessibili fuori dalle policy previste.

Devono comprendere scenari rappresentativi di:

- credenziali assenti, invalide, scadute o revocate;
- client privo della capability;
- utente privo del permesso;
- accesso a risorse di altri account;
- escalation orizzontale e verticale;
- endpoint e varianti media con classificazioni differenti;
- revoca e propagazione delle modifiche;
- assenza di dati sensibili in errori e log.

Il livello tecnico di esecuzione rimane Unit, Framework, Contract Configuration, Integration o End-to-End. Un Authorization Boundary Test descrive la finalità della verifica e non un ulteriore gradino della piramide.

---

## 13. Relazioni fra domini

Una chiamata da un dominio X a un dominio Y viene trattata come consumo di un servizio esterno. Y non conosce né riceve l'identità utente di X.

X usa un endpoint anonimo di Y oppure, se necessario, un service account o application principal appartenente a Y. Non vengono introdotti token exchange, delega o impersonation finché non emerge un caso concreto.

---

## 14. Riferimenti

- [Architecture](Architecture.md)
- [Domain Architecture](DomainArchitecture.md)
- [Infrastructure Architecture](InfrastructureArchitecture.md)
- [Testing Architecture](TestingArchitecture.md)
- [Shared Framework](SharedFramework.md)
- [ADR-0010 — Client e utente sono identità distinte](ADR/ADR-0010-client-and-user-identities-are-distinct.md)
- [ADR-0011 — Esposizione temporanea dei segreti](ADR/ADR-0011-temporary-versioned-secrets-require-low-risk.md)
- [Secret Risk Register](../Security/SecretRiskRegister.md)
- [OAuth 2.0 — RFC 6749](https://www.rfc-editor.org/rfc/rfc6749)
- [OAuth 2.0 for Native Apps — RFC 8252](https://www.rfc-editor.org/rfc/rfc8252)
- [OAuth 2.0 Security Best Current Practice — RFC 9700](https://www.rfc-editor.org/rfc/rfc9700)
