using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Kafka
{
    public sealed class KafkaProducer : IDisposable
    {
        IProducer<string, string> kafkaProducer;
        public KafkaProducer()
        {
            var conf = new ProducerConfig()
            {
                BootstrapServers = "localhost:9092", // Kafka server your producer will connect to

                //Delivery error occurs when either the retry count or the message timeout are exceeded. An exception will be thrown
                MessageTimeoutMs = 500, //Max time librdkafka may use to deliver produced. time 0 is infinity
                RetryBackoffMs = 100, //Time producer waits to retry message after failure.
                MessageSendMaxRetries = 5, //Number of times sending message is retired after failure

                DeliveryReportFields = "key, value, timestamp", //Only required fields. Reduces the size of delivery report. Use "none if you don't want any". Default is "all"
                EnableDeliveryReports = true, //Enables delivery report after successful message delivery

                EnableIdempotence = false, //Set true if message sequence in commit log is important. default is false.

                Acks = Acks.Leader, //Broker send ack to producer when the message in persisted based on the enum value assigned.

            };
            this.kafkaProducer = new ProducerBuilder<string, string>(conf).Build();
        }
        public async Task<DeliveryResult<string,string>> ProduceAsync(string topic,Message<string,string> message)
        {
            return await this.kafkaProducer.ProduceAsync(topic, message);
        }
        public void Dispose()
        {
            kafkaProducer.Flush();
            kafkaProducer.Dispose();
        }
    }
}
