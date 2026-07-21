using Microsoft.Extensions.Logging;
using Moq;
using STA.Core.Services.Transports;
using Xunit;

namespace STA.Tests.Services.Transports;

public class LocalFileTransportTests : IDisposable
{
    private readonly string _tempDir;
    private readonly LocalFileTransport _transport;

    public LocalFileTransportTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"sta_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _transport = new LocalFileTransport(true, Mock.Of<ILogger<LocalFileTransport>>());
    }

    [Fact]
    public async Task UploadFileAsync_CopiaArquivo()
    {
        var source = Path.Combine(_tempDir, "origem.txt");
        var dest = Path.Combine(_tempDir, "destino", "arquivo.txt");
        await File.WriteAllTextAsync(source, "conteudo teste");

        await _transport.UploadFileAsync(source, dest, true);

        Assert.True(File.Exists(dest));
        Assert.Equal("conteudo teste", await File.ReadAllTextAsync(dest));
    }

    [Fact]
    public async Task UploadFileAsync_CriaDiretorioSeNaoExiste()
    {
        var source = Path.Combine(_tempDir, "file.txt");
        var dest = Path.Combine(_tempDir, "sub", "deep", "file.txt");
        await File.WriteAllTextAsync(source, "data");

        await _transport.UploadFileAsync(source, dest, true);

        Assert.True(File.Exists(dest));
    }

    [Fact]
    public async Task DeleteFileAsync_RemoveArquivo()
    {
        var file = Path.Combine(_tempDir, "delete_me.txt");
        await File.WriteAllTextAsync(file, "x");

        await _transport.DeleteFileAsync(file);

        Assert.False(File.Exists(file));
    }

    [Fact]
    public async Task DeleteFileAsync_NaoFalhaSeNaoExiste()
    {
        var file = Path.Combine(_tempDir, "nao_existe.txt");

        var ex = await Record.ExceptionAsync(() => _transport.DeleteFileAsync(file));

        Assert.Null(ex);
    }

    [Fact]
    public async Task TestConnectionAsync_RetornaTrue()
    {
        var result = await _transport.TestConnectionAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task ListFilesAsync_RetornaArquivosDoDiretorio()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "a.txt"), "a");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "b.dat"), "b");

        var files = await _transport.ListFilesAsync(_tempDir);

        Assert.Equal(2, files.Count);
        Assert.Contains("a.txt", files);
        Assert.Contains("b.dat", files);
    }

    [Fact]
    public async Task ListFilesAsync_RetornaVazioSeDiretorioNaoExiste()
    {
        var files = await _transport.ListFilesAsync(Path.Combine(_tempDir, "inexistente"));
        Assert.Empty(files);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }
}
