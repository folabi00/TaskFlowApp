//using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Helpers
{
    public class HashingUtility : IHashingService
    {
        public void HashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));       

            }
            return;
        }
        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using(var hmac = new HMACSHA512(storedSalt))
            {
                var passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return passwordHash.SequenceEqual(storedHash);

            }
        }

        public string GenerateConfirmationToken()
        {
            int bytes = 32;
            var b = new byte[bytes];
            RandomNumberGenerator.Fill(b);
            string base64String = Convert.ToBase64String(b);
            return base64String.Replace('+', '-').Replace('/','_').TrimEnd('=');
        }

        public string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);  

            return Convert.ToHexString(hash);
        }
    }
}
