using MiniAmazonClone.Data;
using MiniAmazonClone.Interfaces;
using MiniAmazonClone.Models;
using Microsoft.EntityFrameworkCore;

namespace MiniAmazonClone.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Orders) // Optional: Include related orders if needed
                    .FirstOrDefaultAsync(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID {userId}");
                throw;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with email {email}");
                throw;
            }
        }

        public async Task<int> AddUserAsync(User user)
        {
            try
            {
                // Check for existing user
                var existingUser = await GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("User with this email already exists");
                }

                // Set default role if not specified
                if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = "Customer";
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created with ID {user.UserId}");
                return user.UserId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new user");
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                // Attach the user and mark as modified
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {user.UserId} updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {user.UserId}");
                throw;
            }
        }

        // Additional methods for more complex user management
        public async Task<bool> ChangeUserRoleAsync(int userId, string newRole)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.Role = newRole;
                await UpdateUserAsync(user);

                _logger.LogInformation($"User {userId} role changed to {newRole}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing role for user {userId}");
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Soft delete or deactivation logic
                await UpdateUserAsync(user);

                _logger.LogInformation($"User {userId} deactivated");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating user {userId}");
                return false;
            }
        }
    }
}
