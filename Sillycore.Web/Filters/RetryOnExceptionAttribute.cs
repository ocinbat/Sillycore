using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Sillycore.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RetryOnExceptionAttribute : Attribute, IAsyncActionFilter
    {
        public Type ExceptionType { get; set; } = typeof(Exception);
        public int RetryCount { get; set; } = 2;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool breakLoop = false;
            int tryCount = 0;
            do
            {
                tryCount++;
                ActionExecutedContext actionExecutedContext = await next();

                if (actionExecutedContext.Exception != null && ExceptionType.IsInstanceOfType(actionExecutedContext.Exception) && tryCount < RetryCount)
                {
                    actionExecutedContext.Exception = null;
                    actionExecutedContext.ExceptionDispatchInfo = null;
                    actionExecutedContext.Result = null;
                    actionExecutedContext.Canceled = false;
                }
                else
                {
                    breakLoop = true;

                    if (actionExecutedContext.Result == null)
                    {
                        // TODO: Find a safer way to do this.
                        object result = typeof(ControllerActionInvoker).GetField("_result", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(next.Target);
                        actionExecutedContext.Result = (IActionResult)result;
                    }
                }

            } while (!breakLoop);
        }
    }
}
