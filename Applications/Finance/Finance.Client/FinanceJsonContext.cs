using System.Text.Json.Serialization;

namespace Finance.Client
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(List<Conto>))]
    internal partial class FinanceJsonContext : JsonSerializerContext
    {
    }
}
