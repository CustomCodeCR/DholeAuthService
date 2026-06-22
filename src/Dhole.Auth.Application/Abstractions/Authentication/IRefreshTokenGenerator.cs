namespace Dhole.Auth.Application.Abstractions.Authentication;

public interface IRefreshTokenGenerator
{
    string Generate();
    string Hash(string refreshToken);
}
