using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sillycore.Web.Filters
{
    /* Note : that's why we do this because model binding is between AuthorizationFilter and ActionFilter on HTTP Message Lifecycle
     * http://www.dotnetcurry.com/aspnet/888/aspnet-webapi-message-lifecycle
     */

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)] 
    
    public class ForcePagingAttribute : Attribute , IAuthorizationFilter
    {
        private readonly int _pageSize;
        private readonly int _page;
        private readonly string _orderBy;
        private readonly string _order;
        private readonly string[] _excludeFilterColumns = {"id", "code"};

        public ForcePagingAttribute(int pageSize = 20, int page = 1, string orderBy = "Id", string order = "asc")
        {
            _page = page;
            _pageSize = pageSize;
            _orderBy = orderBy;
            _order = order; 
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;


            if (request.Method != "GET") return;

            var isExcludable = _excludeFilterColumns
                .Select(column => context.HttpContext.Request.Query.ContainsKey(column)).Any(hasCode => hasCode);
                
            if (isExcludable)
            {
                return;
            }

            var dictionary = new Dictionary<string, string>()
            {
                { "page", _page.ToString() },
                { "pageSize", _pageSize.ToString() },
                { "order", _order },
                { "orderBy", _orderBy }
            };

            foreach (var item in dictionary)
            {
                var containsKey = context.HttpContext.Request.Query.ContainsKey(item.Key);
                if (!containsKey)
                {
                    context.HttpContext.Request.QueryString = context.HttpContext.Request.QueryString.Add(item.Key, item.Value);
                }
            }
        }
    }
}