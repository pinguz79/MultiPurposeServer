# Finance — Trasferimenti e gruppi di movimenti

## Stato e perimetro

Decisioni approvate il 2 ottobre 2026. Prima implementazione delle sole API pubblicata e distribuita
con il commit `961dd7f`. Implementata la GUI di creazione trasferimenti: menu Conti,
pulsante nel dettaglio e menu contestuale delle card (questi ultimi precompilano la destinazione).
La dialog mostra i segni risolti tramite i parametri validi alla data scelta; salva i due movimenti
con una singola richiesta e mantiene aperto il dettaglio conto. Nessun invio automatico ripetuto
in caso di errore; un esito incerto richiede di verificare i movimenti prima di riprovare.
Restano da implementare le dialog di modifica assistita e cancellazione parziale dei gruppi.
Questa specifica sostituisce la correlazione fra coppie di Movimenti precedentemente prevista,
non la correlazione fra Pianificazioni già esistente. Nessuna operazione dispone trasferimenti reali:
Finance registra o prevede soltanto i relativi movimenti.

## Creazione del trasferimento

Una singola operazione crea atomicamente due Movimenti su conti distinti. Tutti i conti sono
selezionabili: non si introducono abilitazioni o restrizioni per tipologia. Origine e destinazione
non possono coincidere. Importo strettamente positivo; data, descrizione e stato di conferma
inizialmente comuni, categoria nessuna. I movimenti possono successivamente avere date,
descrizioni e valori differenti.

Il sistema determina i segni secondo la semantica dei conti, non imponendo segni opposti:

| Origine → destinazione | Movimento origine | Movimento destinazione |
| --- | ---: | ---: |
| Carta → conto corrente | +importo | +importo |
| Conto corrente → carta | −importo | −importo |
| Conto corrente → conto corrente | −importo | +importo |
| Carta → carta | +importo | −importo |

La dialog mostra un riepilogo prima della creazione. Saldo o plafond insufficienti non bloccano
l'operazione: rimangono le normali segnalazioni di criticità. L'importo trasferito è uguale
economicamente sui due movimenti alla creazione; non è un vincolo permanente del gruppo.

- Da confermare: crea entrambi i Movimenti e il gruppo nella stessa operazione atomica.
- Già confermato: crea due Movimenti con valori costanti, senza gruppo.
- Un errore non deve lasciare un solo Movimento né un gruppo incompleto.

## Gruppo di movimenti correlati

Modello logico: `GruppoMovimenti` con identificativo e membri; sul Movimento un riferimento
opzionale al gruppo. Ogni Movimento appartiene al massimo a un gruppo. Non esistono archi
A–B, distinzione diretto/indiretto o propagazione a tappe: tutti i membri sono correlati.
Un gruppo con meno di due membri viene rimosso; l'eventuale superstite resta indipendente.
Il gruppo non è una Pianificazione e non impone uguaglianza di importo, data o descrizione.

### Conferma

Confermare un membro conferma automaticamente tutti i membri, anche futuri e anche con formule
già costanti. Si congelano le formule ai rispettivi valori mantenendo le date, si impostano
i Movimenti come confermati e si rimuovono i collegamenti operativi alle Pianificazioni e il gruppo.
Tutto avviene atomicamente: se un membro non è consolidabile non si modifica alcun membro.
La conferma non rende i dati immutabili e non equivale necessariamente all'avvenuto accredito:
rimane possibile bonificare in seguito anche un Movimento futuro confermato.

### Modifica assistita

La modifica iniziale viene salvata. Successivamente si propone in un'unica dialog l'aggiornamento
degli altri membri del gruppo. Ogni membro mostra conto e valori attuali, una selezione di inclusione
e campi indipendenti precompilati con i nuovi valori delle sole proprietà modificate.
Le proprietà non modificate restano invariate. Per gli importi si rispetta la semantica del conto,
lasciando comunque modificabile la proposta: la correlazione generica non impone importi uguali.

L'utente può, ad esempio, assegnare descrizioni diverse o anticipare di un giorno la data del
movimento sulla carta rispetto all'accredito. Può escludere singoli membri o annullare l'intera
seconda fase: la modifica iniziale rimane salvata. Gli aggiornamenti selezionati nella seconda
fase sono atomici; in caso di errore nessuno viene applicato e la dialog resta aperta per correggere.
Non si ripropongono ricorsivamente i membri già elaborati.

### Eliminazione selettiva

