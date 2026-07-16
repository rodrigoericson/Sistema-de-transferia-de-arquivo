namespace STA.Worker;

public class EstadoExecucao
{
    public bool Executando { get; set; }
    public bool Pausado { get; set; }
    public string? EtapaAtual { get; set; }
    public string? EtapaAtualDestino { get; set; }
    public DateTime? CicloIniciadoEm { get; set; }
    public DateTime UltimoCicloFim { get; set; }
    public int? ArquivosTransferidosCicloAtual { get; set; }
    public DateTime? ProximoCicloEm { get; set; }

    public void IniciarCiclo()
    {
        Executando = true;
        CicloIniciadoEm = DateTime.UtcNow;
        EtapaAtual = null;
        EtapaAtualDestino = null;
        ArquivosTransferidosCicloAtual = 0;
    }

    public void FinalizarCiclo(int totalArquivos, DateTime proximoCiclo)
    {
        Executando = false;
        EtapaAtual = null;
        EtapaAtualDestino = null;
        ArquivosTransferidosCicloAtual = totalArquivos;
        UltimoCicloFim = DateTime.UtcNow;
        ProximoCicloEm = proximoCiclo;
    }

    public void SetEtapa(string etapa, string? destino = null)
    {
        EtapaAtual = etapa;
        EtapaAtualDestino = destino;
    }

    public void IncrementarArquivos() => ArquivosTransferidosCicloAtual = (ArquivosTransferidosCicloAtual ?? 0) + 1;
}
