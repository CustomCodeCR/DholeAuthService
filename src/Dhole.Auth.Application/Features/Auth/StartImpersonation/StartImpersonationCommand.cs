using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Auth.Contracts.Authentication;

namespace Dhole.Auth.Application.Auth.StartImpersonation;

public sealed record StartImpersonationCommand(
    Guid ActorUserId,
    Guid TargetUserId,
    string? IpAddress,
    string? UserAgent
) : ICommand<Result<ImpersonationResponse>>;
