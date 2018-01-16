using System;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using Sillycore.Domain.Abstractions;
using Sillycore.Domain.Enums;
using Sillycore.Domain.Objects;
using Sillycore.Domain.Requests;

namespace Sillycore.EntityFramework.DynamicFiltering
{
    public class FilteredExpressionQuery<TResult> : IFilteredExpressionQuery<TResult> where TResult : class
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private readonly string _fields;
        private IQueryable<TResult> _source;
        private Type _proxyType;
        private IDictionary<string, PropertyInfo> _selectedProperties;
        private PropertyInfo[] _proxyProperties;

        public FilteredExpressionQuery(IQueryable<TResult> source, string fields)
        {
            _source = source ?? throw new ArgumentNullException($"You cannot filter a null object reference. The parameter source should be initialized.", nameof(source));
            _fields = fields;
        }

        public List<TResult> ToList()
        {
            return ToList(_source);
        }

        public List<TResult> ToList(IQueryable<TResult> query)
        {
            IQueryable<object> runtimeTypeSelectExpressionQuery = ToObjectQuery(query);

            // Get result from database
            List<object> listOfObjects = runtimeTypeSelectExpressionQuery.ToList();

            MethodInfo castMethod = typeof(Queryable)
                .GetMethod("Cast", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(_proxyType);

            // Cast list<objects> to IQueryable<runtimeType>
            IQueryable castedSource = castMethod.Invoke(
                null,
                new Object[] { listOfObjects.AsQueryable() }
            ) as IQueryable;

            // Create instance of runtime type parameter
            ParameterExpression runtimeParameter = Expression.Parameter(_proxyType, "p");

            IDictionary<string, PropertyInfo> dynamicTypeFieldsDict =
                _proxyProperties.ToDictionary(f => f.Name.ToLowerInvariant(), f => f);

            // Generate bindings from runtime type to source type
            IEnumerable<MemberBinding> bindingsToTargetType = _selectedProperties.Values
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
                    _proxyType,
                    typeof(TResult),
                    bindingsToTargetType,
                    castedSource,
                    runtimeParameter
                );

            // Return list of T
            return targetTypeSelectExpressionQuery.ToList();
        }

        public async Task<List<TResult>> ToListAsync()
        {
            return await ToListAsync(_source);
        }

