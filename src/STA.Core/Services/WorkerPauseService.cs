using Microsoft.EntityFrameworkCore;
using STA.Core.Data;

namespace STA.Core.Services;

public interface IWorkerPauseService
{
    Task<bool> IsPausedAsync(CancellationToken ct = default);
}

public class WorkerPauseService : IWorkerPauseService
{
    private readonly StaDbContext _context;

    public WorkerPauseService(StaDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsPausedAsync(CancellationToken ct = default)
    {
        try
        {
            var param = await _context.Parametros
                .FirstOrDefaultAsync(p => p.CnParametroSistema == 4, ct);
            return param?.CdParametroSistema == "true";
        }
        catch
        {
            return false;
        }
    }
}
