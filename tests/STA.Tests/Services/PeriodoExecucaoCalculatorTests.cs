using STA.Worker.Services;
using Xunit;

namespace STA.Tests.Services;

public class PeriodoExecucaoCalculatorTests
{
    [Fact]
    public void DentroPeriodo_NoMeio_RetornaTrue()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("18:00:00"),
            TimeSpan.Parse("12:30:00"));

        Assert.True(result);
    }

    [Fact]
    public void DentroPeriodo_LimiteInferior_RetornaTrue()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("18:00:00"),
            TimeSpan.Parse("08:00:00"));

        Assert.True(result);
    }

    [Fact]
    public void DentroPeriodo_LimiteSuperior_RetornaTrue()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("18:00:00"),
            TimeSpan.Parse("18:00:00"));

        Assert.True(result);
    }

    [Fact]
    public void DentroPeriodo_ForaAntes_RetornaFalse()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("18:00:00"),
            TimeSpan.Parse("07:59:59"));

        Assert.False(result);
    }

    [Fact]
    public void DentroPeriodo_ForaDepois_RetornaFalse()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("18:00:00"),
            new TimeSpan(18, 0, 1));

        Assert.False(result);
    }

    [Fact]
    public void DentroPeriodo_JanelaDegenerada_AgoraIgual_RetornaTrue()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("08:00:00"));

        Assert.True(result);
    }

    [Fact]
    public void DentroPeriodo_JanelaDegenerada_AgoraDiferente_RetornaFalse()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("08:00:00"),
            TimeSpan.Parse("08:01:00"));

        Assert.False(result);
    }

    [Fact]
    public void DentroPeriodo_IniMaiorQueFim_RetornaFalse()
    {
        var result = PeriodoExecucaoCalculator.DentroPeriodo(
            TimeSpan.Parse("22:00:00"),
            TimeSpan.Parse("06:00:00"),
            TimeSpan.Parse("12:00:00"));

        Assert.False(result);
    }
}
