using Sillycore.Configuration;

namespace Sillycore.Paging
{
    [Configuration("Sillycore:Paging")]
    public class PagingConfiguration
    {
        public int DefaultPageSize { get; set; } = 20;
    }
}