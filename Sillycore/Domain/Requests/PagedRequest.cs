using Microsoft.Extensions.DependencyInjection;
using Sillycore.Domain.Enums;
using Sillycore.Paging;

namespace Sillycore.Domain.Requests
{
    public class PagedRequest
    {
        private static readonly PagingConfiguration PagingConfiguration = SillycoreApp.Instance.ServiceProvider.GetService<PagingConfiguration>();

        public int? Page { get; set; }
        public int? PageSize { get; set; } = PagingConfiguration.DefaultPageSize;
        public OrderType Order { get; set; }
        public string OrderBy { get; set; }
    }
}
