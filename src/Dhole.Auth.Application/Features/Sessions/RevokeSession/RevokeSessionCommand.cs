using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Sessions.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId, Guid? RevokedBy, string? Reason)
    : ICommand<Result>;
