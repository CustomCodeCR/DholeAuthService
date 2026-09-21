using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Auth.StopImpersonation;

public sealed record StopImpersonationCommand(
    Guid TargetUserId,
    Guid SessionId,
    Guid ImpersonatorUserId
) : ICommand<Result>;
