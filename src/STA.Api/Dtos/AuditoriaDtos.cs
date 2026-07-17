namespace STA.Api.Dtos;

public record AuditoriaDto(
    int CnAuditoria,
    int? CnUsuario,
    string NmUsuario,
    string IdEntidade,
    int IdReferencia,
    string IdAcao,
    DateTime DtAcao,
    string? DsDetalhe
);
