using System;
using System.Collections.Generic;
using System.Text;
using Sillycore.Web.Security;

namespace Sillycore.Web
{
    public class SillycoreAuthenticationBuilder
    {
        private readonly SillycoreWebhostBuilder _sillycoreWebhostBuilder;
        private readonly InMemoryDataStore _dataStore;
        private readonly IList<SillycoreAuthorizationPolicy> _authorizationPolicies;

        public SillycoreAuthenticationBuilder(SillycoreWebhostBuilder sillycoreWebhostBuilder, InMemoryDataStore dataStore)
        {
            _sillycoreWebhostBuilder = sillycoreWebhostBuilder;
            _dataStore = dataStore;
            _authorizationPolicies = new List<SillycoreAuthorizationPolicy>();
        }

        public SillycoreAuthenticationBuilder As(string authServerConfigKey)
        {
            var options = new SillycoreAuthenticationOptions(authServerConfigKey);
            _dataStore.Set(Constants.AuthenticationOptions, options);

            return this;
        }

        public SillycoreAuthenticationBuilder WithPolicy(string policyName, params string[] scopes)
        {
            if (_dataStore.Get(Constants.AuthorizationOptions) == null)
            {
                _dataStore.Set(Constants.AuthorizationOptions, new SillycoreAuthorizationOptions());
            }

            var policy = SillycoreAuthorizationPolicy.Create(policyName, scopes);
            var authorizationOptions = _dataStore.Get<SillycoreAuthorizationOptions>(Constants.AuthorizationOptions);
            authorizationOptions.WithPolicy(policy);

            return this;
        }

        public SillycoreWebhostBuilder Then()
        {
            return _sillycoreWebhostBuilder;
        }
    }
}
