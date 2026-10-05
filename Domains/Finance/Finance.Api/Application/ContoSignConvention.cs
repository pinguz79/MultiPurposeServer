namespace Finance.Api.Application
{
    internal static class ContoSignConvention
    {
        public static async Task<bool> IsDebtAccount(IParametroContoService parameters, Guid id, DateOnly date)
        {
            if (await parameters.Resolve(id, "Plafond", date) is null || await parameters.Resolve(id, "PercentualeScoperto", date) is null)
            {
                return false;
            }
            if (await parameters.Resolve(id, "RipristinoPlafond", date) is not null)
            {
                return await parameters.Resolve(id, "ChiusuraCiclo", date) is not null && await parameters.Resolve(id, "Addebito", date) is not null;
            }
            return await parameters.Resolve(id, "QuotaRata", date) is not null || await parameters.Resolve(id, "Rata", date) is not null;
        }
    }
}
