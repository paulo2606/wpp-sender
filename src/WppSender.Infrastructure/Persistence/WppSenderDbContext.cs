using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class WppSenderDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<Grupo> Grupos => Set<Grupo>();
    public DbSet<Campanha> Campanhas => Set<Campanha>();
    public DbSet<Envio> Envios => Set<Envio>();
    public DbSet<ConfiguracaoEnvio> ConfiguracoesEnvio => Set<ConfiguracaoEnvio>();
    public DbSet<SessaoWhatsApp> SessoesWhatsApp => Set<SessaoWhatsApp>();

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

        modelBuilder.Entity<Campanha>(entity =>
        {
            entity.ToTable("campanhas", t => t.HasCheckConstraint(
                "ck_campanhas_status",
                "status IN ('rascunho','agendada','em_andamento','pausada','concluida')"));
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired();
            entity.Property(c => c.Mensagem).IsRequired();
            entity.Property(c => c.GrupoId).HasColumnName("grupo_id").IsRequired();
            entity.Property(c => c.AgendadoPara).HasColumnName("agendado_para");
            entity.Property(c => c.IntervaloMinSegundos).HasColumnName("intervalo_min_segundos");
            entity.Property(c => c.IntervaloMaxSegundos).HasColumnName("intervalo_max_segundos");
            entity.Property(c => c.Status)
                .HasColumnName("status")
                .HasConversion(v => StatusCampanhaParaTexto(v), v => TextoParaStatusCampanha(v));
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.GrupoId);
        });

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.ToTable("campanha_envios", t => t.HasCheckConstraint(
                "ck_campanha_envios_status",
                "status IN ('pendente','enviado','falhou')"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CampanhaId).HasColumnName("campanha_id").IsRequired();
            entity.Property(e => e.LeadId).HasColumnName("lead_id").IsRequired();
            entity.Property(e => e.EnviadoEm).HasColumnName("enviado_em");
            entity.Property(e => e.Erro).HasColumnName("erro");
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion(v => StatusEnvioParaTexto(v), v => TextoParaStatusEnvio(v));
            entity.HasIndex(e => new { e.CampanhaId, e.Status });
            entity.HasIndex(e => e.LeadId);
            entity.HasIndex(e => new { e.CampanhaId, e.LeadId }).IsUnique();
            // Envios são "fotografados" na criação da campanha (uma linha por lead), então
            // deletar uma campanha em Rascunho/Agendada (único caso permitido) deve arrastar
            // seus envios junto — diferente do padrão SetNull de Grupos, onde leads sobrevivem ao grupo.
            entity.HasOne<Campanha>()
                .WithMany()
                .HasForeignKey(e => e.CampanhaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConfiguracaoEnvio>(entity =>
        {
            entity.ToTable("configuracao_envio", t => t.HasCheckConstraint("ck_configuracao_envio_id", "id = 1"));
            entity.Property<short>("Id").HasColumnName("id");
            entity.HasKey("Id");
            entity.Property(c => c.LimiteDiarioEnvios).HasColumnName("limite_diario_envios");
            entity.Property(c => c.EnviosRealizadosHoje).HasColumnName("envios_realizados_hoje");
            entity.Property(c => c.DataReferencia).HasColumnName("data_referencia");
            entity.HasData(new
            {
                Id = (short)1,
                LimiteDiarioEnvios = 120,
                EnviosRealizadosHoje = 0,
                DataReferencia = new DateOnly(2020, 1, 1),
            });
        });

        modelBuilder.Entity<SessaoWhatsApp>(entity =>
        {
            entity.ToTable("whatsapp_sessao", t => t.HasCheckConstraint(
                "ck_whatsapp_sessao_status",
                "status IN ('desconectado','aguardando_qr','conectado')"));
            entity.Property<short>("Id").HasColumnName("id");
            entity.HasKey("Id");
            entity.Property(s => s.Status)
                .HasColumnName("status")
                .HasConversion(v => StatusSessaoParaTexto(v), v => TextoParaStatusSessao(v));
            entity.HasData(new { Id = (short)1, Status = StatusSessaoWhatsApp.Desconectado });
        });
    }

    private static string StatusCampanhaParaTexto(StatusCampanha status) => status switch
    {
        StatusCampanha.Rascunho => "rascunho",
        StatusCampanha.Agendada => "agendada",
        StatusCampanha.EmAndamento => "em_andamento",
        StatusCampanha.Pausada => "pausada",
        StatusCampanha.Concluida => "concluida",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    private static StatusCampanha TextoParaStatusCampanha(string valor) => valor switch
    {
        "rascunho" => StatusCampanha.Rascunho,
        "agendada" => StatusCampanha.Agendada,
        "em_andamento" => StatusCampanha.EmAndamento,
        "pausada" => StatusCampanha.Pausada,
        "concluida" => StatusCampanha.Concluida,
        _ => throw new ArgumentOutOfRangeException(nameof(valor)),
    };

    private static string StatusEnvioParaTexto(StatusEnvio status) => status switch
    {
        StatusEnvio.Pendente => "pendente",
        StatusEnvio.Enviado => "enviado",
        StatusEnvio.Falhou => "falhou",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    private static StatusEnvio TextoParaStatusEnvio(string valor) => valor switch
    {
        "pendente" => StatusEnvio.Pendente,
        "enviado" => StatusEnvio.Enviado,
        "falhou" => StatusEnvio.Falhou,
        _ => throw new ArgumentOutOfRangeException(nameof(valor)),
    };

    private static string StatusSessaoParaTexto(StatusSessaoWhatsApp status) => status switch
    {
        StatusSessaoWhatsApp.Desconectado => "desconectado",
        StatusSessaoWhatsApp.AguardandoQr => "aguardando_qr",
        StatusSessaoWhatsApp.Conectado => "conectado",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    private static StatusSessaoWhatsApp TextoParaStatusSessao(string valor) => valor switch
    {
        "desconectado" => StatusSessaoWhatsApp.Desconectado,
        "aguardando_qr" => StatusSessaoWhatsApp.AguardandoQr,
        "conectado" => StatusSessaoWhatsApp.Conectado,
        _ => throw new ArgumentOutOfRangeException(nameof(valor)),
    };
}
