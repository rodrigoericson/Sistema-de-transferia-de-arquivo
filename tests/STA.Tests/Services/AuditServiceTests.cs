using Microsoft.Extensions.Logging;
using Moq;
using STA.Core.Data.Entities;
using STA.Core.Data.Repositories;
using STA.Core.Services;
using Xunit;

namespace STA.Tests.Services;

public class AuditServiceTests
{
    [Fact]
    public async Task RegistrarAsync_QuandoRepositorioFalha_NaoPropagaExcecao()
    {
        var mockRepo = new Mock<IAuditoriaRepository>();
        mockRepo.Setup(r => r.InserirAsync(It.IsAny<Auditoria>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Falha simulada"));

        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.CnUsuario).Returns(1);
        mockUser.Setup(u => u.NmUsuario).Returns("admin");

        var mockLogger = new Mock<ILogger<AuditService>>();

        var service = new AuditService(mockRepo.Object, mockUser.Object, mockLogger.Object);

        var exception = await Record.ExceptionAsync(() =>
            service.RegistrarAsync("ETAPA", 1, "CREATE", "teste"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RegistrarAsync_QuandoRepositorioOk_ChamaInserirComDadosDoCurrentUser()
    {
        var mockRepo = new Mock<IAuditoriaRepository>();
        var mockUser = new Mock<ICurrentUser>();
        mockUser.Setup(u => u.CnUsuario).Returns(42);
        mockUser.Setup(u => u.NmUsuario).Returns("joao");

        var mockLogger = new Mock<ILogger<AuditService>>();

        var service = new AuditService(mockRepo.Object, mockUser.Object, mockLogger.Object);

        await service.RegistrarAsync("ROTA", 5, "UPDATE", "diretorio");

        mockRepo.Verify(r => r.InserirAsync(
            It.Is<Auditoria>(a =>
                a.CnUsuario == 42 &&
                a.NmUsuario == "joao" &&
                a.IdEntidade == "ROTA" &&
                a.IdReferencia == 5 &&
                a.IdAcao == "UPDATE" &&
                a.DsDetalhe == "diretorio"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
