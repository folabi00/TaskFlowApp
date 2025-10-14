using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.ApplicationServices
{
    public class AuthService : IAuthService
    {
        private const string ClassName = "AuthService";
        private readonly IUserRepository _userRepository;
        private readonly IHashingService _hashingService;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IRoleRepository _roleRepository;
        public AuthService(IUserRepository userRepository, IHashingService hashingService, ILogger<AuthService> logger, ITokenService tokenService, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _hashingService = hashingService;
            _logger = logger;
            _tokenService = tokenService;
            _roleRepository = roleRepository;
        }
        public Task<bool> IsValidUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task<LoginResponseDTO> ValidateUserLogin(string username, string password)
        {
            string methodName = nameof(ValidateUserLogin);
            LoginResponseDTO loginResponse = new();
            try
            {
                var validUser = await _userRepository.ValidateUser(username);
                var userRole = await _roleRepository.GetRole(validUser.RoleId!.Value);
                if (validUser != null)
                {
                    var passwordCheck = _hashingService.VerifyPassword(password, validUser.PasswordHash, validUser.PasswordSalt);
                    if (passwordCheck)
                    {
                        _logger.LogInformation($"[{ClassName}] [{methodName}] : User login credentials validated successfully ");
                        var token = await _tokenService.GenerateJwtToken(validUser, userRole);
                        loginResponse.ResponseMessage = "Success";
                        loginResponse.AccessToken = token;
                        return loginResponse;
                    }
                    _logger.LogError($"[{ClassName}] [{methodName}] : Invalid Password match");
                    loginResponse.ResponseMessage = "Invalid Login Credentials";
                    return loginResponse;
                }
                _logger.LogError($"[{ClassName}] [{methodName}] : Unable to find user {username}");
                loginResponse.ResponseMessage = "Invalid User";
                return loginResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Something went wrong validating User Details ", ex);
                return loginResponse;
            }
        }
    }
}
