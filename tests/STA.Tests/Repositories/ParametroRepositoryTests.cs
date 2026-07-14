using Microsoft.EntityFrameworkCore;
using STA.Worker.Data;
using STA.Worker.Data.Entities;
using STA.Worker.Data.Repositories;
using Xunit;

namespace STA.Tests.Repositories;

public class ParametroRepositoryTests
{
    [Fact]
    public async Task BuscarParametrosExecucaoAsync_ComParametrosValidos_RetornaParametros()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "param_repo_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        // Setup: criar sistema e parâmetros
        var sistema = new Sistema { CdAliasSistema = "STA" };
        context.Sistemas.Add(sistema);
        context.SaveChanges();

        var param1 = new ParametroSistema
        {
            CnParametroSistema = 1,
            CnSistema = sistema.CnSistema,
            CdParametroSistema = "08:00:00",
            Sistema = sistema
        };
        var param2 = new ParametroSistema
        {
            CnParametroSistema = 2,
            CnSistema = sistema.CnSistema,
            CdParametroSistema = "18:00:00",
            Sistema = sistema
        };
        var param3 = new ParametroSistema
        {
            CnParametroSistema = 3,
            CnSistema = sistema.CnSistema,
            CdParametroSistema = "5",
            Sistema = sistema
        };

        context.Parametros.AddRange(param1, param2, param3);
        context.SaveChanges();

        // Test
        var repository = new ParametroRepository(context);
        var resultado = await repository.BuscarParametrosExecucaoAsync("STA", 1, 2, 3);

        Assert.NotNull(resultado);
        Assert.Equal("08:00:00", resultado.HoraInicial);
        Assert.Equal("18:00:00", resultado.HoraFinal);
        Assert.Equal(5, resultado.PeriodoMinutos);
    }

    [Fact]
    public async Task BuscarParametrosExecucaoAsync_SistemaInexistente_RetornaNull()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "param_repo_null_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        var repository = new ParametroRepository(context);
        var resultado = await repository.BuscarParametrosExecucaoAsync("INEXISTENTE", 1, 2, 3);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarParametrosExecucaoAsync_ParametrosIncompletos_RetornaNull()
    {
        var options = new DbContextOptionsBuilder<StaDbContext>()
            .UseInMemoryDatabase(databaseName: "param_repo_incomplete_test")
            .Options;

        using var context = new StaDbContext(options);
        context.Database.EnsureCreated();

        var sistema = new Sistema { CdAliasSistema = "STA" };
        context.Sistemas.Add(sistema);
        context.SaveChanges();

        // Adicionar apenas 2 dos 3 parâmetros necessários
        var param1 = new ParametroSistema
        {
            CnParametroSistema = 1,
            CnSistema = sistema.CnSistema,
            CdParametroSistema = "08:00:00",
            Sistema = sistema
        };
        var param2 = new ParametroSistema
        {
            CnParametroSistema = 2,
            CnSistema = sistema.CnSistema,
            CdParametroSistema = "18:00:00",
            Sistema = sistema
        };

        context.Parametros.AddRange(param1, param2);
        context.SaveChanges();

        var repository = new ParametroRepository(context);
        var resultado = await repository.BuscarParametrosExecucaoAsync("STA", 1, 2, 3);

        Assert.Null(resultado);
    }
}
