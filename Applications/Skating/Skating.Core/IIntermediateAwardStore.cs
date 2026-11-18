namespace Skating.Core
{
    /// <summary>Configurazione e convalida locale dei premi intermedi.</summary>
    public interface IIntermediateAwardStore
    {
        /// <summary>Legge il criterio associato all'archivio; null per archivi non ancora configurati.</summary>
        AwardAggregation? GetAggregation();

        /// <summary>Indica se il cambio criterio richiede la revoca di convalide intermedie o dell'assoluto Open.</summary>
        bool HasValidatedAwards();

        /// <summary>Cambia criterio senza alterare voti o gare; la revoca di convalide esistenti richiede consenso esplicito.</summary>
        void Configure(AwardAggregation aggregation, bool revokeValidations = false);

        /// <summary>Ricalcola i premi non PRO senza scritture; rifiuta un criterio diverso da quello memorizzato.</summary>
        IReadOnlyList<IntermediateAward> GetAwards(AwardAggregation aggregation);

        /// <summary>Verifica fonti e podio nella transazione che convalida il premio.</summary>
        void Validate(AwardAggregation aggregation, Guid groupId);

        /// <summary>Revoca la convalida senza modificare le gare o i voti originali.</summary>
        void Reopen(AwardAggregation aggregation, Guid groupId);
    }
}
