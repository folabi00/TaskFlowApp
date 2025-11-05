using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Core.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<int> GetEmailCountAsync(string email);
        Task CreateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id, string registrationNumber);
        Task<List<User>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<User> GetUserByIdAsync(Guid userID);
        Task<User> GetUserByRegistrationNumberAsync(string registrationNumber);
        Task<UpdateUserResultDTO> UpdateUserAsync(UpdateUserDTO userDTO);
        Task<bool> ConfirmEmailAsync(Guid userId, string token);
        Task StoreUserTokenAsync(UserConfirmationToken token);
        Task<User> ValidateUser(string email);
        Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken);
    }
}
