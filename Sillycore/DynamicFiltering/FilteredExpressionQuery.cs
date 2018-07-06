using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Enums;
using Sillycore.Domain.Objects;
using Sillycore.Domain.Requests;
using Sillycore.Paging;

namespace Sillycore.DynamicFiltering
{
    public class FilteredExpressionQuery<TResult> : IFilteredExpressionQuery<TResult> where TResult : class
    {
        public static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public readonly string Fields;
        public IQueryable<TResult> Source;
        public Type ProxyType;
        public IDictionary<string, PropertyInfo> SelectedProperties;
        public PropertyInfo[] ProxyProperties;

        public FilteredExpressionQuery(IQueryable<TResult> source, string fields)
        {
            Source = source ?? throw new ArgumentNullException($"You cannot filter a null object reference. The parameter source should be initialized.", nameof(source));
            Fields = fields;
        }

        public List<TResult> ToList()
        {
            return ToList(Source);
        }

        public List<TResult> ToList(IQueryable<TResult> query)
        {
            IQueryable<object> runtimeTypeSelectExpressionQuery = ToObjectQuery(query);

            // Get result from database
            List<object> listOfObjects = runtimeTypeSelectExpressionQuery.ToList();

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

        public TResult First()
        {
            object obj = ToObjectQuery(Source).First();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public TResult FirstOrDefault()
        {
            object obj = ToObjectQuery(Source).FirstOrDefault();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public IPage<TResult> ToPage(PagedRequest request)
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

                return new Page<TResult>(ToList(), 0, 0, 0);
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
            int totalItemCount = Source.Count();

            return new Page<TResult>(ToList(Source.Skip(skip).Take(take)), request.Page.Value, request.PageSize.Value, totalItemCount);
        }

        public IQueryable<object> ToObjectQuery(IQueryable<TResult> query)
        {
            IEnumerable<string> selectedPropertyNames = (Fields ?? "").ToLowerInvariant().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

            // Take properties from the mapped entitiy that match selected properties
            SelectedProperties = GetSelectableProperties<TResult>(selectedPropertyNames);
            selectedPropertyNames = SelectedProperties.Keys;

            // Construct runtime type by given property configuration
            ProxyType = ProxyGenerator.CreateClassProxy<TResult>().GetType();

            // Create instance of source parameter
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TResult), "t");

            // Take fields from generated runtime type
            ProxyProperties = ProxyType.GetProperties();

            // Elect selected fields if any.
            var propertyNames = selectedPropertyNames as string[] ?? selectedPropertyNames.ToArray();
            if (propertyNames.Any())
            {
                ProxyProperties = ProxyProperties.Where(pi => propertyNames.Any(p => pi.Name.ToLowerInvariant() == p)).ToArray();
            }

            // Generate bindings from source type to runtime type
            IEnumerable<MemberBinding> bindingsToRuntimeType = ProxyProperties
                .Select(field => Expression.Bind(
                    field,
                    Expression.Property(
                        sourceParameter,
                        SelectedProperties[field.Name.ToLowerInvariant()]
                    )
                ));

            // Generate projection trom T to runtimeType and cast as IQueryable<object>
            IQueryable<object> runtimeTypeSelectExpressionQuery
                = ExpressionMapper.GenerateProjectedQuery<object>(
                    typeof(TResult),
                    ProxyType,
                    bindingsToRuntimeType,
                    query,
                    sourceParameter
                );

            return runtimeTypeSelectExpressionQuery;
        }

        public IDictionary<string, PropertyInfo> GetSelectableProperties<T>(IEnumerable<string> selectedProperties) where T : class
        {
            var existedProperties = typeof(T)
                .GetProperties()
                .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .ToDictionary(p => p.Name.ToLowerInvariant());

            IEnumerable<string> properties = selectedProperties as string[] ?? selectedProperties.ToArray();

            if (properties.Any())
            {
                return properties
                    .Where(p => existedProperties.ContainsKey(p.ToLowerInvariant()))
                    .ToDictionary(p => p, p => existedProperties[p.ToLowerInvariant()]);
            }

            return existedProperties;
        }

        public void ClearInterceptors(object obj)
        {
            if (obj != null)
            {
                FieldInfo fieldInfo = obj.GetType().GetField("__interceptors");
                if (fieldInfo != null) fieldInfo.SetValue(obj, null);
            }
        }
    }
}