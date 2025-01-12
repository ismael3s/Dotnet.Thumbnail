using Dotnet.Thumbnail.Core.Contracts;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Tags;

namespace Dotnet.Thumbnail.Infra.Minio;

public sealed class MinioFileStorage(IMinioClient client) : IFileStorage
{
    public async Task UploadAsync(FileStorageUploadInput input, CancellationToken cancellationToken = default)
    {
        var tags = new Tagging(input.Metadata, false);
        var args = new PutObjectArgs().WithBucket("bucket")
            .WithStreamData(input.File)
            .WithContentType(input.ContentType)
            .WithObject(input.Name)
            .WithObjectSize(input.File.Length)
            .WithTagging(tags);
        await client.PutObjectAsync(args, cancellationToken);
    }

    public async Task<Stream> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        var args = new GetObjectArgs().WithBucket("bucket").WithObject($"pending/{id}").WithCallbackStream(
            async (cbStream, cbCancellationToken) =>
            {
                await cbStream.CopyToAsync(stream, cbCancellationToken);
            });
        await client.GetObjectAsync(args, cancellationToken);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public async Task MoveToFolderAsync(MoveToFolderInput input, CancellationToken cancellationToken = default)
    {
        var copyObjectArgs = new CopyObjectArgs().WithBucket("bucket").WithObject(input.DestinationFilename).WithCopyObjectSource(
            new CopySourceObjectArgs()
                .WithBucket("bucket")
                .WithObject(input.SourceFilename)
        );
        await client.CopyObjectAsync(copyObjectArgs, cancellationToken);
        await DeleteAsync(new DeleteInput(input.SourceFilename), cancellationToken);
    }

    public async Task DeleteAsync(DeleteInput input, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket("bucket")
            .WithObject(input.Filename);
        await client.RemoveObjectAsync(args, cancellationToken);
    }
}

