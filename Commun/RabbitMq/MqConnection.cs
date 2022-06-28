using RabbitMQ.Client;

namespace Commun.RabbitMq
{
    public class MqConnection
    {
        public static IConnection GetConnection()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri(Config.RabbitMQConnection);
            factory.AutomaticRecoveryEnabled = true;
            factory.NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
            factory.RequestedHeartbeat = TimeSpan.FromSeconds(30);
            return factory.CreateConnection();
        }
    }
}
