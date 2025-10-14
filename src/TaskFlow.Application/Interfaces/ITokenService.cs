using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.Interfaces
{
    public interface ITokenService
    {
        Task <string> GenerateJwtToken( User User, string role);
    }
}
