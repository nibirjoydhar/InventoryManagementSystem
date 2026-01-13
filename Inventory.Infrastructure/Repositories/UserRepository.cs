using Inventory.Domain.Entities;
using Inventory.Domain.Interfaces;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // Add a new user
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        // Get user by username
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.Username == username);
        }

        // Get user by Id (required by IUserRepository)
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.Id == id);
        }

        // Optional: Get all users (for admin purposes)
        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            return await _context.Users
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}
