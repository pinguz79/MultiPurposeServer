# Architettura del Testing

## 1. Scopo del documento

Questo documento descrive l'architettura della suite di test di MultiPurposeServer.

L'obiettivo non è definire come scrivere un singolo test, ma stabilire come organizzare, strutturare ed evolvere l'intero sistema di testing affinché rifletta l'architettura del progetto.

La suite di test rappresenta un'estensione dell'architettura.

Ogni livello verifica responsabilità differenti e contribuisce a mantenere stabile l'evoluzione del sistema.

Le convenzioni di scrittura dei test appartengono al `Documentation/Engineering/MpsPlaybook.md`.

---

## 2. Filosofia del testing

La suite di test deve riflettere la struttura dell'applicazione.

Ogni componente viene verificato al livello più appropriato, evitando sia duplicazioni inutili sia lacune di copertura.

L'obiettivo non consiste nell'aumentare artificialmente la percentuale di code coverage, ma nel garantire che ogni responsabilità significativa sia verificata almeno una volta nel punto corretto dell'architettura.

### 2.1 Piramide del testing

MultiPurposeServer adotta una strategia ispirata alla Test Pyramid.

```text
                End-to-End
             Integration Test
          Contract / Framework Test
               Unit Test
```

La maggior parte della suite dovrebbe essere costituita da Unit Test.

I livelli superiori verificano l'integrazione tra componenti e non devono sostituire i test unitari.

### 2.2 Una responsabilità, un livello

Ogni comportamento dovrebbe essere verificato nel livello che ne possiede la responsabilità.

Ad esempio:

- il comportamento di un Repository viene verificato dai Repository Test;
- la Request Pipeline viene verificata dai Framework Test;
- il Controller verifica la traduzione HTTP;
- il Service verifica la logica applicativa.

Non è necessario ripetere la stessa verifica in livelli differenti.

### 2.3 Test come documentazione

I test rappresentano una forma di documentazione eseguibile.

Un buon test descrive chiaramente:

- il comportamento atteso;
- le condizioni iniziali;
- l'esito previsto.

Il lettore deve poter comprendere il comportamento del componente leggendo i suoi test principali.

---

## 3. Architettura della suite

La struttura dei progetti di test dovrebbe riflettere quella della soluzione principale.

Ad esempio:

```text
src/
    Domains/
    Applications/
    Shared/

tests/
    Domains/
    Applications/
    Shared/
```

Ogni progetto possiede il proprio progetto di test.

Quando necessario possono essere introdotti progetti dedicati ai test di integrazione o infrastrutturali.

### 3.1 Indipendenza

Ogni progetto di test deve essere il più possibile indipendente dagli altri.

La presenza di dipendenze tra progetti di test rappresenta generalmente un segnale di responsabilità non ben separate.

### 3.2 Organizzazione interna

All'interno di ciascun progetto di test è consigliabile mantenere una struttura simile a quella del codice di produzione.

Ad esempio:

```text
Services/
Repositories/
Controllers/
Contracts/
```

Questo facilita la navigazione e rende immediata la corrispondenza tra codice e test.

---

## 4. Unit Test

Gli Unit Test verificano il comportamento di un singolo componente isolato.

Costituiscono il livello principale della suite.

### 4.1 Responsabilità

Uno Unit Test dovrebbe verificare:

- un singolo comportamento;
- un singolo caso d'uso;
- un singolo risultato atteso.

Ogni test dovrebbe fallire per una sola ragione.

### 4.2 Isolamento

Le dipendenze esterne devono essere sostituite tramite stub, fake o mock quando necessario.

L'obiettivo consiste nel verificare esclusivamente il comportamento del componente sottoposto a test.

### 4.3 Cosa non verificare

Uno Unit Test non dovrebbe verificare:

- il comportamento di Entity Framework;
- il comportamento di ASP.NET Core;
- il funzionamento del filesystem;
- il comportamento di librerie esterne già testate.

Queste responsabilità appartengono ad altri livelli della suite.

---

## 5. Framework Test

Il progetto utilizza uno Shared Framework che implementa comportamenti comuni come:

- Request Pipeline;
- normalizzazione;
- validazione;
- gestione Bulk;
- componenti condivisi.

Questi comportamenti vengono verificati una sola volta mediante Framework Test dedicati.

### 5.1 Obiettivo

Lo scopo dei Framework Test consiste nel garantire che il comportamento condiviso rimanga stabile.

Quando il framework è stato verificato, i domini non devono ripetere gli stessi test.

### 5.2 Benefici

Questo approccio permette di:

- ridurre duplicazioni;
- diminuire il numero complessivo di test;
- mantenere la suite più veloce;
- concentrare i test dei domini sulla logica di business.

---

## 6. Contract Test

I Contract Test verificano i contratti pubblici esposti dai domini.

Non verificano la logica applicativa.

### 6.1 Responsabilità

I Contract Test controllano principalmente:

- configurazione dichiarativa;
- attributi;
- mapping;
- serializzazione;
- compatibilità del protocollo pubblico.

### 6.2 Request Pipeline

Le Request che implementano `IRequest` non devono ripetere test già coperti dal framework.

Ad esempio, non è necessario verificare in ogni dominio:

- normalizzazione;
- validazione automatica;
- esecuzione della pipeline.

Questi comportamenti appartengono allo Shared Framework.

I Contract Test verificano esclusivamente che il contratto sia configurato correttamente.

---

## 7. Integration Test

Gli Integration Test verificano la collaborazione tra più componenti.

Il loro obiettivo consiste nel garantire che le diverse parti del sistema cooperino correttamente.

### 7.1 Quando utilizzarli

