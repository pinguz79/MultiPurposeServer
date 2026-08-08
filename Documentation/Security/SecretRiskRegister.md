# Registro dei rischi dei segreti esposti

> **Stato: Stable 1.0 — autorevole.**

## 1. Scopo

Questo documento registra le categorie di segreti la cui presenza temporanea nella configurazione versionata è accettata secondo l'ADR-0011.

Il registro non contiene valori, hash, percorsi precisi o istruzioni che facilitino la localizzazione e l'utilizzo delle credenziali. Una voce documenta la valutazione di una categoria esistente e non autorizza automaticamente nuove esposizioni dello stesso tipo.

Gli identificatori `SR-XXXX` sono stabili e non vengono riutilizzati.

---

## 2. Metodo

Ogni valutazione considera:

- massimo danno concretamente ottenibile;
- difficoltà, costo e tempo di recovery;
- probabilità concreta di attacco;
- possibilità di revoca o rotazione;
- rischio residuo;
- condizioni di revisione.

Soltanto un rischio residuo basso o molto basso può essere accettato temporaneamente. Ogni nuovo segreto deve essere valutato prima dell'esposizione.

### 2.1 Probabilità di attacco

L'esposizione potenziale di un segreto non implica automaticamente che esso sia conosciuto o utilizzabile.

La probabilità considera l'intera catena necessaria all'abuso:

1. accesso al repository privato, a un backup o a un altro supporto che contiene il valore;
2. individuazione del segreto, anche tramite strumenti automatici;
3. riconoscimento del sistema e del protocollo in cui viene utilizzato;
4. disponibilità dell'ulteriore accesso tecnico eventualmente necessario;
5. motivazione concreta a colpire il servizio;
6. esecuzione dell'abuso.

L'assenza di notorietà non costituisce una misura di sicurezza. Repository privato, segreti casuali, mancata esposizione nel browser e nei log, isolamento di rete e limitata appetibilità del bersaglio riducono però la probabilità concreta che l'intera catena si completi.

---

## 3. Registro corrente

| ID | Tipologia | Danno massimo | Recovery | Probabilità | Rischio residuo |
|---|---|---|---|---|---|
| SR-0001 | FrontEnd API key | Molto basso | Semplice | Molto bassa | Molto basso |
| SR-0002 | BackEnd API key | Basso | Moderato | Bassa | Basso |
| SR-0003 | Cache shared secret | Basso | Semplice | Molto bassa | Molto basso |
| SR-0004 | Credenziali database Portfolio.Api | Elevato | Moderato | Molto bassa | Basso |
| SR-0005 | Credenziali database Portfolio.Web | Molto basso | Semplice | Molto bassa | Molto basso |
| SR-0006 | Signing key JWT sperimentale | Molto basso | Semplice | Molto bassa | Molto basso |

Tutte le esposizioni correnti sono **accettate temporaneamente** alle condizioni descritte nelle rispettive schede.

### SR-0001 — FrontEnd API key

- **Scopo:** autenticare Portfolio.Web nell'accesso alle API FrontEnd di Portfolio.
- **Capacità correnti:** sola consultazione; non modifica lo stato del dominio.
- **Danno massimo:** accesso alle operazioni di lettura previste per Portfolio.Web, senza alterazione di dati o contenuti.
- **Recovery:** rotazione della key e aggiornamento coordinato di server e client Web.
- **Probabilità:** molto bassa; la key è casuale, non viene inviata al browser né registrata nei log e richiede accesso a una fonte che la contiene.
- **Rischio residuo:** molto basso.
- **Mitigazioni correnti:** repository privato, accessi limitati, key server-side, assenza dai log, possibilità di rotazione.
- **Condizione di revisione:** introduzione di scritture, accesso a contenuti sensibili o modifica del modello di esposizione della key.

### SR-0002 — BackEnd API key

- **Scopo:** autenticare il client amministrativo nell'accesso alle API BackEnd di Portfolio.
- **Capacità correnti:** creazione di Album, aggiornamento puntuale o massivo di nomi e descrizioni e invalidazione della cache.
- **Danno massimo:** creazione di Album indesiderati, vandalismo editoriale sui metadati e invalidazioni ripetute; non consente cancellazione di Album o Photo né modifica o rimozione degli originali.
- **Recovery:** rimozione degli Album abusivi, correzione o ripristino dei metadati, rotazione della key e ricostruzione della cache.
- **Probabilità:** bassa; gli endpoint sono pubblicamente raggiungibili e la key è l'unica barriera esecutiva, ma il suo abuso richiede scoperta, interpretazione e motivazione a colpire un servizio con limitata appetibilità.
- **Rischio residuo:** basso.
- **Mitigazioni correnti:** repository privato, key casuale e server-side, assenza dai log, superficie di scrittura reversibile, possibilità di rotazione.
- **Condizione di revisione:** prima di introdurre upload, cancellazioni, gestione utenti, accesso a contenuti protetti o altre operazioni amministrative non facilmente reversibili.

