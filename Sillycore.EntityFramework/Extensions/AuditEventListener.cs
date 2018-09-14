using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sillycore.Domain.Abstractions;

namespace Sillycore.EntityFramework.Extensions
{
    public class AuditEventListener : IEntityEventListener
    {
        protected virtual string GetCurrentUserName()
        {
            return Thread.CurrentPrincipal?.Identity?.Name;
        }

        public void NotifyBeforeSaveChanges(DataContextBase dataContext)
        {
            string currentUser = GetCurrentUserName();
            
            IEnumerable<EntityEntry> entities = dataContext.ChangeTracker.Entries().Where(x => x.Entity is IAuditable && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (EntityEntry entry in entities)
            {
                if (entry.Entity is IAuditable)
                {
                    IAuditable auditable = (IAuditable)entry.Entity;

                    if (entry.State == EntityState.Added)
                    {
                        if (auditable.CreatedOn == DateTime.MinValue)
                        {
                            auditable.CreatedOn = SillycoreApp.Instance.DateTimeProvider.Now;

                            if (dataContext.SetUpdatedOnSameAsCreatedOnForNewObjects)
                            {
                                auditable.UpdatedOn = auditable.CreatedOn;
                            }
                        }

                        if (string.IsNullOrEmpty(auditable.CreatedBy))
                        {
                            auditable.CreatedBy = currentUser;

                            if (dataContext.SetUpdatedOnSameAsCreatedOnForNewObjects)
                            {
                                auditable.UpdatedBy = auditable.CreatedBy;
                            }
                        }
                    }
                    else
                    {
                        auditable.UpdatedOn = SillycoreApp.Instance.DateTimeProvider.Now;
                        auditable.UpdatedBy = currentUser;
                    }
                }
            }
        }
    }
}