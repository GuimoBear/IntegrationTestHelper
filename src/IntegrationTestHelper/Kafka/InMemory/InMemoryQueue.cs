using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ViaVarejo.PIX.IntegrationTests.Kafka.InMemory
{
    internal class InMemoryQueue : QueueConsumer, QueueProducer
    {
        private static readonly InMemoryQueue instance = new InMemoryQueue();

        public static QueueConsumer Consumer => instance;
        public static QueueProducer Producer => instance;

        private readonly ConcurrentDictionary<string, Queue<string>> queues = new ConcurrentDictionary<string, Queue<string>>();

        private InMemoryQueue() { }

        public void Enqueue<TMessage>(string topic, TMessage message)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentNullException(nameof(topic));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (queues.TryGetValue(topic, out var queue))
                queue.Enqueue(JsonConvert.SerializeObject(message));
        }

        public void CreateTopicIfNotExists(string topic)
        {
            if (!queues.TryGetValue(topic, out _))
            {
                var queue = new Queue<string>();
                queues.TryAdd(topic, queue);
            }
        }

        public (string topic, string message) Consume(IEnumerable<string> topics, TimeSpan timeout)
        {
            if (topics is null || topics.Count() == 0)
                return default;
            var timeoutDateTime = DateTime.Now.Add(timeout);
            while (timeoutDateTime > DateTime.Now)
            {
                foreach (var topic in topics)
                    if (queues.TryGetValue(topic, out var queue) && queue.Count > 0)
                        return (topic, queue.Dequeue());
                Thread.Sleep(10);
            }
            return default;
        }

        public (string topic, string message) Consume(IEnumerable<string> topics, int millisecondsTimeout)
        {
            if (topics is null || topics.Count() == 0)
                return default;
            return Consume(topics, TimeSpan.FromMilliseconds(millisecondsTimeout));
        }

        public (string topic, string message) Consume(IEnumerable<string> topics, CancellationToken cancellationToken)
        {
            if (topics is null || topics.Count() == 0)
                return default;
            if (cancellationToken == default)
                return Consume(topics, TimeSpan.FromSeconds(20));
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var topic in topics)
                    if (queues.TryGetValue(topic, out var queue) && queue.Count > 0)
                        return (topic, queue.Dequeue());
                Thread.Sleep(10);
            }
            return default;
        }

    }

    public interface QueueProducer
    {
        void Enqueue<TMessage>(string topic, TMessage message);
    }

    public interface QueueConsumer
    {
        void CreateTopicIfNotExists(string topic);

        (string topic, string message) Consume(IEnumerable<string> topics, int millisecondsTimeout);

        (string topic, string message) Consume(IEnumerable<string> topics, CancellationToken cancellationToken);

        (string topic, string message) Consume(IEnumerable<string> topics, TimeSpan timeout);
    }
}
