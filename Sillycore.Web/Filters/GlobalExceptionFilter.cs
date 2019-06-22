using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Sillycore.Domain.Responses;

namespace Sillycore.Web.Filters
{
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly string EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            ControllerActionDescriptor actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (actionDescriptor != null)
            {
                var attribute = actionDescriptor.ControllerTypeInfo.GetCustomAttribute(typeof(ApiControllerAttribute));

                if (attribute != null)
                {
                    _logger.LogError(context.Exception, context.Exception?.Message);

                    ErrorResponse errorResponse = new ErrorResponse();
                    errorResponse.AdditionalInfo = context.Exception?.Message;
                    errorResponse.ErrorCode = "InternalServerError";
                    errorResponse.AddErrorMessage("There was a problem while processing your request.");

                    if (EnvironmentName?.ToLowerInvariant() != "production")
                    {
                        errorResponse.Exception = context.Exception?.ToString();
                    }

                    var objectResult = new ObjectResult(errorResponse);
                    objectResult.StatusCode = 500;

                    context.Result = objectResult;
                }
            }
        }
    }
}