using Dotnet.Thumbnail.Core.Events;
using Dotnet.Thumbnail.Core.Services;
using MassTransit;

namespace Dotnet.Thumbnail.Infra.MassTransit;

public sealed class FileUploadedEventConsumer(ThumbnailService service): IConsumer<FileUploadedEvent>
{
    public async Task Consume(ConsumeContext<FileUploadedEvent> context)
    {
        await service.ResizeImage(context.Message);
    }
}