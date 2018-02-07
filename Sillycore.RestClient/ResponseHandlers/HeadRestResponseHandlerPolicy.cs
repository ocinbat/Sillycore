using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;

namespace Sillycore.RestClient.ResponseHandlers
{
    internal class HeadRestResponseHandlerPolicy : RestResponseHandlerPolicy
    {
        public static HeadRestResponseHandlerPolicy Instance { get; } = new HeadRestResponseHandlerPolicy();

        public HeadRestResponseHandlerPolicy()
            : base(null, response => false, response => { })
        {
        }




    }
}
