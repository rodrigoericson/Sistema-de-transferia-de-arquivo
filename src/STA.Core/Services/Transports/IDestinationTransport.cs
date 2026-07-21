namespace STA.Core.Services.Transports;

public interface IDestinationTransport
{
    Task UploadFileAsync(string sourceFilePath, string remotePath, bool overwrite, CancellationToken ct = default);

    Task DeleteFileAsync(string remotePath, CancellationToken ct = default);

    Task<bool> TestConnectionAsync(CancellationToken ct = default);

    Task<IReadOnlyList<string>> ListFilesAsync(string remoteDirectory, CancellationToken ct = default);
}
