using Finance.Desktop.Models;

namespace Finance.Desktop.Presentation
{
    public static class RevolvingCardPresentation
    {
        public static Color GetBalanceColor(RevolvingIndicators indicators) => indicators.RemainingIncludingOverdraft < 0m ? Color.Firebrick : indicators.RemainingPlafond < 0m ? Color.DarkOrange : SystemColors.ControlText;
    }
}
