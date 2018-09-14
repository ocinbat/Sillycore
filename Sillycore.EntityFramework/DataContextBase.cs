using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sillycore.Domain.Abstractions;
using Sillycore.EntityFramework.Attributes;
using Sillycore.EntityFramework.Extensions;
using Sillycore.EntityFramework.Mapping;

namespace Sillycore.EntityFramework
{
    public abstract class DataContextBase : DbContext, IEntityEventPublisher
    {
        private long _inMemorySequenceId;
        internal List<IEntityEventListener> EventListeners { get; }

        protected DataContextBase(DbContextOptions options, SillycoreDataContextOptions sillycoreDataContextOptions)
            : base(options)
        {
            EventListeners = new List<IEntityEventListener>();
            if (sillycoreDataContextOptions.UseDefaultEventListeners)
            {
                EventListeners.Add(new AuditEventListener(sillycoreDataContextOptions.SetUpdatedOnSameAsCreatedOnForNewObjects));
                EventListeners.Add(new SoftDeleteEventListener());
            }
        }

        protected DataContextBase(DbContextOptions options)
            : this(options, new SillycoreDataContextOptions())
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
            NotifyBeforeSaveChangesEvent();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await SaveChangesAsync(true, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            NotifyBeforeSaveChangesEvent();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void NotifyBeforeSaveChangesEvent()
        {
            foreach (var listener in EventListeners)
            {
                listener.NotifyBeforeSaveChanges(this);
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
        public virtual long GetNextIdRange<TEntity, TId>(int rangeSize) where TEntity : IEntity<long>
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


        public void SubscribeListener(IEntityEventListener listener)
        {
            if (!EventListeners.Contains(listener))
            {
                EventListeners.Add(listener);
            }
        }

        public void UnsubscribeListener(IEntityEventListener listener)
        {
            if (EventListeners.Contains(listener))
            {
                EventListeners.Remove(listener);
            }
        }
    }
}
