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

            if (SillycoreApp.Instance.DataStore.Get<bool>(Constants.IsShuttingDown))
            {
                response.AddErrorMessage("Shutting down...");
                return StatusCode(503, response);
            }

            return Ok(response);
        }
    }
}