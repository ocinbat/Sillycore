using Microsoft.EntityFrameworkCore;

namespace Sillycore.EntityFramework.Tests.Stubs
{
    public class StubDataContextBase : DataContextBase
    {
        public StubDataContextBase(DbContextOptions options, SillycoreDataContextOptions sillycoreDataContextOptions) : base(options, sillycoreDataContextOptions)
        {
        }

        public StubDataContextBase(DbContextOptions options) : base(options)
        {
        }
    }
}