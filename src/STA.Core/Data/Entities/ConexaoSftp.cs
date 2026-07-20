namespace STA.Core.Data.Entities;

public class ConexaoSftp
{
    public int CnConexaoSftp { get; set; }

    public required string NmConexao { get; set; }

    public required string DsHost { get; set; }

    public int NrPorta { get; set; } = 22;

    public required string DsUsuario { get; set; }

    public byte[]? DsSenhaCriptografada { get; set; }

    public string? DsCaminhoChavePrivada { get; set; }

    public required string DsHorariosExecucao { get; set; }

    public string DsDiasSemana { get; set; } = "seg,ter,qua,qui,sex";

    public bool FlArquivoObrigatorio { get; set; }

    public int NrToleranciaMinutos { get; set; } = 10;

    public bool FlAtivo { get; set; } = true;

    public DateTime DtCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DtUltimoUso { get; set; }
}
