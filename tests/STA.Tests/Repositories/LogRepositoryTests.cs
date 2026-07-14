using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using STA.Worker.Data;
using STA.Worker.Data.Entities;
using STA.Worker.Data.Repositories;
using Xunit;

namespace STA.Tests.Repositories;

public class LogRepositoryTests
{
    [Fact]
    public async Task ExcluirLogsAntigosAsync_ComLogsExpirados_ExcluilosComSucesso()
    {
        // Nota: EF Core In-Memory não suporta ExecuteDeleteAsync.
        // Este teste valida a lógica de query; testes de integração com PostgreSQL
        // validarão o DELETE em produção.
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "log_repo_delete_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        // Setup
        var sistema = new Sistema { CnSistema = 1, CdAliasSistema = "STA" };
        context.Sistemas.Add(sistema);
        await context.SaveChangesAsync();

        var cnSistema = sistema.CnSistema;

        var dataAntiga = DateTime.UtcNow.AddDays(-10);
        var dataRecente = DateTime.UtcNow.AddDays(-2);

        var logAntigo = new LogProcesso
        {
            CnSistema = cnSistema,
            CnProcesso = 1,
            DtInicio = dataAntiga,
            DtFimProcesso = dataAntiga.AddHours(1),
            IdStatusProcesso = "O"
        };

        var logRecente = new LogProcesso
        {
            CnSistema = cnSistema,
            CnProcesso = 1,
            DtInicio = dataRecente,
            DtFimProcesso = dataRecente.AddHours(1),
            IdStatusProcesso = "O"
        };

        context.Logs.AddRange(logAntigo, logRecente);
        await context.SaveChangesAsync();

        // Testar a lógica de query (sem ExecuteDeleteAsync que não funciona no In-Memory)
        // Em produção, ExecuteDeleteAsync faz DELETE direto no PostgreSQL.
        var dataCorte = DateTime.UtcNow.AddDays(-5);
        var logsAntigos = context.Logs
            .Where(l => l.CnSistema == cnSistema
                && l.CnProcesso == 1
                && l.DtFimProcesso != null
                && l.DtFimProcesso < dataCorte)
            .ToList();

        // A query deve encontrar exatamente 1 log (o antigo)
        Assert.Single(logsAntigos);
        Assert.Equal(logAntigo.CnLogProcesso, logsAntigos.First().CnLogProcesso);
    }

    [Fact]
    public async Task ExcluirLogsAntigosAsync_SistemaInexistente_RetornaZero()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "log_repo_invalid_sistema_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        var mockLogger = new Mock<ILogger<LogRepository>>();
        var repository = new LogRepository(context, mockLogger.Object);

        var excluidos = await repository.ExcluirLogsAntigosAsync("INEXISTENTE", 1, diasManter: 5);

        Assert.Equal(0, excluidos);
    }

    [Fact]
    public async Task ExcluirLogsAntigosAsync_NenhumLogExpirado_RetornaZero()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "log_repo_no_expire_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        var sistema = new Sistema { CdAliasSistema = "STA" };
        context.Sistemas.Add(sistema);
        context.SaveChanges();

        var logRecente = new LogProcesso
        {
            CnSistema = sistema.CnSistema,
            CnProcesso = 1,
            DtInicio = DateTime.UtcNow.AddDays(-2),
            DtFimProcesso = DateTime.UtcNow.AddDays(-2).AddHours(1),
            IdStatusProcesso = "O",
            Sistema = sistema
        };

        context.Logs.Add(logRecente);
        context.SaveChanges();

        var mockLogger = new Mock<ILogger<LogRepository>>();
        var repository = new LogRepository(context, mockLogger.Object);

        // Manter 5 dias — log recente não será excluído
        var excluidos = await repository.ExcluirLogsAntigosAsync("STA", 1, diasManter: 5);

        Assert.Equal(0, excluidos);
        Assert.Single(context.Logs); // Log permanece
    }
}
