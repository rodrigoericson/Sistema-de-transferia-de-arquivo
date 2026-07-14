namespace STA.Worker.Data.Entities;

/// <summary>
/// Representa um sistema registrado na tabela sta.tbl_sistema.
/// Mapeia sistemas por alias único (e.g., "STA" para Sistema de Transferência de Arquivos).
/// </summary>
public class Sistema
{
    public int CnSistema { get; set; }

    public required string CdAliasSistema { get; set; }

    // Navigation property
    public ICollection<ParametroSistema> Parametros { get; set; } = new List<ParametroSistema>();

    public ICollection<LogProcesso> Logs { get; set; } = new List<LogProcesso>();
}
