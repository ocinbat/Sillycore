namespace Sillycore.RabbitMq
{
    public static class SillycoreAppBuilderExtensions
    {
        public static SillycoreRabbitMqBuilder UseRabbitMq(this SillycoreAppBuilder builder, string urlConfigKey, string usernameConfigKey, string passwordConfigKey)
        {
            return new SillycoreRabbitMqBuilder(builder, builder.Configuration[urlConfigKey], builder.Configuration[usernameConfigKey], builder.Configuration[passwordConfigKey]);
        }
    }
}