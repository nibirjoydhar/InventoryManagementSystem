using Inventory.Application.DTOs.Auth;
using Inventory.Application.Services;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Authentication;
using Moq;
using Xunit;
using BCrypt.Net;

namespace Inventory.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly JwtService _jwtService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtService = new JwtService("TestKey1234567890", "InventoryApi");
            _authService = new AuthService(_userRepoMock.Object, _jwtService);
        }

        [Fact]
        public async Task LoginAsync_ReturnsToken_ForValidUser()
        {
            // Arrange
            var password = "admin";
            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.Admin
            };

            _userRepoMock.Setup(x => x.GetByUsernameAsync("admin")).ReturnsAsync(user);

            var dto = new LoginRequestDto { Username = "admin", Password = password };

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("admin", result!.Username);
            Assert.NotEmpty(result.Token);
        }

        [Fact]
        public async Task LoginAsync_ThrowsException_ForInvalidPassword()
        {
            // Arrange
            var password = "admin";
            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.Admin
            };

            _userRepoMock.Setup(x => x.GetByUsernameAsync("admin")).ReturnsAsync(user);

            var dto = new LoginRequestDto { Username = "admin", Password = "wrong" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(dto));
        }

    }
}
