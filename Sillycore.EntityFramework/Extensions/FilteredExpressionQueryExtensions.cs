using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Enums;
using Sillycore.Domain.Objects;
using Sillycore.Domain.Requests;
using Sillycore.DynamicFiltering;
using Sillycore.Paging;

namespace Sillycore.EntityFramework.Extensions
{
    public static class FilteredExpressionQueryExtensions
    {
        public static async Task<List<TResult>> ToListAsync<TResult>(this FilteredExpressionQuery<TResult> filteredExpressionQuery) where TResult : class
        {
            return await ToListAsync(filteredExpressionQuery, filteredExpressionQuery.Source);
        }

        public static async Task<List<TResult>> ToListAsync<TResult>(this FilteredExpressionQuery<TResult> filteredExpressionQuery, IQueryable<TResult> query) where TResult : class
        {
            IQueryable<object> runtimeTypeSelectExpressionQuery = filteredExpressionQuery.ToObjectQuery(query);

            // Get result from database
            List<object> listOfObjects = await runtimeTypeSelectExpressionQuery.ToListAsync();

            MethodInfo castMethod = typeof(Queryable)
                .GetMethod("Cast", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(filteredExpressionQuery.ProxyType);

            // Cast list<objects> to IQueryable<runtimeType>
            IQueryable castedSource = castMethod.Invoke(
                null,
                new Object[] { listOfObjects.AsQueryable() }
            ) as IQueryable;

            // Create instance of runtime type parameter
            ParameterExpression runtimeParameter = Expression.Parameter(filteredExpressionQuery.ProxyType, "p");

            IDictionary<string, PropertyInfo> dynamicTypeFieldsDict =
                filteredExpressionQuery.ProxyProperties.ToDictionary(f => f.Name.ToLowerInvariant(), f => f);

            // Generate bindings from runtime type to source type
            IEnumerable<MemberBinding> bindingsToTargetType = filteredExpressionQuery.SelectedProperties.Values
                .Select(property => Expression.Bind(
                    property,
                    Expression.Property(
                        runtimeParameter,
                        dynamicTypeFieldsDict[property.Name.ToLowerInvariant()]
                    )
                ));

            // Generate projection trom runtimeType to T and cast as IQueryable<object>
            IQueryable<TResult> targetTypeSelectExpressionQuery
                = ExpressionMapper.GenerateProjectedQuery<TResult>(
                    filteredExpressionQuery.ProxyType,
                    typeof(TResult),
                    bindingsToTargetType,
                    castedSource,
                    runtimeParameter
                );

            // Return list of T
            return targetTypeSelectExpressionQuery.ToList();
        }

        public static async Task<TResult> FirstAsync<TResult>(this FilteredExpressionQuery<TResult> filteredExpressionQuery) where TResult : class
        {
            object obj = await filteredExpressionQuery.ToObjectQuery(filteredExpressionQuery.Source).FirstAsync();

            filteredExpressionQuery.ClearInterceptors(obj);

            return (TResult)obj;
        }

        public static async Task<TResult> FirstOrDefaultAsync<TResult>(this FilteredExpressionQuery<TResult> filteredExpressionQuery) where TResult : class
        {
            object obj = await filteredExpressionQuery.ToObjectQuery(filteredExpressionQuery.Source).FirstOrDefaultAsync();

            filteredExpressionQuery.ClearInterceptors(obj);

            return (TResult)obj;
        }

        public static async Task<IPage<TResult>> ToPageAsync<TResult>(this FilteredExpressionQuery<TResult> filteredExpressionQuery, PagedRequest request) where TResult : class
        {
            if (request == null)
            {
                throw new PagingException($"You need to initialize a paging request before paging on a list. The parameter request should be initialized.");
            }

            if (!request.Page.HasValue)
            {
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    if (request.Order == OrderType.Asc)
                    {
                        filteredExpressionQuery.Source = filteredExpressionQuery.Source.OrderBy(request.OrderBy);
                    }
                    else if (request.Order == OrderType.Desc)
                    {
                        filteredExpressionQuery.Source = filteredExpressionQuery.Source.OrderBy(request.OrderBy + " descending");
                    }
                }

                return new Page<TResult>(await ToListAsync(filteredExpressionQuery), 0, 0, 0);
            }

            if (String.IsNullOrEmpty(request.OrderBy))
            {
                throw new PagingException($"In order to use paging extensions you need to supply an OrderBy parameter.");
            }

            if (request.Order == OrderType.Asc)
            {
                filteredExpressionQuery.Source = filteredExpressionQuery.Source.OrderBy(request.OrderBy);
            }
            else if (request.Order == OrderType.Desc)
            {
                filteredExpressionQuery.Source = filteredExpressionQuery.Source.OrderBy(request.OrderBy + " descending");
            }

            int skip = (request.Page.Value - 1) * request.PageSize.Value;
            int take = request.PageSize.Value;
            int totalItemCount = await filteredExpressionQuery.Source.CountAsync();

            return new Page<TResult>(await ToListAsync(filteredExpressionQuery, filteredExpressionQuery.Source.Skip(skip).Take(take)), request.Page.Value, request.PageSize.Value, totalItemCount);
        }
    }
}