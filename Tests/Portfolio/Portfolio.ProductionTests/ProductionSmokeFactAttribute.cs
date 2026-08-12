namespace Portfolio.ProductionTests
{
    internal sealed class ProductionSmokeFactAttribute : FactAttribute
    {
        private const string EnabledVariable = "PORTFOLIO_RUN_PRODUCTION_SMOKE_TESTS";

        public ProductionSmokeFactAttribute()
        {
            if (!string.Equals(Environment.GetEnvironmentVariable(EnabledVariable), "true", StringComparison.OrdinalIgnoreCase))
            {
                Skip = $"Set {EnabledVariable}=true to run read-only tests against the deployed site.";
            }
        }
    }
}
