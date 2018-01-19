using System;
using System.Collections.Generic;
using System.Text;

namespace Sillycore.Web.Security
{
    public class SillycoreSecurityOptions
    {
        public string Authority { get; }
        public bool RequiresHttpsMetadata { get; private set; } = false;
        public bool LegacyAudienceValidation { get; private set; } = true;

        private readonly List<SillycoreSecurityPolicy> _policies;
        public IReadOnlyList<SillycoreSecurityPolicy> Policies => _policies.AsReadOnly();

        public SillycoreSecurityOptions(string authority)
        {
            this.Authority = authority;
            this._policies = new List<SillycoreSecurityPolicy>();
        }

        public SillycoreSecurityOptions DoesRequireHttpsMetadata(bool doesRequire)
        {
            RequiresHttpsMetadata = doesRequire;
            return this;
        }

        public SillycoreSecurityOptions WithLegacyAudienceValidation(bool isLegacyAudienceValidation)
        {
            LegacyAudienceValidation = isLegacyAudienceValidation;
            return this;
        }

        public SillycoreSecurityOptions AddPolicy(SillycoreSecurityPolicy policy)
        {
            _policies.Add(policy);
            return this;
        }
    }
}
