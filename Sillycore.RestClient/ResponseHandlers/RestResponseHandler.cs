using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using RestSharp;

namespace Sillycore.RestClient.ResponseHandlers
{
    public class RestResponseHandler
    {
        private readonly IRestResponse _response;
        private readonly RestResponseHandlerPolicy _headPolicy = HeadRestResponseHandlerPolicy.Instance;
        private RestResponseHandlerPolicy _tailPolicy = HeadRestResponseHandlerPolicy.Instance;

        public RestResponseHandler(IRestResponse response)
        {
            _response = response;
        }

        public RestResponseHandler Status(HttpStatusCode statusCode, Action<IRestResponse> action)
        {
            var policy = new RestResponseHandlerPolicy(restResponse: _response, predicate: restResponse => restResponse.StatusCode == statusCode, action: action);
            _tailPolicy = _tailPolicy.Next(policy);
            return this;
        }

        public RestResponseHandler BadRequest(Action<IRestResponse> action)
        {
            return Status(HttpStatusCode.BadRequest, action);
        }

        public RestResponseHandler Ok(Action<IRestResponse> action)
        {
            return Status(HttpStatusCode.OK, action);
        }

        public RestResponseHandler Created(Action<IRestResponse> action)
        {
            return Status(HttpStatusCode.Created, action);
        }

        public RestResponseHandler Conflict(Action<IRestResponse> action)
        {
            return Status(HttpStatusCode.Conflict, action);
        }

        public RestResponseHandler InternalServerError(Action<IRestResponse> action)
        {
            return Status(HttpStatusCode.InternalServerError, action);
        }

        public RestResponseHandler ServerError(Action<IRestResponse> action)
        {
            Func<IRestResponse, bool> isServerError = (response) =>
            {
                var statusCode = (int) response.StatusCode;
                return statusCode >= 500 && statusCode < 600;
            };
            var policy = new RestResponseHandlerPolicy(_response, isServerError, action);
            _tailPolicy = _tailPolicy.Next(policy);
            return this;
        }

        public RestResponseHandler ClientError(Action<IRestResponse> action)
        {
            Func<IRestResponse, bool> isClientError = (response) =>
            {
                var statusCode = (int)response.StatusCode;
                return statusCode >= 400 && statusCode < 500;
            };
            var policy = new RestResponseHandlerPolicy( _response, isClientError, action);
            _tailPolicy = _tailPolicy.Next(policy);
            return this;
        }

        public bool Evaluate()
        {
            return _headPolicy.Evaluate();
        }

     
    }
}
