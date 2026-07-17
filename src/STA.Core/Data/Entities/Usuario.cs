namespace STA.Core.Data.Entities;

public class Usuario
{
    public int CnUsuario { get; set; }

    public required string NmUsuario { get; set; }

    public required string NmDisplay { get; set; }

    public required string DsSenhaHash { get; set; }

    public string IdRole { get; set; } = "Viewer";

    public bool FlAtivo { get; set; } = true;

    public int NrTentativasFalhas { get; set; }

    public DateTime? DtBloqueadoAte { get; set; }

    public DateTime DtCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DtUltimoLogin { get; set; }
}
