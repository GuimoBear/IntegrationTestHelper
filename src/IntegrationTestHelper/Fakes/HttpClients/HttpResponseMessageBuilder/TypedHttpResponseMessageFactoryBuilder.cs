using System;
using System.Linq.Expressions;
using System.Net.Http;
using ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder.Interfaces;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder
{
    public class TypedHttpResponseMessageFactoryBuilder : ITypedHttpResponseMessageFactoryBuilder
    {
        private readonly Action<HttpResponseMessageFactory> factorySetter;
        private readonly Type typedClientType;

        internal TypedHttpResponseMessageFactoryBuilder(Action<HttpResponseMessageFactory> factorySetter, Type typedClientType)
        {
            this.factorySetter = factorySetter;
            this.typedClientType = typedClientType;
        }

        public IHttpResponseMessageFactoryBuilder When(Expression<Predicate<HttpRequestMessage>> requestPredicate)
            => new HttpResponseMessageFactoryBuilder(factorySetter, typedClientType ?? typeof(object), requestPredicate);
    }
}
