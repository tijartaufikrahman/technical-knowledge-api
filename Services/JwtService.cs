using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RestAPISystemKnowledge.Interfaces;
using RestAPISystemKnowledge.Models.Entities;

namespace RestAPISystemKnowledge.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public static byte[] GetSigningKeyBytes(string? key)
        {
            var rawKey = string.IsNullOrWhiteSpace(key) ? "KunciRahasia" : key;
            var rawBytes = Encoding.UTF8.GetBytes(rawKey);
            // Pastikan panjang kunci minimal 256 bits (32 bytes) untuk algoritma HMAC-SHA256
            if (rawBytes.Length < 32)
            {
                return SHA256.HashData(rawBytes);
            }
            return rawBytes;
        }

        public string GenerateToken(User user, string roleName)
        {
            var jwtConfig = _config.GetSection("Jwt");
            var keyStr = jwtConfig["Key"] ?? "KunciRahasia";
            var issuer = jwtConfig["Issuer"] ?? "KunciRahasia";
            var audience = jwtConfig["Audience"] ?? "KunciRahasiaApp";

            var keyBytes = GetSigningKeyBytes(keyStr);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? user.NamaLengkap ?? string.Empty),
                new Claim("namaLengkap", user.Name ?? user.NamaLengkap ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, roleName ?? "User"),
                new Claim("role", roleName ?? "User"),
                new Claim("roleId", (user.RoleId ?? 0).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24), // Waktu berlaku token 1 hari / 24 jam
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
