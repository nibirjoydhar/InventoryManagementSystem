using Inventory.Application.DTOs.Auth;
using System.Threading.Tasks;


namespace Inventory.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> RegisterAsync(CreateUserDto dto);
    }
}
