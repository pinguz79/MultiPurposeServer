using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Portfolio.Api.Filters
{
    public sealed class KeyNotFoundExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not KeyNotFoundException)
            {
                return;
            }

            context.Result = new NotFoundResult();
            context.ExceptionHandled = true;
        }
    }
}
