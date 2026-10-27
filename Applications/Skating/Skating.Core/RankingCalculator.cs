namespace Skating.Core
{
    /// <summary>Somma i voti completi; uniforma i giudici solo in presenza di numerosità differenti.</summary>
    public static class RankingCalculator
    {
        /// <summary>Calcola posizioni competitive, conservando incomplete ed escluse fuori dai piazzamenti.</summary>
        public static RaceRanking Calculate(RaceKey key, bool isClosed, IReadOnlyList<RankingEntry> entries) => new(key, isClosed, CalculateEntries(entries));

        /// <summary>Ricalcola il confronto autonomamente dai voti originali, anche per premi derivati.</summary>
        public static IReadOnlyList<RankingEntry> CalculateEntries(IReadOnlyList<RankingEntry> entries)
        {
            var complete = entries.Where(entry => entry.Status == PerformanceStatus.Active && entry.IsComplete).ToList();
            var juries = complete.Select(entry => entry.Scores.Select(score => score.JudgeId).ToHashSet()).ToList();
            var unequal = juries.Select(jury => jury.Count).Distinct().Count() > 1;
            var common = juries.Count == 0 ? [] : juries[0].ToHashSet();
            foreach (var jury in juries.Skip(1))
            {
                common.IntersectWith(jury);
            }

            var ranked = complete.Select(entry => entry with
            {
                TotalTenths = entry.Scores.Where(score => !unequal || common.Contains(score.JudgeId)).Sum(score => score.Tenths),
                ExcludedJudgeIds = unequal ? [.. entry.Scores.Select(score => score.JudgeId).Distinct().Where(id => !common.Contains(id))] : [],
            }).OrderByDescending(entry => entry.TotalTenths).ThenBy(entry => entry.Order).ToList();
            var results = ranked.Select(entry => entry with
            {
                Position = ranked.Count(other => other.TotalTenths > entry.TotalTenths) + 1,
                IsTied = ranked.Count(other => other.TotalTenths == entry.TotalTenths) > 1,
            });
            return [.. results, .. entries.Where(entry => entry.Status != PerformanceStatus.Active || !entry.IsComplete)
                .OrderBy(entry => entry.Status != PerformanceStatus.Active).ThenBy(entry => entry.Order)];
        }
    }
}
