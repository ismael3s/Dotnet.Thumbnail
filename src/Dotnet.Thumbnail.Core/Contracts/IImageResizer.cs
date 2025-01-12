namespace Dotnet.Thumbnail.Core.Contracts;


public sealed record ResizeInput(Stream Stream, int Width = 256, int Height = 256);
public interface IImageResizer
{
    public Task<Stream> Resize(ResizeInput input, CancellationToken cancellationToken = default);
}