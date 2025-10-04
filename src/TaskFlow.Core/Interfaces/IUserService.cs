using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.DTOs;
using TaskFlow.Core.Models;

namespace TaskFlow.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetAllUsers();
        Task<UserDTO> GetUserById(Guid userID);
        Task<UserDTO> GetUserByRegistrationNumber(long registrationNumber);
        Task<CreateUserResultDto> CreateUserAsync(CreateUserDTO userDTO);
        Task<UserDTO> UpdateUser(UserDTO userDTO);
        Task<bool> DeleteUser(Guid id, string registrationNumber);
        Task<bool> DeleteUserByRegistrationNumber(long registrationNumber);
        //Task<UserDTO> UpdateUserByRegistrationNumber(User user, long registrationNumber);
        Task<UserDTO> ConfirmUserEmail(string email);
        Task<bool> ConfirmEmailAsync(Guid userId, string token);
        Task<bool> ResendConfirmationAsync(Guid userId);

    }
}
