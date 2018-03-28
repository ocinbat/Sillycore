using System.Threading.Tasks;
using Sillycore.DependencyInjection;

namespace ConsoleApp.Helpers
{
    public class SomeHelper : ISingleton
    {
        public Task Help()
        {
            throw new System.NotImplementedException();
        }
    }
}