namespace Skating.Core
{
    /// <summary>Voto valido espresso in decimi interi, per evitare arrotondamenti.</summary>
    public sealed record Score(Guid ChoreographyId, Guid JudgeId, Guid CriterionId, int Tenths);
}
