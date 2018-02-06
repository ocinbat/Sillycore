using RestSharp;
using Sillycore.RestClient.ResponseHandlers;

namespace Sillycore.RestClient
{
    public static class RestSharpExtensions
    {
        public static RestResponseHandler When(this IRestResponse response)
        {
            return new RestResponseHandler(response);
        }
    }
}
