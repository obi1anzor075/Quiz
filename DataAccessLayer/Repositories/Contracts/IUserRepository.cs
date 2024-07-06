using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories.Contracts
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByGoogleIdAsync(string googleId);

    }
}
