using MiniAmazonClone.Dtos;
using MiniAmazonClone.Interfaces;
using MiniAmazonClone.Models;
using MiniAmazonClone.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MiniAmazonClone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderController(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        [HttpGet("GetUserOrder")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpGet("all")]
        [Authorize(Policy = "CanViewOrders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Only allow users to see their own orders unless they're admins
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return Ok(order);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder(OrderCreateModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Validate order items
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in model.Items)
            {
                var product = await _productRepository.GetProductByIdAsync(item.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product with ID {item.ProductId} not found");
                }

                if (product.Stock < item.Quantity)
                {
                    return BadRequest($"Not enough stock for product {product.Name}");
                }

                // Update product stock
                product.Stock -= item.Quantity;
                await _productRepository.UpdateProductAsync(product);

                // Add order item
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                };
                orderItems.Add(orderItem);

                totalAmount += orderItem.Price * orderItem.Quantity;
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderItems = orderItems
            };

            var orderId = await _orderRepository.AddOrderAsync(order);
            return CreatedAtAction(nameof(GetOrder), new { id = orderId }, order);
        }

 
    }
}
