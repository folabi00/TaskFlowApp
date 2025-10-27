using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Persistence.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private const string ClassName = nameof(UserRepository);
        private readonly AppDBContext _appDBContext;
        private readonly ILogger<UserRepository> _logger;
        public UserRepository(AppDBContext appDBContext, ILogger<UserRepository> logger)
        {
            _appDBContext = appDBContext;
            _logger = logger;
        }

        public async Task CreateUserAsync(User user)
        {
            string methodName = nameof(CreateUserAsync);
            await _appDBContext.Users.AddAsync(user);
            await _appDBContext.SaveChangesAsync();

            _logger.LogInformation($"[{ClassName}] [{methodName}] : User {user.RegistrationNumber} created  at {DateTimeOffset.UtcNow} ");
            return ;
        }
        public async Task<List<User>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            string methodName = nameof(GetAllUsersAsync);
            var startIndex = (pageNumber - 1) * pageSize;
            var returnedUsers = await _appDBContext.Users.OrderBy(u => u.Id).Skip(startIndex).Take(pageSize).ToListAsync();
            _logger.LogInformation($"[{ClassName}] [{methodName}] : {returnedUsers.Count} number of users found");

            return returnedUsers;
        }
        public async Task<bool> DeleteUserAsync(Guid id, string registrationNumber)
        {
            string methodName = nameof(DeleteUserAsync);
            var returnedUser = await _appDBContext.Users.FindAsync(id);
            if (returnedUser?.RegistrationNumber == registrationNumber)
            {
                returnedUser.IsDeleted = true;
                //_appDBContext.Users.Remove(returnedUser);
                _appDBContext.SaveChanges();

                _logger.LogInformation($"[{ClassName}] [{methodName}] : User details deleted for user {registrationNumber}");
                return true;
            }
            _logger.LogError($"[{ClassName}] [{methodName}] : Delete action for user {registrationNumber} not successful");

            return false;
        }
               
        public async Task<int> GetEmailCountAsync(string email)
        {
            string methodName = nameof(GetEmailCountAsync);
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
            catch (Exception ex) when (ex is DbException || ex is InvalidOperationException)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An error '{ex.Message}' occured while validating email exist");
                return -1;
            }
        }

        public async Task<User> GetUserByIdAsync(Guid userID)
        {
            string methodName = nameof (GetUserByIdAsync);

            var returnedUser = await _appDBContext.Users.FindAsync(userID);
            _logger.LogInformation($"[{ClassName}] [{methodName}] : User '{returnedUser.FirstName},{returnedUser.LastName}' found ");
            return returnedUser;
        }

        public async Task<User> GetUserByRegistrationNumberAsync(string registrationNumber)
        {
            string methodName = nameof (GetUserByRegistrationNumberAsync);
            var returnedUser = await _appDBContext.Users.FindAsync(registrationNumber);
            _logger.LogInformation($"[{ClassName}] [{methodName}] : User '{returnedUser.FirstName},{returnedUser.LastName}' found ");
            return returnedUser;
            throw new NotImplementedException();
        }

        public async Task UpdateUserAsync(UserDTO userDTO)
        {
            string methodName = nameof(UpdateUserAsync);
            var user = await _appDBContext.Users.FirstOrDefaultAsync(u => u.RegistrationNumber == $"{userDTO.RegistrationNumber}");
            if (user != null)
            {
                var returnedUser = user;
                if (returnedUser != null)
                {
                    returnedUser.FirstName = userDTO.FirstName;
                    returnedUser.LastName = userDTO.LastName;
                    returnedUser.Email = userDTO.Email;

                    await _appDBContext.SaveChangesAsync();
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : User details updated for user with Registration Number {userDTO.RegistrationNumber} ");

                }                
            }
            _logger.LogError($"[{ClassName}] [{methodName}] : Unable to find match for user with Registration Number {userDTO.RegistrationNumber} ");
            return ;
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string hashToken)
        {
            string methodName = nameof(ConfirmEmailAsync);
            var tokenEntity = await _appDBContext.UserConfirmationTokens.Where(t => t.UserId == userId && t.TokenHash == hashToken && !t.IsUsed)
                    .OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync();
            if (tokenEntity != null)
            {
                if (tokenEntity.ExpiresAt < DateTimeOffset.UtcNow) { return false; }

                var user = await _appDBContext.Users.FindAsync(userId);
                if (user != null)
                {
                    tokenEntity.IsUsed = true;
                    user.EmailConfirmed = true;

                    await _appDBContext.SaveChangesAsync();
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Email successfully validated for user {user.Email} at {DateTimeOffset.UtcNow} ");
                    return true;
                }
                _logger.LogError($"[{ClassName}] [{methodName}] : Unable to find match for user {userId} ");
                return false;                
            }
            _logger.LogError($"[{ClassName}] [{methodName}] : Confirmation token for {tokenEntity.UserId} not found");
            return false;
        }

        public async Task StoreUserTokenAsync(UserConfirmationToken token)
        {
            await _appDBContext.UserConfirmationTokens.AddAsync(token);
            await _appDBContext.SaveChangesAsync();

            return;
        }

        public async Task<User> ValidateUser(string email)
        {
            string methodName = "ValidateUser";
            var user = await _appDBContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                _logger.LogInformation($"[{ClassName}] [{methodName}] : User not found");
            }
            return user;
            
        }
    }
}
