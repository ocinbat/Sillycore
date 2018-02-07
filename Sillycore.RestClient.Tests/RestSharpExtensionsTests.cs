using NUnit.Framework;
using RestSharp;
using Sillycore.RestClient.ResponseHandlers;

namespace Sillycore.RestClient.Tests
{
    [TestFixture]
    public class RestSharpExtensionsTests
    {
        [Test]
        public void RestResponse_When_ShouldAlways_ReturnResponseHandler()
        {
            IRestResponse sut = new RestResponse();
            var responseHandler = sut.When();
            Assert.IsInstanceOf<RestResponseHandler>(responseHandler);
        }

        
    }

   
}
