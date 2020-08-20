using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MqttTest.Workers
{
    public class MqttServerWorker:IHostedService
    {
        private IMqttServer _server;
        private readonly ILogger<MqttServerWorker> _logger;

        public MqttServerWorker( ILogger<MqttServerWorker> logger)
        {
            _server =new MqttFactory().CreateMqttServer();
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Mqtt Server Started");

            var optionsBuilder = new MqttServerOptionsBuilder().WithDefaultEndpointPort(1886).WithConnectionValidator(async c =>
                {
                    c.ReasonCode = MqttConnectReasonCode.Success;
                })/*.WithPersistentSessions()*/
                .WithDefaultCommunicationTimeout(TimeSpan.FromHours(1));

           await _server.StartAsync(optionsBuilder.Build());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogError("MqttServer Stopped");
            await _server.StopAsync();
            await this.StartAsync(CancellationToken.None);
        }
    }
}