Gli Integration Test sono appropriati quando occorre verificare:

- Repository e database;
- API e middleware;
- autenticazione;
- provider infrastrutturali;
- filesystem;
- cache;
- servizi esterni simulati.

### 7.2 Ambito

Un Integration Test può coinvolgere più componenti reali.

Tuttavia deve mantenere un obiettivo preciso.

Non dovrebbe trasformarsi in un test end-to-end involontario.

### 7.3 Velocità

Gli Integration Test sono generalmente più costosi degli Unit Test.

Per questo motivo devono essere utilizzati quando il comportamento non può essere verificato efficacemente tramite test unitari.

---

## 8. Test Infrastructure

La Test Infrastructure raccoglie tutti i componenti condivisi necessari all'esecuzione della suite di test.

Il suo obiettivo consiste nel ridurre le duplicazioni mantenendo i test semplici, leggibili e indipendenti.

La Test Infrastructure non deve contenere logica di business.

### 8.1 Componenti condivisi

Possono appartenere alla Test Infrastructure:

- fixture condivise;
- builder;
- factory;
- helper per il database;
- fake provider;
- dati di test riutilizzabili;
- utility per la serializzazione;
- componenti comuni di configurazione.

Ogni componente condiviso deve rappresentare una responsabilità reale.

### 8.2 Builder

I Builder permettono di creare rapidamente oggetti complessi utilizzati nei test.

Devono:

- produrre oggetti validi per impostazione predefinita;
- consentire la modifica selettiva delle proprietà rilevanti;
- evitare configurazioni verbose ripetute nei test.

Il loro scopo consiste nel migliorare la leggibilità dei test.

### 8.3 Fake e Mock

Quando possibile è preferibile utilizzare Fake semplici rispetto a Mock complessi.

I Mock dovrebbero essere utilizzati esclusivamente quando è necessario verificare l'interazione tra componenti.

L'implementazione del test deve rimanere focalizzata sul comportamento osservabile.

---

## 9. Test Data

I dati utilizzati nei test devono essere semplici, leggibili e facilmente riconoscibili.

Lo scopo del dato di test consiste nel rendere evidente il comportamento verificato.

### 9.1 Dati significativi

I valori utilizzati nei test devono avere un significato comprensibile.

Ad esempio:

```text
Album "Portfolio Estate"

Photo "Sunset.jpg"

User "Mario Rossi"
```

sono preferibili a valori casuali o privi di significato.

### 9.2 Dati minimi

Ogni test dovrebbe utilizzare esclusivamente i dati strettamente necessari.

L'eccesso di informazioni rende difficile comprendere il comportamento realmente verificato.

### 9.3 Indipendenza

Ogni test deve preparare autonomamente il proprio stato iniziale.

Non deve dipendere dall'esecuzione di altri test.

L'ordine di esecuzione non deve influenzarne il risultato.

---

## 10. Organizzazione della suite

La suite di test deve rimanere facilmente navigabile.

La struttura delle cartelle dovrebbe riflettere quella del codice di produzione.

Ad esempio:

```text
Services/
Repositories/
Controllers/
Contracts/
```

Questo rende immediata l'individuazione dei test relativi a un determinato componente.

### 10.1 Naming

Il nome del test dovrebbe descrivere chiaramente il comportamento verificato.

Il lettore dovrebbe comprendere il risultato atteso senza leggere l'implementazione.

Ad esempio:

```text
CreateAlbum_ShouldReturnAlbumId()

DeletePhoto_ShouldRemoveFile()

Normalize_ShouldTrimWhiteSpaces()
```

### 10.2 Evoluzione

Quando un componente viene spostato o rinominato, anche i relativi test devono seguire la nuova struttura.

La suite deve evolvere insieme all'architettura.

---

## 11. Evoluzione della strategia di testing

La strategia di testing deve evolvere insieme al progetto.

Nuovi livelli di test devono essere introdotti soltanto quando rappresentano una responsabilità distinta.

La crescita della suite non deve produrre duplicazioni sistematiche.

Ogni nuovo test dovrebbe contribuire ad aumentare la fiducia nell'architettura.

Non semplicemente la quantità di codice verificato.

### 11.1 Refactoring

Il refactoring della suite di test segue gli stessi principi del codice di produzione.

In particolare:

- eliminare duplicazioni;
- mantenere responsabilità chiare;
- introdurre astrazioni soltanto quando emergono naturalmente;
- mantenere i test leggibili.

### 11.2 Test come patrimonio architetturale

La suite di test rappresenta parte integrante dell'architettura del progetto.

Una modifica architetturale significativa dovrebbe riflettersi anche nell'organizzazione dei test.

La suite non deve essere considerata un elemento accessorio.

---

## 12. Checklist

Prima di considerare completa una nuova funzionalità verificare che:

- ogni responsabilità significativa sia verificata;
- il livello di test scelto sia appropriato;
- non esistano duplicazioni inutili;
- i test siano indipendenti tra loro;
- i dati utilizzati siano leggibili;
- il comportamento verificato sia chiaramente comprensibile;
- la struttura della suite rifletta quella del codice;
- eventuali componenti condivisi appartengano alla Test Infrastructure;
- il nuovo codice non riduca la qualità complessiva della suite.

---

## 13. Vedi anche

- `Architecture.md`
- `DomainArchitecture.md`
- `InfrastructureArchitecture.md`
- `SecurityArchitecture.md`
- `WebApplicationArchitecture.md`
- `SharedFramework.md`
- `Documentation/Engineering/MpsPlaybook.md`
- `ArchitectureRoadmap.md`
- `Architecture Decision Records (ADR)`