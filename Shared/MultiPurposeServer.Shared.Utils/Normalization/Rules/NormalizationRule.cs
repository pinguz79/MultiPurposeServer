namespace MultiPurposeServer.Shared.Utils.Normalization.Rules
{
    internal abstract class NormalizationRule
    {
        public abstract void Execute(object instance);
    }
}
