using Sillycore.Configuration;
using Swashbuckle.AspNetCore.Swagger;

namespace Sillycore.Web.Swagger
{
    [Configuration("Sillycore:Swagger")]
    public class SwaggerConfiguration
    {
        public string Title { get; set; } = SillycoreAppBuilder.Instance?.DataStore.Get<string>(Constants.ApplicationName);
        public string Description { get; set; }
        public string Version { get; set; } = "v1.0";
        public string TermsOfService { get; set; }
        public SwaggerContact Contact { get; set; }
        public SwaggerLicense License { get; set; }

        public Info GetSwaggerInfo()
        {
            return new Info()
            {
                Description = Description,
                Version = Version,
                TermsOfService = TermsOfService,
                Title = Title,
                Contact = new Contact()
                {
                    Url = Contact?.Url,
                    Name = Contact?.Name,
                    Email = Contact?.Email
                },
                License = new License()
                {
                    Name = License?.Name,
                    Url = License?.Url
                }
            };
        }
    }

    public class SwaggerContact
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class SwaggerLicense
    {
        public string Url { get; set; }
        public string Name { get; set; }
    }
}