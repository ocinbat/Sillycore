using System.Threading.Tasks;
using System.Collections.Generic;
using Sillycore.Domain.Requests;
using Sillycore.Domain.Abstractions;
using Sillycore.DynamicFiltering;

namespace Sillycore.EntityFramework.DynamicFiltering
{
    public interface IAsyncFilteredExpressionQuery<TResult> : IFilteredExpressionQuery<TResult> where TResult : class
    {
        Task<List<TResult>> ToListAsync();
        Task<TResult> FirstAsync();
        Task<TResult> FirstOrDefaultAsync();
        Task<IPage<TResult>> ToPageAsync(PagedRequest request);
    }
}

