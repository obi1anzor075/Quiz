using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.Contracts
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetUserByGoogleIdAsync(string googleId);
        Task<User> GetUserByEmailAsync(string email);
    }
}
