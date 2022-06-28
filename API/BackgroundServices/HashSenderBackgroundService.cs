using API.Channels;
using API.Utils;
using Commun.RabbitMq;
using RabbitMQ.Client;
using System.Buffers;
using System.Threading.Channels;

namespace API.BackgroundServices
{
    public class HashSenderBackgroundService : BackgroundService
    {

        private const int BatchSize = 40_000;
        private const short Sha1Length = 20;

        private readonly ChannelReader<GenerateMessage> _channelReader;
        
        public HashSenderBackgroundService(Channel<GenerateMessage> channel)
        {
            _channelReader = channel.Reader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            using var connection = MqConnection.GetConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            while (!stoppingToken.IsCancellationRequested)
            {
                _ = await _channelReader.ReadAsync(stoppingToken);
                Sha1Generator.Generate(out byte[] data, BatchSize);

                if (data == null || data.Length == 0) continue;

                channel.BasicPublish(exchange: "",
                                 routingKey: "task_queue",
                                 basicProperties: properties,
                                 body: data.AsMemory(0, BatchSize* Sha1Length));

                ArrayPool<byte>.Shared.Return(data);
            }
        }

    }
}
