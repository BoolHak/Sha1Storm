using Commun.Entities;
using Commun.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Processor.Channels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Processor.BackgroundServices
{
    public class ConsumerBackgroundService : BackgroundService
    {
        
        private const short Sha1Length = 20;
        private const ushort NbChannels = 4;
        private const int MaxRowCount = 1000;

        private ChannelWriter<InsertQueryMessage> writer;

        public ConsumerBackgroundService(Channel<InsertQueryMessage> channel)
        {
            writer = channel.Writer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var connection = MqConnection.GetConnection();
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

                    
                    var date = DateTime.Today;
                    var query = new StringBuilder();
                    query.Append("INSERT INTO [dbo].[Hashes] ([Date] ,[Sha1]) VALUES ");
                    int rowCounter = 0;

                    for (int i = 0; i < ea.Body.Length; i += Sha1Length)
                    {
                        rowCounter++;

                        var hashInBytes = ea.Body.Slice(i, Sha1Length);
                        var hashInBase64 = Convert.ToBase64String(hashInBytes.Span);
                        query.Append("('");
                        query.Append(date);
                        query.Append("','");
                        query.Append(hashInBase64);
                        if(i == ea.Body.Length - Sha1Length || rowCounter == MaxRowCount)
                        {
                            rowCounter = 0;
                            query.Append("')");
                            await writer.WriteAsync(new InsertQueryMessage
                            {
                                Query = query.ToString()
                            });
                            query = new StringBuilder();
                            query.Append("INSERT INTO [dbo].[Hashes] ([Date] ,[Sha1]) VALUES ");
                        }

                        else
                            query.Append("'),");
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
