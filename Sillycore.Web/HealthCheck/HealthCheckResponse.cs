using System.Collections.Generic;
using Sillycore.BackgroundProcessing;
using Sillycore.Domain.Responses;

namespace Sillycore.Web.HealthCheck
{
    public class HealthCheckResponse : BaseResponse
    {
        public string DockerImageName { get; set; }
        public List<HealthCheckResult> Results { get; set; }
        public BackgroundJobManager BackgroundJobManager { get; set; }
    }
}