using Microsoft.EntityFrameworkCore;

namespace Sillycore.EntityFramework
{
    public abstract class DataContextBase : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(SillycoreApp.Instance.Configuration[Constants.ConnectionStringKey]);

            base.OnConfiguring(optionsBuilder);
        }
    }
}
