using Microsoft.AspNetCore.Authorization;
using Portfolio.Api.Authentication;

namespace Portfolio.Api.Tests.Swagger.ControllerHelpers
{
    [Authorize(Policy = PortfolioPolicies.FrontEnd)]
    [AllowAnonymous]
    internal sealed class AllowAnonymousController
    {
        public void Get()
        {
        }
    }
}