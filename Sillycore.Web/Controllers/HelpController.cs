using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sillycore.BackgroundProcessing;
using Sillycore.Extensions;
using Sillycore.Web.HealthCheck;

namespace Sillycore.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HelpController : SillyController
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HelpController> _logger;
        private readonly BackgroundJobManager _backgroundJobManager;

        public HelpController(IServiceScopeFactory scopeFactory, ILogger<HelpController> logger, BackgroundJobManager backgroundJobManager)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _backgroundJobManager = backgroundJobManager;
        }

        public IActionResult Index()
        {
            return Redirect($"{Request.GetUri().AbsoluteUri}swagger");
        }

        [HttpGet("healthcheck")]
        public async Task<IActionResult> HealthCheck()
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

                if (container != null && container.HealthCheckers.HasElements())
                {
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
                            result.Success = await checkerInstance.CheckHealth();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"There was a problem while running healthcheckher:{checker.HealthCheckerType.FullName}");

                            result.Message = ex.Message;
                            result.Success = false;
                        }

                        response.Results.Add(result);
                    }
                }

                HttpStatusCode statusCode = HttpStatusCode.OK;

                if (response.Results.HasElements() && response.Results.Any(r => r.IsCtirical && r.Success == false))
                {
                    statusCode = HttpStatusCode.InternalServerError;
                }

                if (_backgroundJobManager.IsActive)
                {
                    response.BackgroundJobManager = _backgroundJobManager;

                    if (response.BackgroundJobManager.JobTimers.Any(t => t.Status == BackgroundJobStatus.Failing))
                    {
                        statusCode = HttpStatusCode.InternalServerError;
                        response.AddErrorMessage("At least one of background jobs is failing.");
                    }
                }

                return StatusCode((int)statusCode, response);
            }
        }
    }
}