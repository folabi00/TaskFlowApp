using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Core.Commons;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserService
    {
        Task<PaginatedResponse<UserDTO>> GetAllUsers(int pageNumber, int pageSize);
        Task<UserDTO> GetUserById(Guid userID);
        Task<UserDTO> GetUserByRegistrationNumber(string registrationNumber);
        Task<CreateUserResultDto> CreateUser(CreateUserDTO userDTO);
        Task<UpdateUserResultDTO> UpdateUser(UpdateUserDTO userDTO);
        Task<bool> DeleteUser(Guid id, string registrationNumber);
        Task<bool> ConfirmEmail(Guid userId, string token);
        Task<bool> ResendConfirmation(Guid userId);

    }
}
