using STA.Core.Data.Entities;

namespace STA.Core.Services.Transports;

public interface ITransportFactory
{
    IDestinationTransport Criar(RotaDestino destino, ConexaoSftp? conexao);
}
