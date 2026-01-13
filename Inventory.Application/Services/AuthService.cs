using Inventory.Application.DTOs.Auth;
using Inventory.Application.Interfaces;
using Inventory.Domain.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

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

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username);
            if (user == null)
                return null;

            // ✅ Hash the incoming password before comparing
            var hashedPassword = HashPassword(dto.Password);

            if (user.PasswordHash != hashedPassword)
                return null;

            var token = _jwtService.GenerateToken(user);

            return new LoginResponseDto
            {
                Username = user.Username,
                Role = user.Role.ToString(),
                Token = token
            };
        }

        public async Task RegisterAsync(string username, string password, string role)
        {
            var existing = await _userRepository.GetByUsernameAsync(username);
            if (existing != null)
                throw new Exception("User already exists");

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password), // ✅ Always hash passwords
                Role = Enum.TryParse<UserRole>(role, true, out var parsed) ? parsed : UserRole.User
            };

            await _userRepository.AddAsync(user);
        }

        // =========================
        // Helper: SHA256 + Base64
        // =========================
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
