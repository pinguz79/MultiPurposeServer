namespace Skating.Core
{
    /// <summary>Colore nominato della palette, memorizzato per nome negli archivi.</summary>
    public sealed record RaceColor(string Name, int Rgb)
    {
        /// <summary>Palette chiara per testo scuro; i nomi sono identificatori persistenti.</summary>
        public static IReadOnlyList<RaceColor> Palette { get; } = Array.AsReadOnly<RaceColor>([
            new("Rosa", 0xF4C6D4), new("Pesca", 0xF5C8AA), new("Albicocca", 0xF2D19C), new("Crema", 0xF3E4B2),
            new("Limone", 0xECECA8), new("Lime", 0xDBEBAA), new("Pistacchio", 0xC8DFA6), new("Salvia", 0xBBD1B6),
            new("Menta", 0xB5E3CF), new("Acquamarina", 0xAEE0DD), new("Ghiaccio", 0xB5DDEB), new("Cielo", 0xAFCCEE),
            new("Pervinca", 0xBEC7EC), new("Lavanda", 0xD1C3EA), new("Lilla", 0xDFC1E7), new("Glicine", 0xCFACD3),
            new("Confetto", 0xEDC5E1), new("Corallo", 0xEDAFA7), new("Sabbia", 0xDCC6A6), new("Tortora", 0xD0C5BD),
            new("Argento", 0xCED3DB), new("Azzurro polvere", 0xB5C9D6), new("Verde polvere", 0xB8CCC7), new("Malva", 0xD7B9C5),
        ]);
    }
}
