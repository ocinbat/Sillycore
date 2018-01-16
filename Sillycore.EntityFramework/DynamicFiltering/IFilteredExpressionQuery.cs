using System.Threading.Tasks;
using System.Collections.Generic;
using Sillycore.Domain.Requests;
using Sillycore.Domain.Abstractions;

namespace Sillycore.EntityFramework.DynamicFiltering
{
    public interface IFilteredExpressionQuery<TResult> where TResult : class
    {
        List<TResult> ToList();
        Task<List<TResult>> ToListAsync();
        TResult First();
        Task<TResult> FirstAsync();
        TResult FirstOrDefault();
        Task<TResult> FirstOrDefaultAsync();
        IPage<TResult> ToPage(PagedRequest request);
        Task<IPage<TResult>> ToPageAsync(PagedRequest request);
    }
}

