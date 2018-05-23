using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Abstractions;

namespace Sillycore.Web.Cors
{
    public class CorsServiceConfigurator : IServiceConfigurator
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors();
        }
    }
}