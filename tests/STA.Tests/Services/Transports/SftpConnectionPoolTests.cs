using Microsoft.Extensions.Logging;
using Moq;
using STA.Core.Data.Entities;
using STA.Core.Services.Transports;
using Xunit;

namespace STA.Tests.Services.Transports;

public class SftpConnectionPoolTests
{
    private readonly Mock<ISftpClientFactory> _factoryMock;
    private readonly Mock<ICredencialProtector> _protectorMock;
    private readonly SftpConnectionPool _pool;

    public SftpConnectionPoolTests()
    {
        _factoryMock = new Mock<ISftpClientFactory>();
        _protectorMock = new Mock<ICredencialProtector>();
        _pool = new SftpConnectionPool(
            _factoryMock.Object,
            _protectorMock.Object,
            Mock.Of<ILogger<SftpConnectionPool>>());
    }

    private ConexaoSftp CriarConexao(int id = 1) => new()
    {
        CnConexaoSftp = id,
        NmConexao = $"Test {id}",
        DsHost = "localhost",
        NrPorta = 22,
        DsUsuario = "user",
        DsSenhaCriptografada = new byte[] { 1 },
        DsHorariosExecucao = "08:00",
        FlAtivo = true
    };

    [Fact]
    public void GetOrCreate_PrimeiraVez_CriaEConecta()
    {
        var clientMock = new Mock<ISftpClientWrapper>();
        clientMock.Setup(c => c.IsConnected).Returns(true);
        _factoryMock.Setup(f => f.Criar(It.IsAny<ConexaoSftp>(), It.IsAny<ICredencialProtector>()))
            .Returns(clientMock.Object);

        var conexao = CriarConexao();
        var client = _pool.GetOrCreate(conexao);

        Assert.NotNull(client);
        clientMock.Verify(c => c.Connect(), Times.Once);
        Assert.Equal(1, _pool.ActiveConnections);
    }

    [Fact]
    public void GetOrCreate_SegundaVez_ReusaConexao()
    {
        var clientMock = new Mock<ISftpClientWrapper>();
        clientMock.Setup(c => c.IsConnected).Returns(true);
        _factoryMock.Setup(f => f.Criar(It.IsAny<ConexaoSftp>(), It.IsAny<ICredencialProtector>()))
            .Returns(clientMock.Object);

        var conexao = CriarConexao();
        _pool.GetOrCreate(conexao);
        _pool.GetOrCreate(conexao);

        _factoryMock.Verify(f => f.Criar(It.IsAny<ConexaoSftp>(), It.IsAny<ICredencialProtector>()), Times.Once);
    }

    [Fact]
    public void GetOrCreate_ConexaoCaiu_Reconecta()
    {
        var clientMock1 = new Mock<ISftpClientWrapper>();
        clientMock1.Setup(c => c.IsConnected).Returns(false);

        var clientMock2 = new Mock<ISftpClientWrapper>();
        clientMock2.Setup(c => c.IsConnected).Returns(true);

        var callCount = 0;
        _factoryMock.Setup(f => f.Criar(It.IsAny<ConexaoSftp>(), It.IsAny<ICredencialProtector>()))
            .Returns(() => callCount++ == 0 ? clientMock1.Object : clientMock2.Object);

        var conexao = CriarConexao();
        _pool.GetOrCreate(conexao); // primeira: conecta mas IsConnected=false na segunda chamada

        clientMock1.Setup(c => c.IsConnected).Returns(false);
        var client = _pool.GetOrCreate(conexao); // detecta queda, reconecta

        Assert.Equal(clientMock2.Object, client);
        clientMock1.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void CloseAll_FechaTodasConexoes()
    {
        var clientMock1 = new Mock<ISftpClientWrapper>();
        clientMock1.Setup(c => c.IsConnected).Returns(true);
        var clientMock2 = new Mock<ISftpClientWrapper>();
        clientMock2.Setup(c => c.IsConnected).Returns(true);

        var callCount = 0;
        _factoryMock.Setup(f => f.Criar(It.IsAny<ConexaoSftp>(), It.IsAny<ICredencialProtector>()))
            .Returns(() => callCount++ == 0 ? clientMock1.Object : clientMock2.Object);

        _pool.GetOrCreate(CriarConexao(1));
        _pool.GetOrCreate(CriarConexao(2));

        _pool.CloseAll();

        clientMock1.Verify(c => c.Disconnect(), Times.Once);
        clientMock2.Verify(c => c.Disconnect(), Times.Once);
        Assert.Equal(0, _pool.ActiveConnections);
    }
}
