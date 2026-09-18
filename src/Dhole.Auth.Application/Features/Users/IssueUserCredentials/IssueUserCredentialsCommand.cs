using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.IssueUserCredentials;

public sealed record IssueUserCredentialsCommand(Guid UserId, Guid? IssuedBy)
    : ICommand<Result<IssuedUserCredentialsDto>>;
