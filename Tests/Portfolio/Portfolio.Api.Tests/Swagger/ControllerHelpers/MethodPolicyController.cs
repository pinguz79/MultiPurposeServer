using Microsoft.AspNetCore.Authorization;
using Portfolio.Api.Authentication;

namespace Portfolio.Api.Tests.Swagger.ControllerHelpers
{
    internal sealed class MethodPolicyController
    {
        [Authorize(Policy = PortfolioPolicies.FrontEnd)]
        public void FrontEnd()
        {
        }

        [Authorize(Policy = PortfolioPolicies.BackEnd)]
        public void BackEnd()
        {
        }

        [Authorize]
        public void Authenticated()
        {
        }
    }
}