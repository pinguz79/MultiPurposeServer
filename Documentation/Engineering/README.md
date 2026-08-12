# Engineering

Questa cartella raccoglie la documentazione relativa alle pratiche di ingegneria adottate durante lo sviluppo di MultiPurposeServer.

## MpsPlaybook

Il `MpsPlaybook.md` definisce:

- principi di ingegneria;
- flusso di sviluppo;
- gestione di commit, cambiamenti e refactoring;
- gestione del debito tecnico;
- evoluzione della documentazione;
- Definition of Done.

Il Playbook si applica a tutti i contributori del progetto:

- sviluppatori;
- assistenti AI;
- strumenti di generazione automatica del codice.

## Code Review

Il `CodeReview.md` descrive come pianificare, eseguire e chiudere una revisione completa della solution.

Definisce:

- quando avviare una code review completa;
- come stabilirne il perimetro;
- come registrare e classificare i rilievi;
- come distinguere correzioni immediate e TODO;
- quali evidenze produrre;
- quando la revisione può essere considerata conclusa.

## Code Review Checklist

Il `CodeReviewChecklist.md` contiene la checklist operativa da utilizzare durante la revisione.

La checklist copre:

- repository e build;
- struttura della solution;
- architettura e dipendenze;
- Contracts e API;
- Application e dominio;
- Infrastructure e persistenza;
- sicurezza;
- Shared Framework;
- testing;
- qualità del codice;
- documentazione;
- verifica finale.

Le istruzioni specifiche dei singoli assistenti AI appartengono invece alla cartella `Documentation/AI`.

## Technical Debt

Il `TechnicalDebt.md` è il registro autorevole del debito tecnico noto.

Per ogni voce conserva:

- identificatore stabile;
- area e origine;
- stato e priorità;
- impatto;
- rapporto costi/benefici;
- urgenza strategica;
- workaround e condizione di revisione.

## Testing Conventions

`TestingConventions.md` definisce le convenzioni implementative autorevoli della suite. Strategia e livelli appartengono a `TestingArchitecture.md`.

## Coding Conventions

`CodingConventions.md` definisce le convenzioni autorevoli per C#, PHP, JavaScript, CSS e SQL, oltre alla quality gate pre-commit e alla distinzione fra enforcement automatico e revisione semantica.
