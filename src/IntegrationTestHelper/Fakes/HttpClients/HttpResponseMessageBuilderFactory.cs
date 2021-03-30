using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder;
using ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder.Interfaces;
using ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.Interfaces;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients
{
    public class HttpResponseMessageBuilderFactory : IHttpResponseMessageBuilderFactory, IBehaviorInjectionFactory
    {
        private static readonly object @lock = new object();

        private List<HttpResponseMessageFactory> factories = new List<HttpResponseMessageFactory>();

        public static readonly HttpResponseMessageBuilderFactory Instance = new HttpResponseMessageBuilderFactory();

        private HttpResponseMessageBuilderFactory() { }

        private void AddFactoryIfNoPreviouslyAdded(HttpResponseMessageFactory factory)
        {
            if (factory is null)
                return;
            lock (@lock)
            {
                if (factories.Contains(factory))
                    factories.Remove(factory);
                factories.Add(factory);
            }
        }

        public IHttpResponseMessageFactoryBuilder When(Expression<Predicate<HttpRequestMessage>> predicate)
            => new HttpResponseMessageFactoryBuilder(AddFactoryIfNoPreviouslyAdded, default, predicate);

        public ITypedHttpResponseMessageFactoryBuilder TypedClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>()
            => new TypedHttpResponseMessageFactoryBuilder(AddFactoryIfNoPreviouslyAdded, typeof(TClient));

        public void RemoveFactory(Expression<Predicate<HttpRequestMessage>> predicate)
            => RemoveFactory(typeof(object), predicate);

        public void RemoveFactory(Type typedClientType, Expression<Predicate<HttpRequestMessage>> predicate)
        {
            if (predicate is null || typedClientType is null)
                return;
            var predicateStringRepresentation = predicate.ToString();

            lock (@lock)
            {
                foreach (var fac in factories.Where(fac => fac.MatchByClientTypeAndPredicateString(typedClientType, predicateStringRepresentation)))
                    factories.Remove(fac);
            }
        }

        public void Cleanup()
        {
            lock (@lock)
            {
                factories.Clear();
            }
        }

        public async Task<HttpResponseMessage> Inject(Type clientType, HttpRequestMessage requestMessage, Func<HttpRequestMessage, Task<HttpResponseMessage>> defaultHandler)
        {
            var fac = factories.FirstOrDefault(fac => fac.Match(clientType, requestMessage));
            if (fac is null)
                return await defaultHandler(requestMessage);
            return fac.Create(requestMessage);
        }
    }
}
