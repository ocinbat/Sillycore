using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Sillycore.Domain.Abstractions;
using Sillycore.EntityFramework.Attributes;
using Sillycore.EntityFramework.Mapping;
using Sillycore.Extensions;

namespace Sillycore.EntityFramework
{
    public abstract class DataContextBase : DbContext
    {
        private long _inMemorySequenceId;

        protected bool SetUpdatedOnSameAsCreatedOnForNewObjects { get; set; }

        protected DataContextBase(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEntityConfigurationsFromAssembly(GetType().GetTypeInfo().Assembly);
        }

        public override int SaveChanges()
        {
            return SaveChanges(true);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            HandleSoftDeletableEntities();
            HandleAuditableEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await SaveChangesAsync(true, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            HandleSoftDeletableEntities();
            HandleAuditableEntities();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void HandleSoftDeletableEntities()
        {
            IEnumerable<EntityEntry> entries = ChangeTracker?.Entries()?.Where(x => x.Entity is ISoftDeletable && x.State == EntityState.Deleted);

            if (entries.HasElements())
            {
                foreach (var entry in entries)
                {
                    entry.State = EntityState.Modified;
                    ISoftDeletable entity = (ISoftDeletable)entry.Entity;
                    entity.IsDeleted = true;
                }
            }
        }

        private void HandleAuditableEntities()
        {
            string currentUser = Thread.CurrentPrincipal?.Identity?.Name;

            IEnumerable<EntityEntry> entities = ChangeTracker?.Entries()?.Where(x => x.Entity is IAuditable && (x.State == EntityState.Added || x.State == EntityState.Modified));

            if (entities.HasElements())
            {
                foreach (EntityEntry entry in entities)
                {
                    if (entry.Entity is IAuditable)
                    {
                        IAuditable auditable = ((IAuditable)entry.Entity);

                        if (entry.State == EntityState.Added)
                        {
                            if (auditable.CreatedOn == DateTime.MinValue)
                            {
                                auditable.CreatedOn = SillycoreApp.Instance?.DateTimeProvider?.Now ?? DateTime.UtcNow;

                                if (SetUpdatedOnSameAsCreatedOnForNewObjects)
                                {
                                    auditable.UpdatedOn = auditable.CreatedOn;
                                }
                            }

                            if (String.IsNullOrEmpty(auditable.CreatedBy))
                            {
                                auditable.CreatedBy = currentUser;

                                if (SetUpdatedOnSameAsCreatedOnForNewObjects)
                                {
                                    auditable.UpdatedBy = auditable.CreatedBy;
                                }
                            }
                        }
                        else
                        {
                            auditable.UpdatedOn = SillycoreApp.Instance?.DateTimeProvider?.Now ?? DateTime.UtcNow;
                            auditable.UpdatedBy = currentUser;
                        }
                    }
                }
            }
        }

        public virtual long GetNextId<TEntity>() where TEntity : IEntity<long>
        {
            if (this.Database.IsInMemory())
            {
                return ++_inMemorySequenceId;
            }

            bool shouldOpenConnection = this.Database.GetDbConnection().State == ConnectionState.Closed;

            try
            {
                if (shouldOpenConnection)
                {
                    this.Database.OpenConnection();
                }

                var sequenceName = GetSequenceName<TEntity>();
                
                var sql = $"SELECT (NEXT VALUE FOR {sequenceName})";
                
                return GetSequenceId(sequenceName, sql);
            }
            finally
            {
                if (shouldOpenConnection)
                {
                    this.Database.CloseConnection();
                }
            }
        }
        public virtual long GetNextIdRange<TEntity, TId>(int rangeSize)  where TEntity : IEntity<long>
        {
            if (this.Database.IsInMemory())
            {
                return _inMemorySequenceId = rangeSize + _inMemorySequenceId;
            } 

            bool shouldOpenConnection = this.Database.GetDbConnection().State == ConnectionState.Closed;

            try
            {
                if (shouldOpenConnection)
                {
                    this.Database.OpenConnection();
                }

                var sequenceName = GetSequenceName<TEntity>();

                var dbCommmand = this.Database.GetDbConnection().CreateCommand();
                var sql = $@"
                DECLARE @range_first_value sql_variant ,   
                        @range_first_value_output sql_variant ;  
                
                EXEC sp_sequence_get_range  
                @sequence_name = N'{sequenceName}'  
                , @range_size = {rangeSize} 
                , @range_first_value = @range_first_value_output OUTPUT ;  
                
                SELECT @range_first_value_output AS FirstNumber ;";
                dbCommmand.CommandText = sql;

                return GetSequenceId(sequenceName, sql);
            }
            finally
            {
                if (shouldOpenConnection)
                {
                    this.Database.CloseConnection();
                }
            }
        }
        private long GetSequenceId(string sequenceName, string sql)
        {
            var dbCommmand = this.Database.GetDbConnection().CreateCommand();
            dbCommmand.CommandText = sql;

            var id = (long)dbCommmand.ExecuteScalar();
            if (id == 0)
            {
                throw new InvalidOperationException($"Database did not return an instance of identity for sequence {sequenceName}.");
            }
            return id;
        }

        private static string GetSequenceName<TEntity>() where TEntity : IEntity<long>
        {
            var customAttribute = typeof(TEntity).GetCustomAttribute<SequenceAttribute>();
            if (customAttribute == null)
            {
                throw new InvalidOperationException("You need to decorate your entity with Sequence attribute to use this extension method.");
            }
            var sequenceName = customAttribute.Name;
            return sequenceName;
        }

      
    }
}
