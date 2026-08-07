# Registro dei rischi dei segreti esposti

## 1. Scopo

Questo documento registra le categorie di segreti la cui esposizione o presenza nella configurazione versionata è temporaneamente accettata secondo l'ADR-0011.

Il registro non contiene valori, hash, percorsi precisi o istruzioni che facilitino la localizzazione e l'utilizzo delle credenziali. La presenza di una voce documenta una valutazione del rischio e non autorizza automaticamente nuove esposizioni della stessa categoria.

Gli identificatori `SR-XXXX` sono stabili e non vengono riutilizzati.

---

## 2. Metodo

Ogni valutazione considera:

- massimo danno ottenibile;
- difficoltà, costo e tempo di recovery;
- probabilità concreta di attacco;
- possibilità di revoca o rotazione;
- rischio residuo;
- condizioni di revisione.

Soltanto un rischio residuo basso o molto basso può essere accettato temporaneamente. Ogni nuovo segreto viene valutato prima dell'esposizione.

---

## 3. Registro corrente

La documentazione Alpha attesta che le categorie correnti sono state censite e classificate complessivamente a rischio basso o molto basso. I dettagli puntuali della valutazione devono essere riportati senza dedurli o ricostruirli dai valori presenti nella configurazione.

| ID | Tipologia | Scopo generale | Danno massimo | Recovery | Probabilità | Rischio residuo | Stato della scheda |
|---|---|---|---|---|---|---|---|
| SR-0001 | API key applicative | Distinguere capacità FrontEnd e BackEnd | Da documentare | Da documentare | Da documentare | Basso o molto basso; valore puntuale da riportare | Valutazione da trascrivere |
| SR-0002 | Credenziali database | Accesso alla persistenza dei domini | Da documentare | Da documentare | Da documentare | Basso o molto basso; valore puntuale da riportare | Valutazione da trascrivere |
| SR-0003 | Signing key e configurazione token | Emissione o validazione di token locali | Da documentare | Da documentare | Da documentare | Basso o molto basso; valore puntuale da riportare | Valutazione da trascrivere |
| SR-0004 | Shared secret applicativi | Comunicazione fra componenti distribuiti | Da documentare | Da documentare | Da documentare | Basso o molto basso; valore puntuale da riportare | Valutazione da trascrivere |
| SR-0005 | Credenziali di provider esterni | Integrazione con servizi terzi | Da documentare | Da documentare | Da documentare | Basso o molto basso; valore puntuale da riportare | Valutazione da trascrivere |

---

## 4. Aggiornamento

Una voce viene rivalutata quando cambia esposizione, impatto, recovery, probabilità, ambiente, criticità dei dati o processo di deployment.

La chiusura di una voce richiede separazione del valore dalla configurazione versionata e, quando il valore è stato presente nella history, rotazione della credenziale sugli ambienti interessati.

## Riferimenti

- [ADR-0011](../Architecture/ADR/ADR-0011-temporary-versioned-secrets-require-low-risk.md)
- [Security Architecture](../Architecture/SecurityArchitecture.md)
- [Technical Debt](../Engineering/TechnicalDebt.md)
