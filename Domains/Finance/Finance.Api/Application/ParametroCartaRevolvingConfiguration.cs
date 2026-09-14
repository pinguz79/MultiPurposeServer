using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public sealed record ParametroCartaRevolvingConfiguration(string Name, string DisplayName, TipoParametroConto Type, decimal Value);
}
