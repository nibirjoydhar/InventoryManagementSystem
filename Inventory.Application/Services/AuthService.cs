using Inventory.Application.DTOs.Auth;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Interfaces;
using BCrypt.Net;

namespace Inventory.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        // Login method
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username);
            if (user == null)
                throw new Exception("Invalid username or password");

            // Verify password using BCrypt
            bool verified = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!verified)
                throw new Exception("Invalid username or password");

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Username = user.Username,
                Role = user.Role.ToString(), // enum to string
                Token = token,
                UserId = user.Id
            };
        }

        // Register method
        public async Task<AuthResponseDto> RegisterAsync(CreateUserDto dto)
        {
            var existing = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existing != null)
                throw new Exception("User already exists");

            // Parse role, default to User
            UserRole role = Enum.TryParse<UserRole>(dto.Role, true, out var parsed) ? parsed : UserRole.User;

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role
            };

            await _userRepository.AddAsync(user);

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Username = user.Username,
                Role = user.Role.ToString(),
                Token = token,
                UserId = user.Id
            };
        }
    }
}
