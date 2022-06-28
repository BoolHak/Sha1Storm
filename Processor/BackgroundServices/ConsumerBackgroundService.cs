using Commun.Entities;
using Commun.RabbitMq;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Processor.BackgroundServices
{
    public class ConsumerBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private const short Sha1Length = 20;
        private const ushort NbChannels = 4;

        public ConsumerBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var connection = MqConnection.GetConnection();
            
            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<HashDbContext>();

            var listOfChannels = new List<IModel>();
            
            for(int i = 0; i < NbChannels; i++)
            {
                var channel = connection.CreateModel();
                channel.QueueDeclare(queue: "task_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) =>
                {
                    if (ea.Body.Length == 0) return;

                    var listToInsert = new List<Hash>();
                    var date = DateTime.Today;
                    for (int i = 0; i < ea.Body.Length; i += Sha1Length)
                    {
                        var hashInBytes = ea.Body.Slice(i, Sha1Length);
                        var hashInBase64 = BitConverter.ToString(hashInBytes.Span.ToArray());
                        listToInsert.Add(new Hash
                        {
                            Sha1 = hashInBase64,
                            Date = date
                        });
                    }

                    if (listToInsert.Count != 0)
                    {
                        await dbContext.Hashes.AddRangeAsync(listToInsert);
                        await dbContext.SaveChangesAsync();
                    }

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(queue: "task_queue",
                                        consumerTag: $"consumer{i}",
                                        noLocal: true,
                                        exclusive: false,
                                        autoAck: false,
                                        arguments: null,
                                        consumer: consumer);

                listOfChannels.Add(channel);
            }


            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(500, stoppingToken);
            }

            foreach(var channel in listOfChannels)
                channel.Dispose();
        }
    }
}
