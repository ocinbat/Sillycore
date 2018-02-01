namespace Sillycore.RabbitMq
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreRabbitMqBuilder UseRabbitMq(this SillycoreAppBuilder builder, string url, string username, string password)
        {
            return new SillycoreRabbitMqBuilder(builder, url, username, password);
        }
    }
}