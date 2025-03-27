using MiniAmazonClone.Models;

namespace MiniAmazonClone.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<int> AddOrderAsync(Order order);
        Task<Order> GetOrderWithDetailsAsync(int orderId);
    }

}
