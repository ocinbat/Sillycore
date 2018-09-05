using System;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Domain.Enums;
using Sillycore.Paging;

namespace Sillycore.Domain.Requests
{
    public class PagedRequest
    {
        private static readonly PagingConfiguration PagingConfiguration = SillycoreApp.Instance?.ServiceProvider.GetService<PagingConfiguration>() ?? new PagingConfiguration();

        public int? Page { get; set; }
        public int? PageSize { get; set; } = PagingConfiguration.DefaultPageSize;

        private OrderType? _order;

        public OrderType? Order
        {
            get
            {
                if (!_order.HasValue && !String.IsNullOrWhiteSpace(OrderBy))
                {
                    throw new PagingException($"You need to supply Order (asc or desc) in order to order collection by {OrderBy}.");
                }

                if (_order.HasValue && String.IsNullOrWhiteSpace(OrderBy))
                {
                    throw new PagingException($"You need to supply OrderBy field in order to order collection {_order.Value.ToString().ToLowerInvariant()}.");
                }

                return _order;
            }
            set { _order = value; }
        }

        public string OrderBy { get; set; }
    }
}
