using Microsoft.Extensions.Logging;
using STA.Core.Data.Entities;

namespace STA.Core.Services.Transports;

public class TransportFactory : ITransportFactory
{
    private readonly ISftpClientFactory _sftpFactory;
    private readonly ICredencialProtector _protector;
    private readonly ILoggerFactory _loggerFactory;
    private readonly bool _overwriteDefault;

    public TransportFactory(
        ISftpClientFactory sftpFactory,
        ICredencialProtector protector,
        ILoggerFactory loggerFactory,
        bool overwriteDefault = true)
    {
        _sftpFactory = sftpFactory;
        _protector = protector;
        _loggerFactory = loggerFactory;
        _overwriteDefault = overwriteDefault;
    }

    public IDestinationTransport Criar(RotaDestino destino, ConexaoSftp? conexao, SftpConnectionPool? pool = null)
    {
        if (destino.IdProtocolo == "SFTP")
        {
            if (conexao == null)
                throw new InvalidOperationException(
                    $"Destino #{destino.CnRotaDestino} configurado como SFTP mas sem conexao associada.");

            ISftpClientWrapper client = pool != null
                ? pool.GetOrCreate(conexao)
                : _sftpFactory.Criar(conexao, _protector);

            try
            {
                return new SftpTransport(client, _loggerFactory.CreateLogger<SftpTransport>());
            }
            catch
            {
                if (pool == null)
                    client.Dispose();
                throw;
            }
        }

        return new LocalFileTransport(_overwriteDefault, _loggerFactory.CreateLogger<LocalFileTransport>());
    }
}
