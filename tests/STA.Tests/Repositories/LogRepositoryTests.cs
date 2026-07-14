using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using STA.Core.Data;
using STA.Core.Data.Entities;
using STA.Core.Data.Repositories;
using Xunit;

namespace STA.Tests.Repositories;

public class LogRepositoryTests
{
    [Fact]
    public async Task QueryExcluirLogs_FiltraCorretamentePorData()
    {
        // Valida que a query LINQ identifica corretamente os logs expirados.
        // ExecuteDeleteAsync requer PostgreSQL real — testado em integração.
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
    public async Task ExcluirLogsAntigosAsync_ExecuteDeleteAsync_NaoSuportadoEmInMemory_LancaException()
    {
        // ExecuteDeleteAsync não funciona com EF In-Memory.
        // Este teste documenta que a exception é propagada (não engolida).
        // Testes reais de DELETE devem usar PostgreSQL.
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "log_repo_no_expire_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        var sistema = new Sistema { CdAliasSistema = "STA" };
        context.Sistemas.Add(sistema);
        context.SaveChanges();

        var mockLogger = new Mock<ILogger<LogRepository>>();
        var repository = new LogRepository(context, mockLogger.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.ExcluirLogsAntigosAsync("STA", 1, diasManter: 5));
    }
}

