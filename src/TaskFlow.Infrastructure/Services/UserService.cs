using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
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
        private const string ClassName = "UserService";
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

        public async Task<CreateUserResultDto> CreateUser(CreateUserDTO userDTO)
        {            
            string methodName = "CreateUser";
            var response = new CreateUserResultDto();
            try
            {
                var emailexist = await GetEmailCountAsync(userDTO.Email);
                if (emailexist >= 1)
                {
                    if(emailexist != -1)
                    {
                        _logger.LogInformation($"[{ClassName}] [{methodName}] : Email {userDTO.Email} exists in User record ");
                        response.Email = userDTO.Email;
                        response.RegistrationNumber = "01";
                        return response;
                    }
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Unable to validate if email exist, please try again ");
                    response.Email = userDTO.Email;
                    response.RegistrationNumber = "02";
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

                _logger.LogInformation($"[{ClassName}] [{methodName}] : User {user.RegistrationNumber} created  at {DateTimeOffset.UtcNow} ");

                var token = await GenerateAndStoreTokenAsync(user, TimeSpan.FromMinutes(60));
                var emailBody = MessageFormatter.FormatEmailConfirmationBody(user.Id, user, token, _configuration["App:BaseUrl"] ?? "");
                await _emailService.SendEmailAsync(user.Email, "ACCOUNT EMAIL CONFIRMATION", emailBody); //handle email sending failure

                response.Email = user.Email;
                response.FirstName = user.FirstName;
                response.LastName = user.LastName;
                response.RegistrationNumber = user.RegistrationNumber;
                response.CurrentTask = user.CurrentTask;
                response.EmailConfirmed = user.EmailConfirmed;
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An unexpected error occured '{ex.Message}' while creating user ");
                return response;
            }
        }
        public async Task<bool> DeleteUser(Guid id, string registrationNumber)
        {
            string methodName = "DeleteUser";
            try
            {
                
                var returnedUser = await _appDBContext.Users.FindAsync(id);
                if (returnedUser?.RegistrationNumber == registrationNumber)
                {
                    returnedUser.IsDeleted = true;
                    //_appDBContext.Users.Remove(returnedUser);
                    _appDBContext.SaveChanges();

                    _logger.LogInformation($"[{ClassName}] [{methodName}] : User details deleted for user {registrationNumber}");
                    return true;
                }
                _logger.LogInformation($"[{ClassName}] [{methodName}] : Delete action not succefful ");

                return false;
            }
            catch(Exception ex )
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An unexpected error occured '{ex.Message}' while trying to delete user");
                return false;
            }
        }

        public async Task<List<UserDTO>> GetAllUsers()
        {
            string methodName = "GetAllUsers";
            UserDTO userDTO = new UserDTO();
            List<UserDTO> users = new List<UserDTO>();
            try
            {
                var returnedUsers = await _appDBContext.Users.ToListAsync();
                _logger.LogInformation($"[{ClassName}] [{methodName}] : {returnedUsers.Count} number of users found");

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
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An Unexpected error occured '{ex.Message}' while trying to fetch users");
                return users;
            }
        }

        public async Task<UserDTO> GetUserById(Guid userID)
        {
            string methodName = "GetUserById";
            UserDTO userDTO = new UserDTO();
            try
            {
                var returnedUser = await _appDBContext.Users.FindAsync(userID);
                _logger.LogInformation($"[{ClassName}] [{methodName}] : User '{returnedUser.FirstName},{returnedUser.LastName}' found ");
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
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An Unexpected error '{ex.Message}' occured while fetching user {userID.ToString()}");
                return userDTO;
            }
        }

        public async Task<UserDTO> GetUserByRegistrationNumber(string registrationNumber)
        {
            string methodName = "GetUserByRegistrtion";
            UserDTO userDTO = new UserDTO();
            try
            {
                var regex = new Regex(@"^\d+$");
                if (!regex.IsMatch(registrationNumber))
                {
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Invalid character found in Registration Number {registrationNumber} ");
                    return userDTO;
                }
                var returnedUser = await _appDBContext.Users.FindAsync(registrationNumber);
                _logger.LogInformation($"[{ClassName}] [{methodName}] : User '{returnedUser.FirstName},{returnedUser.LastName}' found ");
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
            catch(Exception ex) 
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An Unexpected error occured '{ex.Message}' occured while fetching user '{registrationNumber}'");
                return userDTO;
            }
        }

        public async Task<UserDTO> UpdateUser(UserDTO userDTO)
        {
            var methodName = "UpdateUser";
            try
            {
                var user = await _appDBContext.Users.FromSqlRaw("Select * from Users where RegistrationNumber = {0}", $"{userDTO.RegistrationNumber}").SingleOrDefaultAsync();
                if (user == null)
                {
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Unable to find match for user with Registration Number {userDTO.RegistrationNumber} ");
                    return new UserDTO() { FirstName = "", LastName = "", Email = "", RegistrationNumber = "" };
                }
                var returnedUser = await _appDBContext.Users.FindAsync(user.Id);

                if (returnedUser != null)
                {
                    returnedUser.FirstName = userDTO.FirstName;
                    returnedUser.LastName = userDTO.LastName;
                    returnedUser.Email = userDTO.Email;

                    await _appDBContext.SaveChangesAsync();
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : User details updated for user with Registration Number {userDTO.RegistrationNumber} ");

                }
                return userDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An Unexpected error occured '{ex.Message}' occured while updating user");
                return new UserDTO();
            }

        }

        public async Task<int> GetEmailCountAsync(string email)
        {
            string methodName = "GetEmailCountAsync";
            try
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
            catch(Exception ex) when (ex is DbException || ex is InvalidOperationException)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An error '{ex.Message}' occured while validating email exist");
                return -1;                
            }
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            string methodName = "ConfirmEmailAsync";
            try
            {
                var hashToken = HashingUtility.HashToken(token);
                var tokenEntity = await _appDBContext.UserConfirmationTokens.Where(t => t.UserId == userId && t.TokenHash == hashToken && !t.IsUsed)
                    .OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync();
                if (tokenEntity == null)
                {
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Confirmation token for {tokenEntity.UserId} not found");
                    return false;
                }
                if (tokenEntity.ExpiresAt < DateTimeOffset.UtcNow) { return false; }

                var user = await _appDBContext.Users.FindAsync(userId);
                if (user == null) { return false; }

                tokenEntity.IsUsed = true;
                user.EmailConfirmed = true;

                await _appDBContext.SaveChangesAsync();
                _logger.LogInformation($"[{ClassName}] [{methodName}] : Email successfully validated for user {user.Email} at {DateTimeOffset.UtcNow} ");
                return true;
            }
            catch( Exception ex )
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An error '{ex.Message}' occured while updating email confirmation status ");
                return false;
            }           
            
        }

        public async Task<bool> ResendConfirmationAsync(Guid userId)
        {
            string methodName = "ResendConfirmationEmailAsync";
            try
            {
                var user = await _appDBContext.Users.FindAsync(userId);
                if (user == null || user.EmailConfirmed)
                {
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Could not find match for user with id {userId} || User email Status is {user.EmailConfirmed} ");
                    return false;
                }
                ;
                var token = await GenerateAndStoreTokenAsync(user, TimeSpan.FromMinutes(60));
                var emailBody = MessageFormatter.FormatEmailConfirmationBody(user.Id, user, token, _configuration["App:BaseUrl"] ?? "");
                var status = await _emailService.SendEmailAsync(user.Email, "ACCOUNT EMAIL CONFIRMATION", emailBody); //handle email sending failure
                if (string.IsNullOrEmpty(status)) { return false; }

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An error '{ex.Message}' occured while re-sending confirmation email ");
                return false;
            }
            
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
    }
}
