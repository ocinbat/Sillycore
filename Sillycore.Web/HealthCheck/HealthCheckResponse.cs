using System.Collections.Generic;
using Sillycore.Domain.Responses;

namespace Sillycore.Web.HealthCheck
{
    public class HealthCheckResponse : BaseResponse
    {
        public List<HealthCheckResult> Results { get; set; }
    }
}