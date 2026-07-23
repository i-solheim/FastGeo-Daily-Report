using System.Net.Http.Headers;
using DashboardApi.Endpoints;
using DashboardApi.Repositories;
using DashboardApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// CORS
// --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --------------------
// GitHub API
// --------------------
builder.Services.AddHttpClient<GitHubService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue(
            "Bearer",
            config["GitHub:Token"]);

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "FastGeo-Daily-Report");
});

// --------------------
// Dependency Injection
// --------------------
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<IssueRepository>();
builder.Services.AddScoped<StatusChangeRepository>();

builder.Services.AddScoped<WebhookService>();

var app = builder.Build();

app.UseCors("AllowReactDev");

// --------------------
// Endpoints
// --------------------
DashboardEndpoints.MapDashboardEndpoints(app);
WebhookEndpoints.MapWebhookEndpoints(app);

app.Run();