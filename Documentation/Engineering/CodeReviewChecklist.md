# Checklist di Code Review di MultiPurposeServer

## 1. Scopo

Questa checklist guida la revisione completa della solution MultiPurposeServer.

Deve essere utilizzata insieme a:

- `CodeReview.md`

> La checklist verifica la conformità della solution rispetto all'architettura di MultiPurposeServer.
>
> Le verifiche devono essere effettuate utilizzando come riferimento la documentazione architetturale del progetto (Architecture, ADR, Playbook e documentazione specialistica), evitando di applicare automaticamente linee guida o best practice generiche.

Per ogni punto applicabile deve essere registrato uno dei seguenti esiti:

```text
[ ] Da verificare
[x] Verificato
[!] Rilievo registrato
[-] Non applicabile
```

Ogni rilievo deve essere classificato come:

- correggere ora;
- registrare come TODO;
- nessun intervento.

---

## 2. Informazioni della revisione

- [ ] Data della revisione registrata.
- [ ] Revisore o revisori identificati.
- [ ] Branch o commit di riferimento registrato.
- [ ] Perimetro della revisione dichiarato.
- [ ] Milestone o motivo della revisione indicato.
- [ ] Documenti autorevoli individuati.
- [ ] Registro dei rilievi predisposto.

---

## 3. Stato iniziale

### Repository

- [ ] Il working tree è pulito oppure le modifiche presenti sono state comprese e registrate.
- [ ] La branch è aggiornata rispetto al riferimento previsto.
- [ ] Non sono presenti file generati o temporanei versionati.
- [ ] `bin`, `obj`, `.vs` e altri artefatti sono esclusi da Git.
- [ ] Non sono presenti file duplicati derivati da rename o migrazioni incomplete.

### Build

- [ ] La solution completa compila.
- [ ] I singoli progetti rilevanti compilano.
- [ ] Non sono presenti errori ignorati.
- [ ] I warning sono stati esaminati.
- [ ] I warning nuovi sono stati distinti da quelli preesistenti.

### Test

- [ ] Tutti i test vengono scoperti.
- [ ] Tutti i test vengono eseguiti.
- [ ] I test sono verdi oppure i fallimenti iniziali sono stati registrati.
- [ ] Non sono presenti test ignorati senza motivazione.
- [ ] Non sono presenti test instabili già noti e non tracciati.

---

## 4. Struttura della solution

### Progetti

- [ ] Ogni progetto possiede una responsabilità chiara.
- [ ] Non esistono progetti vuoti o non più utilizzati.
- [ ] Non esistono progetti di test frammentati senza una reale necessità.
- [ ] I nomi dei progetti sono coerenti con la responsabilità.
- [ ] I progetti sono collocati nella cartella corretta della repository.
- [ ] I riferimenti di progetto sono strettamente necessari.
- [ ] Non sono presenti riferimenti duplicati o inutilizzati.
- [ ] Il `RootNamespace` è coerente con il progetto.

### Cartelle e file

- [ ] La struttura delle cartelle riflette le responsabilità architetturali.
- [ ] I file sono collocati nella cartella corretta.
- [ ] I namespace corrispondono al progetto e alla struttura delle cartelle.
- [ ] Tutti i namespace utilizzano la convenzione block-scoped.
- [ ] Non sono presenti file con nomi fuorvianti.
- [ ] I refusi nei nomi sono stati registrati per un rename controllato in Visual Studio.
- [ ] Non sono presenti copie obsolete dello stesso file o della stessa classe.
- [ ] I file generati sono chiaramente distinguibili dai sorgenti mantenuti manualmente.

---

## 5. Architettura e dipendenze

### Confini

