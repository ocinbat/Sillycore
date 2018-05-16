using GreenPipes;
using MassTransit;
using MassTransit.Context;
using MassTransit.Scoping;
using MassTransit.Scoping.ConsumerContexts;
using MassTransit.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Sillycore.RabbitMq
{
    internal class SillycoreConsumerScopeProvider : IConsumerScopeProvider
    {
        public void Probe(ProbeContext context)
        {
            context.Add("provider", "dependencyInjection");
        }

        public IConsumerScopeContext GetScope(ConsumeContext context)
        {
            IServiceScope payload;
            if (context.TryGetPayload<IServiceScope>(out payload))
                return (IConsumerScopeContext)new ExistingConsumerScopeContext(context);
            IServiceScope scope1 = SillycoreApp.Instance.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            try
            {
                ConsumeContextProxyScope contextProxyScope = new ConsumeContextProxyScope(context);
                IServiceScope scope = scope1;
                contextProxyScope.GetOrAddPayload<IServiceScope>((PayloadFactory<IServiceScope>)(() => scope));
                return (IConsumerScopeContext)new CreatedConsumerScopeContext<IServiceScope>(scope, (ConsumeContext)contextProxyScope);
            }
            catch
            {
                scope1.Dispose();
                throw;
            }
        }

        public IConsumerScopeContext<TConsumer, T> GetScope<TConsumer, T>(ConsumeContext<T> context) where TConsumer : class where T : class
        {
            if (context.TryGetPayload<IServiceScope>(out var payload))
            {
                TConsumer service = payload.ServiceProvider.GetService<TConsumer>();
                if ((object)service == null)
                    throw new ConsumerException(string.Format("Unable to resolve consumer type '{0}'.", (object)TypeMetadataCache<TConsumer>.ShortName));
                return (IConsumerScopeContext<TConsumer, T>)new ExistingConsumerScopeContext<TConsumer, T>(context.PushConsumer<TConsumer, T>(service));
            }
            IServiceScope scope = SillycoreApp.Instance.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            try
            {
                TConsumer service = scope.ServiceProvider.GetService<TConsumer>();
                if ((object)service == null)
                    throw new ConsumerException(string.Format("Unable to resolve consumer type '{0}'.", (object)TypeMetadataCache<TConsumer>.ShortName));
                ConsumerConsumeContext<TConsumer, T> context1 = context.PushConsumerScope<TConsumer, T, IServiceScope>(service, scope);
                return (IConsumerScopeContext<TConsumer, T>)new CreatedConsumerScopeContext<IServiceScope, TConsumer, T>(scope, context1);
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        }
    }
}