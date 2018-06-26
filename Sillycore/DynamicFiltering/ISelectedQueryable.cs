using System.Collections.Generic;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Requests;

namespace Sillycore.DynamicFiltering
{
    public interface IFilteredExpressionQuery<TResult> where TResult : class
    {
        List<TResult> ToList();

        TResult First();

        TResult FirstOrDefault();

        IPage<TResult> ToPage(PagedRequest request);
    }
}