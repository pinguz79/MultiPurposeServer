namespace Finance.Api.Application
{
    public sealed class TariffarioInUseException() : Exception("La stazione o la tratta è utilizzata da pedaggi o formule esistenti. Correggere prima i riferimenti; nessun dato è stato eliminato.")
    {
    }
}
