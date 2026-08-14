using Microsoft.AspNetCore.Authorization;

using Portfolio.Api.Authentication;

namespace Portfolio.Api.Controllers.BackEnd
{
    [Authorize(Policy = PortfolioPolicies.BackEnd)]
    public abstract class PortfolioBackEndControllerBase : PortfolioControllerBase
    {
    }
}
