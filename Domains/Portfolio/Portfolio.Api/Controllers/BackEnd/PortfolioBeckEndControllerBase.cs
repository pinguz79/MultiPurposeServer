using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Authentication;

namespace Portfolio.Api.Controllers.BackEnd
{
    [Authorize(Policy = PortfolioPolicies.BackEnd)]
    public abstract class PortfolioBackEndControllerBase(ILogger<PortfolioControllerBase> logger) : PortfolioControllerBase(logger)
    {
    }
}