using CustomCodeFramework.Workers.Abstractions;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Worker.Workers;

internal sealed class SessionCleanupWorker(
    ServiceDbContext dbContext,
    ILogger<SessionCleanupWorker> logger
) : IBackgroundWorker
{
    public string Name => "auth.session-cleanup";

    public async Task ExecuteAsync(
        IWorkerExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        var now = DateTime.UtcNow;

        var expiredSessions = await dbContext
            .Sessions.Where(x => !x.IsRevoked && x.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        foreach (var session in expiredSessions)
        {
            session.Revoke(revokedBy: null, reason: "Sesión expirada automáticamente.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Revoked {Count} expired session(s).", expiredSessions.Count);
    }
}
