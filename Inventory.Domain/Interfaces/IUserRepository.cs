using Inventory.Domain.Entities;

namespace Inventory.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);

    }
}
