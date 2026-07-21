using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace STA.Core.Services.Transports;

[SupportedOSPlatform("windows")]

public class DpapiCredencialProtector : ICredencialProtector
{
    private readonly byte[] _entropy;

    public DpapiCredencialProtector(byte[]? entropy = null)
    {
        _entropy = entropy ?? Encoding.UTF8.GetBytes("TAE-STA-2026");
    }

    public byte[] Proteger(string plaintext)
    {
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        return ProtectedData.Protect(bytes, _entropy, DataProtectionScope.CurrentUser);
    }

    public string Recuperar(byte[] encrypted)
    {
        var bytes = ProtectedData.Unprotect(encrypted, _entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }
}
