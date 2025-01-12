using Dotnet.Thumbnail.Core.Contracts;
using Dotnet.Thumbnail.Core.Events;
using Microsoft.Extensions.Logging;

namespace Dotnet.Thumbnail.Core.Services;


public sealed record UploadOriginalImageInput(Stream Stream, string OriginalFileName, string ContentType = "application/octet-stream");
public sealed record UploadOriginalImageOutput(Guid Id);
public sealed class ThumbnailService(ILogger<ThumbnailService> logger, IFileStorage fileStorage, IPublisherBus publisherBus, IImageResizer imageResizer)
{
    public async Task ResizeImage(FileUploadedEvent @event, CancellationToken ct = default)
    {
        List<(int, int)> sizes =
        [
            (256, 256),
            (512, 512),
        ];
        logger.LogInformation("Start processing thumbnail with id {Id}", @event.Id);
        var anErrorHappened = false;
        var tasks = sizes.Select(async (size) =>
        {
            var (width, height) = size;
            logger.LogInformation("Resizing Image from Id {Id} to dimensions {Dimension}", @event.Id, (width, height));
            try
            {
                await using var stream = await fileStorage.DownloadAsync(@event.Id, ct);
                await using var output = await imageResizer.Resize(new ResizeInput(stream, width, height), ct);
                await fileStorage.UploadAsync(
                    new FileStorageUploadInput(output, $"done/{@event.Id}/{width}x{height}.png"), ct);
                logger.LogInformation("Finished processing thumbnail with id {Id} to {Dimension}", @event.Id, size);
            }
            catch (Exception ex)
            {
                anErrorHappened = true;
                logger.LogError(ex, "Failed to resize image {Id} to {Dimension}", @event.Id, size);
            }
        });
        await Task.WhenAll(tasks);
        if (!anErrorHappened)
        {
            await fileStorage.MoveToFolderAsync(new MoveToFolderInput($"pending/{@event.Id}", $"done/{@event.Id}/original.png"), ct);
        }
    }

    public async Task<UploadOriginalImageOutput> UploadOriginalImage(UploadOriginalImageInput input, CancellationToken ct = default)
    {
        var fileId = Guid.CreateVersion7();
        var metadata = new Dictionary<string, string>
        {
            { "OriginalFileName", input.OriginalFileName },
        };
        await fileStorage.UploadAsync(new FileStorageUploadInput(input.Stream, $"pending/{fileId}", input.ContentType, metadata), ct);
        await publisherBus.PublishAsync(new FileUploadedEvent(fileId), ct);
        return new UploadOriginalImageOutput(fileId);
    }

}