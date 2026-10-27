using Skating.Core;

namespace Skating.Desktop.Races
{
    internal sealed record RaceRow(RaceKey Key, string Discipline, string Category, string Level, int Choreographies, int Judges, string Jury, string Status);
}
