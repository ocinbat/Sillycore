using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Sillycore.Domain.Responses;
using Sillycore.Extensions;
using Sillycore.Web.Extensions;

namespace Sillycore.Web.Filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public static readonly ILogger<ValidateModelStateAttribute> Logger = SillycoreApp.Instance?.LoggerFactory?.CreateLogger<ValidateModelStateAttribute>();

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            BaseResponse errorResponse = new BaseResponse();

            if (!actionContext.ModelState.IsValid)
            {
                foreach (var modelState in actionContext.ModelState.Values)
                {
                    if (modelState.Errors.HasElements())
                    {
                        foreach (ModelError modelStateError in modelState.Errors)
                        {
                            errorResponse.AddErrorMessage(modelStateError.GetErrorMessage());
                        }
                    }
                }
            }

            if (errorResponse.HasError)
            {
                var errorResult = new ObjectResult(errorResponse)
                {
                    StatusCode = 400
                };

                Logger?.LogTrace($"{actionContext.ActionDescriptor.DisplayName} is called with wrong arguments. {errorResponse.ToJson()}");

                actionContext.Result = errorResult;
            }

            base.OnActionExecuting(actionContext);
        }
    }
}