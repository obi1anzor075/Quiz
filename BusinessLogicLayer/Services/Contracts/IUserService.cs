using DataAccessLayer.Models;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services.Contracts
{
    public interface IUserService
    {
        Task SaveUserAsync(User user);
    }
}
