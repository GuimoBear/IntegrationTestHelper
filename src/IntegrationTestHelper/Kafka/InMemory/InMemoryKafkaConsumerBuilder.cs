using Confluent.Kafka;

namespace ViaVarejo.PIX.IntegrationTests.Kafka.InMemory
{
    internal class InMemoryKafkaConsumerBuilde
    {
        public IConsumer<string, string> Build()
            => new InMemoryConsumer(InMemoryQueue.Consumer);
    }
}
