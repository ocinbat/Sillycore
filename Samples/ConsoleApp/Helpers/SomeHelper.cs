using System.Threading.Tasks;
using Anetta.Attributes;

namespace ConsoleApp.Helpers
{
    [Singleton]
    public class SomeHelper
    {
        public Task Help()
        {
            throw new System.NotImplementedException();
        }
    }
}