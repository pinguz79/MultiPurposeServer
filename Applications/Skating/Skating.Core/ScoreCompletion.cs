namespace Skating.Core
{
    /// <summary>Completezza rispetto ai giudici richiesti e a tutti i criteri; zero è un voto presente.</summary>
    public static class ScoreCompletion
    {
        public static bool IsComplete(Guid choreographyId, ScorePage page, IReadOnlyCollection<Guid> criteria)
        {
            var judges = page.Judges.Where(judge => judge.ChoreographyId == choreographyId && judge.IsRequired).ToList();
            return criteria.Count > 0 && judges.Count > 0 && judges.All(judge => criteria.All(criterion => page.Scores.Any(score =>
                score.ChoreographyId == choreographyId && score.JudgeId == judge.JudgeId && score.CriterionId == criterion)));
        }
    }
}
