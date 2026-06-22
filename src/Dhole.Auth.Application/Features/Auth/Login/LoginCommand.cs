using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Auth.Contracts.Authentication;

namespace Dhole.Auth.Application.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent
) : ICommand<Result<LoginResponse>>;
