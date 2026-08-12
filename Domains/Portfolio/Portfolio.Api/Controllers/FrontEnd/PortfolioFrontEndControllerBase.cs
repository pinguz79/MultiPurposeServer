using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

using Portfolio.Api.Authentication;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Authorize(Policy = PortfolioPolicies.FrontEnd)]
    public abstract class PortfolioFrontEndControllerBase(ILogger<PortfolioControllerBase> logger)
    : PortfolioControllerBase(logger)
    {
    }
}