- [ ] Le dipendenze puntano verso l'interno.
- [ ] Non esistono dipendenze circolari.
- [ ] I Domains rimangono indipendenti.
- [ ] L'host compone i moduli senza conoscerne i dettagli interni.
- [ ] Le Applications dipendono soltanto dai contratti pubblici necessari.
- [ ] Lo Shared Framework non dipende da domini applicativi specifici.
- [ ] Le dipendenze verso framework e librerie di terze parti sono coerenti con la responsabilità del componente Shared.
- [ ] Le dipendenze Shared verso framework esterni rimangono realmente riutilizzabili tra più domini.
- [ ] Nessun componente è stato promosso nello Shared prematuramente.
- [ ] Le responsabilità non sono state spostate tra layer senza una motivazione architetturale.

### Coerenza con la documentazione

- [ ] `Architecture.md` descrive la struttura reale.
- [ ] I documenti architetturali specializzati descrivono i rispettivi sottosistemi.
- [ ] Gli ADR ancora accettati corrispondono alle decisioni effettivamente applicate.
- [ ] Le decisioni superate sono identificate correttamente.
- [ ] Le evoluzioni non ancora adottate rimangono nella roadmap e non sono descritte come stato corrente.

---

## 6. Contracts e API

### Contracts

- [ ] I Request DTO rappresentano il contratto di ingresso.
- [ ] I Response DTO rappresentano il contratto pubblico di uscita.
- [ ] I Contracts non contengono logica applicativa.
- [ ] Le decorazioni di normalizzazione sono corrette.
- [ ] Le decorazioni di validazione sono corrette.
- [ ] I gruppi dichiarativi sono coerenti.
- [ ] La validazione ricorsiva parent/child è configurata correttamente.
- [ ] I Contracts Bulk rispettano il contratto condiviso.
- [ ] I Response DTO effettuano il mapping previsto senza introdurre dipendenze inverse.
- [ ] Le Request non dipendono da componenti di persistenza.
- [ ] I Response DTO possono dipendere dalle Entity esclusivamente per tradurre il modello interno nel contratto pubblico.
- [ ] I Response DTO non accedono a Repository, DbContext o logica di persistenza.
- [ ] Le Entity non dipendono da `Portfolio.Contracts`.

### Pipeline HTTP

- [ ] La normalizzazione avviene prima della validazione.
- [ ] La normalizzazione e la validazione sono centralizzate.
- [ ] I Controller non invocano manualmente `Normalize()` o `Validate()`.
- [ ] Le Request non valide non raggiungono i Controller.
- [ ] `ValidationException` viene tradotta nella risposta HTTP prevista.
- [ ] Gli errori strutturati mantengono path e chiavi corretti.
- [ ] Non rimangono controlli duplicati appartenenti alla pipeline.

### Controller

- [ ] I Controller ricevono Request già normalizzate e validate.
- [ ] I Controller orchestrano senza contenere logica di business.
- [ ] I Controller effettuano il mapping tra Contracts e Application.
- [ ] Gli status code sono coerenti.
- [ ] Routing, route name e parametri sono coerenti.
- [ ] Le risposte `NotFound`, `BadRequest`, `CreatedAtAction`, `Problem` e `Ok` sono utilizzate correttamente.
- [ ] I Controller non contengono normalizzazioni manuali residue.
- [ ] I Controller Bulk distinguono errori strutturali da errori applicativi sul singolo elemento.
- [ ] Swagger descrive correttamente autenticazione e contratti esposti.

### Compatibilità

- [ ] Le modifiche ai Contracts sono intenzionali.
- [ ] Le modifiche potenzialmente incompatibili sono state identificate.
- [ ] I client interessati sono stati considerati.
- [ ] Le convenzioni di serializzazione sono coerenti.

---

## 7. Application e dominio

### Service

- [ ] I Service implementano logica applicativa.
- [ ] I Service non conoscono HTTP.
- [ ] I Service non dipendono dai Contracts pubblici quando non previsto dall'architettura.
- [ ] I Service non duplicano normalizzazione o validazione dichiarativa.
- [ ] I metodi hanno responsabilità focalizzate.
- [ ] Le operazioni applicative sono atomiche quando necessario.
- [ ] Le transazioni vengono completate soltanto dopo il successo.
- [ ] Le risorse asincrone vengono rilasciate correttamente.
- [ ] Le eccezioni applicative hanno un significato chiaro.

