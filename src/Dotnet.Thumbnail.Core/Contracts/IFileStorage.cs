namespace Dotnet.Thumbnail.Core.Contracts;

public record FileStorageUploadInput(
    Stream File,
    string Name,
    string ContentType = "application/octet-stream",
    Dictionary<string, string>? Metadata = null);
public sealed record MoveToFolderInput(string SourceFilename, string DestinationFilename);

public sealed record DeleteInput(string Filename);

public interface IFileStorage
{
    public Task UploadAsync(FileStorageUploadInput input, CancellationToken cancellationToken = default);
    public Task<Stream> DownloadAsync(Guid id, CancellationToken cancellationToken = default);
    public Task MoveToFolderAsync(MoveToFolderInput input, CancellationToken cancellationToken = default);
    
    public Task DeleteAsync(DeleteInput input, CancellationToken cancellationToken = default);    

}