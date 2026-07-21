using Microsoft.Extensions.Logging;
using Moq;
using STA.Core.Data.Entities;
using STA.Core.Services.Transports;
using Xunit;

namespace STA.Tests.Services.Transports;

public class TransportFactoryTests
{
    [Fact]
    public void Criar_ProtocoloLocal_RetornaLocalFileTransport()
    {
        var factory = new TransportFactory(
            Mock.Of<ISftpClientFactory>(),
            Mock.Of<ICredencialProtector>(),
            new LoggerFactory());

        var destino = new RotaDestino
        {
            CnRotaDestino = 1,
            CnRota = 1,
            NrOrdem = 1,
            DsDiretorioDestino = "C:\\temp",
            IdProtocolo = "LOCAL",
            FlAtivo = true
        };

        var transport = factory.Criar(destino, null);

        Assert.IsType<LocalFileTransport>(transport);
    }

    [Fact]
    public void Criar_ProtocoloSftp_RetornaSftpTransport()
    {
        var clientMock = new Mock<ISftpClientWrapper>();
        var sftpFactoryMock = new Mock<ISftpClientFactory>();
        sftpFactoryMock.Setup(f => f.Criar(It.IsAny<ConexaoSftp>(), It.IsAny<ICredencialProtector>()))
            .Returns(clientMock.Object);

        var factory = new TransportFactory(
            sftpFactoryMock.Object,
            Mock.Of<ICredencialProtector>(),
            new LoggerFactory());

        var conexao = new ConexaoSftp
        {
            CnConexaoSftp = 1,
            NmConexao = "Test",
            DsHost = "localhost",
            NrPorta = 22,
            DsUsuario = "user",
            DsSenhaCriptografada = new byte[] { 1, 2, 3 },
            DsHorariosExecucao = "08:00",
            FlAtivo = true
        };

        var destino = new RotaDestino
        {
            CnRotaDestino = 1,
            CnRota = 1,
            NrOrdem = 1,
            DsDiretorioDestino = "/remote/path",
            IdProtocolo = "SFTP",
            CnConexaoSftp = 1,
            FlAtivo = true
        };

        var transport = factory.Criar(destino, conexao);

        Assert.IsType<SftpTransport>(transport);
    }

    [Fact]
    public void Criar_ProtocoloSftp_SemConexao_LancaException()
    {
        var factory = new TransportFactory(
            Mock.Of<ISftpClientFactory>(),
            Mock.Of<ICredencialProtector>(),
            new LoggerFactory());

        var destino = new RotaDestino
        {
            CnRotaDestino = 1,
            CnRota = 1,
            NrOrdem = 1,
            DsDiretorioDestino = "/remote/path",
            IdProtocolo = "SFTP",
            FlAtivo = true
        };

        Assert.Throws<InvalidOperationException>(() => factory.Criar(destino, null));
    }
}
