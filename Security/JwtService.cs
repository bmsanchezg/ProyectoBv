using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BilleteraVirtual.API.Security
{
    public class JwtService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(IConfiguration config)
        {
            _key = config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key is missing in appsettings.json");
            _issuer = config["Jwt:Issuer"] ?? "yourdomain.com";
            _audience = config["Jwt:Audience"] ?? "yourdomain.com";

            if (_key.Length < 32) // 🔥 Evita claves menores a 256 bits
            {
                throw new Exception("🔴 ERROR: La clave JWT es demasiado corta. Debe tener al menos 32 caracteres.");
            }
        }

        public string GenerateToken(int userId, string email)
        {
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(_key);
                var securityKey = new SymmetricSecurityKey(keyBytes);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    _issuer,
                    _audience,
                    claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 ERROR al generar el token: {ex.Message}");
                throw new Exception("Error al generar el token JWT", ex);
            }
        }
    }
}
