namespace STA.Core.Models;

public record TransferChain(
    string Etapa,
    IReadOnlyList<TransferPath> Nodes);
