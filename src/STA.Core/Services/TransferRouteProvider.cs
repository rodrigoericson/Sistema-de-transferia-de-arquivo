using Microsoft.EntityFrameworkCore;
using STA.Core.Data;

namespace STA.Core.Services;

public record DestinoComConexao(int CnRotaDestino, string Diretorio, string? PadraoRename, bool FlAtivo, int? CnConexaoSftp);

public interface ITransferRouteProvider
{
    Task<IReadOnlyList<DestinoComConexao>> BuscarDestinosComConexaoAsync(IEnumerable<int> rotaIds, CancellationToken ct = default);
}

public class TransferRouteProvider : ITransferRouteProvider
{
    private readonly StaDbContext _context;

    public TransferRouteProvider(StaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DestinoComConexao>> BuscarDestinosComConexaoAsync(IEnumerable<int> rotaIds, CancellationToken ct = default)
    {
        var ids = rotaIds.ToList();
        if (ids.Count == 0) return Array.Empty<DestinoComConexao>();

        return await _context.RotaDestinos.AsNoTracking()
            .Where(d => ids.Contains(d.CnRota) && d.FlAtivo)
            .Select(d => new DestinoComConexao(
                d.CnRotaDestino,
                d.DsDiretorioDestino,
                d.DsPadraoRename,
                d.FlAtivo,
                d.CnConexaoSftp))
            .ToListAsync(ct);
    }
}
