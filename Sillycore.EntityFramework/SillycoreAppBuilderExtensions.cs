using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.EntityFramework
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseDataContext<TContext>(this SillycoreAppBuilder builder, string connectionStringKey)
            where TContext : DataContextBase
        {
            string connectionString = builder.Configuration[connectionStringKey];

            builder.Services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<TContext>((ob) =>
                {
                    ob.UseSqlServer(connectionString);
                });

            return builder;
        }
    }
}
