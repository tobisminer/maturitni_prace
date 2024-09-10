using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .HasMany(e => e.Messages);
        }
        public DbSet<Room?> Rooms { get; set; }
        public DbSet<Message?> Messages { get; set; }
        public DbSet<UserLogin?> UserLogins { get; set; }
        public DbSet<Token> Tokens { get; set; }
    }
}
