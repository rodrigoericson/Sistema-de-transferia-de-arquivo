namespace STA.Core.Services.Transports;

public interface ICredencialProtector
{
    byte[] Proteger(string plaintext);
    string Recuperar(byte[] encrypted);
}
