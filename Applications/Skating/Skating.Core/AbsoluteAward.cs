namespace Skating.Core
{
    /// <summary>Classifica assoluta con collegamenti alle fonti e ai voti originali.</summary>
    public sealed record AbsoluteAward(AbsoluteAwardKind Kind, bool IsValidated, IReadOnlyList<IntermediateAward> IntermediateSources,
        IReadOnlyList<RaceRanking> RaceSources, IReadOnlyList<RankingEntry> Entries)
    {
        /// <summary>Fonti da convalidare: premi intermedi per non PRO, gare per PRO.</summary>
        public int SourceCount => Kind == AbsoluteAwardKind.Pro ? RaceSources.Count : IntermediateSources.Count;

        /// <summary>Numero di fonti definitive.</summary>
        public int ValidatedSourceCount => Kind == AbsoluteAwardKind.Pro ? RaceSources.Count(source => source.IsClosed) : IntermediateSources.Count(source => source.IsValidated);

        /// <summary>Tutte le fonti del percorso sono definitive.</summary>
        public bool AreSourcesValidated => SourceCount > 0 && SourceCount == ValidatedSourceCount;

        /// <summary>Parità che coinvolgono primo, secondo o terzo posto.</summary>
        public bool HasPodiumTies => Entries.Any(entry => entry.IsTied && entry.Position <= 3);

        /// <summary>Convalida esplicita dopo la verifica delle fonti e del podio.</summary>
        public bool CanValidate => !IsValidated && AreSourcesValidated && Entries.Count > 0 && !HasPodiumTies;
    }
}
