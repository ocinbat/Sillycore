using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sillycore.Domain.Abstractions;

namespace Sillycore.EntityFramework.Extensions
{
    public class SoftDeleteEventListener : IEntityEventListener
    {
        public void NotifyBeforeSaveChanges(DataContextBase dataContext)
        {
            IEnumerable<EntityEntry> entries = dataContext.ChangeTracker.Entries().Where(x => x.Entity is ISoftDeletable && x.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                ISoftDeletable entity = (ISoftDeletable)entry.Entity;
                entity.IsDeleted = true;
            }
        }
    }
}