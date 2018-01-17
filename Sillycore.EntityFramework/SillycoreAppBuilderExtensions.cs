using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.EntityFramework
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreAppBuilder UseEntityFramework<TContext>(this SillycoreAppBuilder builder, string connectionStringKey = "ConnectionString")
            where TContext : DataContextBase
        {
            builder.DataStore.Set(Constants.ConnectionStringKey, connectionStringKey);

            builder.Services.AddEntityFrameworkSqlServer().AddDbContext<TContext>();

            return builder;
        }
    }
}
