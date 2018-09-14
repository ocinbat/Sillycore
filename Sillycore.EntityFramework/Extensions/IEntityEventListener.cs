using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Sillycore.EntityFramework.Extensions
{
    public interface IEntityEventListener
    {
        void NotifyBeforeSaveChanges(DataContextBase dataContext);
    }
}