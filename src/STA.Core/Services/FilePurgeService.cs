using Microsoft.Extensions.Logging;
using STA.Core.Models;

namespace STA.Core.Services;

public interface IFilePurgeService
{
    int PurgeDirectory(string directory, DateTime cutoff, string mask);
    int PurgeNode(TransferPath node);
}

public class FilePurgeService : IFilePurgeService
{
    private readonly IFileMaskMatcher _maskMatcher;
    private readonly ILogger<FilePurgeService> _logger;

    public FilePurgeService(IFileMaskMatcher maskMatcher, ILogger<FilePurgeService> logger)
    {
        _maskMatcher = maskMatcher;
        _logger = logger;
    }

    public int PurgeNode(TransferPath node)
    {
        if (node.DiasExcluir <= 0)
            return 0;

        var cutoff = DateTime.Now.AddDays(-node.DiasExcluir);
        var total = PurgeDirectory(node.DiretorioPrincipal, cutoff, node.MascaraArq);

        if (!string.IsNullOrWhiteSpace(node.DiretorioBackup))
            total += PurgeDirectory(node.DiretorioBackup, cutoff, node.MascaraArq);

        return total;
    }

    public int PurgeDirectory(string directory, DateTime cutoff, string mask)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return 0;

        var purged = 0;

        try
        {
            foreach (var file in new DirectoryInfo(directory).GetFiles())
            {
                if (!ShouldPurge(file, cutoff, mask))
                    continue;

                try
                {
                    file.Delete();
                    purged++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao excluir '{File}'.", file.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao purgar diretório '{Dir}'.", directory);
        }

        return purged;
    }

    private bool ShouldPurge(FileInfo file, DateTime cutoff, string mask)
    {
        if (file.LastWriteTime >= cutoff)
            return false;

        return string.IsNullOrWhiteSpace(mask) || _maskMatcher.Match(file.Name, mask);
    }
}