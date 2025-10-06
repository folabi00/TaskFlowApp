using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Data;

namespace TaskFlow.Infrastructure.Helpers
{
    public class UserRegistrationNumberGenerator
    {
        private readonly AppDBContext _appDBContext;
        public UserRegistrationNumberGenerator(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }
        public async Task<long> GenerateRegNumber()
        {
            long number;
            do
            {
                number = Generate();
            } while (await _appDBContext.Users.AnyAsync(u => u.RegistrationNumber == number.ToString()));

            return number;
        }
        public long Generate()
        {
            int firstDigit = RandomNumberGenerator.GetInt32(1, 10);
            byte[] buffer = new byte[8];
            RandomNumberGenerator.Fill(buffer);
            ulong randomPart = BitConverter.ToUInt64(buffer, 0) % 10000000000UL;
            string fullNumber = firstDigit.ToString() + randomPart.ToString("D10");
            return long.Parse(fullNumber);
        }
    }
}
