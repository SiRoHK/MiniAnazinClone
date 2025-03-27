using MiniAmazonClone.Interfaces;
using MiniAmazonClone.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MiniAmazonClone.Repositories
{
    public class DapperRepository : IDapperRepository
    {
        private readonly IDbConnection _connection;

        public DapperRepository(IConfiguration configuration)
        {
            _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        // Dapper query to fetch a customer's orders
        public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(int userId)
        {
            var query = @"
            SELECT o.*, oi.*, p.*
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
            LEFT JOIN Products p ON oi.ProductId = p.ProductId
            WHERE o.UserId = @UserId
        ";

            var orderDictionary = new Dictionary<int, Order>();

            await _connection.QueryAsync<Order, OrderItem, Product, Order>(
                query,
                (order, orderItem, product) =>
                {
                    if (!orderDictionary.TryGetValue(order.OrderId, out var currentOrder))
                    {
                        currentOrder = order;
                        currentOrder.OrderItems = new List<OrderItem>();
                        orderDictionary.Add(currentOrder.OrderId, currentOrder);
                    }

                    if (orderItem != null)
                    {
                        orderItem.Product = product;
                        ((List<OrderItem>)currentOrder.OrderItems).Add(orderItem);
                    }

                    return currentOrder;
                },
                new { UserId = userId },
                splitOn: "OrderItemId,ProductId"
            );

            return orderDictionary.Values;
        }

        // Dapper query to fetch a product by ID
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            var query = "SELECT * FROM Products WHERE ProductId = @ProductId";
            return await _connection.QueryFirstOrDefaultAsync<Product>(query, new { ProductId = productId });
        }
    }
}
