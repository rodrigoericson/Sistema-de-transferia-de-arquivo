namespace STA.Worker.Settings;

/// <summary>
/// Configurações da aplicação STA lidas do appsettings.json (seção "StaSettings").
/// Usa pattern Options do Microsoft.Extensions.Options.
/// </summary>
public class StaSettings
{
    /// <summary>
    /// Nome/alias do sistema (e.g., "STA"). Usado para buscar parâmetros e registrar logs.
    /// </summary>
    public required string NomeSistema { get; set; }

    /// <summary>
    /// ID numérico do processo. Usado para diferenciar logs de processos diferentes.
    /// </summary>
    public int CnProcesso { get; set; } = 1;

    /// <summary>
    /// Caminho absoluto do arquivo XML de configuração de caminhos de transferência.
    /// Exemplo: C:\\sistemas\\paths.xml
    /// </summary>
    public required string ArquivoPathsXml { get; set; }

    /// <summary>
    /// Caminho completo do executável 7-Zip.
    /// Exemplo: C:\\Program Files\\7-Zip\\7z.exe
    /// </summary>
    public required string Arquivo7Zip { get; set; }

    /// <summary>
    /// Timeout para operações de compactação/descompactação em milissegundos.
    /// Padrão: 1800000 (30 minutos).
    /// </summary>
    public int TimeoutCompactacaoMs { get; set; } = 1800000;

    /// <summary>
    /// Se true, sobrescreve arquivos já existentes no destino.
    /// </summary>
    public bool SobreEscreverArquivos { get; set; } = true;

    /// <summary>
    /// Número de dias para manter logs antes de excluir.
    /// </summary>
    public int QtdDiasExcluirLog { get; set; } = 5;

    /// <summary>
    /// Se true, gera logs de sucesso no Event Viewer do Windows.
    /// </summary>
    public bool GeraLogSucessoEventView { get; set; } = true;

    /// <summary>
    /// Se true, gera logs de sucesso no banco de dados.
    /// </summary>
    public bool GeraLogSucessoBancoDados { get; set; } = true;
}
