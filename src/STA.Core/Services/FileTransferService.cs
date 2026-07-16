using Microsoft.Extensions.Logging;
using STA.Core.Data.Entities;
using STA.Core.Data.Repositories;
using STA.Core.Models;

namespace STA.Core.Services;

public record FileTransferResult(
    int FilesProcessed,
    int FilesSucceeded,
    int FilesFailed,
    List<string> ErrorMessages);

public interface IFileTransferService
{
    Task<FileTransferResult> TransferFanOutAsync(
        TransferPath config,
        string sourceDirectory,
        IReadOnlyList<string> destinationDirectories,
        bool overwriteExisting,
        int timeoutCompactacaoMs,
        int? cnLogProcesso,
        CancellationToken cancellationToken);
}

public class FileTransferService : IFileTransferService
{
    private readonly IFileMaskMatcher _maskMatcher;
    private readonly IFileSizeValidator _sizeValidator;
    private readonly IFileLockChecker _lockChecker;
    private readonly IFileCompressor _compressor;
    private readonly IFilePurgeService _purgeService;
    private readonly ILogArquivoRepository _logArquivoRepository;
    private readonly ILogger<FileTransferService> _logger;

    public FileTransferService(
        IFileMaskMatcher maskMatcher,
        IFileSizeValidator sizeValidator,
        IFileLockChecker lockChecker,
        IFileCompressor compressor,
        IFilePurgeService purgeService,
        ILogArquivoRepository logArquivoRepository,
        ILogger<FileTransferService> logger)
    {
        _maskMatcher = maskMatcher;
        _sizeValidator = sizeValidator;
        _lockChecker = lockChecker;
        _compressor = compressor;
        _purgeService = purgeService;
        _logArquivoRepository = logArquivoRepository;
        _logger = logger;
    }

    public async Task<FileTransferResult> TransferFanOutAsync(
        TransferPath config,
        string sourceDirectory,
        IReadOnlyList<string> destinationDirectories,
        bool overwriteExisting,
        int timeoutCompactacaoMs,
        int? cnLogProcesso,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            _logger.LogWarning("Diretório de origem não existe: '{Path}'.", sourceDirectory);
            return new FileTransferResult(0, 0, 0, [$"Diretório de origem não existe: {sourceDirectory}"]);
        }

        foreach (var destDir in destinationDirectories)
        {
            try { Directory.CreateDirectory(destDir); }
            catch (Exception ex) { _logger.LogWarning(ex, "Não foi possível criar diretório de destino: '{Path}'.", destDir); }
        }
        if (!string.IsNullOrWhiteSpace(config.DiretorioBackup))
        {
            try { Directory.CreateDirectory(config.DiretorioBackup); }
            catch (Exception ex) { _logger.LogWarning(ex, "Não foi possível criar diretório de backup: '{Path}'.", config.DiretorioBackup); }
        }

