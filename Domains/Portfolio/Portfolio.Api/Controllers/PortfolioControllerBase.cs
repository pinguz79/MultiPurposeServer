using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Portfolio.Api.Controllers
{
    public abstract class PortfolioControllerBase(ILogger<PortfolioControllerBase> logger) : ControllerBase
    {

        protected static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }
    }
}
