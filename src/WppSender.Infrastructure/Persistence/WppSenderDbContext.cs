using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class WppSenderDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Grupo> Grupos => Set<Grupo>();

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

        modelBuilder.Entity<Endereco>(entity =>
        {
            entity.ToTable("enderecos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rua).IsRequired();
            entity.Property(e => e.Numero).IsRequired();
            entity.Property(e => e.Bairro).IsRequired();
            entity.Property(e => e.Cidade).IsRequired();
            entity.Property(e => e.Estado).IsRequired();
            entity.Property(e => e.Cep).IsRequired();
        });

        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.ToTable("grupos");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Nome).IsRequired();
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("leads");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Nome).IsRequired();
            entity.Property(l => l.TelefoneNormalizado).HasColumnName("telefone").IsRequired();
            entity.HasIndex(l => l.TelefoneNormalizado)
                .IsUnique()
                .HasDatabaseName("uq_leads_telefone_ativo")
                .HasFilter("deletado_em IS NULL");
            entity.HasOne(l => l.Endereco).WithMany().HasForeignKey("EnderecoId").IsRequired(false);
            entity.HasOne<Grupo>()
                .WithMany()
                .HasForeignKey(l => l.GrupoId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            entity.HasIndex(l => l.GrupoId);
        });
    }
}
