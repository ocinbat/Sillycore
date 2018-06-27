using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Enums;
using Sillycore.Domain.Objects;
using Sillycore.Domain.Requests;
using Sillycore.EntityFramework.DynamicFiltering;
using Sillycore.Paging;

namespace Sillycore.EntityFramework.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<IPage<T>> ToPageAsync<T>(this IQueryable<T> source, PagedRequest request)
        {
            if (source == null)
            {
                throw new PagingException($"You cannot pageinate on a null object reference. The parameter source should be initialized.");
            }

            if (request == null)
            {
                throw new PagingException($"You need to initialize a paging request before paging on a list. The parameter request should be initialized.");
            }

            if (request.Page == 0)
            {
                if (!String.IsNullOrEmpty(request.OrderBy))
                {
                    if (request.Order == OrderType.Asc)
                    {
                        source = source.OrderBy(request.OrderBy);
                    }
                    else
                    {
                        source = source.OrderBy(request.OrderBy + " descending");
                    }
                }

                return new Page<T>(await source.ToListAsync(), 0, 0, 0);
            }

            if (String.IsNullOrEmpty(request.OrderBy))
            {
                throw new PagingException($"In order to use paging extensions you need to supply an OrderBy parameter.");
            }

            if (request.Order == OrderType.Asc)
            {
                source = source.OrderBy(request.OrderBy);
            }
            else
            {
                source = source.OrderBy(request.OrderBy + " descending");
            }

            int skip = (request.Page - 1) * request.PageSize;
            int take = request.PageSize;
            int totalItemCount = await source.CountAsync();

            return new Page<T>(await source.Skip(skip).Take(take).ToListAsync(), request.Page, request.PageSize, totalItemCount);
        }

        public static IAsyncFilteredExpressionQuery<TResult> Select<TResult>(this IQueryable<TResult> source, string fields)
            where TResult : class
        {
            return new AsyncFilteredExpressionQuery<TResult>(source.AsNoTracking(), fields);
        }
    }
}