        public async Task<List<TResult>> ToListAsync(IQueryable<TResult> query)
        {
            IQueryable<object> runtimeTypeSelectExpressionQuery = ToObjectQuery(query);

            // Get result from database
            List<object> listOfObjects = await runtimeTypeSelectExpressionQuery.ToListAsync();

            MethodInfo castMethod = typeof(Queryable)
                .GetMethod("Cast", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(_proxyType);

            // Cast list<objects> to IQueryable<runtimeType>
            IQueryable castedSource = castMethod.Invoke(
                null,
                new Object[] { listOfObjects.AsQueryable() }
            ) as IQueryable;

            // Create instance of runtime type parameter
            ParameterExpression runtimeParameter = Expression.Parameter(_proxyType, "p");

            IDictionary<string, PropertyInfo> dynamicTypeFieldsDict =
                _proxyProperties.ToDictionary(f => f.Name.ToLowerInvariant(), f => f);

            // Generate bindings from runtime type to source type
            IEnumerable<MemberBinding> bindingsToTargetType = _selectedProperties.Values
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
                    _proxyType,
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
            object obj = ToObjectQuery(_source).First();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public TResult FirstOrDefault()
        {
            object obj = ToObjectQuery(_source).FirstOrDefault();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public async Task<TResult> FirstAsync()
        {
            object obj = await ToObjectQuery(_source).FirstAsync();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public async Task<TResult> FirstOrDefaultAsync()
        {
            object obj = await ToObjectQuery(_source).FirstOrDefaultAsync();

            ClearInterceptors(obj);

            return (TResult)obj;
        }

        public IPage<TResult> ToPage(PagedRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException($"You need to initialize a paging request before paging on a list. The parameter request should be initialized.", nameof(request));
            }

            if (request.Page == 0)
            {
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    if (request.Order == OrderType.Asc)
                    {
                        _source = _source.OrderBy(request.OrderBy);
                    }
                    else
                    {
                        _source = _source.OrderBy(request.OrderBy + " descending");
                    }
                }

                return new Page<TResult>(ToList(), 0, 0, 0);
            }

            if (string.IsNullOrEmpty(request.OrderBy))
            {
                throw new InvalidOperationException($"In order to use paging extensions you need to supply an OrderBy parameter.");
            }

            if (request.Order == OrderType.Asc)
            {
                _source = _source.OrderBy(request.OrderBy);
            }
            else
            {
                _source = _source.OrderBy(request.OrderBy + " descending");
            }

            int skip = (request.Page - 1) * request.PageSize;
            int take = request.PageSize;
            int totalItemCount = _source.Count();

            return new Page<TResult>(ToList(_source.Skip(skip).Take(take)), request.Page, request.PageSize, totalItemCount);
        }

        public async Task<IPage<TResult>> ToPageAsync(PagedRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException($"You need to initialize a paging request before paging on a list. The parameter request should be initialized.", nameof(request));
            }

            if (request.Page == 0)
            {
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    if (request.Order == OrderType.Asc)
                    {
                        _source = _source.OrderBy(request.OrderBy);
                    }
                    else
                    {
                        _source = _source.OrderBy(request.OrderBy + " descending");
                    }
                }

                return new Page<TResult>(await ToListAsync(), 0, 0, 0);
            }

            if (string.IsNullOrEmpty(request.OrderBy))
            {
                throw new InvalidOperationException($"In order to use paging extensions you need to supply an OrderBy parameter.");
            }

            if (request.Order == OrderType.Asc)
            {
                _source = _source.OrderBy(request.OrderBy);
            }
            else
            {
                _source = _source.OrderBy(request.OrderBy + " descending");
            }

            int skip = (request.Page - 1) * request.PageSize;
            int take = request.PageSize;
            int totalItemCount = await _source.CountAsync();

            return new Page<TResult>(await ToListAsync(_source.Skip(skip).Take(take)), request.Page, request.PageSize, totalItemCount);
        }

        private IQueryable<object> ToObjectQuery(IQueryable<TResult> query)
        {
            IEnumerable<string> selectedPropertyNames = (_fields ?? "").ToLowerInvariant().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

            // Take properties from the mapped entitiy that match selected properties
            _selectedProperties = GetSelectableProperties<TResult>(selectedPropertyNames);
            selectedPropertyNames = _selectedProperties.Keys;

            // Construct runtime type by given property configuration
            _proxyType = ProxyGenerator.CreateClassProxy<TResult>().GetType();

            // Create instance of source parameter
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TResult), "t");

            // Take fields from generated runtime type
            _proxyProperties = _proxyType.GetProperties();

            // Elect selected fields if any.
            var propertyNames = selectedPropertyNames as string[] ?? selectedPropertyNames.ToArray();
            if (propertyNames.Any())
            {
                _proxyProperties = _proxyProperties.Where(pi => propertyNames.Any(p => pi.Name.ToLowerInvariant() == p)).ToArray();
            }

            // Generate bindings from source type to runtime type
            IEnumerable<MemberBinding> bindingsToRuntimeType = _proxyProperties
                .Select(field => Expression.Bind(
                    field,
                    Expression.Property(
                        sourceParameter,
                        _selectedProperties[field.Name.ToLowerInvariant()]
                    )
                ));

            // Generate projection trom T to runtimeType and cast as IQueryable<object>
            IQueryable<object> runtimeTypeSelectExpressionQuery
                = ExpressionMapper.GenerateProjectedQuery<object>(
                    typeof(TResult),
                    _proxyType,
                    bindingsToRuntimeType,
                    query,
                    sourceParameter
                );

            return runtimeTypeSelectExpressionQuery;
        }

        private static IDictionary<string, PropertyInfo> GetSelectableProperties<T>(IEnumerable<string> selectedProperties) where T : class
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

        private static void ClearInterceptors(object obj)
        {
            if (obj != null)
            {
                FieldInfo fieldInfo = obj.GetType().GetField("__interceptors");
                if (fieldInfo != null) fieldInfo.SetValue(obj, null);
            }
        }
    }
}
