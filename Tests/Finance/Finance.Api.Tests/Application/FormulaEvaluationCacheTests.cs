using System.Diagnostics;

using Finance.Api.Application;
using Finance.Api.Infrastructure.Caching;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

using Xunit.Abstractions;

namespace Finance.Api.Tests.Application
{
    public class FormulaEvaluationCacheTests(ITestOutputHelper output)
    {
        [Theory]
        [InlineData(12, 100)]
        [InlineData(120, 100)]
        [InlineData(120, 0)]
        public async Task Evaluate_WhenManyMonthlyResets_ReadsEachClosingHistoryOnce(int months, decimal amount)
        {
            // Arrange
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloCard", DisplayName = "Hello Card" };
            var start = new DateOnly(2026, 1, 1);
            var movements = new List<Movimento>();
            for (var index = 0; index < months; index++)
            {
                DateOnly month = start.AddMonths(index);
                if (index > 0)
                {
                    movements.Add(new Movimento { Id = Guid.NewGuid(), ContoId = conto.Id, Date = month.AddDays(5), Formula = "-[HelloCard.SaldoUltimoCicloChiuso]" });
                }

                movements.Add(new Movimento { Id = Guid.NewGuid(), ContoId = conto.Id, Date = month.AddDays(9), Formula = amount == 0m ? "0.00" : "100.00" });
            }

            var conti = new Mock<IContoRepository>();
            conti.Setup(repository => repository.GetByName(conto.Name)).ReturnsAsync(conto);
            var parametri = new Mock<IParametroContoRepository>();
            parametri.Setup(repository => repository.GetByName(conto.Id, "ChiusuraCiclo"))
                .ReturnsAsync([new ParametroConto { Name = "ChiusuraCiclo", Value = 21m }]);
            var movimenti = new Mock<IMovimentoRepository>();
            var reads = new List<DateOnly>();
            movimenti.Setup(repository => repository.GetByContoThrough(conto.Id, It.IsAny<DateOnly>()))
                .ReturnsAsync((Guid id, DateOnly date) =>
                {
                    reads.Add(date);
                    // Il limite fa fallire subito la regressione senza attendere un calcolo esponenziale.
                    if (reads.Count > months)
                    {
                        throw new InvalidOperationException("Storico di chiusura riletto inutilmente.");
                    }

                    return (IReadOnlyList<Movimento>)[.. movements.Where(movement => movement.Date <= date)];
                });
            var resolver = new Mock<IFormulaResolver>();
            resolver.Setup(service => service.ResolveCanonicalName(It.IsAny<string>())).ReturnsAsync((string name) => name);
            var evaluator = new FormulaEvaluator(resolver.Object, conti.Object, movimenti.Object, parametri.Object);

            // Act
            var elapsed = Stopwatch.StartNew();
            FormulaEvaluationResult result = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", start.AddMonths(months - 1).AddDays(21));
            FormulaEvaluationResult repeated = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", start.AddMonths(months - 1).AddDays(22));

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(amount);
            repeated.Should().BeEquivalentTo(result);
            reads.Should().HaveCount(months).And.OnlyHaveUniqueItems();
            output.WriteLine($"{months} cicli, {reads.Count} letture dello storico, {elapsed.ElapsedMilliseconds} ms (repository in memoria).");
        }

        [Fact]
        public async Task Evaluate_WhenCircularReference_ReturnsErrorOnEveryAttempt()
        {
            // Arrange
            var conto = new Conto { Id = Guid.NewGuid(), Name = "HelloCard" };
            var date = new DateOnly(2026, 1, 21);
            var conti = new Mock<IContoRepository>();
            conti.Setup(repository => repository.GetByName(conto.Name)).ReturnsAsync(conto);
            var parametri = new Mock<IParametroContoRepository>();
            parametri.Setup(repository => repository.GetByName(conto.Id, "ChiusuraCiclo"))
                .ReturnsAsync([new ParametroConto { Name = "ChiusuraCiclo", Value = 21m }]);
            var movimenti = new Mock<IMovimentoRepository>();
            movimenti.Setup(repository => repository.GetByContoThrough(conto.Id, date))
                .ReturnsAsync([new Movimento { Id = Guid.NewGuid(), ContoId = conto.Id, Date = date, Formula = "[HelloCard.SaldoUltimoCicloChiuso]" }]);
            var resolver = new Mock<IFormulaResolver>();
            resolver.Setup(service => service.ResolveCanonicalName(It.IsAny<string>())).ReturnsAsync((string name) => name);
            var evaluator = new FormulaEvaluator(resolver.Object, conti.Object, movimenti.Object, parametri.Object);
            await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date);

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date);

            // Assert
            result.Value.Should().BeNull();
            result.Error.Should().Contain("Circular formula reference");
        }

        [Fact]
        public async Task Evaluate_WhenSameFormulaAndDate_ReusesResolvedResult()
        {
            // Arrange
            var resolver = new Mock<IFormulaResolver>();
            resolver.Setup(service => service.ResolveCanonicalName("Affitto")).ReturnsAsync("Affitto");
            resolver.Setup(service => service.Resolve(It.IsAny<IReadOnlyList<string>>(), It.IsAny<DateOnly>()))
                .ReturnsAsync([new ResolvedFormulaParameter("Affitto", 400m, false)]);
            var evaluator = new FormulaEvaluator(resolver.Object);
            var date = new DateOnly(2026, 9, 7);
            await evaluator.Evaluate("[Affitto]", date);

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[Affitto]", date);

            // Assert
            result.Value.Should().Be(400m);
            resolver.Verify(service => service.Resolve(It.IsAny<IReadOnlyList<string>>(), date), Times.Once);
        }

        [Fact]
        public async Task Evaluate_WhenPreviousResultFailed_RetriesResolution()
        {
            // Arrange
            var resolver = new Mock<IFormulaResolver>();
            resolver.Setup(service => service.ResolveCanonicalName("Affitto")).ReturnsAsync("Affitto");
            resolver.SetupSequence(service => service.Resolve(It.IsAny<IReadOnlyList<string>>(), It.IsAny<DateOnly>()))
                .ThrowsAsync(new InvalidOperationException("Valore non disponibile."))
                .ReturnsAsync([new ResolvedFormulaParameter("Affitto", 400m, false)]);
            var evaluator = new FormulaEvaluator(resolver.Object);
            var date = new DateOnly(2026, 9, 7);
            await evaluator.Evaluate("[Affitto]", date);

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[Affitto]", date);

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(400m);
        }

        [Fact]
        public async Task Evaluate_WhenDifferentRequest_DoesNotReusePreviousResult()
        {
            // Arrange
            var resolver = new Mock<IFormulaResolver>();
            resolver.Setup(service => service.ResolveCanonicalName("Affitto")).ReturnsAsync("Affitto");
            resolver.SetupSequence(service => service.Resolve(It.IsAny<IReadOnlyList<string>>(), It.IsAny<DateOnly>()))
                .ReturnsAsync([new ResolvedFormulaParameter("Affitto", 400m, false)])
                .ReturnsAsync([new ResolvedFormulaParameter("Affitto", 500m, false)]);
            var firstRequest = new FormulaEvaluator(resolver.Object, new FormulaEvaluationCache());
            var nextRequest = new FormulaEvaluator(resolver.Object, new FormulaEvaluationCache());
            var date = new DateOnly(2026, 9, 7);
            await firstRequest.Evaluate("[Affitto]", date);

            // Act
            FormulaEvaluationResult result = await nextRequest.Evaluate("[Affitto]", date);

            // Assert
            result.Value.Should().Be(500m);
        }
    }
}
