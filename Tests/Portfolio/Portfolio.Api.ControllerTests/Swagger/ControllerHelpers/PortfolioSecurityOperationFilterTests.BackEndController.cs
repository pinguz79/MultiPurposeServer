using Microsoft.AspNetCore.Authorization;
using Portfolio.Api.Authentication;

namespace Portfolio.Api.ControllerTests.Swagger
{
    public partial class PortfolioSecurityOperationFilterTests
    {
        [Authorize(Policy = PortfolioPolicies.BackEnd)]
        private sealed class BackEndController
        {
            public void Get()
            {
            }
        }
    }
}