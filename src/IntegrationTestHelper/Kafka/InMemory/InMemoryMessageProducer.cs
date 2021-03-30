using IntegrationTestHelper.Kafka.Producer.Interfaces;
using System;
using System.Threading.Tasks;

namespace ViaVarejo.PIX.IntegrationTests.Kafka.InMemory
{
    internal class InMemoryMessageProducer : IMessageProducer
    {
        public Task ProduceAsync<TMessage>(string key, TMessage message)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            InMemoryQueue.Producer.Enqueue(key, message);
            return Task.CompletedTask;
        }
    }
}
