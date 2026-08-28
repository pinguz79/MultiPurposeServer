namespace Finance.ExpressionEngineSpike;

internal static class FinanceFormulaSamples
{
    public const string VoceRicorrente = "[affitto]";
    public const string CartaPrincipale = "-[cartaPrincipale.spesoCicloPrecedente]";
    public const string RataAmex = "-Max([amex.saldoCicloPrecedente] / 10, [amex.rataMinima])";
    public const string RataAgos = "-Min([agos.rata], [agos.debitoResiduo])";
    public const string ScopertoAmex = "([amex.plafond] - [amex.spesoTotale]) * (1 + [amex.tassoInteresse])";
    public const string InteressiAmex = "[amex.saldoCicloPrecedente] * [amex.tassoInteresse]";
    public const string InteressiAgos = "[agos.saldoCicloPrecedente] * [agos.tassoInteresse]";
    public const string ArrotondamentoIntermedio = "[primoImporto] / 2 + [secondoImporto] / 2";
}
