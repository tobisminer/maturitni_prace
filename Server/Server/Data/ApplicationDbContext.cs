using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Reflection.Metadata;

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
    }
}
