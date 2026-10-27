namespace Skating.Core
{
    /// <summary>Classifica consultabile anche prima della preparazione delle schede.</summary>
    public sealed record RaceRanking(RaceKey Key, bool IsClosed, IReadOnlyList<RankingEntry> Entries)
    {
        /// <summary>Parità che coinvolge almeno una posizione del podio.</summary>
        public bool HasPodiumTies => Entries.Any(entry => entry.IsTied && entry.Position <= 3);

        /// <summary>Tutte le schede sono confermate o escluse, senza parità sul podio.</summary>
        public bool CanClose => !IsClosed && Entries.Count > 0 && !HasPodiumTies && Entries.All(entry => entry.Status != PerformanceStatus.Active || entry.IsComplete && entry.IsConfirmed);
    }
}
