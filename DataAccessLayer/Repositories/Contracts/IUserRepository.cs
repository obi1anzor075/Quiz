using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Contracts;

public interface IUserRepository
{
    Task<User> GetUserByIdAsync(int id);
    Task<User> GetUserByNameAsync(string userName);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    // Другие методы, если необходимо
}