using MultiPurposeServer.Shared.Utils.Normalization.Rules;

namespace MultiPurposeServer.Shared.Utils.Normalization
{
    internal sealed class NormalizationPlan(IReadOnlyList<NormalizationRule> rules)
    {
        public void Execute(object instance)
        {
            foreach (NormalizationRule rule in rules)
                rule.Execute(instance);
        }
    }
}