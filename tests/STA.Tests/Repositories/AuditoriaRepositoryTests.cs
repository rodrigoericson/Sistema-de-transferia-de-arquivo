using Microsoft.EntityFrameworkCore;
using STA.Core.Data;
using STA.Core.Data.Entities;
using STA.Core.Data.Repositories;
using Xunit;

namespace STA.Tests.Repositories;

public class AuditoriaRepositoryTests
{
    private static StaDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var context = new StaDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task InserirAsync_PersisteAuditoria()
    {
        using var context = CreateContext("audit_insert_test");
        var repo = new AuditoriaRepository(context);

        var auditoria = new Auditoria
        {
            CnUsuario = 1,
            NmUsuario = "admin",
            IdEntidade = "ETAPA",
            IdReferencia = 10,
            IdAcao = "CREATE",
            DtAcao = DateTime.UtcNow,
            DsDetalhe = "Teste"
        };

        await repo.InserirAsync(auditoria);

        var saved = await context.Auditorias.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("admin", saved.NmUsuario);
        Assert.Equal("ETAPA", saved.IdEntidade);
        Assert.Equal(10, saved.IdReferencia);
        Assert.Equal("CREATE", saved.IdAcao);
        Assert.Equal("Teste", saved.DsDetalhe);
    }

    [Fact]
    public async Task ListarAsync_FiltraPorUsuarioEPeriodo()
    {
        using var context = CreateContext("audit_filter_user_period");
        var repo = new AuditoriaRepository(context);

        var hoje = DateTime.UtcNow;
        var ontem = hoje.AddDays(-1);

        context.Auditorias.AddRange(
            new Auditoria { NmUsuario = "admin", IdEntidade = "ETAPA", IdReferencia = 1, IdAcao = "CREATE", DtAcao = hoje },
            new Auditoria { NmUsuario = "joao", IdEntidade = "ROTA", IdReferencia = 2, IdAcao = "UPDATE", DtAcao = hoje },
            new Auditoria { NmUsuario = "admin", IdEntidade = "ETAPA", IdReferencia = 3, IdAcao = "DELETE", DtAcao = ontem }
        );
        await context.SaveChangesAsync();

        var (items, total) = await repo.ListarAsync(usuario: "admin", de: hoje.Date);

        Assert.Equal(1, total);
        Assert.Single(items);
        Assert.Equal("admin", items[0].NmUsuario);
        Assert.Equal(1, items[0].IdReferencia);
    }

    [Fact]
    public async Task ListarAsync_FiltraPorEntidadeEAcao()
    {
        using var context = CreateContext("audit_filter_entidade_acao");
        var repo = new AuditoriaRepository(context);

        context.Auditorias.AddRange(
            new Auditoria { NmUsuario = "admin", IdEntidade = "ETAPA", IdReferencia = 1, IdAcao = "CREATE", DtAcao = DateTime.UtcNow },
            new Auditoria { NmUsuario = "admin", IdEntidade = "ROTA", IdReferencia = 2, IdAcao = "CREATE", DtAcao = DateTime.UtcNow },
            new Auditoria { NmUsuario = "admin", IdEntidade = "ETAPA", IdReferencia = 3, IdAcao = "DELETE", DtAcao = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var (items, total) = await repo.ListarAsync(entidade: "ETAPA", acao: "CREATE");

        Assert.Equal(1, total);
        Assert.Single(items);
        Assert.Equal("ETAPA", items[0].IdEntidade);
        Assert.Equal("CREATE", items[0].IdAcao);
    }
}