### Dominio

- [ ] Le invarianti documentate sono rispettate.
- [ ] I nomi dei concetti corrispondono al linguaggio del dominio.
- [ ] Le Entity non dipendono da Controller, DTO o infrastruttura HTTP.
- [ ] Il modello di dominio non è deformato da esigenze di presentazione.
- [ ] I concetti emergenti sono documentati senza generalizzazioni premature.
- [ ] Le regole di business sono collocate nel livello corretto.

---

## 8. Infrastructure e persistenza

### Dependency Injection

- [ ] Ogni dominio registra autonomamente le proprie dipendenze.
- [ ] I lifetime DI sono coerenti con il comportamento dei componenti.
- [ ] Non esistono registrazioni duplicate o contrastanti.
- [ ] Le dipendenze opzionali e obbligatorie sono gestite esplicitamente.
- [ ] Le Options sono associate alla sezione corretta.
- [ ] Gli errori di configurazione producono messaggi chiari.

### Repository

- [ ] I Repository si occupano esclusivamente di persistenza.
- [ ] I Repository non conoscono HTTP o Contracts pubblici.
- [ ] Le query sono corrette e comprensibili.
- [ ] Il caricamento lazy o eager è intenzionale.
- [ ] Le operazioni di scrittura rispettano le transazioni previste.
- [ ] La gestione di entità mancanti è coerente.
- [ ] Non è presente logica di business nei Repository.

### Database

- [ ] Ogni dominio possiede il proprio DbContext.
- [ ] Migration e database appartengono al dominio corretto.
- [ ] Le migration pendenti sono note.
- [ ] I test di persistenza utilizzano isolamento adeguato.
- [ ] Connessioni e contesti vengono rilasciati correttamente.
- [ ] I test non condividono stato involontariamente.

### Filesystem, media e cache

- [ ] I path derivano da configurazione appropriata.
- [ ] Gli originali rimangono contenuti autorevoli.
- [ ] Cache e varianti possono essere ricostruite.
- [ ] Le risorse temporanee vengono eliminate.
- [ ] I file stream vengono rilasciati.
- [ ] La cache viene invalidata al momento corretto.
- [ ] Gli errori dei servizi esterni sono gestiti in modo coerente.
- [ ] Timeout e BaseAddress degli HttpClient sono configurati.

### Logging ed error handling

- [ ] Gli errori significativi vengono registrati.
- [ ] Il logging non espone segreti o dati sensibili.
- [ ] Non vengono catturate eccezioni senza una decisione esplicita.
- [ ] Le eccezioni vengono tradotte nel livello corretto.
- [ ] Non sono presenti `catch` generici che nascondono difetti.
- [ ] I messaggi di errore sono coerenti e comprensibili.

---

## 9. Sicurezza

### Authentication

- [ ] L'autenticazione del client è distinta da quella dell'utente.
- [ ] Gli schemi sono registrati correttamente.
- [ ] Le credenziali non sono presenti nel codice sorgente.
- [ ] Header e chiavi sono configurabili.
- [ ] La revoca o sostituzione delle chiavi è possibile.

### Authorization

- [ ] Le policy FrontEnd e BackEnd applicano i permessi previsti.
- [ ] I claim richiesti sono corretti.
- [ ] Gli endpoint espongono la policy appropriata.
- [ ] Il backend rimane la fonte autorevole delle decisioni.
- [ ] Non esistono endpoint accidentalmente anonimi.

### Ambiente Development

- [ ] Gli eventuali bypass sono limitati a Development.
- [ ] Il comportamento Production rimane protetto.
- [ ] I test distinguono esplicitamente Development e Production.
- [ ] Swagger non modifica involontariamente la sicurezza reale delle API.
- [ ] I bypass sono documentati.

