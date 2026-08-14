using Microsoft.AspNetCore.Authorization;

using Portfolio.Api.Authentication;

namespace Portfolio.Api.Controllers.FrontEnd
{
    [Authorize(Policy = PortfolioPolicies.FrontEnd)]
    public abstract class PortfolioFrontEndControllerBase : PortfolioControllerBase
    {
    }
}
