using Microsoft.EntityFrameworkCore;

namespace Commun.Entities
{
    public class HashDbContext : DbContext
    {

        public DbSet<Hash> Hashes { get; set; }
        public HashDbContext(DbContextOptions<HashDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hash>().HasKey(m => m.Id);

        }

    }
}