### Segreti e configurazione

- [ ] I segreti non sono versionati.
- [ ] I file di configurazione pubblici non contengono valori sensibili.
- [ ] Le Options sensibili sono validate.
- [ ] I log non includono chiavi, token o segreti.

---

## 10. Shared Framework

### Responsabilità

- [ ] Ogni componente Shared è utilizzato o giustificato da più contesti.
- [ ] Non sono presenti dipendenze da Portfolio o altri domini applicativi specifici.
- [ ] Le dipendenze verso framework e librerie di terze parti sono coerenti con la responsabilità del componente.
- [ ] Le utility framework-specific rimangono realmente cross-domain.
- [ ] Le astrazioni rappresentano concetti stabili.
- [ ] Non sono state introdotte interfacce senza una reale necessità.
- [ ] Il comportamento condiviso rimane indipendente dal trasporto.

> **Nota**
>
> Una dipendenza verso un framework o una libreria di terze parti (ad esempio Entity Framework Core, ASP.NET Core o altre librerie infrastrutturali) non costituisce automaticamente un rilievo architetturale.
>
> La verifica deve stabilire se tale dipendenza è coerente con la responsabilità del componente Shared e se il comportamento rimane realmente riutilizzabile tra più domini.
>
> Una dipendenza verso un dominio applicativo specifico costituisce invece un rilievo, salvo diversa decisione architetturale documentata.

### Normalization Framework

- [ ] Gli attributi supportati corrispondono alle regole implementate.
- [ ] I piani vengono costruiti e riutilizzati correttamente.
- [ ] Le proprietà non supportate producono errori chiari.
- [ ] La normalizzazione ricorsiva compone correttamente i figli.
- [ ] Non rimangono normalizzazioni manuali duplicate nei consumer.

### Validation Framework

- [ ] Gli attributi supportati corrispondono alle regole implementate.
- [ ] `Required` mantiene la semantica prevista per stringhe, collezioni e value type.
- [ ] I gruppi vengono costruiti una sola volta per piano.
- [ ] `RequiredAtLeastOneTrue` rifiuta configurazioni non booleane.
- [ ] `ValidateChildren` gestisce oggetti, collezioni, elementi null e path indicizzati.
- [ ] Tutti gli errori vengono raccolti senza interrompere prematuramente la validazione.
- [ ] Non rimangono validazioni manuali duplicate nei consumer.

### Concorrenza e cache

- [ ] Le cache condivise sono thread-safe.
- [ ] Gli oggetti memorizzati in cache sono immutabili o utilizzati in sicurezza.
- [ ] La reflection è limitata alla costruzione dei piani quando previsto.
- [ ] Non sono presenti cache statiche dipendenti da configurazione mutabile.

---

## 11. Testing

### Strategia

- [ ] Ogni test verifica la responsabilità corretta.
- [ ] I test unitari non verificano il comportamento della pipeline MVC.
- [ ] I Contract Test verificano la configurazione dichiarativa.
- [ ] I Framework Test verificano il comportamento dei motori condivisi.
- [ ] Gli Integration Test previsti sono identificati.
- [ ] I test end-to-end sono introdotti soltanto quando giustificati.

### Copertura significativa

- [ ] Sono coperti i percorsi di successo.
- [ ] Sono coperti gli errori applicativi rilevanti.
- [ ] Sono coperti null, collezioni vuote e valori limite quando significativi.
- [ ] Sono coperti i gruppi dichiarativi.
- [ ] Sono coperti oggetti figli e collezioni annidate.
- [ ] Sono coperti mapping e status code dei Controller.
- [ ] Sono coperte transazioni e rilascio delle risorse.
- [ ] Sono coperte configurazioni Development e Production quando differiscono.

### Qualità dei test

