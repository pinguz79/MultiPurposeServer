namespace Skating.Core
{
    /// <summary>Gare derivate dalle iscrizioni e configurazione della giuria prima delle votazioni.</summary>
    public interface IRaceStore
    {
        /// <summary>Legge la classifica corrente senza predisporre schede o modificare presenze.</summary>
        RaceRanking GetRanking(RaceKey key);

        /// <summary>Chiude una gara con tutte le schede gestite e podio senza parità.</summary>
        void Close(RaceKey key);

        /// <summary>Rende nuovamente provvisoria la classifica senza riaprire automaticamente le schede.</summary>
        void Reopen(RaceKey key);

        /// <summary>Cambia o revoca l'esito, conservando i voti; richiede gara aperta e riapre la scheda alla revoca.</summary>
        void SetStatus(Guid choreographyId, PerformanceStatus status);

        /// <summary>Elenca solo gare con coreografie. La lettura non modifica l'archivio.</summary>
        IReadOnlyList<Race> GetRaces();

        /// <summary>Salva una giuria esplicita di almeno tre giudici distinti esistenti, senza modificare le altre gare.</summary>
        void SetJury(RaceKey key, IReadOnlyCollection<Guid> judgeIds);

        /// <summary>Ripristina la giuria predefinita con tutti i giudici, compresi quelli aggiunti in seguito.</summary>
        void UseDefaultJury(RaceKey key);
    }
}
