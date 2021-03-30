using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder.Interfaces
{
    public interface IHttpResponseMessageFactoryBuilder
    {
        void Return(Func<HttpRequestMessage, HttpResponseMessage> responseFactory);
        void Return(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory);
        void Return(Func<HttpResponseMessage> responseFactory);
        void Return(Func<Task<HttpResponseMessage>> responseFactory);
        void Throw(Func<Exception> exceptionFactory);
        void Throw(Func<HttpRequestMessage, Exception> exceptionFactory);
        void Throw(Func<HttpRequestMessage, HttpResponseMessage> responseFactory);
        void Throw(Func<HttpRequestMessage, Task<Exception>> exceptionFactory);
        void Throw(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory);
        void Throw(Func<HttpResponseMessage> responseFactory);
        void Throw(Func<Task<Exception>> exceptionFactory);
        void Throw(Func<Task<HttpResponseMessage>> responseFactory);
        IHttpResponseMessageFactoryBuilder WithAsyncInterceptor(Func<HttpRequestMessage, Task> asyncInterceptor);
        IHttpResponseMessageFactoryBuilder WithInterceptor(Action<HttpRequestMessage> interceptor);
    }
}