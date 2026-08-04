# ADR-0011 — Temporary inline secrets during bootstrap

## Stato

Accepted

## Contesto

MultiPurposeServer e Portfolio.Web sono ancora in una fase di sviluppo iniziale e vengono pubblicati frequentemente.

Il deployment corrente è intenzionalmente semplice:

- Portfolio.Web viene distribuito copiando via FTP l'intera cartella applicativa su Altervista;
- MultiPurposeServer viene pubblicato da Visual Studio su una cartella e successivamente copiato su Aruba.

La presenza delle configurazioni complete direttamente nei file distribuiti consente di effettuare il deploy senza modificare manualmente i file sui server dopo ogni pubblicazione.

La separazione immediata dei segreti richiederebbe invece operazioni aggiuntive a ogni deploy, aumentando il rischio di errori e rallentando l'evoluzione del progetto.

Sono attualmente presenti nei file di configurazione valori sensibili quali:

- API key FrontEnd e BackEnd;
- shared secret tra MultiPurposeServer e Portfolio.Web;
- credenziali del database MySQL;
- chiavi JWT;
- credenziali o configurazioni di provider esterni.

## Decisione

Durante la fase di bootstrap e stabilizzazione dell'architettura è temporaneamente accettato mantenere alcuni segreti direttamente nei file di configurazione utilizzati per il deployment.

La decisione privilegia:

- rapidità di pubblicazione;
- ripetibilità del deploy;
- riduzione delle modifiche manuali sugli host;
- facilità di disaster recovery durante una fase ancora instabile.

Questa configurazione non rappresenta lo stato di sicurezza definitivo del sistema.

## Vincoli

Durante questa fase:

- il repository deve rimanere privato;
- l'accesso al repository deve essere limitato;
- i log non devono contenere segreti, password o token;
- i segreti non devono essere restituiti nelle risposte HTTP;
- le chiavi devono poter essere sostituite senza modificare il codice;
- eventuali credenziali compromesse devono essere ruotate;
- non devono essere aggiunti nuovi segreti senza registrarli nella documentazione di sicurezza.

## Rischi accettati

La decisione comporta temporaneamente i seguenti rischi:

- esposizione dei segreti attraverso la cronologia Git;
- propagazione dei segreti nei clone e nei backup del repository;
- maggiore impatto in caso di accesso non autorizzato al repository;
- necessità di ruotare tutte le credenziali al termine della fase di bootstrap;
- difficoltà nel distinguere configurazioni di sviluppo e produzione.

Tali rischi sono accettati esclusivamente per la fase iniziale del progetto.

## Condizioni di revisione

La decisione deve essere riesaminata quando si verifica almeno una delle seguenti condizioni:

- stabilizzazione del processo di deployment;
- introduzione di ulteriori sviluppatori;
- apertura o condivisione del repository;
- aumento del numero di segreti;
- introduzione di dati o servizi con maggiore criticità;
- rilascio pubblico stabile;
- disponibilità di un meccanismo automatizzato di distribuzione della configurazione;
- audit o requisito esterno di sicurezza.

## Strategia futura

L'hardening dovrà definire esattamente come separare codice, configurazione pubblica e segreti.

La soluzione futura dovrà valutare:

- file di configurazione pubblici con valori vuoti o placeholder;
- file locali esclusi da Git;
- variabili ambiente;
- secret store compatibili con Aruba e Altervista;
- trasformazioni automatiche durante il deployment;
- gestione distinta degli ambienti Development e Production;
- archivio cifrato per disaster recovery;
- backup cifrati del database;
- rotazione delle chiavi già esposte nella cronologia Git;
- procedura documentata di ripristino completo;
- verifica automatica dell'assenza di segreti nei commit.

## Piano di migrazione futuro

Quando verrà avviato l'hardening:

1. censire tutti i segreti presenti nella solution e negli host;
2. classificare ciascun segreto per sistema, ambiente e possibilità di rotazione;
3. definire il meccanismo di distribuzione sicura per Aruba e Altervista;
4. creare configurazioni di esempio versionate;
5. escludere da Git i file contenenti valori reali;
6. predisporre un archivio cifrato di disaster recovery;
7. ruotare password, API key, shared secret e chiavi JWT già versionate;
8. aggiornare gli host con i nuovi valori;
9. verificare il deploy completo senza modifiche manuali non documentate;
10. controllare che nessun segreto rimanga nella working tree o nei nuovi commit.

## Conseguenze positive

- deploy immediato durante la fase di sviluppo;
- minore rischio di dimenticare configurazioni necessarie;
- ripristino rapido dell'ambiente corrente;
- nessuna infrastruttura aggiuntiva richiesta nell'immediato.

## Conseguenze negative

- segreti presenti nella cronologia del repository;
- necessità futura di rotazione completa;
- livello di sicurezza inferiore allo stato obiettivo;
- maggiore attenzione richiesta nella gestione degli accessi al repository.

## Alternative considerate

### Separazione immediata dei segreti

Scartata temporaneamente perché richiederebbe modifiche manuali frequenti sugli host o l'introduzione anticipata di un processo di deployment più complesso.

### Conservazione esclusiva sugli host

Scartata perché renderebbe più fragile il disaster recovery e aumenterebbe il rischio di perdita dei valori necessari al ripristino.

### Archivio cifrato esterno già nella fase corrente

Considerato valido come misura futura o complementare, ma non sufficiente da solo a mantenere l'attuale semplicità di deployment.

## Relazioni

Questo ADR integra:

- ADR-0004 — Client authentication is distinct from user authentication;
- SecurityArchitecture.md;
- InfrastructureArchitecture.md;
- ArchitectureRoadmap.md.

## Nota per le code review

Finché questo ADR rimane `Accepted`, la presenza temporanea di segreti nei file di configurazione non deve essere classificata come violazione non intenzionale.

Deve però essere sempre segnalata come rischio accettato e deve essere verificato che:

- non vengano esposti nei log;
- non vengano restituiti nelle risposte;
- il repository rimanga privato;
- la futura migrazione rimanga pianificata.