using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder.Interfaces;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder
{
    public class HttpResponseMessageFactoryBuilder : IHttpResponseMessageFactoryBuilder
    {
        private readonly Action<HttpResponseMessageFactory> factorySetter;
        private readonly HttpResponseMessageFactory factory;

        internal HttpResponseMessageFactoryBuilder(Action<HttpResponseMessageFactory> factorySetter, Type typedClientType, Expression<Predicate<HttpRequestMessage>> requestPredicate)
        {
            this.factorySetter = factorySetter;
            factory = new HttpResponseMessageFactory(typedClientType, requestPredicate);
        }

        public IHttpResponseMessageFactoryBuilder WithInterceptor(Action<HttpRequestMessage> interceptor)
        {
            factory.AddInterceptor(interceptor);
            return this;
        }

        public IHttpResponseMessageFactoryBuilder WithAsyncInterceptor(Func<HttpRequestMessage, Task> asyncInterceptor)
        {
            factory.AddAsyncInterceptor(asyncInterceptor);
            return this;
        }

        public void Return(Func<HttpResponseMessage> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Return(Func<Task<HttpResponseMessage>> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Return(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Return(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<HttpResponseMessage> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<Task<HttpResponseMessage>> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory)
        {
            factory.Return(responseFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<Exception> exceptionFactory)
        {
            factory.ThrowException(exceptionFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<Task<Exception>> exceptionFactory)
        {
            factory.ThrowException(exceptionFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<HttpRequestMessage, Exception> exceptionFactory)
        {
            factory.ThrowException(exceptionFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        public void Throw(Func<HttpRequestMessage, Task<Exception>> exceptionFactory)
        {
            factory.ThrowException(exceptionFactory);
            AddFactoryIfNoPreviouslyAdded();
        }

        private void AddFactoryIfNoPreviouslyAdded()
        {
            factorySetter(factory);
        }
    }
}
