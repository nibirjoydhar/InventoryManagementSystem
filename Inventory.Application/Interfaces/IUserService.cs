using Inventory.Application.DTOs.Auth;
using Inventory.Domain.Entities;
using System.Threading.Tasks;


namespace Inventory.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User> CreateUserAsync(CreateUserDto dto);
    }
}
