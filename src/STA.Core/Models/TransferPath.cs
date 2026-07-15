namespace STA.Core.Models;

public record TransferPath(
    string Etapa,
    string DiretorioPrincipal,
    string DiretorioBackup,
    int DiasExcluir,
    string MascaraArq,
    string CompactaOrigemTipo,
    string DescompactaDestino,
    long TamanhoInicialArqBytes,
    long TamanhoFinalArqBytes,
    int? CnEtapa = null,
    int? CnRota = null);
