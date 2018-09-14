namespace Sillycore.EntityFramework.Extensions
{
    public interface IEntityEventListener
    {
        void NotifyBeforeSaveChanges(DataContextBase dataContext);
    }
}