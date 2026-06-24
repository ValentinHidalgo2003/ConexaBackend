using Conexa.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Conexa.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Movie>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Director).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Producer).HasMaxLength(200).IsRequired();
            entity.Property(m => m.SwapiUid).HasMaxLength(20);
            entity.HasIndex(m => m.SwapiUid).IsUnique();
            entity.Property(m => m.Source).HasConversion<string>().HasMaxLength(20);
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        });
    }
}
