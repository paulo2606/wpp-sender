using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class WppSenderDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public WppSenderDbContext(DbContextOptions<WppSenderDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.SenhaHash).IsRequired();
        });
    }
}
