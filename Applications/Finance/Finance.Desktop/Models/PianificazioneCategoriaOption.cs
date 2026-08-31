namespace Finance.Desktop.Models
{
    internal sealed record PianificazioneCategoriaOption(ModalitaCategoria Mode, string? Name, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}
