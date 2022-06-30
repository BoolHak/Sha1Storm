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
                
                var cache = await dbContext.HashCaches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Date == message.Date);
                if(cache != null)
                {
                    
                    cache.Count += nbRows;
                    dbContext.HashCaches.Update(cache);
                }
                else
                {
                    cache = new HashCache
                    {
                        Date = message.Date,
                        Count = nbRows
                    };
                    await dbContext.HashCaches.AddAsync(cache, stoppingToken);
                }

                await dbContext.SaveChangesAsync(stoppingToken);
                dbContext.Entry(cache).State = EntityState.Detached;
                
                await dbContextTransaction.CommitAsync(stoppingToken);

            }
        }
    }
}
