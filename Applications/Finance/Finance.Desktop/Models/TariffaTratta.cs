namespace Finance.Desktop.Models
{
    public sealed record TariffaTratta(Guid Id, Guid CaselloAId, Guid CaselloBId, string Formula, DateOnly? ValidFrom, DateOnly? ValidTo, int Index);
}
