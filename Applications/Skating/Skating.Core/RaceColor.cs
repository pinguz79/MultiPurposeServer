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
            new("Rosa tenue", 0xF8DAE3), new("Rosa chiarissimo", 0xFBEBF0),
            new("Pesca tenue", 0xF9DBC8), new("Pesca chiarissimo", 0xFCECE1),
            new("Albicocca tenue", 0xF7E1BF), new("Albicocca chiarissimo", 0xFAEFDC),
            new("Crema tenue", 0xF7EDCD), new("Crema chiarissimo", 0xFBF6E4),
            new("Limone tenue", 0xF3F3C6), new("Limone chiarissimo", 0xF8F8E1),
            new("Lime tenue", 0xE8F2C8), new("Lime chiarissimo", 0xF2F8E1),
            new("Pistacchio tenue", 0xDBEAC5), new("Pistacchio chiarissimo", 0xECF4E0),
            new("Salvia tenue", 0xD3E1D0), new("Salvia chiarissimo", 0xE7EFE5),
            new("Menta tenue", 0xCFEDE0), new("Menta chiarissimo", 0xE5F5EE),
            new("Acquamarina tenue", 0xCAEBE9), new("Acquamarina chiarissimo", 0xE3F4F3),
            new("Ghiaccio tenue", 0xCFE9F2), new("Ghiaccio chiarissimo", 0xE5F3F8),
            new("Cielo tenue", 0xCBDEF4), new("Cielo chiarissimo", 0xE3EDF9),
            new("Pervinca tenue", 0xD5DBF3), new("Pervinca chiarissimo", 0xE8EBF8),
            new("Lavanda tenue", 0xE1D8F1), new("Lavanda chiarissimo", 0xEFEAF8),
            new("Lilla tenue", 0xEAD7EF), new("Lilla chiarissimo", 0xF4E9F7),
            new("Glicine tenue", 0xE0C9E2), new("Glicine chiarissimo", 0xEEE2F0),
            new("Confetto tenue", 0xF3D9EC), new("Confetto chiarissimo", 0xF9EBF5),
            new("Corallo tenue", 0xF3CBC6), new("Corallo chiarissimo", 0xF9E3E0),
            new("Sabbia tenue", 0xE8DAC5), new("Sabbia chiarissimo", 0xF3EBE0),
            new("Tortora tenue", 0xE0D9D4), new("Tortora chiarissimo", 0xEFEBE8),
            new("Argento tenue", 0xDFE2E8), new("Argento chiarissimo", 0xEEF0F2),
            new("Azzurro polvere tenue", 0xCFDCE4), new("Azzurro polvere chiarissimo", 0xE5ECF1),
            new("Verde polvere tenue", 0xD1DEDB), new("Verde polvere chiarissimo", 0xE6EDEB),
            new("Malva tenue", 0xE5D2D9), new("Malva chiarissimo", 0xF1E7EB),
        ]);
    }
}
