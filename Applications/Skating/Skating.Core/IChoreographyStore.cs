namespace Skating.Core
{
    /// <summary>Iscrizioni dell'archivio: salvataggio atomico della coreografia e dei suoi componenti.</summary>
    public interface IChoreographyStore
    {
        /// <summary>Restituisce le coreografie e le identità dei componenti, senza collegamenti a connessioni aperte.</summary>
        IReadOnlyList<Choreography> GetChoreographies();

        /// <summary>Inserisce un'iscrizione con riferimenti esistenti; rifiuta stessi nome, gara e insieme di atleti.</summary>
        void AddChoreography(Choreography choreography);

        /// <summary>Aggiorna l'iscrizione esistente e sostituisce atomicamente i componenti, conservando gli atleti anagrafici.</summary>
        void UpdateChoreography(Choreography choreography);
    }
}
