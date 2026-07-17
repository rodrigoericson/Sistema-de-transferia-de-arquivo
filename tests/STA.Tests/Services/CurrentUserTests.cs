using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using STA.Core.Services;
using Xunit;

namespace STA.Tests.Services;

public class CurrentUserTests
{
    [Fact]
    public void CurrentUser_LeCnUsuarioDoClaimsPrincipal()
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Name, "joao"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "test"));

        var httpContext = new DefaultHttpContext { User = claims };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(accessor.Object);

        Assert.Equal(42, currentUser.CnUsuario);
        Assert.Equal("joao", currentUser.NmUsuario);
        Assert.Equal("Admin", currentUser.Role);
    }

    [Fact]
    public void CurrentUser_RetornaNullQuandoClaimAusente()
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity());

        var httpContext = new DefaultHttpContext { User = claims };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var currentUser = new CurrentUser(accessor.Object);

        Assert.Null(currentUser.CnUsuario);
        Assert.Null(currentUser.NmUsuario);
        Assert.Null(currentUser.Role);
    }
}
