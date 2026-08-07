# ADR-0001 — I domini sono autonomi e ricomponibili

## Stato

Accettato

## Ambito

Piattaforma

## Data della decisione

2026-08-06

## Origine

- `ADR-ALPHA-0002`
- consolidamento di `Architecture.md` e `DomainArchitecture.md`

## Contesto

MultiPurposeServer ospita più aree funzionali per ridurre costi infrastrutturali e complessità operativa. La condivisione dello stesso host rischia però di trasformarsi in condivisione accidentale di logica, dati, configurazione e ciclo evolutivo.

Era necessario distinguere la scelta di deployment dalla struttura applicativa e impedire che l'host diventasse il proprietario delle responsabilità dei domini.

## Decisione

MPS compone domini applicativi autonomi all'interno di un host condiviso.

Ogni dominio possiede protocollo pubblico, logica applicativa, dati, configurazione, sicurezza, dipendenze e ciclo evolutivo. Espone all'host punti pubblici di composizione e mantiene nascosti i propri dettagli interni.

I domini non condividono Entity persistite, database o schemi logici, foreign key, transazioni, account o implementazioni applicative. Possono condividere server, provider e servizi tecnici senza condividere ownership.

L'autonomia viene verificata tramite estraibilità per ricomposizione: un dominio deve poter essere ospitato in un nuovo host trasferendo i suoi moduli, la configurazione, il datastore e le dipendenze Shared necessarie, senza modificare la propria logica applicativa.

Un dominio che consuma eccezionalmente l'API pubblica di un altro lo tratta come un servizio esterno e non sfrutta privilegi derivanti dalla co-ubicazione.

## Conseguenze

### Positive

- L'host rimane un composition root privo di business logic.
- Ogni dominio può evolvere dati e funzionalità indipendentemente.
- La condivisione infrastrutturale non crea accoppiamento applicativo.
- È possibile ricomporre un dominio in una solution e in un host dedicati.

### Negative

- Configurazioni e meccanismi simili possono essere duplicati.
- Non sono disponibili transazioni applicative implicite tra domini.
- L'integrazione tra domini richiede gli stessi contratti e le stesse cautele di un servizio esterno.

## Alternative considerate

### Host proprietario delle dipendenze dei domini

Scartato perché renderebbe la composizione dipendente dai dettagli interni di ogni modulo.

### Modello dati applicativo condiviso

Scartato perché introdurrebbe ownership ambigua e impedirebbe l'evoluzione indipendente.

### Deployment separato immediato

Non richiesto: l'estraibilità è un test dei confini e non impone microservizi o deployment autonomi.

## Riferimenti

- [Architecture](../Architecture.md)
- [Domain Architecture](../DomainArchitecture.md)

