namespace Portfolio.ProductionTests.Models
{
    internal sealed class NavigationRun(string phase)
    {
        private readonly List<NavigationFailure> _failures = [];

        public string Phase { get; } = phase;
        public IReadOnlyList<NavigationFailure> Failures => _failures;

        public void AddFailure(string source, string target, string message) => _failures.Add(new NavigationFailure(source, target, message));

        public string Format() => _failures.Count == 0 ? $"{Phase}: all discovered API endpoints and Web pages are reachable." : $"{Phase}: {_failures.Count} failure(s).{Environment.NewLine}" + string.Join(Environment.NewLine, _failures.Select(failure => $"- [{failure.Source}] {failure.Target}: {failure.Message}"));
    }
}
