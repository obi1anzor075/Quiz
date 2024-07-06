using System.Threading.Tasks;
using DataAccessLayer.Models;

namespace BusinessLogicLayer.Services.Contracts
{
    public interface IUserService
    {
        Task SaveUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(int id);
    }
}