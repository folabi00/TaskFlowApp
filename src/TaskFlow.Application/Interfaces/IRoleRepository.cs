using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<string> GetRole(int roleId);
    }
}
