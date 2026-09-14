using Finance.Api.Application;
using Finance.Api.Infrastructure.Caching;
using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel;
using Finance.DataModel.Models;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using MultiPurposeServer.Shared.Persistence.EntityFramework;

namespace Finance.Api.Tests.Infrastructure
{
    public sealed class RevolvingFormulaScenario : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        public FinanceContext Context { get; }
        public FormulaEvaluator Evaluator { get; }
        public MovimentoRepository Movements { get; }
        public ParametroContoRepository Parameters { get; }
        public Conto Card { get; }
        public ContoRepository Accounts { get; }
        public FormulaEvaluationCache Cache { get; }
        public EntityFrameworkPersistenceCoordinator<FinanceContext> Persistence { get; }

        private RevolvingFormulaScenario(SqliteConnection connection, FinanceContext context, Conto card)
        {
            _connection = connection;
            Context = context;
            Card = card;
            Cache = new FormulaEvaluationCache();
            Persistence = new EntityFrameworkPersistenceCoordinator<FinanceContext>(context);
            Accounts = new ContoRepository(context, Persistence, Cache);
            Movements = new MovimentoRepository(context, Persistence, Cache);
            Parameters = new ParametroContoRepository(context, Persistence, Cache);
            var resolver = new FormulaResolver(new VoceRicorrenteRepository(context, Persistence, Cache), Accounts, Parameters, Cache);
            Evaluator = new FormulaEvaluator(resolver, Accounts, Movements, Parameters, Cache);
        }

        public static async Task<RevolvingFormulaScenario> Create(decimal initialCapital = 1000m, bool configureParameters = true)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var context = new FinanceContext(new DbContextOptionsBuilder<FinanceContext>().UseLazyLoadingProxies().UseSqlite(connection).Options);
            await context.Database.EnsureCreatedAsync();
            var card = new Conto { Id = Guid.NewGuid(), Name = "AmEx", DisplayName = "American Express", InitialBalance = initialCapital };
            context.Conti.Add(card);
            await context.SaveChangesAsync();
            var scenario = new RevolvingFormulaScenario(connection, context, card);
            if (!configureParameters)
            {
                return scenario;
            }
            await scenario.SetParameter("ChiusuraCiclo", 6m, TipoParametroConto.Intero);
            await scenario.SetParameter("Tan", 0.12m, TipoParametroConto.Percentuale);
            await scenario.SetParameter("Plafond", 1600m);
            await scenario.SetParameter("QuotaRata", 0.10m, TipoParametroConto.Percentuale);
            await scenario.SetParameter("RataMinima", 72.32m);
            await scenario.SetParameter("Bollo", 2m);
            await scenario.SetParameter("SogliaBollo", 70m);
            return scenario;
        }

        public Task<IReadOnlyList<ParametroConto>> SetParameter(string name, decimal value, TipoParametroConto type = TipoParametroConto.Importo)
            => Parameters.Replace(Card.Id, name, name, type, [new ParametroConto { DisplayName = name, Value = value }]);

        public Task<Movimento> Add(DateOnly date, string formula, NaturaMovimento nature = NaturaMovimento.Ordinario)
            => Movements.Create(Card.Id, date, "Descrizione indipendente dalla natura", formula, natura: nature);

        public async Task AddCycle(DateOnly closingDate)
        {
            await Add(closingDate, "[AmEx.InteressiCiclo]", NaturaMovimento.Interessi);
            await Add(closingDate, "[AmEx.BolloCiclo]", NaturaMovimento.Bollo);
            await Add(new DateOnly(closingDate.Year, closingDate.Month, 19), "-[AmEx.RataUltimoCicloChiuso]", NaturaMovimento.Rimborso);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
