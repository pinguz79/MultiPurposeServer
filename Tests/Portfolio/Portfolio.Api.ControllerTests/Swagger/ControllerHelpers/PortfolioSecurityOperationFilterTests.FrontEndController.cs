using Microsoft.AspNetCore.Authorization;
using Portfolio.Api.Authentication;

namespace Portfolio.Api.ControllerTests.Swagger
{
    public partial class PortfolioSecurityOperationFilterTests
    {
        [Authorize(Policy = PortfolioPolicies.FrontEnd)]
        private sealed class FrontEndController
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
        }
    }
}