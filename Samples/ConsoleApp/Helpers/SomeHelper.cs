using System.Threading.Tasks;
using Sillycore.DependencyInjection;

namespace ConsoleApp.Helpers
{
    public class SomeHelper : IHelper, ITransient
    {
        public Task Help()
        {
            throw new System.NotImplementedException();
        }
    }
}