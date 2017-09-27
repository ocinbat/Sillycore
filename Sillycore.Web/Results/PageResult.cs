using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sillycore.Domain.Abstractions;

namespace Sillycore.Web.Results
{
    public class PageResult<T> : JsonResult
    {
        private readonly IPage<T> _page;

        public PageResult(IPage<T> page)
            : base(page.Items)
        {
            _page = page;
        }

        public override void ExecuteResult(ActionContext context)
        {
            base.ExecuteResult(context);

            SetHeaders(context);
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            await base.ExecuteResultAsync(context);
            SetHeaders(context);
        }

        private void SetHeaders(ActionContext context)
        {
            context.HttpContext.Response.Headers.Add("X-Paging-Index", _page.Index.ToString());
            context.HttpContext.Response.Headers.Add("X-Paging-Size", _page.Size.ToString());
            context.HttpContext.Response.Headers.Add("X-Paging-TotalCount", _page.TotalCount.ToString());
            context.HttpContext.Response.Headers.Add("X-Paging-TotalPages", _page.TotalPages.ToString());
            context.HttpContext.Response.Headers.Add("X-Paging-HasPreviousPage", _page.HasPreviousPage.ToString().ToLowerInvariant());
            context.HttpContext.Response.Headers.Add("X-Paging-HasNextPage", _page.HasNextPage.ToString().ToLowerInvariant());
            context.HttpContext.Response.Headers.Add("Link", GetLinkHeaderForPageResult(context.HttpContext.Request, _page));
        }

        private string GetLinkHeaderForPageResult<T>(HttpRequest request, IPage<T> page)
        {
            var requestUrl = request.GetUri().ToString();
            string headerValue = String.Empty;

            if (page.HasNextPage)
            {
                var nextPageUrl = requestUrl.Replace($"page={page.Index}", $"page={page.Index + 1}");
                headerValue += $"<{nextPageUrl}>; rel=\"next\",";
            }

            var lastPageUrl = requestUrl.Replace($"page={page.Index}", $"page={page.TotalPages}");
            headerValue += $"<{lastPageUrl}>; rel=\"last\",";

            var firstPageUrl = requestUrl.Replace($"page={page.Index}", "page=1");
            headerValue += $"<{firstPageUrl}>; rel=\"first\",";

            if (page.HasPreviousPage)
            {
                var previousPageUrl = requestUrl.Replace($"page={page.Index}", $"page={page.Index - 1}");
                headerValue += $"<{previousPageUrl}>; rel=\"prev\",";
            }

            if (!String.IsNullOrEmpty(headerValue))
            {
                headerValue = headerValue.TrimEnd(',');
            }

            return headerValue;
        }
    }
}