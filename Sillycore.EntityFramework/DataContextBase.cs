using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Sillycore.EntityFramework.Mapping;

namespace Sillycore.EntityFramework
{
    public abstract class DataContextBase : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddEntityConfigurationsFromAssembly(GetType().GetTypeInfo().Assembly);
        }
    }
}
