namespace STA.Core.Data.Entities;

/// <summary>
/// Representa um registro de execução de processo de transferência de arquivos.
/// Mapeia sta.tbl_log_processo.
/// Cada linha registra um ciclo de execução com status (O=sucesso, W=aviso, E=erro).
/// </summary>
public class LogProcesso
{
    public int CnLogProcesso { get; set; }

    public int CnSistema { get; set; }

    public int CnProcesso { get; set; }

    public DateTime DtInicio { get; set; }

    public DateTime? DtFimProcesso { get; set; }

    /// <summary>
    /// Status do processo: 'O' (sucesso), 'W' (aviso), 'E' (erro).
    /// </summary>
    public required string IdStatusProcesso { get; set; }

    public long QtRegistrosProcessados { get; set; }

    public long VlRegistrosProcessados { get; set; }

    public long QtRegistrosErro { get; set; }

    public long VlRegistrosErro { get; set; }

    /// <summary>
    /// Observações do processo em formato XML.
    /// Exemplo: <Etapa>Transferencia de arquivos</Etapa><Observacao>Falha ao copiar arquivo.../<Observacao>
    /// </summary>
    public string? XmlObsProcesso { get; set; }

    // Navigation property
    public Sistema? Sistema { get; set; }
}
