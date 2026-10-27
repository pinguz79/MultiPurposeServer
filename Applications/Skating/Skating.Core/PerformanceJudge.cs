namespace Skating.Core
{
    /// <summary>Presenza richiesta sulla singola esibizione, inizializzata una sola volta dal blocco.</summary>
    public sealed record PerformanceJudge(Guid ChoreographyId, Guid JudgeId, bool IsRequired);
}
