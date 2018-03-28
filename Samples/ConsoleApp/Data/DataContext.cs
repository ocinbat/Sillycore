using ConsoleApp.Entities;
using Microsoft.EntityFrameworkCore;
using Sillycore.EntityFramework;

namespace ConsoleApp.Data
{
    public class DataContext : DataContextBase
    {
        public DataContext(DbContextOptions options) 
            : base(options)
        {
        }

        public virtual DbSet<Sample> Samples { get; set; }
    }
}