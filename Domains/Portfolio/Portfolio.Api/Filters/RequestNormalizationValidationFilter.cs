using Microsoft.AspNetCore.Mvc.Filters;

using MultiPurposeServer.Shared.Contracts.Abstractions;

namespace Portfolio.Api.Filters
{
    public sealed class RequestNormalizationValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (IRequest request in context.ActionArguments.Values.OfType<IRequest>())
            {
                request.Normalize();
                request.Validate();
            }

            await next();
        }
    }
}
