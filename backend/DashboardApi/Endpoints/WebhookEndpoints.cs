using System.Text.Json;
using DashboardApi.Services;

namespace DashboardApi.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this WebApplication app)
    {
        app.MapPost("/webhooks/github",
        async (
            HttpRequest request,
            IConfiguration configuration,
            WebhookService webhook) =>
        {
            string body =
                await new StreamReader(request.Body)
                    .ReadToEndAsync();

            string? signature =
                request.Headers["X-Hub-Signature-256"];

            if (!GitHubWebhookVerifier.Verify(
                body,
                signature,
                configuration["GitHub:WebhookSecret"]!))
            {
                return Results.Unauthorized();
            }

            string? eventType =
                request.Headers["X-GitHub-Event"];

            if (eventType == "ping")
                return Results.Ok();

            using var doc = JsonDocument.Parse(body);

            await webhook.ProcessWebhook(
                eventType!,
                doc.RootElement);

            return Results.Ok();
        });
    }
}