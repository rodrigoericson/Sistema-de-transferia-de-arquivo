using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using STA.Core.Data.Entities;

namespace STA.Core.Data;

/// <summary>
/// DbContext principal da aplicação STA.
/// Mapeia entidades para o schema 'sta' do PostgreSQL.
/// </summary>
public class StaDbContext : DbContext
{
    public StaDbContext(DbContextOptions<StaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Sistema> Sistemas { get; set; } = null!;
    public DbSet<ParametroSistema> Parametros { get; set; } = null!;
    public DbSet<LogProcesso> Logs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Usar schema 'sta' para todas as tabelas
        modelBuilder.HasDefaultSchema("sta");

        ConfigureSistema(modelBuilder.Entity<Sistema>());
        ConfigureParametroSistema(modelBuilder.Entity<ParametroSistema>());
        ConfigureLogProcesso(modelBuilder.Entity<LogProcesso>());
    }

    private static void ConfigureSistema(EntityTypeBuilder<Sistema> builder)
    {
        builder.ToTable("tbl_sistema");

        builder.HasKey(s => s.CnSistema);

        builder.Property(s => s.CnSistema)
            .HasColumnName("cn_sistema")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.CdAliasSistema)
            .HasColumnName("cd_alias_sistema")
            .HasMaxLength(20)
            .IsRequired();

        // Índice único para alias (segurança e performance)
        builder.HasIndex(s => s.CdAliasSistema).IsUnique();

        // Relacionamentos
        builder.HasMany(s => s.Parametros)
            .WithOne(p => p.Sistema)
            .HasForeignKey(p => p.CnSistema)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Logs)
            .WithOne(l => l.Sistema)
            .HasForeignKey(l => l.CnSistema)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureParametroSistema(EntityTypeBuilder<ParametroSistema> builder)
    {
        builder.ToTable("tbl_parametro_sistema");

        builder.HasKey(p => new { p.CnParametroSistema, p.CnSistema });

        builder.Property(p => p.CnParametroSistema)
            .HasColumnName("cn_parametro_sistema")
            .ValueGeneratedNever();

        builder.Property(p => p.CnSistema)
            .HasColumnName("cn_sistema");

        builder.Property(p => p.CdParametroSistema)
            .HasColumnName("cd_parametro_sistema")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(p => p.Sistema)
            .WithMany(s => s.Parametros)
            .HasForeignKey(p => p.CnSistema)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureLogProcesso(EntityTypeBuilder<LogProcesso> builder)
    {
        builder.ToTable("tbl_log_processo");

        builder.HasKey(l => l.CnLogProcesso);

        builder.Property(l => l.CnLogProcesso)
            .HasColumnName("cn_log_processo")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.CnSistema)
            .HasColumnName("cn_sistema")
            .IsRequired();

        builder.Property(l => l.CnProcesso)
            .HasColumnName("cn_processo")
            .IsRequired();

        builder.Property(l => l.DtInicio)
            .HasColumnName("dt_inicio")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(l => l.DtFimProcesso)
            .HasColumnName("dt_fim_processo");

        builder.Property(l => l.IdStatusProcesso)
            .HasColumnName("id_status_processo")
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(l => l.QtRegistrosProcessados)
            .HasColumnName("qt_registros_processados")
            .HasDefaultValue(0L);

        builder.Property(l => l.VlRegistrosProcessados)
            .HasColumnName("vl_registros_processados")
            .HasDefaultValue(0L);

        builder.Property(l => l.QtRegistrosErro)
            .HasColumnName("qt_registros_erro")
            .HasDefaultValue(0L);

        builder.Property(l => l.VlRegistrosErro)
            .HasColumnName("vl_registros_erro")
            .HasDefaultValue(0L);

        builder.Property(l => l.XmlObsProcesso)
            .HasColumnName("xml_obs_processo")
            .HasColumnType("text");

        builder.HasOne(l => l.Sistema)
            .WithMany(s => s.Logs)
            .HasForeignKey(l => l.CnSistema)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para queries comuns
        builder.HasIndex(l => new { l.CnSistema, l.CnProcesso });
        builder.HasIndex(l => l.DtInicio);
    }
}
