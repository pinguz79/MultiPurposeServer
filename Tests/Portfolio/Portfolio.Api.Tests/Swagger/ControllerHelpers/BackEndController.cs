using Microsoft.AspNetCore.Authorization;
using Portfolio.Api.Authentication;

namespace Portfolio.Api.Tests.Swagger.ControllerHelpers
{
    [Authorize(Policy = PortfolioPolicies.BackEnd)]
    internal sealed class BackEndController
    {
        public void Get()
        {
        }
    }
}