using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.EntityFramework
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseEntityFramework<TContext>(this SillycoreAppBuilder builder, Action<DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            builder.Services.AddDbContext<TContext>(optionsAction);

            return builder;
        }
    }
}
