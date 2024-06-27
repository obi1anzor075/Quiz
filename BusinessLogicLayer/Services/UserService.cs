using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.DataContext;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.GoogleId == user.GoogleId);

            if (existingUser == null)
            {
                _context.Users.Add(user);
            }
            else
            {
                existingUser.Email = user.Email;
                existingUser.Name = user.Name;
                existingUser.CreatedAt = user.CreatedAt; // Возможно, не нужно обновлять CreatedAt
            }

            await _context.SaveChangesAsync();
        }
    }
}
