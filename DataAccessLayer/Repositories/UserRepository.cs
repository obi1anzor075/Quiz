using DataAccessLayer.DataContext;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(DataStoreDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User> GetUserByGoogleIdAsync(string googleId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
