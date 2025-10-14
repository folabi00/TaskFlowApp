using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Persistence.Data;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDBContext _appDBContext;
        public RoleRepository(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }
        public async Task<string> GetRole(int roleId)
        {
            var userRole = await _appDBContext.Roles.FindAsync(roleId);
            return userRole.RoleName;
        }
    }
}
