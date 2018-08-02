using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Sillycore.EntityFramework.Extensions;

namespace Sillycore.EntityFramework.Utils
{
    public class SimpleQueryBuilder<TEntity> where TEntity : class
    {
        private IQueryable<TEntity> _query;

        public SimpleQueryBuilder(IQueryable<TEntity> query)
        {
            _query = query;
        }

        public static SimpleQueryBuilder<T> For<T>(IQueryable<T> queryable) where T : class
        {
            return new SimpleQueryBuilder<T>(queryable);
        }

        #region Equals

        public SimpleQueryBuilder<TEntity> Equals<TMember>(Expression<Func<TEntity, TMember?>> expression, TMember? value)
          where TMember : struct
        {
            if (value.HasValue)
            {
                WhereEquals(expression, value.Value);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> Equals<TMember>(Expression<Func<TEntity, TMember>> expression, TMember? value)
            where TMember : struct
        {
            if (value.HasValue)
            {
                WhereEquals(expression, value.Value);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> Equals(Expression<Func<TEntity, string>> expression, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                WhereEquals(expression, value);
            }

            return this;
        }

        private void WhereEquals<TMember>(
            Expression<Func<TEntity, TMember>> expression, TMember value)
            where TMember : struct
        {
            var constant = Expression.Constant(value);
            Where(Expression.Equal, expression, constant);
        }

        private void WhereEquals<TMember>(
            Expression<Func<TEntity, TMember?>> expression, TMember value)
            where TMember : struct
        {
            var constant = Expression.Constant(value);
            var nullableConstants = Expression.Convert(constant, typeof(TMember?));
            Where(Expression.Equal, expression, nullableConstants);
        }

        private void WhereEquals(
            Expression<Func<TEntity, string>> expression, string value)
        {
            var constant = Expression.Constant(value);
            Where(Expression.Equal, expression, constant);
        }
        #endregion

        #region Strings Functions

        public SimpleQueryBuilder<TEntity> Like<TMember>(Expression<Func<TEntity, TMember>> expression, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _query = _query.Like(expression, value);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> StartsWith<TMember>(Expression<Func<TEntity, TMember>> expression, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                string likeValue = $"{value}*";
                _query = _query.Like(expression, likeValue);
            }

            return this;
        }

        #endregion

        #region GreaterThan

        public SimpleQueryBuilder<TEntity> GreaterThanOrEquals<TMember>(Expression<Func<TEntity, TMember>> expression,
           TMember? value)
           where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                Where(Expression.GreaterThanOrEqual, expression, constant);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> GreaterThanOrEquals<TMember>(Expression<Func<TEntity, TMember?>> expression,
            TMember? value)
            where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                var nullableConstants = Expression.Convert(constant, typeof(TMember?));
                Where(Expression.GreaterThanOrEqual, expression, nullableConstants);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> GreaterThan<TMember>(Expression<Func<TEntity, TMember>> expression,
            TMember? value)
            where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                Where(Expression.GreaterThan, expression, constant);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> GreaterThan<TMember>(Expression<Func<TEntity, TMember?>> expression,
            TMember? value)
            where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                var nullableConstants = Expression.Convert(constant, typeof(TMember?));
                Where(Expression.GreaterThan, expression, nullableConstants);
            }

            return this;
        }

        #endregion

        #region LessThan


        public SimpleQueryBuilder<TEntity> LessThanOrEquals<TMember>(Expression<Func<TEntity, TMember>> expression,
         TMember? value)
         where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                Where(Expression.LessThanOrEqual, expression, constant);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> LessThanOrEquals<TMember>(Expression<Func<TEntity, TMember?>> expression,
            TMember? value)
            where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                var nullableConstants = Expression.Convert(constant, typeof(TMember?));
                Where(Expression.LessThanOrEqual, expression, nullableConstants);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> LessThan<TMember>(Expression<Func<TEntity, TMember>> expression,
            TMember? value)
            where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                Where(Expression.LessThan, expression, constant);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> LessThan<TMember>(Expression<Func<TEntity, TMember?>> expression,
            TMember? value)
            where TMember : struct
        {
            if (value.HasValue)
            {
                var constant = Expression.Constant(value);
                var nullableConstants = Expression.Convert(constant, typeof(TMember?));
                Where(Expression.LessThan, expression, nullableConstants);
            }

            return this;
        }

        #endregion

        #region In

        public SimpleQueryBuilder<TEntity> In<TMember>(Expression<Func<TEntity, TMember>> member, TMember[] value)
        {
            if (value?.Length > 0)
            {
                var inExpressions = CreateInExpression(member, value);
                var param = inExpressions.param;
                var whereExpression = Expression.Lambda<Func<TEntity, bool>>(inExpressions.call, param);
                _query = _query.Where(whereExpression);
            }

            return this;
        }

        public SimpleQueryBuilder<TEntity> NotIn<TMember>(Expression<Func<TEntity, TMember>> member, TMember[] value)
        {
            if (value?.Length > 0)
            {
                var inExpressions = CreateInExpression(member, value);
                var param = inExpressions.param;
                var finalExpression = Expression.Not(inExpressions.call);
                var whereExpression = Expression.Lambda<Func<TEntity, bool>>(finalExpression, param);
                _query = _query.Where(whereExpression);
            }

            return this;
        }


        private static (ParameterExpression param, MethodCallExpression call) CreateInExpression<TMember>(Expression<Func<TEntity, TMember>> member, TMember[] value)
        {
            var param = Expression.Parameter(typeof(TEntity), "t");
            var containsMethod = typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(new Type[] { typeof(TMember) });
            var arrayExpression = Expression.Constant(value);
            var memberExpr = LinqExtensions.GetMemberExpression(member, param);
            var callExpression = Expression.Call(null, containsMethod, arrayExpression, memberExpr);
            return (param, callExpression);
        }

        #endregion

        public void Where(Func<Expression, Expression, BinaryExpression> comparisionExpression,
            Expression member, Expression value)
        {
            var param = Expression.Parameter(typeof(TEntity), "t");
            var memberExpr = LinqExtensions.GetMemberExpression(member, param);
            var finalExpression = comparisionExpression(memberExpr, value);
            var whereExpression = Expression.Lambda<Func<TEntity, bool>>(finalExpression, param);
            _query = _query.Where(whereExpression);
        }

        public IQueryable<TEntity> Queryable()
        {
            return _query;
        }
    }

    public class SimpleQueryBuilder
    {
        public static SimpleQueryBuilder<T> For<T>(IQueryable<T> queryable) where T : class
        {
            return new SimpleQueryBuilder<T>(queryable);
        }
    }
}