using Infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly IMessageBrokerService messageBrokerService;
        public ConsumerHostedService(IMessageBrokerService messageBrokerService)
        {
            this.messageBrokerService = messageBrokerService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await messageBrokerService.ReadMessage();
        }
    }
}
