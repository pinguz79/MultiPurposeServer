namespace Finance.DataModel.Models
{
    public class Conto
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; } = string.Empty;
        public virtual string DisplayName { get; set; } = string.Empty;
        public virtual decimal InitialBalance { get; set; }

        public virtual ICollection<Movimento> Movimenti { get; set; } = [];

        public decimal Balance => InitialBalance + Movimenti.Where(movimento => movimento.Date <= DateOnly.FromDateTime(DateTime.Today)).Sum(ParseFormula);

        private static decimal ParseFormula(Movimento movimento)
        {
            try
            {
                return decimal.Parse(movimento.Formula, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
            }
            catch (Exception exception) when (exception is FormatException or OverflowException)
            {
                throw new FormulaEvaluationException(movimento, exception);
            }
        }
    }
}
