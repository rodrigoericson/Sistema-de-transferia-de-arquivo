using Microsoft.EntityFrameworkCore;
using STA.Worker.Data;
using STA.Worker.Data.Entities;
using Xunit;

namespace STA.Tests;

public class SmokeTests
{
    [Fact]
    public void DbContext_CriaSchema_SemErros()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "smoke_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        Assert.NotNull(context.Sistemas);
        Assert.NotNull(context.Parametros);
        Assert.NotNull(context.Logs);
    }

    [Fact]
    public void DbContext_InsereSistema_RetornaIdGerado()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "insert_sistema_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        var sistema = new Sistema { CdAliasSistema = "STA" };
        context.Sistemas.Add(sistema);
        context.SaveChanges();

        Assert.True(sistema.CnSistema > 0);
    }

    [Fact]
    public void DbContext_InsereLogProcesso_PersisteDados()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "insert_log_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        var sistema = new Sistema { CdAliasSistema = "STA" };
        context.Sistemas.Add(sistema);
        context.SaveChanges();

        var log = new LogProcesso
        {
            CnSistema = sistema.CnSistema,
            CnProcesso = 1,
            DtInicio = DateTime.UtcNow,
            IdStatusProcesso = "O",
            QtRegistrosProcessados = 10,
            VlRegistrosProcessados = 100,
            QtRegistrosErro = 0,
            VlRegistrosErro = 0,
            XmlObsProcesso = "<Etapa>Teste</Etapa><Observacao>OK</Observacao>",
            Sistema = sistema
        };

        context.Logs.Add(log);
        context.SaveChanges();

        var logSalvo = context.Logs.First();
        Assert.Equal("O", logSalvo.IdStatusProcesso);
        Assert.Equal(10, logSalvo.QtRegistrosProcessados);
    }
}
