using Microsoft.EntityFrameworkCore;
using teladoc.domain.Entities;

namespace teladoc.dao.Context
{
    public class TeladocDbContext : DbContext
    {
        public TeladocDbContext(DbContextOptions<TeladocDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .UseCollation("Latin1_General_CI_AS");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.EmailNormalized)
                     .IsRequired()
                     .HasMaxLength(255)
                     .HasColumnType("nvarchar(255)");

                entity.Property(e => e.NickName)
                    .HasMaxLength(100);

                entity.Property(e => e.DateOfBirth)
                    .IsRequired();

                entity.Property(e => e.FriendCount)
                    .HasDefaultValue(0);

                entity.HasIndex(e => e.EmailNormalized).IsUnique();
            });
        }
    }
}
