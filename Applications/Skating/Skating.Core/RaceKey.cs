namespace Skating.Core
{
    /// <summary>Identità della gara derivata dalla combinazione iscritta, indipendente dai nomi dei cataloghi.</summary>
    public sealed record RaceKey(Guid DisciplineId, Guid CategoryId, Guid LevelId);
}
