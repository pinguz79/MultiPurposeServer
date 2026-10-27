namespace Skating.Core
{
    /// <summary>Configurazione e convalida locale dei premi intermedi.</summary>
    public interface IIntermediateAwardStore
    {
        /// <summary>Legge il criterio associato all'archivio; null per archivi non ancora configurati.</summary>
        AwardAggregation? GetAggregation();

        /// <summary>Associa il criterio letto all'avvio. Dopo l'avvio non cambia una scelta già memorizzata.</summary>
        void Configure(AwardAggregation aggregation);

        /// <summary>Ricalcola i premi non PRO senza scritture; rifiuta un criterio diverso da quello memorizzato.</summary>
        IReadOnlyList<IntermediateAward> GetAwards(AwardAggregation aggregation);

        /// <summary>Verifica fonti e podio nella transazione che convalida il premio.</summary>
        void Validate(AwardAggregation aggregation, Guid groupId);

        /// <summary>Revoca la convalida senza modificare le gare o i voti originali.</summary>
        void Reopen(AwardAggregation aggregation, Guid groupId);
    }
}
