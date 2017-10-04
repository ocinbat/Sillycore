using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sillycore.Domain.Responses;

namespace Sillycore.Web.Filters
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            while (context.Exception?.InnerException != null)
            {
                context.Exception = context.Exception.InnerException;
            }

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