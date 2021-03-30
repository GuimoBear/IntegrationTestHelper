using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.HttpResponseMessageBuilder.Interfaces;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.Interfaces
{
    public interface IHttpResponseMessageBuilderFactory
    {
        ITypedHttpResponseMessageFactoryBuilder TypedClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TClient>();
        IHttpResponseMessageFactoryBuilder When(Expression<Predicate<HttpRequestMessage>> predicate);

        void RemoveFactory(Expression<Predicate<HttpRequestMessage>> predicate);
        void RemoveFactory(Type typedClientType, Expression<Predicate<HttpRequestMessage>> predicate);

        void Cleanup();
    }

    public interface IBehaviorInjectionFactory
    {
        Task<HttpResponseMessage> Inject(Type clientType, HttpRequestMessage requestMessage, Func<HttpRequestMessage, Task<HttpResponseMessage>> defaultHandler);
    }
}