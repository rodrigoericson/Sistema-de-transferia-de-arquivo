using Microsoft.Extensions.Options;
using STA.Worker.Settings;

namespace STA.Worker;

/// <summary>
/// Orquestrador principal do STA. Substitui o Service1.Timer1_Elapsed do código legado.
/// Roda em loop, respeitando a janela de horário configurada no banco de dados.
/// Fases 2-4 preencherão a lógica de negócio via injeção de dependência.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IOptions<StaSettings> _settings;

    // Período padrão de 5 minutos enquanto não há configuração do banco
    private TimeSpan _interval = TimeSpan.FromMinutes(5);

    public Worker(ILogger<Worker> logger, IOptions<StaSettings> settings)
    {
        _logger = logger;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("STA Worker iniciado em: {Time}", DateTimeOffset.Now);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecutarCicloAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Erro no ciclo de execução. Próxima tentativa em {Interval}.", _interval);
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown solicitado — sai limpo
        }

        _logger.LogInformation("STA Worker encerrado em: {Time}", DateTimeOffset.Now);
    }

    /// <summary>
    /// Executa um ciclo completo de transferência de arquivos.
    /// Fase 2 adicionará: buscar parâmetros do banco, verificar janela horária.
    /// Fase 3 adicionará: chamar FileTransferService, FileRetentionService.
    /// </summary>
    private Task ExecutarCicloAsync(CancellationToken stoppingToken)
    {
        // TODO Fase 2: buscar parâmetros do banco (HoraIni, HoraFim, PeriodoMin)
        // TODO Fase 2: verificar janela de execução (DentroPeriodo)
        // TODO Fase 3: executar transferências (FileTransferService)
        // TODO Fase 3: excluir logs expirados (FileRetentionService)

        _logger.LogInformation("Ciclo de execução concluído em: {Time}", DateTimeOffset.Now);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Atualiza o intervalo do próximo ciclo com base no parâmetro do banco.
    /// Substitui o Timer1.Interval = PeriodoSistemaMin * 60000 do código legado.
    /// </summary>
    protected void AtualizarIntervalo(int periodoMinutos)
    {
        if (periodoMinutos > 0)
            _interval = TimeSpan.FromMinutes(periodoMinutos);
    }
}
