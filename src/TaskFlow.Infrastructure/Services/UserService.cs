using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskFlow.Core.Data;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Helpers;

namespace TaskFlow.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDBContext _appDBContext;
        private readonly ILogger<UserService> _logger;
        private const string MethodName = "UserService";
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public UserService(AppDBContext appDBContext, ILogger<UserService> logger, IEmailService emailService, 
            IConfiguration configuration, IMemoryCache cache)
        {
            _appDBContext = appDBContext;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
            _cache = cache;
        }
        public Task<UserDTO> ConfirmUserEmail(string email)
        {
            
            throw new NotImplementedException();
        }

        public async Task<CreateUserResultDto> CreateUserAsync(CreateUserDTO userDTO)
        {            
            var Classname = "CreateUser";
            var response = new CreateUserResultDto();
            var emailexist = await GetEmailCountAsync(userDTO.Email);
            if (emailexist >= 1)
            {
                _logger.LogInformation($"{MethodName} , {Classname} email {userDTO.Email} exists in User record ");
                response.Email = userDTO.Email;
                response.RegistrationNumber = "01";
                return response;
            }
            byte[] passwordHash, passwordSalt;
            HashingUtility.HashPassword(userDTO.Password, out passwordHash, out passwordSalt);
            //var generator = await GenerateRegNumber();
            var generator = new UserRegistrationNumberGenerator(_appDBContext).GenerateRegNumber();
            var user = new User() 
            { 
                Id = Guid.NewGuid(),
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName, 
                Email = userDTO.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                RegistrationNumber = generator.Result.ToString(),
                EmailConfirmed = false
            };
            await _appDBContext.Users.AddAsync(user); 
            await _appDBContext.SaveChangesAsync();

            _logger.LogInformation($"{MethodName} , {Classname} user {user.RegistrationNumber} created  at {DateTimeOffset.UtcNow} ");

            //var token = await GenerateAndStoreTokenAsync(user, TimeSpan.FromMinutes(60));
            //var emailBody = MessageFormatter.FormatEmailConfirmationBody(user.Id, user, token, _configuration["App:BaseUrl"] ?? "");
            //await _emailService.SendEmailAsync(user.Email, "ACCOUNT EMAIL CONFIRMATION", emailBody); //handle email sending failure

            response.Email = user.Email;
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.RegistrationNumber = user.RegistrationNumber;
            response.CurrentTask = user.CurrentTask;
            response.EmailConfirmed = user.EmailConfirmed;
            return response;
        }

        public async Task<bool> DeleteUser(Guid id, string registrationNumber)
        {
            var ClassName = "DeleteUser";
            var returnedUser = await _appDBContext.Users.FindAsync(id);
            if (returnedUser?.RegistrationNumber == registrationNumber)
            {
                _appDBContext.Users.Remove(returnedUser);
                _appDBContext.SaveChanges();

                _logger.LogInformation($"{MethodName} , {ClassName} User details deleted for user {registrationNumber} ");
                return true;
            }
            _logger.LogInformation($"{MethodName} , {ClassName} Unable to match User Id and Registration Number  ");

            return false;
        }

        public Task<bool> DeleteUserByRegistrationNumber(long registrationNumber)
        {
           //implement soft delete later
            throw new NotImplementedException();
        }

        public async Task<List<UserDTO>> GetAllUsers()
        {
            UserDTO userDTO = new UserDTO();
            List<UserDTO> users = new List<UserDTO>();
            var returnedUsers = await _appDBContext.Users.ToListAsync();
            foreach (var user in returnedUsers)
            {
                userDTO.FirstName = user.FirstName;
                userDTO.LastName = user.LastName;
                userDTO.Email = user.Email;
                userDTO.RegistrationNumber = user.RegistrationNumber;
                userDTO.HasCurrentTask = user.HasCurrentTask;
                userDTO.CurrentTask = user.CurrentTask;
                userDTO.TotalTasksAssigned = user.TotalTasksAssigned;
                userDTO.TotalTasksCompleted = user.TotalTasksCompleted;
                userDTO.TotalTasksFailed = user.TotalTasksFailed;
                users.Add(userDTO);
            }
            return users.ToList();
        }

        public async Task<UserDTO> GetUserById(Guid userID)
        {
            UserDTO userDTO = new UserDTO();
            var returnedUser = await _appDBContext.Users.FindAsync(userID);
            if (returnedUser != null)
            {
                userDTO.FirstName = returnedUser.FirstName;
                userDTO.LastName = returnedUser.LastName;
                userDTO.Email = returnedUser.Email;
                userDTO.RegistrationNumber = returnedUser.RegistrationNumber;
                userDTO.HasCurrentTask = returnedUser.HasCurrentTask;
                userDTO.CurrentTask = returnedUser.CurrentTask;
                userDTO.TotalTasksAssigned = returnedUser.TotalTasksAssigned;
                userDTO.TotalTasksCompleted = returnedUser.TotalTasksCompleted;
                userDTO.TotalTasksFailed = returnedUser.TotalTasksFailed;
            }
            return userDTO;
        }

        public async Task<UserDTO> GetUserByRegistrationNumber(long registrationNumber)
        {
            UserDTO userDTO = new UserDTO();
            var regex = new Regex(@"^\d+$");
            if (!regex.IsMatch(registrationNumber.ToString()))
            {
                return userDTO;
            }
            var returnedUser = await _appDBContext.Users.FindAsync(registrationNumber);
            if (returnedUser != null)
            {
                userDTO.FirstName = returnedUser.FirstName;
                userDTO.LastName = returnedUser.LastName;
                userDTO.Email = returnedUser.Email;
                userDTO.RegistrationNumber = returnedUser.RegistrationNumber;
                userDTO.HasCurrentTask = returnedUser.HasCurrentTask;
                userDTO.CurrentTask = returnedUser.CurrentTask;
                userDTO.TotalTasksAssigned = returnedUser.TotalTasksAssigned;
                userDTO.TotalTasksCompleted = returnedUser.TotalTasksCompleted;
                userDTO.TotalTasksFailed = returnedUser.TotalTasksFailed;
            }
            return userDTO;
        }

        public async Task<UserDTO> UpdateUser(UserDTO userDTO)
        {
            var className = "Update User";
            var user = await _appDBContext.Users.FromSqlRaw("Select * from Users where RegistrationNumber = {0}", $"{userDTO.RegistrationNumber}").SingleOrDefaultAsync();
            if (user == null)
            {
                _logger.LogInformation($"{MethodName} , {className} Unable to find match for user with Registration Number {userDTO.RegistrationNumber} ");
                return new UserDTO() { FirstName = "", LastName = "", Email = "", RegistrationNumber = "" };
            }
            var returnedUser = await _appDBContext.Users.FindAsync(user.Id);     
            
            if (returnedUser != null)
            {
                returnedUser.FirstName = userDTO.FirstName;
                returnedUser.LastName = userDTO.LastName;
                returnedUser.Email = userDTO.Email;

                await _appDBContext.SaveChangesAsync();
                _logger.LogInformation($"{MethodName} , {className} User details updated for user with Registration Number {userDTO.RegistrationNumber} ");

            }
            return userDTO;
            
        }

        //private int CheckIfEmailExist (string email)
        //{
        //    var emailParameter = new SqlParameter("@emailAddress", email);
        //    var countParameter = new SqlParameter() { ParameterName = "@count" , SqlDbType = System.Data.SqlDbType.Int, Direction = System.Data.ParameterDirection.Output};

        //    var emailExist = _appDBContext.Database.ExecuteSqlRawAsync("exec [dbo].[spCheckIfEmailExist] @emailAddress, @count OUTPUT", emailParameter, countParameter);

        //    int count = (int)countParameter.Value;
        //    return count;

        //}
        public async Task<int> GetEmailCountAsync(string email)
        {
            var connectionString = _appDBContext.Database.GetConnectionString();
            await using var connection = new SqlConnection(connectionString);
            await using var command = connection.CreateCommand();

            command.CommandText = "exec [dbo].[spCheckIfEmailExist] @emailAddress";
            command.Parameters.Add(new SqlParameter("@emailAddress", email));

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);

        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            var hashToken = HashingUtility.HashToken(token);
            var tokenEntity = await _appDBContext.UserConfirmationTokens.Where(t => t.UserId == userId && t.TokenHash == hashToken && !t.IsUsed )
                .OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync();
            if (tokenEntity == null)
            {
                return false;
            }
            if (tokenEntity.ExpiresAt < DateTimeOffset.UtcNow) { return false; }

            var user = await _appDBContext.Users.FindAsync(userId);
            if (user == null) { return false; }

            tokenEntity.IsUsed = true;
            user.EmailConfirmed = true;

            await _appDBContext.SaveChangesAsync();
            _logger.LogInformation($"Email successfully validated for user {user.Email} at {DateTimeOffset.UtcNow} ");
            return true;
            
        }

        public async Task<bool> ResendConfirmationAsync(Guid userId)
        {
            var user = await _appDBContext.Users.FindAsync(userId);
            if (user == null || user.EmailConfirmed) { return false; };
            var token = await GenerateAndStoreTokenAsync(user, TimeSpan.FromMinutes(60));
            var emailBody = MessageFormatter.FormatEmailConfirmationBody(user.Id, user, token, _configuration["App:BaseUrl"] ?? "");
            var status = await _emailService.SendEmailAsync(user.Email, "ACCOUNT EMAIL CONFIRMATION", emailBody); //handle email sending failure
            if (string.IsNullOrEmpty(status)) { return false; }

            return true;
        }
        private async Task<string> GenerateAndStoreTokenAsync (User user, TimeSpan? expiry = null)
        {
            string tokenCache = string.Empty;
            expiry ??= TimeSpan.FromMinutes(60);
            if(!_cache.TryGetValue($"token:{user.Id}", out tokenCache))
            {
                if (expiry.Value <= TimeSpan.Zero)
                    throw new ArgumentException("Expiry must be greater than zero.", nameof(expiry));
                var token = HashingUtility.GenerateConfirmationToken();
                var hashedToken = HashingUtility.HashToken(token);

                UserConfirmationToken t = new UserConfirmationToken()
                {
                    UserId = user.Id,
                    TokenHash = hashedToken,
                    ExpiresAt = DateTimeOffset.UtcNow.Add(expiry.Value),
                    IsUsed = false

                };
                _appDBContext.UserConfirmationTokens.Add(t);
                await _appDBContext.SaveChangesAsync();
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(55));
                _cache.Set($"token:{user.Id}", token, options);

                return token;
            }

            return tokenCache;
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
