using Sillycore.Domain.Requests;

namespace WebApplication.Requests
{
    public class QuerySamplesRequest : PagedRequest
    {
        public string Fields { get; set; }
    }
}