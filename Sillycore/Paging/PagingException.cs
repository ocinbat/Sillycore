using System;

namespace Sillycore.Paging
{
    public class PagingException : Exception
    {
        public PagingException(string message)
            : base(message)
        {
        }
    }
}