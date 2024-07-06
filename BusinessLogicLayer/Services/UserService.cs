using System.Threading.Tasks;
using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.DataContext;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly DataStoreDbContext _context;

        public UserService(DataStoreDbContext context)
        {
            _context = context;
        }

        public async Task SaveUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}