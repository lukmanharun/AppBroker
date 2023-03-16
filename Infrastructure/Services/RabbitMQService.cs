using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SixLabors.ImageSharp;

namespace Infrastructure.Services
{
    public sealed class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQConfiguration configuration;
        public RabbitMQService(IOptions<RabbitMQConfiguration> options)
        {
            this.configuration = options.Value;
        }
        public IConnection CreateChannel()
        {
            ConnectionFactory connection = new ConnectionFactory()
            {
                UserName = configuration.Username,
                Password = configuration.Password,
                HostName = configuration.HostName
            };
            connection.DispatchConsumersAsync = true;
            var channel = connection.CreateConnection();
            return channel;
        }
    }
}
