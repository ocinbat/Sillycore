using System;
using System.Collections.Generic;
using System.Text;

namespace Sillycore.Web.Security
{
    public class SillycoreAuthenticationOptions
    {
        public string AuthorityConfigKey { get; }
        public bool RequiresHttpsMetadata { get; private set; } = false;
        public bool LegacyAudienceValidation { get; private set; } = true;

        public SillycoreAuthenticationOptions(string authorityConfigKey)
        {
            this.AuthorityConfigKey = authorityConfigKey;
        }

        public SillycoreAuthenticationOptions RequireHttpsMetadata(bool require)
        {
            RequiresHttpsMetadata = require;
            return this;
        }

        public SillycoreAuthenticationOptions WithLegacyAudienceValidation(bool isLegacyAudienceValidation)
        {
            LegacyAudienceValidation = isLegacyAudienceValidation;
            return this;
        }
    }
}
