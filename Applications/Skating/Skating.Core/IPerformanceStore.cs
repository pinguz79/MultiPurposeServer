namespace Skating.Core
{
    /// <summary>Preparazione dei blocchi e trascrizione locale dei voti, distinta dalle classifiche.</summary>
    public interface IPerformanceStore
    {
        /// <summary>Legge ordine operativo, numeri stabili e blocchi confermati senza modificare l'archivio.</summary>
        PerformanceSchedule GetSchedule();

        /// <summary>Prepara blocchi di dimensione iniziale uniforme; rifiutato dopo la conferma di un blocco.</summary>
        void Prepare(int blockSize);

        /// <summary>Sposta un'esibizione fra blocchi non ancora confermati.</summary>
        void AssignBlock(Guid choreographyId, int block);

        /// <summary>Sposta di una posizione l'ordine operativo (-1 o +1), conservando numero e blocco.</summary>
        void Move(Guid choreographyId, int direction);

        /// <summary>Conferma i blocchi attuali, anche supplementari, rendendone stabile la composizione.</summary>
        void ConfirmBlocks();

        /// <summary>Avvia esplicitamente la manifestazione dopo la verifica di blocchi, giurie e criteri.</summary>
        void Start();

        /// <summary>Alla prima apertura acquisisce le presenze; le successive conservano la stessa scheda.</summary>
        ScorePage OpenPage(int block);

        /// <summary>Salva un voto in decimi 0-100; null svuota. Un voto forzato rende obbligatori tutti i criteri del giudice.</summary>
        void SaveScore(Guid choreographyId, Guid judgeId, Guid criterionId, int? tenths, bool allowAbsent = false);

        /// <summary>Modifica la presenza sulla riga aperta; l'assenza richiede tutti i voti del giudice vuoti.</summary>
        void SetJudgeRequired(Guid choreographyId, Guid judgeId, bool required);

        /// <summary>Conferma solo righe complete, con copia preventiva; restituisce il numero di conferme.</summary>
        int ConfirmComplete(int block, Guid? choreographyId = null);

        /// <summary>Rende nuovamente modificabile un'esibizione, conservando i voti.</summary>
        void Reopen(Guid choreographyId);
    }
}
