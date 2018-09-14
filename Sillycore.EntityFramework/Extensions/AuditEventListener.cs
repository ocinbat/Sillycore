using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sillycore.Domain.Abstractions;

namespace Sillycore.EntityFramework.Extensions
{
    public class AuditEventListener : IEntityEventListener
    {
        private readonly bool _setUpdatedOnSameAsCreatedOnForNewObjects;

        public AuditEventListener(bool setUpdatedOnSameAsCreatedOnForNewObjects)
        {
            _setUpdatedOnSameAsCreatedOnForNewObjects = setUpdatedOnSameAsCreatedOnForNewObjects;
        }

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
                    IAuditable auditable = ((IAuditable)entry.Entity);

                    if (entry.State == EntityState.Added)
                    {
                        if (auditable.CreatedOn == DateTime.MinValue)
                        {
                            auditable.CreatedOn = SillycoreApp.Instance.DateTimeProvider.Now;

                            if (_setUpdatedOnSameAsCreatedOnForNewObjects)
                            {
                                auditable.UpdatedOn = auditable.CreatedOn;
                            }
                        }

                        if (String.IsNullOrEmpty(auditable.CreatedBy))
                        {
                            auditable.CreatedBy = currentUser;

                            if (_setUpdatedOnSameAsCreatedOnForNewObjects)
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