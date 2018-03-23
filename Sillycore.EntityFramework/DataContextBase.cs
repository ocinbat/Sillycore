using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Sillycore.EntityFramework.Mapping;

namespace Sillycore.EntityFramework
{
    public abstract class DataContextBase : DbContext
    {
        protected DataContextBase(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEntityConfigurationsFromAssembly(GetType().GetTypeInfo().Assembly, GetType());
        }
    }
}
