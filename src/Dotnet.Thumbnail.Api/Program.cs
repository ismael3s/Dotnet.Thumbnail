using System.Text.Json;
using Dotnet.Thumbnail.Core.Events;
using Dotnet.Thumbnail.Core.Services;
using Dotnet.Thumbnail.Infra;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureTelemetry();

builder.Services.AddInfra(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/", async (IFormFile file,
        [FromServices]
        LinkGenerator linkGenerator,
    ThumbnailService processThumbnailService,
    CancellationToken  cancellationToken
            ) =>
    {
        var output = await processThumbnailService.UploadOriginalImage(new UploadOriginalImageInput(file.OpenReadStream(), file.Name, file.ContentType), cancellationToken);
        var path = linkGenerator.GetPathByName("FindById", new
        {
            id = output.Id
        });
        return TypedResults.Accepted(path);
    })
    .DisableAntiforgery();

app.MapGet("/{id:guid}", (string id, ILogger<Program> logger) => id).WithName("FindById");


app.Run();
