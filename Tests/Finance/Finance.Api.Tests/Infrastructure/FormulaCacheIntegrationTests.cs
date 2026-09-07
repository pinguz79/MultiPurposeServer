using Finance.Api.Application;
using Finance.Api.Extensions;
using Finance.Api.Infrastructure.Caching;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel;
using Finance.DataModel.Models;

using FluentAssertions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Transactions;

namespace Finance.Api.Tests.Infrastructure
{
    public class FormulaCacheIntegrationTests
    {
        [Fact]
        public async Task Cache_WhenResolvingScopes_IsSharedOnlyInsideRequest()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using ServiceProvider services = CreateServices(connection);
            using IServiceScope first = services.CreateScope();
            using IServiceScope second = services.CreateScope();
            FormulaEvaluationCache cache = first.ServiceProvider.GetRequiredService<FormulaEvaluationCache>();

            // Act
            FormulaEvaluationCache sameRequest = first.ServiceProvider.GetRequiredService<FormulaEvaluationCache>();
            FormulaEvaluationCache nextRequest = second.ServiceProvider.GetRequiredService<FormulaEvaluationCache>();

            // Assert
            sameRequest.Should().BeSameAs(cache);
            nextRequest.Should().NotBeSameAs(cache);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Evaluate_WhenMovementUpdatedInRequest_DiscardsClosingBalance(bool rollback)
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using ServiceProvider services = CreateServices(connection);
            Guid movementId = await Seed(services);
            using IServiceScope request = services.CreateScope();
            IFormulaEvaluator evaluator = request.ServiceProvider.GetRequiredService<IFormulaEvaluator>();
            var date = new DateOnly(2026, 2, 6);
            await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date);
            var persistence = request.ServiceProvider.GetRequiredService<EntityFrameworkPersistenceCoordinator<FinanceContext>>();
            await using IPersistenceTransaction transaction = await persistence.BeginTransaction();
            await request.ServiceProvider.GetRequiredService<IMovimentoRepository>().Update(movementId, null, null, "200.00", null, false);
            await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date);
            if (rollback)
            {
                await transaction.DisposeAsync();
            }
            else
            {
                await transaction.Commit();
            }

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date);

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(rollback ? 100m : 200m);
        }

        [Fact]
        public async Task GetStatus_WhenCardHasTenYearsOfResets_ReturnsBalanceAndIndicators()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using ServiceProvider services = CreateServices(connection);
            await Seed(services);
            using (IServiceScope seed = services.CreateScope())
            {
                FinanceContext context = seed.ServiceProvider.GetRequiredService<FinanceContext>();
                Conto card = await context.Conti.SingleAsync();
                context.ParametriConto.AddRange(
                    new ParametroConto { Id = Guid.NewGuid(), ContoId = card.Id, Name = "Plafond", DisplayName = "Plafond", Type = TipoParametroConto.Importo, Value = 5000m },
                    new ParametroConto { Id = Guid.NewGuid(), ContoId = card.Id, Name = "PercentualeScoperto", DisplayName = "Scoperto", Type = TipoParametroConto.Percentuale, Value = 0.1m },
                    new ParametroConto { Id = Guid.NewGuid(), ContoId = card.Id, Name = "Addebito", DisplayName = "Addebito", Type = TipoParametroConto.Intero, Value = 5m },
                    new ParametroConto { Id = Guid.NewGuid(), ContoId = card.Id, Name = "RipristinoPlafond", DisplayName = "Ripristino", Type = TipoParametroConto.Intero, Value = 6m });
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                var month = new DateOnly(today.Year, today.Month, 6);
                for (var index = 1; index <= 120; index++)
                {
                    context.Movimenti.Add(new Movimento
                    {
                        Id = Guid.NewGuid(),
                        ContoId = card.Id,
                        Date = month.AddMonths(index),
                        Description = "Ripristino plafond",
                        Formula = "-[HelloCard.SaldoUltimoCicloChiuso]",
                    });
                }

                await context.SaveChangesAsync();
            }

            using IServiceScope request = services.CreateScope();
            IContoService service = request.ServiceProvider.GetRequiredService<IContoService>();
            Conto conto = (await service.GetConti()).Single();

            // Act
            ContoStatus result = await service.GetStatus(conto);

            // Assert
            result.Balance.Should().Be(100m);
            result.FirstNegativeBalance.Should().BeNull();
            result.CycleIndicators.Should().NotBeNull();
        }

        [Fact]
        public async Task Evaluate_WhenRecurringDefinitionsReplaced_RefreshesResolverAndResults()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using ServiceProvider services = CreateServices(connection);
            await Seed(services);
            using IServiceScope request = services.CreateScope();
            IFormulaEvaluator evaluator = request.ServiceProvider.GetRequiredService<IFormulaEvaluator>();
            var date = new DateOnly(2026, 2, 6);
            await evaluator.Evaluate("[Affitto]", date);
            await request.ServiceProvider.GetRequiredService<IVoceRicorrenteRepository>()
                .Replace("Affitto", "Affitto", [new VoceRicorrente { DisplayName = "Affitto", Value = 500m }]);

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[Affitto]", date);

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(500m);
        }

        [Fact]
        public async Task Evaluate_WhenClosingDayReplaced_UsesNewClosingDate()
        {
            // Arrange
            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            await using ServiceProvider services = CreateServices(connection);
            await Seed(services);
            using IServiceScope request = services.CreateScope();
            IFormulaEvaluator evaluator = request.ServiceProvider.GetRequiredService<IFormulaEvaluator>();
            var date = new DateOnly(2026, 1, 22);
            await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date);
            Conto conto = (await request.ServiceProvider.GetRequiredService<IContoRepository>().GetByName("HelloCard"))!;
            await request.ServiceProvider.GetRequiredService<IParametroContoRepository>()
                .Replace(conto.Id, "ChiusuraCiclo", "ChiusuraCiclo", TipoParametroConto.Intero, [new ParametroConto { DisplayName = "Chiusura", Value = 25m }]);

            // Act
            FormulaEvaluationResult result = await evaluator.Evaluate("[HelloCard.SaldoUltimoCicloChiuso]", date);

            // Assert
            result.Error.Should().BeNull();
            result.Value.Should().Be(0m);
        }

        private static ServiceProvider CreateServices(SqliteConnection connection)
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            var environment = new Mock<IHostEnvironment>();
            environment.SetupGet(host => host.EnvironmentName).Returns(Environments.Development);
            services.AddLogging();
            services.AddFinance(configuration.GetSection("Finance"), environment.Object);
            services.AddDbContext<FinanceContext>(options => options.UseSqlite(connection));

            return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        }

        private static async Task<Guid> Seed(ServiceProvider services)
        {
            using IServiceScope seed = services.CreateScope();
            await seed.ServiceProvider.GetRequiredService<FinanceContext>().Database.EnsureCreatedAsync();
            Conto conto = await seed.ServiceProvider.GetRequiredService<IContoRepository>().CreateConto("HelloCard", "Hello Card", 0m);
            await seed.ServiceProvider.GetRequiredService<IParametroContoRepository>()
                .Replace(conto.Id, null, "ChiusuraCiclo", TipoParametroConto.Intero, [new ParametroConto { DisplayName = "Chiusura", Value = 21m }]);
            await seed.ServiceProvider.GetRequiredService<IVoceRicorrenteRepository>()
                .Replace(null, "Affitto", [new VoceRicorrente { DisplayName = "Affitto", Value = 400m }]);
            Movimento movimento = await seed.ServiceProvider.GetRequiredService<IMovimentoRepository>()
                .Create(conto.Id, new DateOnly(2026, 1, 10), "Spesa", "100.00");

            return movimento.Id;
        }
    }
}
