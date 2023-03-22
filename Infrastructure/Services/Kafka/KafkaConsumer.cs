using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
namespace Infrastructure.Services.Kafka
{
    public sealed class KafkaConsumer : IHostedService
    {
        public KafkaConsumer(){}
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ConsumerConfig config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "User", //This id should be unique to every consumer. Kafka guarantees that a message is only ever read by a single consumer in the group.
                EnableAutoCommit = true, //Auto commits the offsets so that when consumer reconnects to the broker, broker has information of the last offset this consumer read the data from.
                AutoOffsetReset = AutoOffsetReset.Earliest, //If broker doesn't have consumer's last offset information it is auto set it.
                FetchWaitMaxMs = 500, //Max time consumer waits before filling the response with min bytes.
                EnablePartitionEof = true, //Triggers an event letting consumer know that there is no more data to consume.
                FetchErrorBackoffMs = 200, //If error occurs postpone the next fetch request for topic+partition.
            };

            try
            {
                using (var consumerBuilder = new ConsumerBuilder<Ignore, string>(config).Build())
                {
                    consumerBuilder.Subscribe("SignIn");
                    var cancelToken = new CancellationTokenSource();

                    try
                    {
                        while (true)
                        {
                            var consumer = consumerBuilder.Consume(cancelToken.Token);
                            Console.WriteLine($"Processing Order Id:");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Processing Exception Order Id:" + e.Message);
                        Console.WriteLine($"{e.Message}");
                    }
                    finally
                    {
                        consumerBuilder.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processing Exception Order Id:" + ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        //protected override Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    ConsumerConfig config = new ConsumerConfig
        //    {
        //        BootstrapServers = "localhost:9092",
        //        GroupId = "User", //This id should be unique to every consumer. Kafka guarantees that a message is only ever read by a single consumer in the group.
        //        EnableAutoCommit = true, //Auto commits the offsets so that when consumer reconnects to the broker, broker has information of the last offset this consumer read the data from.
        //        AutoOffsetReset = AutoOffsetReset.Earliest, //If broker doesn't have consumer's last offset information it is auto set it.
        //        FetchWaitMaxMs = 500, //Max time consumer waits before filling the response with min bytes.
        //        EnablePartitionEof = true, //Triggers an event letting consumer know that there is no more data to consume.
        //        FetchErrorBackoffMs = 200, //If error occurs postpone the next fetch request for topic+partition.
        //    };

        //    try
        //    {
        //        using (var consumerBuilder = new ConsumerBuilder
        //        <Ignore, string>(config).Build())
        //        {
        //            consumerBuilder.Subscribe("SignIn");
        //            var cancelToken = new CancellationTokenSource();

        //            try
        //            {
        //                while (true)
        //                {
        //                    var consumer = consumerBuilder.Consume
        //                       (cancelToken.Token);
        //                    Console.WriteLine($"Processing Order Id:");
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Console.WriteLine($"Processing Exception Order Id:" + e.Message);
        //                Console.WriteLine($"{e.Message}");
        //            }
        //            finally
        //            {
        //                consumerBuilder.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Processing Exception Order Id:" + ex.Message);
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //    }

        //    return Task.CompletedTask;
        //}
    }
}
