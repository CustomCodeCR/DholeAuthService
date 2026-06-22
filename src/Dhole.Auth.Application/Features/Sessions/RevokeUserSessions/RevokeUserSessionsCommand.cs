using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Sessions.RevokeUserSessions;

public sealed record RevokeUserSessionsCommand(Guid UserId, Guid? RevokedBy, string? Reason)
    : ICommand<Result>;
