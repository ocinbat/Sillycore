using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.Abstractions
{
    public interface IServiceConfigurator
    {
        void Configure(IServiceCollection services, IConfiguration configuration);
    }
}