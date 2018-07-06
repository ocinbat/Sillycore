using System;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Enums;
using Sillycore.Domain.Objects;
using Sillycore.Domain.Requests;
using Sillycore.DynamicFiltering;
using Sillycore.Paging;

namespace Sillycore.EntityFramework.DynamicFiltering
{
    public class AsyncFilteredExpressionQuery<TResult> : FilteredExpressionQuery<TResult>, IAsyncFilteredExpressionQuery<TResult> where TResult : class
    {
        public AsyncFilteredExpressionQuery(IQueryable<TResult> source, string fields)
            : base(source, fields)
        {
        }

        public async Task<List<TResult>> ToListAsync()
        {
            return await ToListAsync(Source);
        }

        public async Task<List<TResult>> ToListAsync(IQueryable<TResult> query)
        {
            IQueryable<object> runtimeTypeSelectExpressionQuery = ToObjectQuery(query);

            // Get result from database
            List<object> listOfObjects = await runtimeTypeSelectExpressionQuery.ToListAsync();

            MethodInfo castMethod = typeof(Queryable)
                .GetMethod("Cast", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(ProxyType);

            // Cast list<objects> to IQueryable<runtimeType>
            IQueryable castedSource = castMethod.Invoke(
                null,
                new Object[] { listOfObjects.AsQueryable() }
            ) as IQueryable;

            // Create instance of runtime type parameter
            ParameterExpression runtimeParameter = Expression.Parameter(ProxyType, "p");

            IDictionary<string, PropertyInfo> dynamicTypeFieldsDict =
                ProxyProperties.ToDictionary(f => f.Name.ToLowerInvariant(), f => f);

            // Generate bindings from runtime type to source type
            IEnumerable<MemberBinding> bindingsToTargetType = SelectedProperties.Values
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
                    ProxyType,
                    typeof(TResult),
                    bindingsToTargetType,
                    castedSource,
                    runtimeParameter
                );

            // Return list of T
            return targetTypeSelectExpressionQuery.ToList();
        }

        public async Task<TResult> FirstAsync()
        {
            object obj = await ToObjectQuery(Source).FirstAsync();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public async Task<TResult> FirstOrDefaultAsync()
        {
            object obj = await ToObjectQuery(Source).FirstOrDefaultAsync();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public async Task<IPage<TResult>> ToPageAsync(PagedRequest request)
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
                        Source = Source.OrderBy(request.OrderBy);
                    }
                    else if (request.Order == OrderType.Desc)
                    {
                        Source = Source.OrderBy(request.OrderBy + " descending");
                    }
                }

                return new Page<TResult>(await ToListAsync(), 0, 0, 0);
            }

            if (String.IsNullOrEmpty(request.OrderBy))
            {
                throw new PagingException($"In order to use paging extensions you need to supply an OrderBy parameter.");
            }

            if (request.Order == OrderType.Asc)
            {
                Source = Source.OrderBy(request.OrderBy);
            }
            else if (request.Order == OrderType.Desc)
            {
                Source = Source.OrderBy(request.OrderBy + " descending");
            }

            int skip = (request.Page.Value - 1) * request.PageSize.Value;
            int take = request.PageSize.Value;
            int totalItemCount = await Source.CountAsync();

            return new Page<TResult>(await ToListAsync(Source.Skip(skip).Take(take)), request.Page.Value, request.PageSize.Value, totalItemCount);
        }
    }
}
