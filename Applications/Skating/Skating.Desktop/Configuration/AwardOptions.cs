using System.Text.Json;
using System.Text.Json.Serialization;

using Skating.Core;

namespace Skating.Desktop.Configuration
{
    internal sealed record AwardOptions(AwardAggregation? IntermediateAwardAggregation)
    {
        internal static AwardOptions Load(string path)
        {
            var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter<AwardAggregation>(allowIntegerValues: false) } };
            return JsonSerializer.Deserialize<AwardOptions>(File.ReadAllText(path), options) ?? throw new InvalidDataException("Configurazione Skating vuota.");
        }
    }
}
