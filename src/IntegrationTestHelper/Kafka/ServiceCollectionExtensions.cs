using IntegrationTestHelper.Kafka.Producer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ViaVarejo.PIX.IntegrationTests.Kafka.InMemory;

namespace ViaVarejo.PIX.IntegrationTests.Kafka
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection InjectInMemoryKafka(this IServiceCollection services)
        {
            services.AddTransient<IMessageProducer, InMemoryMessageProducer>();

            return services;
        }
    }
}
