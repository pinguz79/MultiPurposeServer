namespace Skating.Core
{
    /// <summary>Posizione operativa distinta da numero e blocco della scheda voti.</summary>
    public sealed record Performance(Guid ChoreographyId, int Number, int Block, int Order, bool IsConfirmed,
        PerformanceStatus Status = PerformanceStatus.Active, bool IsRaceClosed = false);
}
