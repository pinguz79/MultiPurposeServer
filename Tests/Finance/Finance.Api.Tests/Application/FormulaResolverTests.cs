using Finance.Api.Application;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

using FluentAssertions;

using Moq;

namespace Finance.Api.Tests.Application
{
    public class FormulaResolverTests
    {
        [Fact]
        public async Task AccountParameterIsCanonicalizedAndResolvedAtRequestedDate()
        {
            // Arrange
            var contoId = Guid.NewGuid();
            var voceRepository = new Mock<IVoceRicorrenteRepository>();
            var contoRepository = new Mock<IContoRepository>();
            contoRepository.Setup(item => item.GetByName("helloCard"))
                .ReturnsAsync(new Conto { Id = contoId, Name = "HelloCard", DisplayName = "Hello Card" });
            contoRepository.Setup(item => item.GetByName("HelloCard"))
                .ReturnsAsync(new Conto { Id = contoId, Name = "HelloCard", DisplayName = "Hello Card" });
            var parametroRepository = new Mock<IParametroContoRepository>();
            parametroRepository.Setup(item => item.GetByName(contoId, "pLAFOND")).ReturnsAsync([
                new ParametroConto { Name = "Plafond", DisplayName = "Plafond", Value = 5_000m, Index = 0 },
            ]);
            parametroRepository.Setup(item => item.GetByName(contoId, "Plafond")).ReturnsAsync([
                new ParametroConto { Name = "Plafond", DisplayName = "Plafond", Value = 5_000m, Index = 0 },
            ]);
            var resolver = new FormulaResolver(voceRepository.Object, contoRepository.Object, parametroRepository.Object);
            var evaluator = new FormulaEvaluator(resolver);

            // Act
            FormulaValidationResult validation = await evaluator.Validate("[helloCard.pLAFOND]");
            FormulaEvaluationResult result = await evaluator.Evaluate(validation.Formula, new DateOnly(2026, 9, 4));

            // Assert
            validation.Formula.Should().Be("[HelloCard.Plafond]");
            result.Value.Should().Be(5_000m);
        }
    }
}
