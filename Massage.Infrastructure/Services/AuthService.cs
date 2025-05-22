using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Massage.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;
using Massage.Application.Interfaces.Services;

namespace Massage.Infrastructure.Services;

public class AuthService(AppDbContext _context, IConfiguration _configuration) : IAuthService
{
    public string HashPassword(string password)
    {
        return BC.HashPassword(password);
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        return BC.Verify(password, passwordHash);
    }

    public string GenerateJwtToken(Guid userId, string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<(bool Success, string Token, string RefreshToken)> LoginAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return (false, null, null);

        if (!VerifyPassword(user.PasswordHash, password))
            return (false, null, null);

        var role = user.Role.ToString();
        var token = GenerateJwtToken(user.Id, user.Email, role);
        var refreshToken = Guid.NewGuid().ToString();

        // Store refresh token and its expiry in the database
        // Implementation omitted for brevity

        return (true, token, refreshToken);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
