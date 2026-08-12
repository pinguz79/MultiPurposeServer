# Copilot Instructions

## Lingua

- Rispondere in italiano (`it-IT`).
- Scrivere i commenti nel codice in italiano, salvo sigle e nomi di pattern consolidati come `AAA`.

## Fonti autorevoli

Prima di generare o modificare codice, applicare nell'ordine:

1. [Coding Conventions](../Documentation/Engineering/CodingConventions.md);
2. [Testing Conventions](../Documentation/Engineering/TestingConventions.md), quando sono coinvolti test;
3. [MPS Playbook](../Documentation/Engineering/MpsPlaybook.md);
4. il file `.editorconfig` applicabile al sorgente.

La documentazione definisce le regole semantiche ed editoriali; `.editorconfig`, formatter e analyzer applicano
soltanto la parte deterministica. In caso di conflitto prevale la documentazione più specifica.

## Modifiche e review

- Non introdurre cambiamenti funzionali durante attività di solo code style.
- Non applicare wrapping automatico: la guida di 100 caratteri, con tolleranza fino a circa 105, richiede giudizio
  umano e le righe vanno spezzate solo dove serve.
- Prima di proporre una commit, riesaminare i file modificati per intero rispetto a `HEAD`, non soltanto l'ultima
  modifica incrementale.
- Applicare automaticamente solo correzioni deterministiche e sicure; segnalare le scelte semantiche che
  richiedono revisione umana.
- Nei test mantenere sempre le sezioni commentate `Arrange`, `Act`, `Assert`; sotto `Act` deve comparire soltanto
  l'operazione descritta dal nome del test.
