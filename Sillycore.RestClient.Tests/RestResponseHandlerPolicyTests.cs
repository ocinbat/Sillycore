using Moq;
using NUnit.Framework;
using RestSharp;
using Sillycore.RestClient.ResponseHandlers;

namespace Sillycore.RestClient.Tests
{
    [TestFixture()]
    public class RestResponseHandlerPolicyTests
    {
        private IRestResponse _mockRestResponse;

        [OneTimeSetUp]
        public void Init()
        {
            _mockRestResponse = Mock.Of<IRestResponse>();
        }

        [Test]
        public void Next_ShouldSetAlternatePolicy()
        {
            var sut = new RestResponseHandlerPolicy(_mockRestResponse, (restResponse => true), restResponse => { });
            var alternatePolicy = new RestResponseHandlerPolicy(_mockRestResponse, (restResponse => true), restResponse => { });
            var returnVal = sut.Next(alternatePolicy);
            Assert.AreSame(alternatePolicy, returnVal);
        }

        [Test]
        public void Evaluate_WhenMatched_ShouldNotCallAlternatePolicy()
        {
            IRestResponse response = Mock.Of<IRestResponse>();
            int result = 0;
            var sut = new RestResponseHandlerPolicy(_mockRestResponse, (restResponse => true), restResponse => { result = 1; });
            var alternatePolicy = new RestResponseHandlerPolicy(_mockRestResponse, (restResponse => true),
                restResponse => { result = 2; });
            var handled = sut.Evaluate();
            sut.Next(alternatePolicy);
            Assert.IsTrue(handled);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Evaluate_WhenNotMatchedAndAlteratePolicyExists_ShouldCallAlternatePolicy()
        {
            int result = 0;
            var sut = new RestResponseHandlerPolicy(_mockRestResponse, (restResponse => false), restResponse => { result = 1; });
            var alternatePolicy = new RestResponseHandlerPolicy(_mockRestResponse, (restResponse => true),
                restResponse => { result = 2; });
            sut.Next(alternatePolicy);
            var handled = sut.Evaluate();
            Assert.IsTrue(handled);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void Evaluate_WhenNotMatchedAndAlternatePolicyDoesntExist_ShouldReturnFalse()
        {
            var sut = new RestResponseHandlerPolicy(_mockRestResponse, (restResponse => false), restResponse => {});
            var handled = sut.Evaluate();
            Assert.IsFalse(handled);
        }

    }

}
