using Microsoft.EntityFrameworkCore;

namespace Sillycore.EntityFramework
{
    public abstract class DataContextBase : DbContext
    {
        protected DataContextBase(DbContextOptions options) : base(options)
        {
        }
    }
}
