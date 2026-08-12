namespace Portfolio.ProductionTests
{
    internal sealed class ProductionFactAttribute : FactAttribute
    {
        private const string EnabledVariable = "PORTFOLIO_RUN_CACHE_REGENERATION_TESTS";

        public ProductionFactAttribute()
        {
            if (!string.Equals(Environment.GetEnvironmentVariable(EnabledVariable), "true", StringComparison.OrdinalIgnoreCase))
            {
                Skip = $"Set {EnabledVariable}=true to run tests that operate on the production cache.";
            }
        }
    }
}
