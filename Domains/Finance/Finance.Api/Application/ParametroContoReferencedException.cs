namespace Finance.Api.Application
{
    public class ParametroContoReferencedException(string reference)
        : Exception($"Account parameter '{reference}' is referenced by one or more formulas.");
}
