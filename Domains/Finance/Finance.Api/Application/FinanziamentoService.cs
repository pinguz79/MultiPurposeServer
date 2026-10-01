using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class FinanziamentoService(IFinanziamentoRepository repository) : IFinanziamentoService
    {
        public Task<IReadOnlyList<Finanziamento>> GetAll(bool includeClosed = false) => repository.GetAll(includeClosed);

        public async Task<Finanziamento> Resolve(string name)
            => await repository.GetByName(ContoService.NormalizeName(name)) ?? throw new KeyNotFoundException($"Finanziamento '{name}' was not found.");

        public async Task<Finanziamento> Create(CreateFinanziamentoRequest request)
        {
            var loan = new Finanziamento
            {
                Id = Guid.NewGuid(), Name = ContoService.NormalizeName(request.Name), DisplayName = Text(request.DisplayName), Lender = Text(request.Lender),
                InitialPrincipal = request.InitialPrincipal, Tan = request.Tan, Installment = request.Installment, Insurance = request.Insurance, Fees = request.Fees,
                FirstDueDate = request.FirstDueDate, DueDay = request.DueDay ?? request.FirstDueDate.Day, InstallmentCount = request.InstallmentCount,
            };
            Validate(loan);
            if (await repository.GetByName(loan.Name) is not null)
            {
                throw new DuplicateNameException(loan.Name);
            }
            return await repository.Create(loan);
        }

        public async Task<Finanziamento> Update(string name, UpdateFinanziamentoRequest request)
        {
            Finanziamento loan = await Resolve(name);
            var candidate = new Finanziamento
            {
                InitialPrincipal = request.InitialPrincipal ?? loan.InitialPrincipal, Tan = request.Tan ?? loan.Tan, Installment = request.Installment ?? loan.Installment,
                Insurance = request.Insurance ?? loan.Insurance, Fees = request.Fees ?? loan.Fees, FirstDueDate = request.FirstDueDate ?? loan.FirstDueDate,
                DueDay = request.DueDay ?? loan.DueDay, InstallmentCount = request.InstallmentCount ?? loan.InstallmentCount,
            };
            bool changed = candidate.InitialPrincipal != loan.InitialPrincipal || candidate.Tan != loan.Tan || candidate.Installment != loan.Installment
                || candidate.Insurance != loan.Insurance || candidate.Fees != loan.Fees || candidate.FirstDueDate != loan.FirstDueDate
                || candidate.DueDay != loan.DueDay || candidate.InstallmentCount != loan.InstallmentCount;
            if (changed && loan.Riallineamenti.Count > 0)
            {
                throw new FinanziamentoLockedException();
            }
            Validate(candidate);
            string displayName = request.DisplayName is null ? loan.DisplayName : Text(request.DisplayName);
            string lender = request.Lender is null ? loan.Lender : Text(request.Lender);
            loan.DisplayName = displayName;
            loan.Lender = lender;
            loan.InitialPrincipal = candidate.InitialPrincipal;
            loan.Tan = candidate.Tan;
            loan.Installment = candidate.Installment;
            loan.Insurance = candidate.Insurance;
            loan.Fees = candidate.Fees;
            loan.FirstDueDate = candidate.FirstDueDate;
            loan.DueDay = candidate.DueDay;
            loan.InstallmentCount = candidate.InstallmentCount;
            loan.IsClosed = request.IsClosed ?? loan.IsClosed;
            await repository.Save();
            return loan;
        }

        public async Task SaveAlignment(string name, int number, decimal principal, DateOnly today, bool create)
        {
            Finanziamento loan = await Resolve(name);
            if (number < 1 || number > loan.InstallmentCount || FinanziamentoCalculator.GetDueDate(loan, number) >= today)
            {
                throw new ArgumentException("The alignment must refer to a past installment of this loan.");
            }
            ValidateMoney(principal);
            ValidatePayment(principal, loan.Tan, loan.Installment);
            RiallineamentoFinanziamento? alignment = loan.Riallineamenti.SingleOrDefault(item => item.InstallmentNumber == number);
            if (create && alignment is not null)
            {
                throw new DuplicateNameException($"{loan.Name}/{number}");
            }
            if (!create && alignment is null)
            {
                throw new KeyNotFoundException("Alignment was not found.");
            }
            if (alignment is null)
            {
                alignment = new RiallineamentoFinanziamento { Id = Guid.NewGuid(), FinanziamentoId = loan.Id, InstallmentNumber = number, Principal = principal };
                await repository.AddAlignment(alignment);
                return;
            }
            alignment.Principal = principal;
            await repository.Save();
        }

        public async Task DeleteAlignment(string name, int number)
        {
            Finanziamento loan = await Resolve(name);
            RiallineamentoFinanziamento alignment = loan.Riallineamenti.SingleOrDefault(item => item.InstallmentNumber == number)
                ?? throw new KeyNotFoundException("Alignment was not found.");
            await repository.RemoveAlignment(alignment);
        }

        private static string Text(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Text cannot be empty.") : value.Trim();

        private static void Validate(Finanziamento loan)
        {
            ValidateMoney(loan.InitialPrincipal);
            ValidateMoney(loan.Installment);
            ValidateMoney(loan.Insurance);
            ValidateMoney(loan.Fees);
            if (loan.InitialPrincipal == 0 || loan.Installment == 0 || loan.Tan < 0 || loan.DueDay is < 1 or > 31 || loan.InstallmentCount is < 1 or > 1200)
            {
                throw new ArgumentException("Invalid principal, installment, TAN, due day or installment count (1-1200).");
            }
            try
            {
                if (FinanziamentoCalculator.GetDueDate(loan, 1) != loan.FirstDueDate)
                {
                    throw new ArgumentException("FirstDueDate must match the contractual DueDay, clamped to the end of the month.");
                }
                _ = FinanziamentoCalculator.GetDueDate(loan, loan.InstallmentCount);
                ValidatePayment(loan.InitialPrincipal, loan.Tan, loan.Installment);
            }
            catch (OverflowException)
            {
                throw new ArgumentException("Amounts or TAN exceed the supported calculation range.");
            }
        }

        private static void ValidateMoney(decimal value)
        {
            if (value < 0 || value > long.MaxValue / 100m || decimal.Round(value, 2) != value)
            {
                throw new ArgumentException("Amounts must be non-negative, have at most two decimals and fit the monetary storage range.");
            }
        }

        private static void ValidatePayment(decimal principal, decimal tan, decimal installment)
        {
            try
            {
                if (principal > 0 && decimal.Round(principal * tan / 12m, 2, MidpointRounding.AwayFromZero) >= installment)
                {
                    throw new ArgumentException("The installment must cover the monthly interest and reduce principal.");
                }
            }
            catch (OverflowException)
            {
                throw new ArgumentException("Amounts or TAN exceed the supported calculation range.");
            }
        }
    }
}
