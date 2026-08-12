using Microsoft.AspNetCore.Authorization;

using Portfolio.Api.Authentication;

namespace Portfolio.Api.Tests.Swagger.ControllerHelpers
{
    [Authorize(Policy = PortfolioPolicies.FrontEnd)]
    internal sealed class FrontEndController
    {
        public void Get()
        {
        }

        [Authorize(Policy = PortfolioPolicies.FrontEnd)]
        public void FrontEnd()
        {
        }

        [Authorize(Policy = PortfolioPolicies.BackEnd)]
        public void BackEnd()
        {
        }

        [AllowAnonymous]
        public void Anonymous()
        {
        }
    }
}
