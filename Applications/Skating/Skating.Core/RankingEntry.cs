namespace Skating.Core
{
    /// <summary>Risultato con voti originali e giudici esclusi soltanto da questo confronto.</summary>
    public sealed record RankingEntry(Guid ChoreographyId, int Order, int? Number, int? Block, bool IsConfirmed, PerformanceStatus Status,
        bool IsComplete, int? Position, int? TotalTenths, bool IsTied, IReadOnlyList<Score> Scores, IReadOnlyList<Guid> ExcludedJudgeIds);
}
