using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sillycore.Domain.Abstractions;
using Sillycore.Extensions;

namespace Sillycore.EntityFramework.Mapping
{
    public abstract class EntityMappingConfiguration<T> : IEntityMappingConfiguration<T> where T : class
    {
        public abstract void Map(EntityTypeBuilder<T> b);

        public void Map(ModelBuilder builder)
        {
            if (typeof(T).Implements(typeof(ISoftDeletable)))
            {
                ParameterExpression argParam = Expression.Parameter(typeof(T), "e");
                Expression isDeletedProperty = Expression.Property(argParam, "IsDeleted");
                ConstantExpression trueValue = Expression.Constant(true);
                Expression expression = Expression.Equal(isDeletedProperty, trueValue);
                Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(expression, argParam);

                builder.Entity<T>().HasQueryFilter(lambda);
            }

            Map(builder.Entity<T>());
        }
    }
}
