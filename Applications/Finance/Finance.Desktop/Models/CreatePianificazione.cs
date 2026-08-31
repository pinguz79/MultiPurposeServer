namespace Finance.Desktop.Models
{
    public record CreatePianificazione(
        string ContoName,
        string Description,
        string MovimentoDescription,
        string Formula,
        DateOnly ValidFrom,
        DateOnly ValidTo,
        int Interval,
        int? DayOfMonth,
        bool EndOfMonth);
}
