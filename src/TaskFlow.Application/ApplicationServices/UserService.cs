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
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Commons;
using TaskFlow.Core.Models;
//using TaskFlow.Infrastructure.Helpers;

namespace TaskFlow.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private const string ClassName = "UserService";
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IHashingService _hashingService;
        private readonly IUserRegistrationNumberGenerator _registrationNumberGenerator;

        public UserService(ILogger<UserService> logger, IEmailService emailService,
            IConfiguration configuration, IMemoryCache cache, IUserRegistrationNumberGenerator registrationNumberGenerator, IHashingService hashingService, IUserRepository userRepository)
        {
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
            _cache = cache;
            _registrationNumberGenerator = registrationNumberGenerator;
            _hashingService = hashingService;
            _userRepository = userRepository;
        }

        public async Task<CreateUserResultDto> CreateUser(CreateUserDTO userDTO)
        {            
            string methodName = "CreateUser";
            var response = new CreateUserResultDto();
            try
            {
                var emailexist = await _userRepository.GetEmailCountAsync(userDTO.Email);
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
                _hashingService.HashPassword(userDTO.Password, out passwordHash, out passwordSalt);
                //var generator = await GenerateRegNumber();
                var generator = await _registrationNumberGenerator.GenerateRegNumberAsync();
                var user = new User()
                {
                    Id = Guid.NewGuid(),
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    RegistrationNumber = generator.ToString(),
                    EmailConfirmed = false
                };
                await _userRepository.CreateUserAsync(user);

                var token = await GenerateAndStoreTokenAsync(user, TimeSpan.FromMinutes(60));
                var emailBody = FormatEmailConfirmationBody(user.Id, user, token, _configuration["App:BaseUrl"] ?? "");
                await _emailService.SendEmailAsync(user.Email, "ACCOUNT EMAIL CONFIRMATION", emailBody);

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
                var deleteUser = await _userRepository.DeleteUserAsync(id, registrationNumber);
                if (deleteUser == false)
                {
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Delete action not successful ");
                }
                return deleteUser;
            }
            catch(Exception ex )
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An unexpected error occured '{ex.Message}' while trying to delete user");
                return false;
            }
        }

        public async Task<PaginatedResponse<UserDTO>> GetAllUsers(int pageNumber, int pageSize)
        {
            string methodName = "GetAllUsers";
            PaginatedResponse<UserDTO> usersDTO = new ();
            List<UserDTO> users = new List<UserDTO>();
            try
            {
                var returnedUsers = await _userRepository.GetAllUsersAsync(pageNumber, pageSize);

                foreach (var user in returnedUsers)
                {
                    var userDTO = new UserDTO
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        RegistrationNumber = user.RegistrationNumber,
                        HasCurrentTask = user.HasCurrentTask,
                        CurrentTask = user.CurrentTask,
                        TotalTasksAssigned = user.TotalTasksAssigned,
                        TotalTasksCompleted = user.TotalTasksCompleted,
                        TotalTasksFailed = user.TotalTasksFailed
                    };
                    //userDTO.FirstName = user.FirstName;
                    //userDTO.LastName = user.LastName;
                    //userDTO.Email = user.Email;
                    //userDTO.RegistrationNumber = user.RegistrationNumber;
                    //userDTO.HasCurrentTask = user.HasCurrentTask;
                    //userDTO.CurrentTask = user.CurrentTask;
                    //userDTO.TotalTasksAssigned = user.TotalTasksAssigned;
                    //userDTO.TotalTasksCompleted = user.TotalTasksCompleted;
                    //userDTO.TotalTasksFailed = user.TotalTasksFailed;
                    users.Add(userDTO);
                }
                usersDTO.Items = users;
                usersDTO.TotalCount = usersDTO.Items.Count;
                return usersDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An Unexpected error occured '{ex.Message}' while trying to fetch users");
                return usersDTO;
            }
        }

        public async Task<UserDTO> GetUserById(Guid userID)
        {
            string methodName = "GetUserById";
            UserDTO userDTO = new UserDTO();
            try
            {
                var returnedUser = await _userRepository.GetUserByIdAsync(userID);
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
                var returnedUser = await _userRepository.GetUserByRegistrationNumberAsync(registrationNumber);
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
                //var user = await _appDBContext.Users.FromSqlRaw("Select * from Users where RegistrationNumber = {0}", $"{userDTO.RegistrationNumber}").SingleOrDefaultAsync();
                //if (user == null)
                //{
                //    _logger.LogInformation($"[{ClassName}] [{methodName}] : Unable to find match for user with Registration Number {userDTO.RegistrationNumber} ");
                //    return new UserDTO() { FirstName = "", LastName = "", Email = "", RegistrationNumber = "" };
                //}
                //var returnedUser = await _appDBContext.Users.FindAsync(user.Id);

                //if (returnedUser != null)
                //{
                //    returnedUser.FirstName = userDTO.FirstName;
                //    returnedUser.LastName = userDTO.LastName;
                //    returnedUser.Email = userDTO.Email;

                //    await _appDBContext.SaveChangesAsync();
                //    _logger.LogInformation($"[{ClassName}] [{methodName}] : User details updated for user with Registration Number {userDTO.RegistrationNumber} ");

                //}
                await _userRepository.UpdateUserAsync(userDTO);
                return userDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An Unexpected error occured '{ex.Message}' occured while updating user");
                return new UserDTO();
            }

        }

        public async Task<bool> ConfirmEmail(Guid userId, string token)
        {
            string methodName = "ConfirmEmailAsync";
            try
            {
                var hashToken = _hashingService.HashToken(token);
                var result = await _userRepository.ConfirmEmailAsync(userId, hashToken);
               
                return result;
            }
            catch( Exception ex )
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An error '{ex.Message}' occured while updating email confirmation status ");
                return false;
            }           
            
        }

        public async Task<bool> ResendConfirmation(Guid userId)
        {
            string methodName = "ResendConfirmationEmailAsync";
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null && !user.EmailConfirmed)
                {
                    if(!user.EmailConfirmed)
                    {
                        var token = await GenerateAndStoreTokenAsync(user, TimeSpan.FromMinutes(60));
                        var emailBody = FormatEmailConfirmationBody(user.Id, user, token, _configuration["App:BaseUrl"] ?? "");
                        var status = await _emailService.SendEmailAsync(user.Email, "ACCOUNT EMAIL CONFIRMATION", emailBody);
                        if (string.IsNullOrEmpty(status)) { return false; }
                        return true;
                    }
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : User email Status is {user.EmailConfirmed} ");
                    return false;

                }
                _logger.LogInformation($"[{ClassName}] [{methodName}] : Could not find match for user with id {userId} ");
                return false;
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
                var token = _hashingService.GenerateConfirmationToken();
                var hashedToken = _hashingService.HashToken(token);

                UserConfirmationToken t = new UserConfirmationToken()
                {
                    UserId = user.Id,
                    TokenHash = hashedToken,
                    ExpiresAt = DateTimeOffset.UtcNow.Add(expiry.Value),
                    IsUsed = false

                };
                await _userRepository.StoreUserTokenAsync(t);
                var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(55));
                _cache.Set($"token:{user.Id}", token, options);

                return token;
            }

            return tokenCache;
        }
        private static string FormatEmailConfirmationBody(Guid userId, User user, string token, string baseUrl)
        {
            var link = $"{baseUrl}/api/User/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}";

            var body = $@"
            <p>Hi, {user.LastName.ToUpper()}, {user.FirstName}</p>
            <p>Click <a href='{link}'>this link</a> to confirm your email address. This link will expire in 1 hour </p><br> </br>
            <p>If you did not create an account, ignore this email.</p>";
            return body;
        }
    }
}
