using System;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sillycore.Web.HealthCheck;

namespace Sillycore.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HelpController : SillyController
    {
        private readonly ILogger<HelpController> _logger;

        public HelpController(ILogger<HelpController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            _logger.LogInformation("Application root url called.");
            return Redirect($"{Request.GetUri().AbsoluteUri}swagger");
        }

        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            HealthCheckResponse response = new HealthCheckResponse();
            response.DockerImageName = Environment.GetEnvironmentVariable("Sillycore.DockerImageName");
            return Ok(response);
        }
    }
}