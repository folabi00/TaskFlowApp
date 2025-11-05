using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.ApplicationServices
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            var secret = _config["JWT_SECRET_KEY"] ?? throw new InvalidOperationException("JWT_SECRET_KEY not configured.");
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }
        public async Task<string> GenerateJwtToken(User user, string role)
        {
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var encryption = new SymmetricSignatureProvider(_key, SecurityAlgorithms.Aes256Encryption);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.FirstName+user.LastName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.AuthenticationMethod, "role"),
                new(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: "MyApp",
                audience: "MyAppUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
