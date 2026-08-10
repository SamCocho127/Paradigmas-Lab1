using HackerRank1.Domain;

namespace HackerRank1.Application;

public interface ITokenGenerator
{
    string Generate(User user, JwtSettings jwtSettings);
}
