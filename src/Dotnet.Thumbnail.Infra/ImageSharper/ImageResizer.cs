using Dotnet.Thumbnail.Core.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Dotnet.Thumbnail.Infra.ImageSharper;

public sealed class ImageResizer: IImageResizer
{
    public async Task<Stream> Resize(ResizeInput input, CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(input.Stream, cancellationToken);
        image.Mutate(x => x.Resize(input.Width, input.Height));
        var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, new PngEncoder(), cancellationToken);
        outputStream.Seek(0, SeekOrigin.Begin);
        return outputStream;
    }
}