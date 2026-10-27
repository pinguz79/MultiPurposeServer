using System.Text.Json;
using System.Text.Json.Nodes;
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

        internal void Save(string path)
        {
            if (IntermediateAwardAggregation is AwardAggregation aggregation && !Enum.IsDefined(aggregation))
            {
                throw new ArgumentOutOfRangeException(nameof(IntermediateAwardAggregation));
            }

            var document = File.Exists(path) ? JsonNode.Parse(File.ReadAllText(path)) as JsonObject
                ?? throw new InvalidDataException("La configurazione deve essere un oggetto JSON.") : new JsonObject();
            document[nameof(IntermediateAwardAggregation)] = IntermediateAwardAggregation?.ToString();
            var temporary = $"{path}.{Guid.NewGuid():N}.tmp";
            try
            {
                File.WriteAllText(temporary, document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
                if (File.Exists(path))
                {
                    File.Replace(temporary, path, null);
                }
                else
                {
                    File.Move(temporary, path);
                }
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        }
    }
}