### SR-0003 — Cache shared secret

- **Scopo:** autorizzare l'invalidazione della cache di Portfolio.Web richiesta da MPS.
- **Capacità correnti:** svuotamento della cache applicativa.
- **Danno massimo:** degrado prestazionale o indisponibilità temporanea tramite richieste ripetute di invalidazione; nessuna alterazione persistente dei dati.
- **Recovery:** rotazione del secret, blocco della sorgente dell'abuso e ricostruzione naturale della cache.
- **Probabilità:** molto bassa; richiede scoperta del valore, individuazione dell'endpoint e un attacco mirato privo di beneficio economico concreto.
- **Rischio residuo:** molto basso.
- **Mitigazioni correnti:** repository privato, secret server-side, assenza dai log, cache ricostruibile e possibilità di rotazione.
- **Condizione di revisione:** crescita rilevante del traffico o del costo di ricostruzione, evidenza di abusi oppure maggiore rilevanza pubblica del servizio.

### SR-0004 — Credenziali database Portfolio.Api

- **Scopo:** accesso di Portfolio.Api al proprio database autorevole.
- **Capacità correnti:** lettura e scrittura dei dati persistiti dal dominio Portfolio.
- **Danno massimo:** lettura, alterazione o cancellazione completa dei dati del database; gli originali fotografici sul filesystem rimangono separati.
- **Recovery:** ripristino dal backup giornaliero Aruba, con possibile perdita delle modifiche successive all'ultimo backup; backup mensile come ulteriore salvaguardia.
- **Probabilità:** molto bassa; il database non è raggiungibile direttamente da Internet e la credenziale deve essere combinata con la compromissione dell'host o con una posizione di rete autorizzata.
- **Rischio residuo:** basso, nonostante l'impatto massimo elevato.
- **Mitigazioni correnti:** isolamento di rete, repository privato, backup giornaliero e mensile, accessi limitati e possibilità di rotazione.
- **Condizione di revisione:** apertura dell'accesso remoto, introduzione di dati personali più sensibili, ampliamento degli utenti o dei privilegi, modifica dell'hosting o riduzione delle garanzie di backup.

### SR-0005 — Credenziali database Portfolio.Web

- **Scopo:** accesso di Portfolio.Web al database locale usato per cache e routing.
- **Capacità correnti:** lettura e scrittura di dati applicativi ricostruibili; non contiene dati autorevoli del dominio.
- **Danno massimo:** perdita o alterazione temporanea di cache e routing.
- **Recovery:** svuotamento e rigenerazione del database, oltre alla rotazione delle credenziali.
- **Probabilità:** molto bassa; richiede scoperta delle credenziali e possibilità di utilizzarle nel contesto ammesso dall'hosting.
- **Rischio residuo:** molto basso.
- **Mitigazioni correnti:** repository privato, dati ricostruibili, accessi limitati e possibilità di rotazione.
- **Condizione di revisione:** introduzione di sessioni, profili, dati utente o altre informazioni non ricostruibili.

### SR-0006 — Signing key JWT sperimentale

- **Scopo:** studio di fattibilità dell'autenticazione JWT nella SampleApp.
- **Capacità correnti:** emissione e validazione di token sperimentali, senza capacità produttive di Portfolio.
- **Danno massimo:** falsificazione di token della SampleApp e accesso alle sole funzionalità dimostrative.
- **Recovery:** rotazione della key e invalidazione dei token sperimentali.
- **Probabilità:** molto bassa; richiede scoperta della key e interesse verso una superficie non produttiva.
- **Rischio residuo:** molto basso.
- **Mitigazioni correnti:** repository privato, uso non produttivo, assenza dai log e possibilità di rotazione.
- **Condizione di revisione:** prima di utilizzare JWT per utenti, dati o funzionalità reali.

### 3.1 Valori esclusi

Il Google Client ID presente nella configurazione è un identificatore pubblico e non viene classificato come segreto. Eventuali futuri client secret o credenziali di provider esterni richiederanno una nuova scheda prima di essere versionati.

---

## 4. Aggiornamento

Una voce viene rivalutata quando cambia esposizione, impatto, recovery, probabilità, ambiente, criticità dei dati o processo di deployment.

La chiusura di una voce richiede separazione del valore dalla configurazione versionata e, quando il valore è stato presente nella history, rotazione della credenziale negli ambienti interessati.

## Riferimenti

- [ADR-0011](../Architecture/ADR/ADR-0011-temporary-versioned-secrets-require-low-risk.md)
- [Security Architecture](../Architecture/SecurityArchitecture.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
