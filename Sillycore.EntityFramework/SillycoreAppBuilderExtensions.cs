using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.EntityFramework
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseEntityFramework<TContext>(this SillycoreAppBuilder builder, string connectionString)
            where TContext : DataContextBase
        {
            builder.Services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString));

            return builder;
        }
    }
}
