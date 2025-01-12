using Dotnet.Thumbnail.Core.Contracts;
using MassTransit;

namespace Dotnet.Thumbnail.Infra.MassTransit;

public sealed class PublisherBus(IBus bus) : IPublisherBus
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await bus.Publish(message, cancellationToken);
    }
}