namespace Skating.Core
{
    /// <summary>Stato della preparazione e blocchi confermati, senza implicare chiusura delle gare.</summary>
    public sealed record PerformanceSchedule(bool IsStarted, IReadOnlyList<Performance> Performances, IReadOnlyList<int> ConfirmedBlocks);
}
