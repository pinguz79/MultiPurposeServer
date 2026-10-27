using Skating.Core;

namespace Skating.Persistence
{
    internal static class AwardInvalidation
    {
        internal const string Sql = """
            UPDATE IntermediateAward SET IsValidated = 0
            WHERE ((Aggregation = 1 AND GroupId = $discipline) OR (Aggregation = 2 AND GroupId = $category))
                AND EXISTS (SELECT 1 FROM Level WHERE Id = $level AND IsPro = 0);
            UPDATE AbsoluteAward SET IsValidated = 0
            WHERE Kind = (SELECT IsPro FROM Level WHERE Id = $level);
            """;

        internal static void ForRace(PerformanceSession session, RaceKey key) => session.Execute(Sql, RankingReader.Arguments(key));
    }
}
