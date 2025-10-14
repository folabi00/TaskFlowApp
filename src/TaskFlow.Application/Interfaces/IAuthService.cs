using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> IsValidUser { get;  set; }
        Task<LoginResponseDTO> ValidateUserLogin(string username, string password);
    }
}
