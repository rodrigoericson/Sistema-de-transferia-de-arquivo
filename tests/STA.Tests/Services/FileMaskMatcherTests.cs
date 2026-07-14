using STA.Worker.Services;
using Xunit;

namespace STA.Tests.Services;

public class FileMaskMatcherTests
{
    private readonly FileMaskMatcher _matcher = new();

    [Theory]
    [InlineData("arquivo.txt", "*.txt", true)]
    [InlineData("arquivo.txt", "*.log", false)]
    [InlineData("ARQUIVO.TXT", "*.txt", true)]
    [InlineData("relatorio_2024.csv", "relatorio_*.csv", true)]
    [InlineData("relatorio_2024.csv", "relatorio_????.csv", true)]
    [InlineData("relatorio_24.csv", "relatorio_????.csv", false)]
    [InlineData("qualquer.coisa", "*.*", true)]
    [InlineData("qualquer.coisa", "*", true)]
    [InlineData("TRANSACOESRECARGA_001.REM", "TRANSACOESRECARGA*.REM", true)]
    [InlineData("SB.EF.10.BBB9.06.RET.001", "SB.EF.10.BBB9.06.RET.*", true)]
    [InlineData("outro.arquivo", "SB.EF.10.BBB9.06.RET.*", false)]
    public void Match_Cenarios(string filename, string mask, bool expected)
    {
        Assert.Equal(expected, _matcher.Match(filename, mask));
    }

    [Theory]
    [InlineData(null, "*.txt")]
    [InlineData("file.txt", null)]
    [InlineData("", "*.txt")]
    [InlineData("file.txt", "")]
    public void Match_NullOuVazio_RetornaFalse(string? filename, string? mask)
    {
        Assert.False(_matcher.Match(filename!, mask!));
    }

    [Theory]
    [InlineData("arquivo.txt", "*.txt", true)]
    [InlineData("arquivo.txt", "*.log", false)]
    [InlineData("ARQUIVO.TXT", "*.txt", true)]
    [InlineData("relatorio_2024.csv", "relatorio_*.csv", true)]
    [InlineData("qualquer.coisa", "*.*", true)]
    [InlineData("qualquer.coisa", "*", true)]
    [InlineData("TRANSACOESRECARGA_001.REM", "TRANSACOESRECARGA*.REM", true)]
    [InlineData("file_start.txt", "file_*.txt", true)]
    [InlineData("prefixSUFFIX.txt", "prefix*.txt", true)]
    [InlineData("nome_completo.txt", "nome_completo.txt", true)]
    [InlineData("nome_completo.txt", "outro_nome.txt", false)]
    public void MatchSimples_Cenarios(string filename, string mask, bool expected)
    {
        Assert.Equal(expected, _matcher.MatchSimples(filename, mask));
    }

    [Theory]
    [InlineData(null, "*.txt")]
    [InlineData("file.txt", null)]
    [InlineData("", "*.txt")]
    public void MatchSimples_NullOuVazio_RetornaFalse(string? filename, string? mask)
    {
        Assert.False(_matcher.MatchSimples(filename!, mask!));
    }
}
