using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Globalization;

namespace Sillycore.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select(
                (f, i) => new
                {
                    f,
                    s = second.Parameters[i]
                }).ToDictionary(p => p.s, p => p.f);

            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }
        public static Func<T, bool> AndAlso<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return arg => predicate1(arg) && predicate2(arg);
        }
        public static Func<T, bool> OrElse<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return arg => predicate1(arg) || predicate2(arg);
        }
        public static IOrderedQueryable<T> Sort<T>(this IQueryable<T> source, string sortBy, bool sortDirection)
        {
            var param = Expression.Parameter(typeof(T), "item");

            var propertyInfo = typeof(T).GetProperty(sortBy);

            switch (propertyInfo.PropertyType.ToString().ToLower(new CultureInfo("en-US", false)))
            {
                case "system.datetime":
                    var sortExpression = Expression.Lambda<Func<T, DateTime>>(Expression.Convert(Expression.Property(param, sortBy), typeof(DateTime)), param);
                    return sortDirection ? source.AsQueryable().OrderByDescending(sortExpression) : source.AsQueryable().OrderBy(sortExpression);

                case "system.guid":
                    var sortExpression1 = Expression.Lambda<Func<T, Guid>>(Expression.Convert(Expression.Property(param, sortBy), typeof(Guid)), param);
                    return sortDirection ? source.AsQueryable().OrderByDescending(sortExpression1) : source.AsQueryable().OrderBy(sortExpression1);

                case "system.int32":
                    var sortExpression3 = Expression.Lambda<Func<T, int>>(Expression.Convert(Expression.Property(param, sortBy), typeof(int)), param);
                    return sortDirection ? source.AsQueryable().OrderByDescending(sortExpression3) : source.AsQueryable().OrderBy(sortExpression3);

                case "system.string":
                    var sortExpression4 = Expression.Lambda<Func<T, string>>(Expression.Convert(Expression.Property(param, sortBy), typeof(string)), param);
                    return sortDirection ? source.AsQueryable().OrderByDescending(sortExpression4) : source.AsQueryable().OrderBy(sortExpression4);

                default:
                    var sortExpression2 = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(param, sortBy), typeof(object)), param);
                    return sortDirection ? source.AsQueryable().OrderByDescending(sortExpression2) : source.AsQueryable().OrderBy(sortExpression2);
            }
        }
        public static IQueryable<T> SortThenBy<T>(this IOrderedQueryable<T> source, Dictionary<string, bool> sortThenBy)
        {
            foreach (var sort in sortThenBy)
            {
                var param = Expression.Parameter(typeof(T), "item");
                var propertyInfo = typeof(T).GetProperty(sort.Key);

                switch (propertyInfo.PropertyType.ToString().ToLower(new CultureInfo("en-US", false)))
                {
                    case "system.datetime":
                        var sortExpression = Expression.Lambda<Func<T, DateTime>>(Expression.Convert(Expression.Property(param, sort.Key), typeof(DateTime)), param);
                        source = sort.Value ? source.ThenByDescending(sortExpression) : source.ThenBy(sortExpression);
                        break;

                    case "system.guid":
                        var sortExpression2 = Expression.Lambda<Func<T, Guid>>(Expression.Convert(Expression.Property(param, sort.Key), typeof(Guid)), param);
                        source = sort.Value ? source.ThenByDescending(sortExpression2) : source.ThenBy(sortExpression2);
                        break;

                    case "system.int32":
                        var sortExpression3 = Expression.Lambda<Func<T, int>>(Expression.Convert(Expression.Property(param, sort.Key), typeof(int)), param);
                        source = sort.Value ? source.ThenByDescending(sortExpression3) : source.ThenBy(sortExpression3);
                        break;

                    case "system.string":
                        var sortExpression4 = Expression.Lambda<Func<T, string>>(Expression.Convert(Expression.Property(param, sort.Key), typeof(string)), param);
                        source = sort.Value ? source.ThenByDescending(sortExpression4) : source.ThenBy(sortExpression4);
                        break;

                    default:
                        var sortExpression5 = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(param, sort.Key), typeof(object)), param);
                        source = sort.Value ? source.ThenByDescending(sortExpression5) : source.ThenBy(sortExpression5);
                        break;
                }
            }

            return source;
        }
    }

    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;
        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }
        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (this.map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }
}
