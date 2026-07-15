using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace STA.Core.Data.Repositories;

/// <summary>
/// Persistência de logs de processo via EF Core + PostgreSQL.
/// </summary>
public class LogRepository : ILogRepository
{
    private readonly StaDbContext _context;
    private readonly ILogger<LogRepository> _logger;

    public LogRepository(StaDbContext context, ILogger<LogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> InserirLogAsync(
        string aliasSistema,
        int cnProcesso,
        DateTime dtInicio,
        string status,
        long qtRegistrosProcessados,
        long vlRegistrosProcessados,
        long qtRegistrosErro,
        long vlRegistrosErro,
        string xmlObservacao,
        CancellationToken cancellationToken = default)
    {
        var result = await _context.Database
            .SqlQuery<int>($@"
                SELECT sta.fn_inclui_log_processo(
                    {aliasSistema},
                    {cnProcesso},
                    {dtInicio},
                    {status},
                    {qtRegistrosProcessados},
                    {vlRegistrosProcessados},
                    {qtRegistrosErro},
                    {vlRegistrosErro},
                    {xmlObservacao}
                ) AS ""Value""")
            .ToListAsync(cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<int> ExcluirLogsAntigosAsync(
        string aliasSistema,
        int cnProcesso,
        int diasManter,
        CancellationToken cancellationToken = default)
    {
        var dataCorte = DateTime.UtcNow.AddDays(-diasManter);

        var cnSistema = await _context.Sistemas
            .Where(s => s.CdAliasSistema == aliasSistema)
            .Select(s => s.CnSistema)
            .FirstOrDefaultAsync(cancellationToken);

        if (cnSistema == 0)
        {
            _logger.LogWarning("Sistema '{Sistema}' não encontrado para exclusão de logs.", aliasSistema);
            return 0;
        }

        var excluidos = await _context.Logs
            .Where(l => l.CnSistema == cnSistema
                && l.CnProcesso == cnProcesso
                && l.DtFimProcesso != null
                && l.DtFimProcesso < dataCorte)
            .ExecuteDeleteAsync(cancellationToken);

        if (excluidos > 0)
            _logger.LogInformation("Excluídos {Count} logs antigos (anteriores a {DataCorte:yyyy-MM-dd}).", excluidos, dataCorte);

        return excluidos;
    }

    public async Task<int> AtualizarFimAsync(
        int cnLogProcesso,
        DateTime dtFim,
        string statusFinal,
        CancellationToken cancellationToken = default)
    {
        return await _context.Logs
            .Where(l => l.CnLogProcesso == cnLogProcesso)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(l => l.DtFimProcesso, dtFim)
                    .SetProperty(l => l.IdStatusProcesso, statusFinal),
                cancellationToken);
    }
}
