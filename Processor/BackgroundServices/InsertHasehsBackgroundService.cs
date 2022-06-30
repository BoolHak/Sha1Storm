using Commun.Entities;
using Microsoft.EntityFrameworkCore;
using Processor.Channels;
using System.Threading.Channels;

namespace Processor.BackgroundServices
{
    public class InsertHasehsBackgroundService : BackgroundService
    {
        private readonly ChannelReader<InsertQueryMessage> _channelReader;
        private readonly IServiceScopeFactory _scopeFactory;
        public InsertHasehsBackgroundService(IServiceScopeFactory scopeFactory, Channel<InsertQueryMessage> channel)
        {
            _channelReader = channel.Reader;
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<HashDbContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _channelReader.ReadAsync(stoppingToken);
                if (message == null) return;
                if(message.Query == null) return;
                using var dbContextTransaction = dbContext.Database.BeginTransaction();

                int nbRows = await dbContext.Database.ExecuteSqlRawAsync(message.Query, stoppingToken);
                await dbContext.SaveChangesAsync(stoppingToken);
                await dbContextTransaction.CommitAsync(stoppingToken);

            }
        }
    }
}
