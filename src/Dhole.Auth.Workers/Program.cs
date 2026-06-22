using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Mongo.DependencyInjection;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Mongo;
using Dhole.Auth.Application.Abstractions.Security;
using Dhole.Auth.Application.DependencyInjection;
using Dhole.Auth.Infrastructure.Authentication;
using Dhole.Auth.Infrastructure.Mongo;
using Dhole.Auth.Infrastructure.Security;
using Dhole.Auth.Infrastructure.Time;
using Dhole.Auth.Persistence.DependencyInjection;
using Dhole.Auth.Worker.DependencyInjection;
using Dhole.Auth.Workers.Security;

var contentRoot = Path.Combine(Directory.GetCurrentDirectory(), "src", "Dhole.Auth.Workers");

if (!Directory.Exists(contentRoot))
{
    contentRoot = Directory.GetCurrentDirectory();
}

var builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings { Args = args, ContentRootPath = contentRoot }
);

builder.Configuration.Sources.Clear();

builder
    .Configuration.SetBasePath(contentRoot)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine($"Postgres: {builder.Configuration["Postgres:ConnectionString"]}");

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<ICurrentUser, WorkerCurrentUser>();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<IJwtTokenGenerator, WorkerJwtTokenGenerator>();

builder.Services.Configure<LoginRateLimitOptions>(
    builder.Configuration.GetSection(LoginRateLimitOptions.SectionName)
);

builder.Services.AddScoped<ILoginRateLimiter, RedisLoginRateLimiter>();

builder.Services.AddScoped<IRevokedTokenStore, RedisRevokedTokenStore>();
builder.Services.AddScoped<IActiveSessionStore, RedisActiveSessionStore>();

builder.Services.AddCustomCodeMongo(builder.Configuration);
builder.Services.AddScoped<IAuthLoginAttemptLogWriter, AuthLoginAttemptLogWriter>();
builder.Services.AddScoped<IAuthUserSecuritySnapshotWriter, AuthUserSecuritySnapshotWriter>();

builder.Services.AddAuthWorker(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
