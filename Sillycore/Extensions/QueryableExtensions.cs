using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Enums;
using Sillycore.Domain.Objects;
using Sillycore.Domain.Requests;
using Sillycore.DynamicFiltering;
using Sillycore.Paging;

namespace Sillycore.Extensions
{
    public static class QueryableExtensions
    {
        public static IPage<T> ToPage<T>(this IQueryable<T> source, PagedRequest request)
        {
            if (source == null)
            {
                throw new PagingException($"You cannot pageinate on a null object reference. The parameter source should be initialized.");
            }

            if (request == null)
            {
                throw new PagingException($"You need to initialize a paging request before paging on a list. The parameter request should be initialized.");
            }

            if (!request.Page.HasValue)
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

                return new Page<T>(source, 0, 0, 0);
            }

            if (String.IsNullOrEmpty(request.OrderBy))
            {
                throw new PagingException($"In order to use paging extensions you need to supply an OrderBy parameter.");
            }

            int totalItemCount = source.Count();

            if (request.Order == OrderType.Asc)
            {
                source = source.OrderBy(request.OrderBy);
            }
            else
            {
                source = source.OrderBy(request.OrderBy + " descending");
            }

            int skip = (request.Page.Value - 1) * request.PageSize.Value;
            int take = request.PageSize.Value;

            return new Page<T>(source.Skip(skip).Take(take), request.Page.Value, request.PageSize.Value, totalItemCount);
        }

        public static IFilteredExpressionQuery<TResult> Select<TResult>(this IQueryable<TResult> source, string fields)
            where TResult : class
        {
            return new FilteredExpressionQuery<TResult>(source, fields);
        }
    }
}