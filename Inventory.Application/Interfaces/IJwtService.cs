using Inventory.Domain.Entities;

namespace Inventory.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
