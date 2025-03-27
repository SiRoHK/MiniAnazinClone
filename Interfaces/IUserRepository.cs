using MiniAmazonClone.Models;

namespace MiniAmazonClone.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<int> AddUserAsync(User user);
        Task UpdateUserAsync(User user);
    }


}
