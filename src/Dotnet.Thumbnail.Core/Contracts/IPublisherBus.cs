namespace Dotnet.Thumbnail.Core.Contracts;

public interface IPublisherBus
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}