- [ ] I nomi rispettano `Metodo_WhenCondizione_RisultatoAtteso`.
- [ ] I test seguono Arrange, Act, Assert.
- [ ] Ogni test verifica un comportamento comprensibile.
- [ ] Gli assert non dipendono da proprietà assenti nel tipo effettivo.
- [ ] I mock verificano le interazioni realmente significative.
- [ ] I test non replicano internamente l'implementazione.
- [ ] Gli helper privati sono conservati e collocati correttamente.
- [ ] I dati di test sono leggibili.
- [ ] I test non dipendono dall'ordine di esecuzione.
- [ ] I test non condividono stato mutabile.
- [ ] I messaggi attesi corrispondono al comportamento reale.

### Struttura

- [ ] I progetti di test riflettono i progetti produttivi.
- [ ] Le cartelle dei test rispecchiano le cartelle del progetto testato.
- [ ] Helper e infrastruttura di test sono separati dai test senza frammentazione inutile.
- [ ] Le classi base di test sono collocate nel livello corretto.
- [ ] Non sono presenti progetti di test vuoti.
- [ ] Nessun file di test è stato perso durante consolidamenti o spostamenti.
- [ ] Rename e move sono stati verificati senza duplicare classi.

### Test mancanti o rimossi

- [ ] I test rimossi perché appartenenti a un altro livello sono stati registrati come specifiche.
- [ ] Gli Integration Test della pipeline MVC sono presenti nella roadmap.
- [ ] I TODO di testing specificano comportamento e livello previsto.
- [ ] Non sono stati cancellati casi d'uso senza conservarne l'intenzione.

---

## 12. Qualità e organizzazione del codice

### Leggibilità

- [ ] I nomi esprimono l'intenzione.
- [ ] I metodi sono focalizzati.
- [ ] Le classi hanno una responsabilità comprensibile.
- [ ] La complessità è giustificata.
- [ ] Non sono presenti condizioni o rami irraggiungibili.
- [ ] Non sono presenti commenti che contraddicono il codice.
- [ ] Il codice morto è stato eliminato o registrato.

### Duplicazione

- [ ] La duplicazione accidentale è stata rimossa.
- [ ] La duplicazione informativa non è stata astratta prematuramente.
- [ ] Le utility condivise hanno una responsabilità stabile.
- [ ] Non esistono implementazioni parallele dello stesso comportamento.

### Organizzazione interna

- [ ] L'ordine dei membri è coerente.
- [ ] I metodi correlati sono vicini.
- [ ] I metodi di test sono separati dagli helper.
- [ ] Factory, fixture, dati di test e tipi annidati sono riconoscibili.
- [ ] Le `#region` vengono utilizzate soltanto quando migliorano la navigazione.
- [ ] I file piccoli non sono appesantiti da sezioni inutili.
- [ ] La formattazione esistente non viene alterata senza necessità.
- [ ] Non sono stati introdotti ritorni a capo non necessari.
- [ ] Le fluent call rimangono su una riga quando leggibili.

### Convenzioni C#

- [ ] I namespace sono block-scoped.
- [ ] `var` e tipi espliciti seguono le convenzioni del progetto.
- [ ] Nullability è gestita correttamente.
- [ ] Le collection expression sono utilizzate coerentemente.
- [ ] Le risorse asincrone utilizzano `await using` quando necessario.
- [ ] Le API sincrone e asincrone sono scelte consapevolmente.
- [ ] Non sono presenti suppressions senza motivazione.

---

## 13. Dipendenze e configurazione dei progetti

- [ ] I `PackageReference` sono necessari.
- [ ] Le versioni dei package sono coerenti.
- [ ] Non sono presenti package duplicati dopo merge di progetti.
- [ ] Ogni `ProjectReference` è necessario e coerente con l'architettura documentata.
- [ ] Le dipendenze di test non sono penetrate nei progetti produttivi.
- [ ] `InternalsVisibleTo` è ancora corretto.
- [ ] Analyzer e impostazioni del compilatore sono applicati.
- [ ] Target framework e language version sono coerenti.
- [ ] I file `.csproj` non includono manualmente `bin` o `obj`.
- [ ] Le cartelle vuote non introducono elementi inutili nel progetto.