        var errors = new List<string>();
        int succeeded = 0, failed = 0;
        var files = new DirectoryInfo(sourceDirectory).GetFiles();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_maskMatcher.Match(file.Name, config.MascaraArq))
                continue;

            if (_lockChecker.IsFileLocked(file.FullName))
            {
                _logger.LogWarning("Arquivo em uso, ignorado: '{File}'.", file.Name);
                await GravarLogArquivoAsync(
                    cnLogProcesso, config, file.Name, sourceDirectory, destinationDirectories.FirstOrDefault() ?? "",
                    file.Length, DateTime.UtcNow, "W", "Arquivo em uso (locked) — será tentado no próximo ciclo", false, false, cancellationToken);
                continue;
            }

            if (!_sizeValidator.IsWithinRange(file.Length, config.TamanhoInicialArqBytes, config.TamanhoFinalArqBytes))
            {
                _logger.LogDebug("Arquivo fora da faixa de tamanho: '{File}' ({Size} bytes).", file.Name, file.Length);
                continue;
            }

            // Processa 1 arquivo por vez: backup + fan-out para todos os destinos + apaga origem só no final
            var dtInicioArquivo = DateTime.UtcNow;
            bool compressed = false;
            try
            {
                var (filePath, fileName, wasCompressed) = await TryCompressAsync(file, config, sourceDirectory, timeoutCompactacaoMs, cancellationToken);
                compressed = wasCompressed;

                // 1. Backup (se configurado)
                CopyToBackup(filePath, fileName, config.DiretorioBackup, overwriteExisting);

                // 2. Fan-out: copia para TODOS os destinos
                bool fanOutOk = true;
                foreach (var destDir in destinationDirectories)
                {
                    try
                    {
                        CopyToDestination(filePath, fileName, destDir, overwriteExisting);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao copiar '{File}' para '{Dest}'.", fileName, destDir);
                        errors.Add($"{file.Name} → {destDir}: {ex.Message}");
                        fanOutOk = false;
                    }
                }

                // 3. Só apaga origem se fan-out foi 100% bem-sucedido
                if (fanOutOk)
                {
                    CleanupSource(file.FullName, filePath);
                }

                // 4. Grava log por destino (status S se copia ok, E se falhou)
                int destIdx = 0;
                var destErrors = errors.Where(e => e.StartsWith($"{file.Name} →")).ToList();
                foreach (var destDir in destinationDirectories)
                {
                    var errMsg = destIdx < destErrors.Count
                        ? destErrors[destIdx].Split("→", 2)[1].Trim().TrimStart(':').Trim()
                        : null;
                    await GravarLogArquivoAsync(
                        cnLogProcesso, config, fileName, sourceDirectory, destDir,
                        file.Length, dtInicioArquivo, errMsg != null ? "E" : "S", errMsg, compressed, false, cancellationToken);
                    destIdx++;
                }
                if (!fanOutOk) failed++;
                else succeeded++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failed++;
                errors.Add($"{file.Name}: {ex.Message}");
                _logger.LogError(ex, "Erro ao transferir '{File}'.", file.Name);
                await GravarLogArquivoAsync(
                    cnLogProcesso, config, file.Name, sourceDirectory, destinationDirectories.FirstOrDefault() ?? "",
                    file.Length, dtInicioArquivo, "E", ex.Message, compressed, false, cancellationToken);
            }
        }

        PurgeBackupIfNeeded(config);

        return new FileTransferResult(files.Length, succeeded, failed, errors);
    }


    private async Task<(bool Compressed, bool Decompressed)> ProcessFileAsync(
        FileInfo file,
        TransferPath config,
        string sourceDirectory,
        string destinationDirectory,
        bool overwriteExisting,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var (filePath, fileName, compressed) = await TryCompressAsync(file, config, sourceDirectory, timeoutMs, cancellationToken);

        CopyToDestination(filePath, fileName, destinationDirectory, overwriteExisting);
        CopyToBackup(filePath, fileName, config.DiretorioBackup, overwriteExisting);

        var decompressed = await TryDecompressAtDestinationAsync(
            Path.Combine(destinationDirectory, fileName), destinationDirectory, config, timeoutMs, cancellationToken);

        CleanupSource(file.FullName, filePath);

        return (compressed, decompressed);
    }

    private async Task<(string FilePath, string FileName, bool Compressed)> TryCompressAsync(
        FileInfo file,
        TransferPath config,
        string sourceDirectory,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        if (!_compressor.IsCompressionTypeSupported(config.CompactaOrigemTipo)
            || _compressor.IsFileCompressed(file.FullName))
            return (file.FullName, file.Name, false);

        var archiveName = file.Name + "." + config.CompactaOrigemTipo.ToLowerInvariant();
        var archivePath = Path.Combine(sourceDirectory, archiveName);

        var ok = await _compressor.CompressAsync(
            file.FullName, archivePath, config.CompactaOrigemTipo, timeoutMs, cancellationToken);

        if (ok)
            return (archivePath, archiveName, true);

        _logger.LogWarning("Falha na compactação de '{File}'. Transferindo original.", file.Name);
        return (file.FullName, file.Name, false);
    }

    private static void CopyToDestination(string sourceFilePath, string fileName, string destDir, bool overwrite)
    {
        var destPath = Path.Combine(destDir, fileName);
        File.Copy(sourceFilePath, destPath, overwrite);
    }

    private void CopyToBackup(string sourceFilePath, string fileName, string backupDir, bool overwrite)
    {
        if (string.IsNullOrWhiteSpace(backupDir))
            return;

        try
        {
            var backupPath = Path.Combine(backupDir, fileName);
            File.Copy(sourceFilePath, backupPath, overwrite);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao criar backup de '{File}'.", fileName);
        }
    }

    private async Task<bool> TryDecompressAtDestinationAsync(
        string destinationFilePath,
        string destinationDirectory,
        TransferPath config,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(config.DescompactaDestino)
            || !config.DescompactaDestino.Equals("SIM", StringComparison.OrdinalIgnoreCase)
            || !_compressor.IsFileCompressed(destinationFilePath))
            return false;

        var ok = await _compressor.DecompressAsync(
            destinationFilePath, destinationDirectory, timeoutMs, cancellationToken);

        if (ok)
            File.Delete(destinationFilePath);
        else
            _logger.LogWarning("Falha na descompactação de '{File}' no destino.", Path.GetFileName(destinationFilePath));

        return ok;
    }

    private static void CleanupSource(string originalPath, string transferredPath)
    {
        if (transferredPath != originalPath && File.Exists(transferredPath))
            File.Delete(transferredPath);

        if (File.Exists(originalPath))
            File.Delete(originalPath);
    }

    private void PurgeBackupIfNeeded(TransferPath config)
    {
        if (string.IsNullOrWhiteSpace(config.DiretorioBackup) || config.DiasExcluir <= 0)
            return;

        var cutoff = DateTime.Now.AddDays(-config.DiasExcluir);
        _purgeService.PurgeDirectory(config.DiretorioBackup, cutoff, config.MascaraArq);
    }

    private async Task GravarLogArquivoAsync(
        int? cnLogProcesso,
        TransferPath config,
        string nomeArquivo,
        string diretorioOrigem,
        string diretorioDestino,
        long tamanhoBytes,
        DateTime dtInicio,
        string status,
        string? mensagem,
        bool compactado,
        bool descompactado,
        CancellationToken cancellationToken)
    {
        try
        {
            var log = new LogArquivo
            {
                CnLogProcesso = cnLogProcesso,
                CnEtapa = config.CnEtapa,
                CnRota = config.CnRota,
                NmArquivo = nomeArquivo,
                DsDiretorioOrigem = diretorioOrigem,
                DsDiretorioDestino = diretorioDestino,
                NrTamanhoBytes = tamanhoBytes,
                DtInicio = dtInicio,
                DtFim = DateTime.UtcNow,
                IdStatus = status,
                DsMensagem = mensagem,
                FlCompactado = compactado,
                FlDescompactado = descompactado
            };

            await _logArquivoRepository.InserirAsync(log, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Falha ao gravar log de arquivo '{File}'.", nomeArquivo);
        }
    }
}
