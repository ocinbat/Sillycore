using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Sillycore.Domain.Responses;

namespace Sillycore.Web.Filters
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            while (context.Exception?.InnerException != null)
            {
                context.Exception = context.Exception.InnerException;
            }

            _logger.LogError(context.Exception, "There was a problem while processing your request.");

            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.AdditionalInfo = context.Exception?.Message;
            errorResponse.ErrorCode = "InternalServerError";
            // TODO Bunu resource'a çıkar.
            errorResponse.AddErrorMessage("There was a problem while processing your request.");

            var objectResult = new ObjectResult(errorResponse);
            objectResult.StatusCode = 500;

            context.Result = objectResult;
        }
    }
}