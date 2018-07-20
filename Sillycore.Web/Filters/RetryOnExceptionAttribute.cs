using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sillycore.Web.Filters
{
    public class RetryOnExceptionAttribute : Attribute, IAsyncActionFilter
    {
        public Type ExceptionType { get; set; } = typeof(Exception);
        public int RetryCount { get; set; } = 2;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int tryCount = 0;
            ActionExecutedContext actionExecutedContext = null;
            do
            {
                tryCount++;
                actionExecutedContext = await next();
            } while (actionExecutedContext?.Exception != null && actionExecutedContext.Exception.GetType() == ExceptionType && tryCount < RetryCount);
        }
    }
}
