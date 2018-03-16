using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Sillycore.Web.Middlewares
{
    public class DockerImageVersionMiddleware
    {
        private readonly RequestDelegate _next;

        public DockerImageVersionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string dockerImageName = Environment.GetEnvironmentVariable("Sillycore.DockerImageName");

            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;
                httpContext.Response.Headers.Add("X-Docker-Image-Name", new[] { dockerImageName });
                return Task.CompletedTask;
            }, context);

            await _next(context);
        }
    }
}