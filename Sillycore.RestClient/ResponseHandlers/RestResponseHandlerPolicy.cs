using System;
using RestSharp;

namespace Sillycore.RestClient.ResponseHandlers
{
    public class RestResponseHandlerPolicy
    {
        private readonly Func<IRestResponse, bool> _predicate;
        private readonly Action<IRestResponse> _action;
        private readonly IRestResponse _restResponse;

        private RestResponseHandlerPolicy _next;

        public RestResponseHandlerPolicy(IRestResponse restResponse, Func<IRestResponse, bool> predicate, Action<IRestResponse> action)
        {
            _predicate = predicate;
            _action = action;
            _restResponse = restResponse;
        }

        public RestResponseHandlerPolicy Next(RestResponseHandlerPolicy next)
        {
            _next = next;
            return _next;
        }

        public bool Evaluate()
        {
            if (_predicate(_restResponse))
            {
                _action(_restResponse);
                return true;
            }
            else
            {
                return _next?.Evaluate() ?? false;

            }
        }
    }
}
