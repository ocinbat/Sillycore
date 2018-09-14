namespace Sillycore.EntityFramework.Extensions
{
    public interface IEntityEventPublisher
    {
        void SubscribeListener(IEntityEventListener listener);
        void UnsubscribeListener(IEntityEventListener listener);
    }
}