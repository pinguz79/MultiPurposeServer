namespace Skating.Core
{
    /// <summary>Premio derivato dai vincitori, senza copie dei voti ufficiali.</summary>
    public sealed record IntermediateAward(Guid GroupId, string Name, AwardAggregation Aggregation, bool IsValidated,
        IReadOnlyList<RaceRanking> Sources, IReadOnlyList<RankingEntry> Entries)
    {
        /// <summary>Tutte le gare di origine sono definitive.</summary>
        public bool AreSourcesClosed => Sources.Count > 0 && Sources.All(source => source.IsClosed);

        /// <summary>Le parità sul podio richiedono una rettifica umana dei voti originali.</summary>
        public bool HasPodiumTies => Entries.Any(entry => entry.IsTied && entry.Position <= 3);

        /// <summary>Convalida esplicita consentita solo con partecipanti e fonti definitive.</summary>
        public bool CanValidate => !IsValidated && AreSourcesClosed && Entries.Count > 0 && !HasPodiumTies;
    }
}
