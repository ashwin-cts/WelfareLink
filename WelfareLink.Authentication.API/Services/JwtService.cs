using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WelfareLink.Authentication.API.Models;

namespace WelfareLink.Authentication.API.Services
{
    public interface IJwtService
    {
        string GenerateToken(AuthUser user);
        DateTime GetTokenExpiry();
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secret = configuration["JwtSettings:Secret"] 
                ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");
            _issuer = configuration["JwtSettings:Issuer"] 
                ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured");
            _audience = configuration["JwtSettings:Audience"] 
                ?? throw new InvalidOperationException("JwtSettings:Audience is not configured");

            if (!int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out _expiryMinutes))
            {
                _expiryMinutes = 60; // Default to 60 minutes
            }
        }

        public string GenerateToken(AuthUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Username", user.Username ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? ""),
                new Claim("FullName", user.FullName ?? ""),
                new Claim("Email", user.Email ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime GetTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(_expiryMinutes);
        }
    }
}
