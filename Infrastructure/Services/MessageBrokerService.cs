using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Services
{
    public sealed class MessageBrokerService : IMessageBrokerService
    {
        private readonly IModel model;
        private readonly IConnection connection;
        private const string queueName = "UserLoginQueue";
        private const string queueExhange = "UserLoginExchange";
        public MessageBrokerService(IRabbitMQService rabbitMQService)
        {
            this.connection = rabbitMQService.CreateChannel();
            this.model = connection.CreateModel();
            model.QueueDeclare(queueName,exclusive:false,autoDelete:false);
            //Durable: true: stored on disk false stored on memory,
            //autoDelete is true, the queue is cleared when all Consumers are disconnected from RabbitMq.
            //But if it is true, the queue remains, Even if no Consumer is connected to it.

            //Exchange type which can be Headers, Topic, Fanout or Direct. If it is equal to Fanout,
            //and if the data enters the Exchange, it will send it to all the queues to which it is attached.
            //But if the type is Direct, it sends the data to a specific queue; Using the routeKey parameter.
            model.ExchangeDeclare(queueExhange, ExchangeType.Fanout,durable:true,autoDelete:false);
            model.QueueBind(queueName, queueExhange, string.Empty);
        }
        public async Task ReadMessage()
        {
            var consumer = new AsyncEventingBasicConsumer(model);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var text = System.Text.Encoding.UTF8.GetString(body);
                Console.WriteLine(text);
                await Task.CompletedTask;
                //We further inform RabbitMq that the data submitted for the queue was received by the Consumer,
                //Using the BasicAck method.This will send a delivery to RabbitMq to clear the sent data.
                //If we do not call this method, every time the program is run, we retrieve all the previous data
                //and it does not delete the data until we send the delivery to RabbitMq.
                model.BasicAck(ea.DeliveryTag, false);
            };
            model.BasicConsume(queueName, false, consumer);
            await Task.CompletedTask;
        }
    }
}
