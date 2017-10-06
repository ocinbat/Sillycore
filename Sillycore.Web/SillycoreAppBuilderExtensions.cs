namespace Sillycore.Web
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreWebhostBuilder UseWebApi(this SillycoreAppBuilder builder, string applicationName, string[] args = null)
        {
            return new SillycoreWebhostBuilder(builder, applicationName, args);
        }
    }
}