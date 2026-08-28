using FluentAssertions;
using NCalc;

namespace Finance.ExpressionEngineSpike;

public sealed class NCalcSpikeTests
{
    [Fact]
    public void Evaluate_WhenFormulaUsesFinanceVocabulary_ReturnsExpectedValues()
    {
        // Arrange
        var context = CreateContext();
        var cases = CreateCases();

        // Act
        var results = cases.Select(item => Evaluate(item.Formula, context)).ToList();

        // Assert
        results.Should().Equal(cases.Select(item => item.Expected));
    }

    [Fact]
    public void GetDependencies_WhenReferencesDifferOnlyByCase_ReturnsDistinctReferences()
    {
        // Arrange
        const string formula = "[Amex.Saldo] + [amex.saldo] + [affitto]";

        // Act
        var dependencies = FinanceFormulaTranslator.GetDependencies(formula);

        // Assert
        dependencies.Should().Equal("Amex.Saldo", "affitto");
    }

    [Fact]
    public void Evaluate_WhenIntermediateResultsHaveHalfCent_DoesNotApplyFinanceRoundingPolicy()
    {
        // Arrange
        var context = new FormulaContext
        {
            ["primoImporto"] = 1.01m,
            ["secondoImporto"] = 1.01m,
        };

        // Act
        var result = Evaluate(FinanceFormulaSamples.ArrotondamentoIntermedio, context);

        // Assert
        result.Should().Be(1.01m);
        result.Should().NotBe(1.02m, "Finance arrotonda ogni divisione a 0,51 prima della somma");
    }

    private static decimal Evaluate(string formula, FormulaContext context)
    {
        var expression = new Expression(FinanceFormulaTranslator.ForNCalc(formula));

        foreach (var dependency in FinanceFormulaTranslator.GetDependencies(formula))
            expression.Parameters[dependency] = context[dependency];

        return Convert.ToDecimal(expression.Evaluate());
    }

    private static FormulaContext CreateContext() => new()
    {
        ["affitto"] = 650.00m,
        ["cartaPrincipale.spesoCicloPrecedente"] = 425.75m,
        ["amex.saldoCicloPrecedente"] = 1245.65m,
        ["amex.rataMinima"] = 150.00m,
        ["amex.plafond"] = 5000.00m,
        ["amex.spesoTotale"] = 5400.00m,
        ["amex.tassoInteresse"] = 0.0125m,
        ["agos.rata"] = 200.00m,
        ["agos.debitoResiduo"] = 125.00m,
        ["agos.saldoCicloPrecedente"] = 2000.00m,
        ["agos.tassoInteresse"] = 0.018m,
    };

    private static IReadOnlyList<(string Formula, decimal Expected)> CreateCases() =>
    [
        (FinanceFormulaSamples.VoceRicorrente, 650.00m),
        (FinanceFormulaSamples.CartaPrincipale, -425.75m),
        (FinanceFormulaSamples.RataAmex, -150.00m),
        (FinanceFormulaSamples.RataAgos, -125.00m),
        (FinanceFormulaSamples.ScopertoAmex, -405.000000m),
        (FinanceFormulaSamples.InteressiAmex, 15.570625m),
        (FinanceFormulaSamples.InteressiAgos, 36.00000m),
    ];
}
