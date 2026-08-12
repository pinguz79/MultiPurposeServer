using System.Collections;

namespace MultiPurposeServer.Shared.Utils.Normalization.Rules
{
    internal sealed class CollectionNormalizationRule(Func<object, IEnumerable?> getter) : NormalizationRule
    {
        public override void Execute(object instance)
        {
            IEnumerable? collection = getter(instance);

            if (collection is null)
            {
                return;
            }

            foreach (object? item in collection)
            {
                if (item is not null)
                {
                    Normalizer.Normalize(item);
                }
            }
        }
    }
}
