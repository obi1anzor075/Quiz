using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Contracts;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;

        public UserService(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task SaveUserAsync(User user)
        {
            var existingUser = await _userRepository.FindByConditionAsync(u => u.GoogleId == user.GoogleId || u.Email == user.Email);
            if (existingUser == null)
            {
                await _userRepository.AddAsync(user);
            }
        }
    }
}
