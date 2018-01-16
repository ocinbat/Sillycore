using Sillycore.Domain.Enums;

namespace Sillycore.Domain.Requests
{
    public class PagedRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public OrderType Order { get; set; }
        public string OrderBy { get; set; }
    }
}
