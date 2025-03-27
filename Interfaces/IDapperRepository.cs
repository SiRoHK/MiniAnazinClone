using MiniAmazonClone.Models;

namespace MiniAmazonClone.Interfaces
{
    public interface IDapperRepository
    {
        Task<IEnumerable<Order>> GetCustomerOrdersAsync(int userId);
        Task<Product> GetProductByIdAsync(int productId);
    }
}
