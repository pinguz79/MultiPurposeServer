# Premi assoluti

Blocco del 26 ottobre 2026, coerente con DS-054/055/056/066.

## Percorsi e schermata

Il menu **Assolute** apre due classifiche indipendenti:

- **Non PRO**: primi classificati dei premi intermedi, secondo l'aggregatore già configurato.
- **PRO**: primi classificati delle gare PRO, senza premi intermedi né necessità di impostare l'aggregatore JSON.

La vista mostra posizione, coreografia, scuola, qualificazione, punti e parità. La selezione
mostra i voti originali, con quelli esclusi dal conteggio in grigio; permette di aprire la
gara di origine per correggere i voti. La vista non duplica né modifica direttamente i voti.
Le qualificazioni provvisorie sono visibili; tutte le fonti devono essere definitive prima
della convalida esplicita dell'assoluta. Una classifica vuota non è convalidabile.

## Calcolo e correzioni

Si usano i voti ufficiali correnti, comprese le correzioni per spareggio. L'esclusione dei
giudici è ricalcolata autonomamente sui partecipanti dell'assoluta, senza ereditare totali
o esclusioni dei premi precedenti. Parità sul podio impediscono la convalida; quelle fuori
dal podio restano evidenziate senza bloccarla. Una parità provvisoria al primo posto porta
tutti i candidati nella vista successiva, ma la fonte deve essere regolarizzata e convalidata.

Riaprire una gara non PRO rende provvisori il premio intermedio collegato e l'assoluta non
PRO; riaprire una gara PRO invalida solo l'assoluta PRO. Riaprire un premio intermedio
invalida l'assoluta non PRO. Richiudere o riconvalidare le fonti non riconvalida automaticamente
l'assoluta: serve una nuova verifica esplicita. Riaprire soltanto l'assoluta non modifica le fonti.

## Archivio e verifica

Formato v8: tabella `AbsoluteAward` con convalida distinta per percorso. Nessuna copia di
coreografie o voti. Letture in transazione senza scritture, convalida rivalutata nella
stessa transazione di aggiornamento. Migrazione esplicita da v1–v7 con le opzioni di backup
già disponibili; i premi intermedi esistenti vengono conservati.

Prova consigliata: chiudere gare di almeno due discipline, convalidare i premi intermedi e
aprire l'assoluta non PRO; verificare separatamente la PRO. Riaprire una gara, correggere
un voto e ripercorrere le convalide: i punti si aggiornano senza alterare l'altro percorso.
Notifiche e ripristino guidato dei backup restano fuori da questo blocco: usare dati dimostrativi.
