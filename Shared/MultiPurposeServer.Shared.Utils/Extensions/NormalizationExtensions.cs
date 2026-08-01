using MultiPurposeServer.Shared.Utils.Normalization;

namespace MultiPurposeServer.Shared.Utils.Extensions
{
    public static class NormalizationExtensions
    {
        public static void Normalize(this object instance) => Normalizer.Normalize(instance);

        public static void Normalize<T>(this IEnumerable<T> instances) where T : class => Normalizer.Normalize(instances);
    }
}