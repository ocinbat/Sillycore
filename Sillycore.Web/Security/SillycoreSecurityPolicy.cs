using System;
using System.Collections.Generic;
using System.Text;

namespace Sillycore.Web.Security
{
    public class SillycoreSecurityPolicy
    {
        public string Name { get; set; }

        public string[] RequiredScopes { get; set; }

        private SillycoreSecurityPolicy()
        {
        }

        public static SillycoreSecurityPolicy Create(string name, params string[] requiredScopes)
        {
            SillycoreSecurityPolicy policy = new SillycoreSecurityPolicy()
            {
                Name = name,
                RequiredScopes = requiredScopes
            };

            return policy;
        }
    }
}
