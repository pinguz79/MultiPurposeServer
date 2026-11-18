# Skating System — aggiornamento feedback cliente del 17 novembre 2026

- Palette ampliata a 72 colori, mantenendo univocità e leggibilità del testo.
- Eliminazione con conferma di scuole/atleti non utilizzati e coreografie prima dell'avvio,
  conservando le anagrafiche collegate e aggiornando la scaletta.
- Correzione dei nuovi blocchi da una sola coreografia: capienza salvata e riuso dell'ultimo
  blocco libero fino a esaurimento, senza alterare quelli confermati o già utilizzati.
- Rigenerazione prima dell'avvio con avviso sulle assegnazioni manuali; ordine conservato.
- Criterio dei premi per disciplina/categoria salvato per manifestazione e modificabile
  senza riavvio; eventuali convalide intermedie e Open revocate solo previo consenso.
- Voti ufficiali, gare e percorso PRO invariati. Nessun ripescaggio per completare il podio.

Formato archivio v10, con aggiornamento e scelta di backup. Gli archivi precedenti mantengono
i blocchi esistenti e ricevono capienza predefinita 20 (prima non veniva memorizzata).
Nuovi filtri/ordinamenti avanzati esclusi: valutazione separata in un branch di prova.
Nessuna nuova dipendenza runtime di terze parti o modifica al server MPS.
