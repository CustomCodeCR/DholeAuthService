using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Auth.Api.Endpoints;
using Dhole.Auth.Api.Middleware;
using Dhole.Auth.Application.DependencyInjection;
using Dhole.Auth.Infrastructure.DependencyInjection;
using Dhole.Auth.Infrastructure.Time;
using Dhole.Auth.Persistence.DbContexts;
using Dhole.Auth.Persistence.DependencyInjection;
using Dhole.Auth.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "DholeWebCors";

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

builder.Services.AddCustomCodeApiWithSwagger(title: "Dhole Auth Service", version: "v1");

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        CorsPolicyName,
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "http://127.0.0.1:5173",
                    "http://192.168.1.193:5173"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseCustomCodeApi();

app.UseCors(CorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.MapGet(
        "/health",
        () =>
        {
            return Results.Ok(
                new
                {
                    service = "DholeAuthService",
                    status = "Healthy",
                    timestamp = DateTimeOffset.UtcNow,
                }
            );
        }
    )
    .AllowAnonymous();

app.UseAuthentication();
app.UseMiddleware<AuditExecutionContextMiddleware>();
app.UseAuthorization();
app.UseMiddleware<AuditEndpointMiddleware>();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapRoleEndpoints();
app.MapScopeEndpoints();
app.MapSessionEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();

    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
