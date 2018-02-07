using System.Net;
using Moq;
using NUnit.Framework;
using RestSharp;
using Sillycore.RestClient.ResponseHandlers;

namespace Sillycore.RestClient.Tests
{
    [TestFixture]
    public class RestResponseHandlerTests
    {
        [Test]
        public void Status_Should_ReturnRestResponseHandler()
        {
            IRestResponse response = Mock.Of<IRestResponse>();
            var sut = new RestResponseHandler(response);
            var restResponseHandler = sut.Status(HttpStatusCode.OK, (res) => { return; });
            Assert.AreEqual(sut, restResponseHandler);
        }

        [Test]
        public void Evaluate_WhenConditionsIsMet_ShouldHandle()
        {
            IRestResponse response = Mock.Of<IRestResponse>();
            response.StatusCode = HttpStatusCode.BadRequest;

            var sut = new RestResponseHandler(response);
            var result = HttpStatusCode.Continue;
            sut = sut.Status(HttpStatusCode.OK, (res) => result = HttpStatusCode.OK)
                .Status(HttpStatusCode.BadRequest, (res => result = HttpStatusCode.BadRequest))
                .Status(HttpStatusCode.InternalServerError, res => result = HttpStatusCode.InternalServerError);
            var handled =  sut.Evaluate();

            Assert.AreEqual(true, handled);
            Assert.AreEqual(response.StatusCode, result);
        }

        [Test]
        public void ClientError_WhenConfigured_ShouldHandle4XX()
        {
            IRestResponse badResponse = Mock.Of<IRestResponse>();
            badResponse.StatusCode = HttpStatusCode.BadRequest;

            var sut = new RestResponseHandler(badResponse);
            sut.ClientError(restResponse => {});
            var handled = sut.Evaluate();

            Assert.AreEqual(true, handled);
        }

        [Test]
        public void ClientError_WhenConfigured_ShouldNotHandleNon4XX()
        {
            IRestResponse badResponse = Mock.Of<IRestResponse>();
            badResponse.StatusCode = HttpStatusCode.TemporaryRedirect;

            IRestResponse internalServerErrorResponse = Mock.Of<IRestResponse>();
            internalServerErrorResponse.StatusCode = HttpStatusCode.InternalServerError;

            var sut = new RestResponseHandler(badResponse);
            sut.ClientError(restResponse => {});
            var handled = sut.Evaluate();

            Assert.AreEqual(false, handled);

            sut = new RestResponseHandler(internalServerErrorResponse);
            sut.ClientError(restResponse => { });
            handled = sut.Evaluate();

            Assert.AreEqual(false, handled);
        }

        [Test]
        public void ServerError_WhenConfigured_ShouldHandle5XX()
        {
            IRestResponse internalServerErrorResponse = Mock.Of<IRestResponse>();
            internalServerErrorResponse.StatusCode = HttpStatusCode.InternalServerError;

            var sut = new RestResponseHandler(internalServerErrorResponse);
            sut.ServerError(restResponse => { });
            var handled = sut.Evaluate();

            Assert.AreEqual(true, handled);
        }

        [Test]
        public void ServerError_WhenConfigured_ShouldNotHandleNon5XX()
        {
            IRestResponse upgradeRequiredResponse = Mock.Of<IRestResponse>();
            upgradeRequiredResponse.StatusCode = HttpStatusCode.UpgradeRequired;

            var sut = new RestResponseHandler(upgradeRequiredResponse);
            sut.ServerError(restResponse => { });
            var handled = sut.Evaluate();

            Assert.AreEqual(false, handled);
        }
    }
}
