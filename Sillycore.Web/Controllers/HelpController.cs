using System;
using Microsoft.AspNetCore.Mvc;
using Sillycore.Web.HealthCheck;

namespace Sillycore.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HelpController : SillyController
    {
        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            HealthCheckResponse response = new HealthCheckResponse();
            response.DockerImageName = Environment.GetEnvironmentVariable("Sillycore.DockerImageName");
            return Ok(response);
        }
    }
}