using System;
using System.Collections.Generic;
using System.Text;

namespace Sillycore.Web.Security
{
    public class SillycoreAuthorizationOptions
    {
        private readonly List<SillycoreAuthorizationPolicy> _policies;

        public IReadOnlyList<SillycoreAuthorizationPolicy> Policies => _policies.AsReadOnly();

        public SillycoreAuthorizationOptions()
        {
            this._policies = new List<SillycoreAuthorizationPolicy>();
        }

        public SillycoreAuthorizationOptions WithPolicy(SillycoreAuthorizationPolicy policy)
        {
            _policies.Add(policy);
            return this;
        }
    }
}
