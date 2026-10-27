namespace Skating.Core
{
    /// <summary>Snapshot della gara e della giuria prevista; non rappresenta stato di votazione o presenze.</summary>
    public sealed record Race(RaceKey Key, IReadOnlyList<Guid> ChoreographyIds, IReadOnlyList<Guid> JudgeIds, bool HasCustomJury);
}
