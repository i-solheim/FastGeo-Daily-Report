using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
              .AllowAnyHeader()
              .AllowCredentials();
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

builder.Services.AddHttpClient<GitHubOAuthService>(client =>
{
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "FastGeo-Daily-Report");
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"],

                ValidateLifetime = true
            };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["auth"];

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// --------------------
// Dependency Injection
// --------------------
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<IssueRepository>();
builder.Services.AddScoped<StatusChangeRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddSingleton<DailyReportFormatter>();

builder.Services.AddScoped<WebhookService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordService>();
var app = builder.Build();

app.UseCors("AllowReactDev");
app.UseAuthentication();
app.UseAuthorization();

// --------------------
// Endpoints
// --------------------
DashboardEndpoints.MapDashboardEndpoints(app);
WebhookEndpoints.MapWebhookEndpoints(app);
AuthEndpoints.MapAuthEndpoints(app);

app.Run();