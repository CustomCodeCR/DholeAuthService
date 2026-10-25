namespace Dhole.Auth.Application.Abstractions.Auditing;

public interface IAuthAuditService
{
    Task PublishAsync(AuthAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
