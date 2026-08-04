using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Portfolio.Api.Controllers
{
    public abstract class PortfolioControllerBase(ILogger<PortfolioControllerBase> logger) : ControllerBase
    {
        protected ILogger<PortfolioControllerBase> Logger => logger;
    }
}
