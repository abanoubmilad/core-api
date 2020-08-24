using core_api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace core_api.Data
{
    public class DataContext : IdentityDbContext<User>
    {

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Firm> Firms { get; set; }
        public DbSet<FirmUser> FirmUsers { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User and Booking one-to-one
            builder.Entity<User>()
            .HasMany(x => x.Bookings)
            .WithOne(x => x.BookedBy);

            // Meeting and Booking one-to-one
            builder.Entity<Meeting>()
            .HasMany(x => x.Bookings)
            .WithOne(x => x.Meeting);

            // Meeting and Firm one-to-one
            builder.Entity<Firm>()
            .HasMany(x => x.Meetings)
            .WithOne(x => x.Firm);

            // User and Firm many-to-many
            builder.Entity<FirmUser>()
            .HasKey(x => new { x.UserId, x.FirmId });

            builder.Entity<FirmUser>()
            .HasOne(x => x.User)
            .WithMany(x => x.FirmUsers)
            .HasForeignKey(x => x.UserId);

            builder.Entity<FirmUser>()
            .HasOne(x => x.Firm)
            .WithMany(x => x.FirmUsers)
            .HasForeignKey(x => x.FirmId);

        }
    }
}