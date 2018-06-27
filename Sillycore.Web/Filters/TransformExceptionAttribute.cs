using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Sillycore.Domain.Responses;

namespace Sillycore.Web.Filters
{
    public class TransformExceptionAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<TransformExceptionAttribute> _logger = SillycoreApp.Instance?.LoggerFactory?.CreateLogger<TransformExceptionAttribute>();

        public Type ExceptionType { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Message { get; set; }

        public string Code { get; set; }

        public TransformExceptionAttribute(Type type, HttpStatusCode statusCode, string message = null, string code = null)
        {
            ExceptionType = type;
            StatusCode = statusCode;
            Message = message;
            Code = code;

            if ((int) StatusCode < 400)
            {
                throw new Exception("You cannot transform exceptions to successful http status codes.");
            }
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception == null)
            {
                context.ExceptionHandled = false;
                return;
            }

            if (!ExceptionType.IsInstanceOfType(context.Exception))
            {
                context.ExceptionHandled = false;
                return;
            }

            ErrorResponse errorResponse = new ErrorResponse();
            errorResponse.ErrorCode = Code;
            errorResponse.AddErrorMessage(Message);
            errorResponse.AdditionalInfo = context.Exception.Message;

            ObjectResult objectResult = new ObjectResult(errorResponse) {StatusCode = (int) StatusCode};

            context.ExceptionHandled = true;
            context.Result = objectResult;

            _logger?.LogInformation($"Exception:{context.Exception.GetType()} is transfrormed.");
        }
    }
}