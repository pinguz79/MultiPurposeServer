namespace Skating.Core
{
    /// <summary>Snapshot delle presenze e dei voti di un blocco; i voti mancanti non sono zero.</summary>
    public sealed record ScorePage(IReadOnlyList<PerformanceJudge> Judges, IReadOnlyList<Score> Scores);
}
