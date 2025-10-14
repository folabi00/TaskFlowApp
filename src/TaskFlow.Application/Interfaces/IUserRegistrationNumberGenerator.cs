using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserRegistrationNumberGenerator
    {
        Task<long> GenerateRegNumberAsync();
    }
}
