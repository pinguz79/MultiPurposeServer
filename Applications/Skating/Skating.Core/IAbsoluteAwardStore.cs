namespace Skating.Core
{
    /// <summary>Assolute separate, ricalcolate dai voti ufficiali senza duplicazioni.</summary>
    public interface IAbsoluteAwardStore
    {
        /// <summary>Legge senza scritture. Il criterio intermedio serve solo al percorso non PRO.</summary>
        AbsoluteAward GetAward(AbsoluteAwardKind kind, AwardAggregation? aggregation = null);

        /// <summary>Convalida nella stessa transazione che verifica fonti e parità.</summary>
        void Validate(AbsoluteAwardKind kind, AwardAggregation? aggregation = null);

        /// <summary>Revoca solo la convalida dell'assoluta, lasciando intatte le fonti.</summary>
        void Reopen(AbsoluteAwardKind kind);
    }
}
