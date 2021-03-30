using System;
using System.Linq.Expressions;
using System.Net.Http;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder.Interfaces
{
    public interface ITypedHttpResponseMessageFactoryBuilder
    {
        IHttpResponseMessageFactoryBuilder When(Expression<Predicate<HttpRequestMessage>> requestPredicate);
    }
}