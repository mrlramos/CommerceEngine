using Identity.API.Models;
using System.Security.Claims;

namespace Identity.API.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string ipAddress);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string ipAddress, string reason);
        Task<bool> ValidateRefreshTokenAsync(string token);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task CleanupExpiredTokensAsync();
    }
} 