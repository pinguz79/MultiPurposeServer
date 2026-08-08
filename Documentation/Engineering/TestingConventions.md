# Convenzioni di testing

> **Stato: Alpha 0 — non autorevole.** Le convenzioni devono essere verificate sul codice e completate prima della promozione.

## 1. Scopo

Questo documento raccoglie le convenzioni implementative della suite di test. Strategia, livelli e responsabilità appartengono alla [Testing Architecture](../Architecture/TestingArchitecture.md).

---

## 2. Convenzioni già concordate

- xUnit è il test framework corrente.
- Ogni test segue Arrange-Act-Assert.
- I commenti `// Arrange`, `// Act` e `// Assert` sono sempre presenti, anche nei test brevi.
- Il naming segue normalmente `MethodUnderTest_WhenCondition_ShouldExpectedBehavior`.
- Quando non esiste un singolo metodo dominante, il nome può partire dal componente o comportamento osservato.
- I test verificano la responsabilità del livello sotto test e non duplicano sistematicamente i test interni dei collaboratori.

I Builder producono oggetti validi per impostazione predefinita e permettono variazioni selettive. I dati di test sono minimi, leggibili e significativi.

Stub, fake e mock mantengono il test focalizzato sul comportamento osservabile e non simulano in modo fragile il funzionamento interno delle tecnologie esterne.

---

## 3. Decisioni ancora da consolidare

- libreria per le asserzioni fluenti, previa verifica di licenza e sostenibilità;
- convenzioni per test parametrizzati;
- organizzazione concreta di progetti, cartelle e namespace;
- lifecycle e naming dei dati di test;
- utilizzo di fixture, builder, factory e utility condivise;
- criteri implementativi per scegliere stub, fake e mock;
- provisioning degli ambienti isolati di integrazione;
- trigger, filtri e comandi della pipeline di test;
- strumenti per descrivere o confrontare la rappresentazione pubblica serializzata dei Contracts;
- organizzazione ed esecuzione degli Authorization Boundary Test.

---

## 4. Riferimenti

- [Testing Architecture](../Architecture/TestingArchitecture.md)
- [MPS Playbook](MpsPlaybook.md)
- [Code Review](CodeReview.md)
- [Code Review Checklist](CodeReviewChecklist.md)
