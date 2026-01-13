using Inventory.Application.DTOs.Auth;

namespace Inventory.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
        Task RegisterAsync(string username, string password, string role);
    }
}