---

## 14. Documentazione

### Completezza

- [ ] La documentazione architetturale riflette il codice.
- [ ] Il Playbook riflette le convenzioni consolidate.
- [ ] La roadmap contiene soltanto evoluzioni non concluse.
- [ ] Gli ADR descrivono decisioni reali e storicamente corrette.
- [ ] La documentazione di dominio descrive il linguaggio corrente.
- [ ] Il Glossario contiene i termini rilevanti.
- [ ] README e riferimenti incrociati sono aggiornati.

### Coerenza

- [ ] Tutta la documentazione è in italiano, salvo nomenclatura tecnica mantenuta intenzionalmente.
- [ ] I nomi dei file citati esistono realmente.
- [ ] I path documentali sono corretti.
- [ ] Non sono presenti sezioni duplicate.
- [ ] Non sono presenti code fence non chiusi.
- [ ] Non sono presenti contenuti WIP descritti come completati.
- [ ] La fonte autorevole di ogni concetto è chiara.

### Aggiornamenti emersi dalla review

- [ ] I nuovi concetti stabili sono stati documentati.
- [ ] Il debito tecnico è stato aggiunto alla roadmap.
- [ ] Le decisioni architetturali permanenti richiedono o non richiedono un ADR.
- [ ] I riferimenti al Playbook utilizzano il path corrente.
- [ ] I TODO temporanei non sono stati trasformati impropriamente in regole ufficiali.

---

## 15. Pulizia finale

### Codice e repository

- [ ] Non sono presenti file temporanei.
- [ ] Non sono presenti copie obsolete.
- [ ] Non sono presenti progetti vuoti.
- [ ] Non sono presenti cartelle `bin`, `obj` o `.vs` tracciate.
- [ ] Non sono presenti using inutilizzati.
- [ ] Non sono presenti warning nuovi non registrati.
- [ ] Non sono presenti TODO nel codice senza destinazione o spiegazione.
- [ ] Il diff contiene soltanto modifiche intenzionali.

### Build e test finali

- [ ] Clean eseguita quando necessaria.
- [ ] Rebuild completa riuscita.
- [ ] Tutti i test sono stati eseguiti.
- [ ] Tutti i test sono verdi.
- [ ] Il numero dei test è coerente con lo stato precedente e con le modifiche intenzionali.
- [ ] Gli eventuali test rimossi sono tracciati.
- [ ] Non sono state modificate aspettative soltanto per ottenere test verdi.

### Documentazione e tracciamento

- [ ] Il registro dei rilievi è aggiornato.
- [ ] Tutti i problemi bloccanti sono risolti.
- [ ] Tutti i problemi rinviati hanno un TODO, issue o voce di roadmap.
- [ ] La checklist è stata completata.
- [ ] L'esito finale è stato registrato.
- [ ] Il commit finale è focalizzato e descrittivo.

---

## 16. Esito finale

### Risultato

- [ ] Approvata
- [ ] Approvata con TODO
- [ ] Non approvata

### Verifiche conclusive

- [ ] Build completa: superata.
- [ ] Test completi: superati.
- [ ] Warning residui: registrati.
- [ ] TODO residui: registrati.
- [ ] Documentazione: aggiornata.
- [ ] Milestone: conclusa oppure mantenuta aperta con motivazione.

### Note

```text
Inserire qui il riepilogo della revisione, le principali decisioni e i riferimenti ai rilievi registrati.
```

---

## 17. Vedi anche

- `CodeReview.md`
- `MpsPlaybook.md`
- `../Architecture/Architecture.md`
- `../Architecture/ArchitectureRoadmap.md`
- `../Architecture/TestingArchitecture.md`
- `../Architecture/SecurityArchitecture.md`
