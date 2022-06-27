using API.Channels;
using API.Utils;
using System.Buffers;
using System.Threading.Channels;

namespace API.BackgroundServices
{
    public class HashSenderBackgroundService : BackgroundService
    {

        private const int BatchSize = 40_000;

        private readonly ChannelReader<GenerateMessage> _channelReader;
        public HashSenderBackgroundService(Channel<GenerateMessage> channel)
        {
            _channelReader = channel.Reader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _ = await _channelReader.ReadAsync(stoppingToken);
                Sha1Generator.Generate(out byte[] data, BatchSize);
                if (data == null || data.Length == 0) continue;

                SendWithRabbitMq(data);

                ArrayPool<byte>.Shared.Return(data);
            }
        }

        private void SendWithRabbitMq(byte[] data)
        {
            Console.WriteLine("Sending with rabbit mq");
        }
    }
}
