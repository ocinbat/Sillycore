using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore.Extensions;
using Sillycore.Web.HealthCheck;

namespace Sillycore.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HelpController : SillyController
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HelpController> _logger;

        public HelpController(IServiceScopeFactory scopeFactory, ILogger<HelpController> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return Redirect($"{Request.GetUri().AbsoluteUri}swagger");
        }

        [HttpGet("healthcheck")]
        public IActionResult HealthCheck()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                HealthCheckResponse response = new HealthCheckResponse();

                response.DockerImageName = Environment.GetEnvironmentVariable("Sillycore.DockerImageName");

                if (SillycoreApp.Instance.DataStore.Get<bool>(Constants.IsShuttingDown))
                {
                    response.AddErrorMessage("Shutting down...");
                    return StatusCode(503, response);
                }

                HealthCheckerContainer container = SillycoreApp.Instance.DataStore.Get<HealthCheckerContainer>(Constants.HealthCheckerContainerDataKey);

                if (container == null || container.HealthCheckers.IsEmpty())
                {
                    return Ok(response);
                }

                response.Results = new List<HealthCheckResult>();

                foreach (HealthChecker checker in container.HealthCheckers)
                {
                    IHealthChecker checkerInstance = (IHealthChecker)scope.ServiceProvider.GetService(checker.HealthCheckerType);

                    if (checkerInstance == null)
                    {
                        throw new ApplicationException($"There was a problem while creating healthchecker instance for type:{checker.HealthCheckerType.FullName}. Check if your type implements IHealthChecker interface.");
                    }

                    HealthCheckResult result = new HealthCheckResult();
                    result.Key = checkerInstance.Key;
                    result.IsCtirical = checkerInstance.IsCritical;

                    try
                    {
                        result.Success = checkerInstance.CheckHealth();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"There was a problem while running healthcheckher:{checker.HealthCheckerType.FullName}");

                        result.Message = ex.Message;
                        result.Success = false;
                    }

                    response.Results.Add(result);
                }

                HttpStatusCode statusCode = HttpStatusCode.OK;

                if (response.Results.HasElements() && response.Results.Any(r => r.IsCtirical && r.Success == false))
                {
                    statusCode = HttpStatusCode.InternalServerError;
                }

                return StatusCode((int)statusCode, response);
            }
        }
    }
}