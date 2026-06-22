using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Auth.Contracts.Authentication;

namespace Dhole.Auth.Application.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress, string? UserAgent)
    : ICommand<Result<RefreshTokenResponse>>;
