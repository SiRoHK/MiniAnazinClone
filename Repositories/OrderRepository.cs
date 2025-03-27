using Assiment5_MiniAmazonClone.Data;
using Assiment5_MiniAmazonClone.Interfaces;
using Assiment5_MiniAmazonClone.Models;
using Microsoft.EntityFrameworkCore;

namespace MiniAmazonClone.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        


        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        // Eager loading query to fetch orders with their products
        public async Task<Order> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<int> AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order.OrderId;
        }


    }
 
}
