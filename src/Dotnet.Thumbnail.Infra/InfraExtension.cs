using Dotnet.Thumbnail.Core;
using Dotnet.Thumbnail.Core.Contracts;
using Dotnet.Thumbnail.Core.Services;
using Dotnet.Thumbnail.Infra.ImageSharper;
using Dotnet.Thumbnail.Infra.MassTransit;
using Dotnet.Thumbnail.Infra.Minio;
using MassTransit;
using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RabbitMQ.Client;

namespace Dotnet.Thumbnail.Infra;

public static class InfraExtension
{
    public static IHostApplicationBuilder ConfigureTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeScopes = true;
            logging.IncludeFormattedMessage = true;
        });
        builder.Services.AddOpenTelemetry().WithTracing(tracing =>
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
            )
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation()
            )
            .UseOtlpExporter()
            ;
        return builder;
    }

    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        bool.TryParse(configuration["Minio:SSL"], out bool minioSsl);
        services.AddScoped<ThumbnailService>();
        services.AddSingleton<IFileStorage, MinioFileStorage>();
        services.AddSingleton<IPublisherBus, PublisherBus>();
        services.AddSingleton<IImageResizer, ImageResizer>();
        services.AddMinio(configureClient => configureClient
            .WithEndpoint(configuration["Minio:Endpoint"])
            .WithCredentials(configuration["Minio:AccessKey"], configuration["Minio:SecretKey"])
            .WithSSL(minioSsl));
        services.AddMassTransit(x =>
        {
            x.AddConsumer<FileUploadedEventConsumer>();
            x.UsingInMemory((ctx, cfg) => { cfg.ConfigureEndpoints(ctx); });
        });
        return services;
    }
}