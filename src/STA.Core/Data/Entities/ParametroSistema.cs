namespace STA.Core.Data.Entities;

/// <summary>
/// Representa um parâmetro do sistema (horário inicial, final, período de execução).
/// Mapeia sta.tbl_parametro_sistema.
/// Exemplo: cn_parametro_sistema=1 → HoraInicial, =2 → HoraFinal, =3 → PeriodoMinutos
/// </summary>
public class ParametroSistema
{
    public int CnParametroSistema { get; set; }

    public int CnSistema { get; set; }

    public required string CdParametroSistema { get; set; }

    // Navigation property
    public Sistema? Sistema { get; set; }
}
