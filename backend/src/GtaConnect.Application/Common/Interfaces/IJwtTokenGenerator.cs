namespace GtaConnect.Application.Common.Interfaces;

public record JwtToken(string Value, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    JwtToken GenerateToken(Guid userId, string email, string displayName, IReadOnlyList<string> roles);
}
