using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace Portfolio.Api.Filters
{
    public sealed class ValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not ValidationException exception)
                return;

            Dictionary<string, string[]> errors = exception.Errors.ToDictionary(error => error.Key, error => error.Value.ToArray());

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
            context.ExceptionHandled = true;
        }
    }
}