Prima di cancellare si mostra l'intero gruppo, con il Movimento iniziale già selezionato e
selezione indipendente dei membri. Annullare non modifica nulla. Si eliminano atomicamente
solo i Movimenti scelti; i superstiti conservano il gruppo se sono almeno due. Non occorre
ricostruire collegamenti transitivi. Con zero o un superstite il gruppo viene eliminato.

## Client desktop

La stessa dialog è raggiungibile da:

- menu `Conti → Trasferimento…`;
- pulsante `Trasferimento…` nel dettaglio conto;
- menu contestuale della card, tasto destro → `Trasferimento…`.

Gli ultimi due accessi precompilano il conto come destinazione, comunque modificabile.
Default: data odierna, importo zero (da correggere prima del salvataggio), descrizione vuota,
Confermato non selezionato. Origine e destinazione sono modificabili.

Nei campi numerici di inserimento/modifica Movimento e della dialog Trasferimento:

- acquisendo il focus si seleziona l'intero testo;
- il separatore decimale del tastierino viene interpretato come virgola nel formato italiano.

Queste regole riguardano il controllo numerico, non il parser delle Formule o i formati delle API.

## API e persistenza

L'orchestrazione resta nei Controller, con Service.Operation/Repository.Transaction condivise,
senza operazioni annidate. Non sono autorizzate bonifiche automatiche dei movimenti esistenti.
La conferma valuta i valori prima di applicare le scritture del gruppo.

- `POST /Finance/BackEnd/Trasferimento`: OrigineName, DestinazioneName, Amount, Date,
  Description, IsConfirmed (default false). Restituisce `201` con i due Movimenti, origine prima.
- `GET /Finance/BackEnd/GruppoMovimenti/{id}`: membri attuali, dati e formule per la modifica assistita.
- `PATCH /Finance/BackEnd/GruppoMovimenti/{id}`: Items con Id e Changes (campi del normale
  UpdateMovimentoRequest). Modifica solo i membri selezionati, atomicamente. IsConfirmed non è
  ammesso qui: la conferma collettiva usa il comando dedicato. Restituisce il gruppo aggiornato.
- `DELETE /Finance/BackEnd/GruppoMovimenti/{id}`: body Ids, selezione non vuota e senza duplicati;
  tutti gli identificativi devono appartenere al gruppo. Risposta `204`.
- `POST /Finance/BackEnd/Movimento/Consolida`: il comando esistente espande la selezione ai gruppi,
  deduplica i membri e restituisce il numero effettivo di movimenti confermati.
- `PATCH /Finance/BackEnd/Movimento/{id}` con IsConfirmed=true applica la medesima conferma del gruppo.
  La normale DELETE singola mantiene la pulizia del gruppo superstite.

Le risposte configurazione e timeline dei Movimenti espongono GruppoMovimentiId opzionale.
Il normale PATCH singolo resta il primo salvataggio: il client consulta successivamente il gruppo
e invia il secondo PATCH con i valori esplicitamente scelti, senza uguaglianze imposte dal server.
Non viene introdotto un endpoint generico per raggruppare retroattivamente movimenti esistenti.

Errori: `400` input/selezione non validi, `404` risorsa mancante, `422` errore di valutazione durante
la conferma. Autenticazione Finance esistente obbligatoria. Nessuna scrittura nelle GET.
Il segno è ricavato dal profilo carta a saldo/revolving alla data richiesta, usando i parametri
esistenti (nessun parametro di abilitazione nuovo); in assenza di profilo il conto ha semantica ordinaria.

Migrazione AddGruppiMovimenti: tabella GruppiMovimenti e FK nullable su Movimenti con indice e
SetNull alla rimozione del gruppo. Nessun raggruppamento o modifica automatica dei dati preesistenti.

## Verifiche

Verificati localmente 241 test API e 5 test DataModel, compresi segni, rollback, conferma futura,
eliminazione parziale e persistenza della modifica iniziale quando fallisce la seconda fase.
Restano da realizzare e collaudare i flussi GUI sotto elencati.

Verificare: segni nelle quattro combinazioni, conti uguali e importi non positivi rifiutati;
rollback di creazione/conferma/seconda fase modifica/eliminazione; conferma di membri futuri;
date preservate; rimozione legami; modifica indipendente e annullamento seconda fase;
eliminazione parziale con due, uno o zero superstiti; accessi GUI e default;
selezione testo e separatore del tastierino. Nessun deploy o import è implicato da questa specifica.
