using System;
using System.Collections.Generic;
using System.Text;

namespace Sillycore.Web.Security
{
    public class SillycoreAuthorizationPolicy
    {
        public string Name { get; set; }

        public string[] RequiredScopes { get; set; }

        public static SillycoreAuthorizationPolicy Create(string name, params string[] requiredScopes)
        {
            SillycoreAuthorizationPolicy policy = new SillycoreAuthorizationPolicy()
            {
                Name = name,
                RequiredScopes = requiredScopes
            };

            return policy;
        }
    }
}
