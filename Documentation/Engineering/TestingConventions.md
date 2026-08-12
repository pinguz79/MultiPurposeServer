# Convenzioni di testing

## 1. Scopo

Guida implementativa autorevole della suite. Strategia, livelli e responsabilita appartengono alla
[Testing Architecture](../Architecture/TestingArchitecture.md); lo stile generale alle
[Coding Conventions](CodingConventions.md).

## 2. Stack e isolamento

xUnit e il framework corrente. Le assertion devono essere fluenti, specifiche e leggibili. La libreria resta
aperta: FluentAssertions 8.10.0 richiede una valutazione di licenza/sostenibilita rispetto agli usi commerciali
futuri.

Ogni livello verifica la propria responsabilita: controller con service mockati, service con repository mockati,
repository con test propri isolando EF/persistenza quando utile. Integration test aggiuntivi coprono mapping,
query, constraint e comportamento relazionale. I Contracts verificano forma pubblica, attributi, nullability,
serializzazione e mapping propri del DTO, non logica applicativa.

## 3. Naming e region

Naming: `MetodoSottoTest_WhenCondition_RisultatoAtteso`. Senza metodo dominante, primo segmento uguale a
componente/comportamento. Risultati osservabili e precisi (`ReturnsEmptyList`, `ThrowsArgumentException`,
`CallsRepositoryOnce`), mai `Works`, `IsCorrect` o numerazioni. Nomi in inglese, coerenti col codice.

Batterie sullo stesso metodo possono avere una region col suo nome, solo se nella classe esistono almeno due
gruppi significativi. Nessuna region con un solo test; test isolati fuori dalle region.

## 4. Arrange, Act, Assert

Ogni test contiene sempre `// Arrange`, `// Act`, `// Assert`, anche se una sezione e vuota. Sezioni separate da
una riga vuota.

`Act` contiene esclusivamente l'operazione dichiarata nel primo segmento del nome. Ogni azione propedeutica alla
condizione `WhenCondition`, anche sul sistema sotto test, appartiene ad `Arrange`. Normalmente Act rappresenta una
sola operazione concettuale.

```csharp
[Fact]
public void DeleteAlbum_WhenAlbumContainsPhotos_ThrowsInvalidOperationException()
{
    // Arrange
    var album = _service.CreateAlbum("Test");
    _service.AddPhoto(album.Id, new Photo());

    // Act
    var action = () => _service.DeleteAlbum(album.Id);

    // Assert
    action.Should().Throw<InvalidOperationException>();
}
```

Un test verifica un comportamento. Piu assertion sono corrette se descrivono lo stesso stato/interazione; si
separa quando rappresentano regole indipendenti, richiedono Arrange diversi o rendono ambiguo il failure.

## 5. Assertion, eccezioni e interazioni

Preferire assertion specifiche (`BeEmpty`, `ContainSingle`, `BeEquivalentTo`) a controlli generici/manuali.
Confronto strutturale se descrive il contratto; assertion distinte per proprieta escluse o semantiche diverse.

Per eccezioni, Act cattura l'azione e Assert verifica tipo e dettagli contrattuali. Non verificare l'intero
messaggio salvo requisito stabile; preferire parametro, codice e proprieta. Verificare assenza di effetti
collaterali quando contrattuale e dichiararla nel nome (`Throws...WithoutCallingRepository`).

Moq sostituisce dipendenze senza replicarne l'implementazione. Verificare solo interazioni osservabili, niente
`VerifyAll()` indiscriminato. `MockBehavior.Strict` se tutte le interazioni ammesse devono essere esplicite,
standard se chiamate accessorie non sono oggetto del test.

Verificare l'ordine solo se e contratto reale e compare nel nome (`ValidatesBeforePersisting`); preferire
stato/risultato quando gia sufficienti.

## 6. Dati, builder e fixture

Costruzione diretta per pochi dati. Builder/factory per setup ripetuti, molti campi o varianti significative.
Builder valido e minimale per default; ogni `With...` cambia solo il dichiarato, senza replicare logica. Valori
rilevanti visibili; default stabili, niente casualita salvo property-based con seed.

Costruttore della classe per setup comune deterministico necessario alla maggioranza; condizione specifica nel
singolo Arrange. Niente gerarchie base per poche righe; fixture base per lifecycle reale (DB, server, directory).
Nessuno stato mutabile condiviso; test indipendenti e parallelizzabili salvo risorse serializzate esplicitamente.
`Dispose`/`IAsyncLifetime` gestiscono lifecycle, non assertion.

## 7. Fact, Theory e dati

`[Fact]` per scenario concreto; `[Theory]` se struttura AAA e comportamento restano uguali variando input/esito.
Setup o contratti differenti restano test separati. `InlineData` per semplici, `MemberData`/`ClassData` per
complessi/numerosi/riusati. Provider e scenari nominati; dati vicini al test salvo riuso.

Evitare `object[]` opachi, preferire scenari tipizzati. Unit test non leggono rete, database o file esterni.
Payload reali diventano fixture versionate, minimali e senza dati sensibili, con path indipendenti dalla macchina.

## 8. Async

Test async restituiscono `Task`, mai `async void`; attendono tutto e non usano `.Wait()`, `.Result` o sleep
arbitrari. Tempo/retry controllati con `TimeProvider`, token o sincronizzazione. Nessun suffisso `Async` nel nome
solo per il Task. Test di cancellazione gestiscono il source con lifecycle esplicito e verificano il contratto.

## 9. Suite specialistiche

### 9.1 Non regressione

Progetto o alberatura/namespace distinto. Commento italiano sintetico con bug storico e issue/backlog; dettagli
nell'issue tracker. Il test storico non sostituisce quello della regola generale emersa dal bug.

```csharp
// Regressione issue #1 / BL-0001:
// una route annidata non veniva inserita correttamente nella cache.
```

### 9.2 Produzione e smoke

Progetto dedicato, disabilitati per default, attivati esplicitamente. Read-only salvo mutazione dichiarata e
autorizzata. Endpoint/credenziali da configurazione esterna, mai hardcoded. Errori diagnostici senza segreti.

### 9.3 Integrazione e authorization boundary

Test che avviano DB, filesystem reale o rete non sono unit test e sono distinguibili tramite progetto, namespace
o categoria. Provisioning e organizzazione degli Authorization Boundary Test si definiscono con le suite.

## 10. Punti aperti

- scegliere una libreria di assertion fluenti sostenibile;
- definire provisioning degli ambienti di integrazione;
- definire trigger/filtri delle suite specialistiche;
- definire strumenti per confrontare Contracts serializzati;
- definire gli Authorization Boundary Test.

## 11. Riferimenti

- [Coding Conventions](CodingConventions.md)
- [Testing Architecture](../Architecture/TestingArchitecture.md)
- [MPS Playbook](MpsPlaybook.md)
- [Code Review](CodeReview.md)
