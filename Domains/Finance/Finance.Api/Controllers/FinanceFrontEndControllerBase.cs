using Finance.Api.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers
{
    [Authorize(Policy = FinancePolicies.Desktop)]
    public abstract class FinanceFrontEndControllerBase : ControllerBase;
}
