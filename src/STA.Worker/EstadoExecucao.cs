namespace STA.Worker;

public class EstadoExecucao
{
    private readonly object _lock = new();

    public bool Executando { get; private set; }
    public bool Pausado { get; private set; }
    public string? EtapaAtual { get; private set; }
    public string? EtapaAtualDestino { get; private set; }
    public DateTime? CicloIniciadoEm { get; private set; }
    public DateTime UltimoCicloFim { get; private set; }
    public int? ArquivosTransferidosCicloAtual { get; private set; }
    public DateTime? ProximoCicloEm { get; private set; }

    public void IniciarCiclo()
    {
        lock (_lock)
        {
            Executando = true;
            CicloIniciadoEm = DateTime.UtcNow;
            EtapaAtual = null;
            EtapaAtualDestino = null;
            ArquivosTransferidosCicloAtual = 0;
        }
    }

    public void FinalizarCiclo(int totalArquivos, DateTime proximoCiclo)
    {
        lock (_lock)
        {
            Executando = false;
            EtapaAtual = null;
            EtapaAtualDestino = null;
            ArquivosTransferidosCicloAtual = totalArquivos;
            UltimoCicloFim = DateTime.UtcNow;
            ProximoCicloEm = proximoCiclo;
        }
    }

    public void SetEtapa(string etapa, string? destino = null)
    {
        lock (_lock)
        {
            EtapaAtual = etapa;
            EtapaAtualDestino = destino;
        }
    }

    public void IncrementarArquivos()
    {
        lock (_lock)
        {
            ArquivosTransferidosCicloAtual = (ArquivosTransferidosCicloAtual ?? 0) + 1;
        }
    }

    public void SetPausado(bool pausado, DateTime? proximoCiclo = null)
    {
        lock (_lock)
        {
            Pausado = pausado;
            Executando = false;
            if (proximoCiclo.HasValue)
                ProximoCicloEm = proximoCiclo;
        }
    }
}
