using System.Collections;

namespace MultiPurposeServer.Shared.Utils.Validation
{
    internal static class ValidationValue
    {
        public static bool IsMissing(object? value) => value switch
        {
            null => true,
            string text => string.IsNullOrWhiteSpace(text),
            ICollection collection => collection.Count == 0,
            IEnumerable enumerable => IsEmpty(enumerable),
            _ => false
        };

        private static bool IsEmpty(IEnumerable enumerable)
        {
            IEnumerator enumerator = enumerable.GetEnumerator();

            try
            {
                return !enumerator.MoveNext();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
    }
}