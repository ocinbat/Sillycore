using Microsoft.EntityFrameworkCore;
using Sillycore.Domain.Abstractions;
using System;

namespace Sillycore.EntityFramework.Tests.Stubs
{
    public class StubDataContextBase : DataContextBase
    {
        public StubDataContextBase(DbContextOptions options, SillycoreDataContextOptions sillycoreDataContextOptions) :
            base(options, sillycoreDataContextOptions)
        {
        }

        public StubDataContextBase(DbContextOptions options) : base(options)
        {
        }

        public void MarkAsModified<T>(T item) where T : class
        {
            Entry(item).State = EntityState.Modified;
        }

        public virtual DbSet<StubEntity> StubEntities { get; set; }
    }

    public class StubEntity : IEntity<long>, IAuditable
    {
        public long Id { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }
}