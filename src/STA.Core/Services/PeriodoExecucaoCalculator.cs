namespace STA.Core.Services;

public static class PeriodoExecucaoCalculator
{
    public static bool DentroPeriodo(TimeSpan horaIni, TimeSpan horaFim, TimeSpan agora)
    {
        if (horaIni > horaFim)
            return false;

        return agora >= horaIni && agora <= horaFim;
    }
}
