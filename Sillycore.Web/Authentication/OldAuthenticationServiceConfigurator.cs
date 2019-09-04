using Anetta.ServiceConfiguration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Web.Security;

namespace Sillycore.Web.Authentication
{
    public class OldAuthenticationServiceConfigurator : IServiceConfigurator
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            if (SillycoreAppBuilder.Instance.DataStore.Get<bool>(Constants.RequiresAuthentication))
            {
                ConfigureAuthentication(services);
                ConfigureAuthorization(services);
            }
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            var authorizationOptions = SillycoreAppBuilder.Instance.DataStore.Get<SillycoreAuthorizationOptions>(Constants.AuthorizationOptions);
            if (authorizationOptions != null)
            {
                services.AddAuthorization(options =>
                {
                    ConfigureAuthorizationPolicies(authorizationOptions, options);
                });
            }
        }

        private static void ConfigureAuthorizationPolicies(SillycoreAuthorizationOptions authorizationOptions,
            AuthorizationOptions options)
        {
            foreach (var authorizationPolicy in authorizationOptions.Policies)
            {
                options.AddPolicy(authorizationPolicy.Name, builder =>
                {
                    builder.RequireClaim("scope", authorizationPolicy.RequiredScopes);
                });
            }
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            var authenticationOptions = SillycoreAppBuilder.Instance.DataStore.Get<SillycoreAuthenticationOptions>(Constants.AuthenticationOptions);

            if (authenticationOptions != null)
            {
                var authority = SillycoreAppBuilder.Instance.DataStore.Get<IConfiguration>(Sillycore.Constants.Configuration).GetValue<string>(authenticationOptions.AuthorityConfigKey);

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authority;
                        options.RequireHttpsMetadata = authenticationOptions.RequiresHttpsMetadata;
                        if (authenticationOptions.LegacyAudienceValidation)
                        {
                            options.TokenValidationParameters.ValidateAudience = false;
                        }
                    });
            }
        }
    }
}