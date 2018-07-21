using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Sillycore.Web.Middlewares
{
    public class SillycoreMiddleware
    {
        private readonly RequestDelegate _next;

        public SillycoreMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(state =>
            {
                bool isShuttingDown = SillycoreApp.Instance.DataStore.Get<bool>(Constants.IsShuttingDown);

                if (isShuttingDown)
                {
                    var httpContext = (HttpContext)state;
                    httpContext.Response.Headers.Add("Connection", new[] { "close" });
                }

                return Task.CompletedTask;
            }, context);

            await _next(context);
        }
    }
}