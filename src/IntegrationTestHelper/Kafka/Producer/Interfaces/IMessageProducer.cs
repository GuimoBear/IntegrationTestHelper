using System.Threading.Tasks;

namespace IntegrationTestHelper.Kafka.Producer.Interfaces
{
    public interface IMessageProducer
    {
        Task ProduceAsync<TMessage>(string key, TMessage message);
    }
}
