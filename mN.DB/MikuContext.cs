using Microsoft.EntityFrameworkCore;
using mN.DB.Models;

namespace mN.DB
{
    public class MikuContext : DbContext
    {
        public DbSet<MikuGuild> MikuGuilds { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("testDB");
            //=> optionsBuilder.UseSqlite("Data Source=miku.sqlite");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MikuGuild>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.MikuMusic).WithOne(x => x.MikuGuild).HasForeignKey<MikuGuild>(x => x.Id);
            });

            modelBuilder.Entity<MikuMusic>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.MikuGuild).WithOne(x => x.MikuMusic).HasForeignKey<MikuMusic>(x => x.Id);
                entity.Property(x => x.ConnectionState).HasDefaultValue((ConnectionState)0);
                entity.Property(x => x.MusicOptions).HasDefaultValue((MusicOptions)0);
                entity.Property(x => x.PlayState).HasDefaultValue((PlayState)0);
                entity.HasOne(x => x.CurrentTrack).WithOne(x => x.MikuMusic).HasForeignKey<MikuMusic>(x => x.Id);
                entity.HasMany(x => x.QueueTracks).WithOne(x => x.MikuMusic).HasForeignKey(x => x.Id);
            });

            modelBuilder.Entity<CurrentTrack>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.MikuMusic).WithOne(x => x.CurrentTrack).HasForeignKey<CurrentTrack>(x => x.Id);
                entity.Property(x => x.TrackString).IsRequired();
                entity.Property(x => x.AddedBy).IsRequired();
                entity.Property(x => x.AddedAt).IsRequired();
            });

            modelBuilder.Entity<QueueTrack>(entity =>
            {
                entity.HasKey(x => new { x.Id, x.Position });
                entity.HasOne(x => x.MikuMusic).WithMany(x => x.QueueTracks).HasForeignKey(x => x.Id);
                entity.Property(x => x.TrackString).IsRequired();
                entity.Property(x => x.AddedBy).IsRequired();
                entity.Property(x => x.AddedAt).IsRequired();
            });
        }
    }
}
