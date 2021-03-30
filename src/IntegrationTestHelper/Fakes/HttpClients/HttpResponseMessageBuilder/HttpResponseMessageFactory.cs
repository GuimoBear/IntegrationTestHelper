using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTestHelper.Caches;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder
{
    internal class HttpResponseMessageFactory : IEquatable<HttpResponseMessageFactory>
    {
        private readonly Type typedClientType;
        private readonly string predicateStringRepresentation;
        private readonly Predicate<HttpRequestMessage> requestPredicate;

        private Func<HttpRequestMessage, HttpResponseMessage> responseFactory;
        private Func<HttpRequestMessage, Exception> exceptionFactory;

        private ICollection<Action<HttpRequestMessage>> interceptors = new List<Action<HttpRequestMessage>>();

        internal HttpResponseMessageFactory(Type typedClientType, Expression<Predicate<HttpRequestMessage>> requestPredicate)
        {
            if (requestPredicate is null)
                throw new ArgumentNullException(nameof(requestPredicate));
            this.typedClientType = typedClientType;
            predicateStringRepresentation = requestPredicate.ToString();
            this.requestPredicate = PredicateCache.GetCachedPredicate(typedClientType, requestPredicate);
        }

        internal bool Match(Type clientType, HttpRequestMessage requestMessage)
            => (typedClientType == default || typedClientType == clientType) && requestPredicate(requestMessage);

        internal void AddInterceptor(Action<HttpRequestMessage> interceptor)
        {
            if (interceptor is not null)
                interceptors.Add(interceptor);
        }

        internal void AddAsyncInterceptor(Func<HttpRequestMessage, Task> asyncInterceptor)
        {
            if (asyncInterceptor is not null)
                interceptors.Add(requestMessage => asyncInterceptor(requestMessage).Wait());
        }

        internal void Return(Func<HttpResponseMessage> responseFactory)
        {
            if (responseFactory is not null)
                this.responseFactory = _ => responseFactory();
        }

        internal void Return(Func<Task<HttpResponseMessage>> responseAsyncFactory)
        {
            if (responseAsyncFactory is not null)
                responseFactory = _ =>
                {
                    var task = responseAsyncFactory();
                    task.Wait();
                    return task.Result;
                };
        }

        internal void Return(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            if (responseFactory is not null)
                this.responseFactory = requestMessage => responseFactory(requestMessage);
        }

        internal void Return(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseAsyncFactory)
        {
            if (responseAsyncFactory is not null)
                responseFactory = requestMessage =>
                {
                    var task = responseAsyncFactory(requestMessage);
                    task.Wait();
                    return task.Result;
                };
        }

        internal void ThrowException(Func<Exception> exceptionFactory)
        {
            if (exceptionFactory is not null)
                this.exceptionFactory = _ => exceptionFactory();
        }

        internal void ThrowException(Func<Task<Exception>> exceptionFactory)
        {
            if (exceptionFactory is not null)
                this.exceptionFactory = _ =>
                {
                    var task = exceptionFactory();
                    task.Wait();
                    throw task.Result;
                };
        }

        internal void ThrowException(Func<HttpRequestMessage, Exception> exceptionFactory)
        {
            if (exceptionFactory is not null)
                this.exceptionFactory = requestMessage => exceptionFactory(requestMessage);
        }

        internal void ThrowException(Func<HttpRequestMessage, Task<Exception>> exceptionFactory)
        {
            if (exceptionFactory is not null)
                this.exceptionFactory = requestMessage =>
                {
                    var task = exceptionFactory(requestMessage);
                    task.Wait();
                    throw task.Result;
                };
        }

        internal HttpResponseMessage Create(HttpRequestMessage requestMessage)
        {
            foreach (var interceptor in interceptors)
                interceptor(requestMessage);
            if (exceptionFactory is not null)
                throw exceptionFactory(requestMessage);
            if (responseFactory is not null)
                return responseFactory(requestMessage);
            return new HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented);
        }

        public override int GetHashCode()
        {
            return typedClientType.GetHashCode() ^
                   predicateStringRepresentation.GetHashCode();
        }

        public bool Equals(HttpResponseMessageFactory other)
        {
            if (other is null)
                return false;
            return typedClientType.Equals(other.typedClientType) &&
                   predicateStringRepresentation.Equals(other.predicateStringRepresentation);
        }

        public bool MatchByClientTypeAndPredicateString(Type typedClientType, string predicateStringRepresentation)
        {
            return this.typedClientType == typedClientType && 
                   this.predicateStringRepresentation == predicateStringRepresentation;
        }
    }
}
