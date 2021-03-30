using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients.Interfaces;

namespace ViaVarejo.PIX.IntegrationTests.Fakes.HttpClients
{
    internal class MessageHandlerWithBehaviorInjection : DelegatingHandler
    {
        private readonly Type clientType;
        private readonly IBehaviorInjectionFactory behaviorInjectionFactory;

        public MessageHandlerWithBehaviorInjection(HttpMessageHandler innerHandler, Type clientType, IBehaviorInjectionFactory behaviorInjectionFactory)
            : base(innerHandler)
        {
            this.clientType = clientType;
            this.behaviorInjectionFactory = behaviorInjectionFactory;
        }

        public MessageHandlerWithBehaviorInjection(Type clientType, IBehaviorInjectionFactory behaviorInjectionFactory)
            : base()
        {
            this.clientType = clientType;
            this.behaviorInjectionFactory = behaviorInjectionFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await behaviorInjectionFactory.Inject(clientType, request, async req => await base.SendAsync(req, cancellationToken));
        }
    }
}
