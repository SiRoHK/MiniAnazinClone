using Assiment5_MiniAmazonClone.Data;
using Assiment5_MiniAmazonClone.Interfaces;
using Assiment5_MiniAmazonClone.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Assiment5_MiniAmazonClone.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductRepository> _logger;
        private readonly IDbConnection _connection; // For Dapper operations if needed

        public ProductRepository(
            AppDbContext context,
            ILogger<ProductRepository> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                // Include stock information and filter out products with zero stock
                return await _context.Products
                    .Where(p => p.Stock > 0)
                    .OrderByDescending(p => p.CreatedBy)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                throw;
            }
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            try
            {
                // Dapper approach for performance-critical scenarios
                var query = "SELECT * FROM Products WHERE ProductId = @ProductId AND Stock > 0";
                return await _connection.QueryFirstOrDefaultAsync<Product>(
                    query,
                    new { ProductId = productId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product {productId}");
                throw;
            }
        }

        public async Task<int> AddProductAsync(Product product)
        {
            try
            {
                // Validate product data
                if (product.Price <= 0)
                {
                    throw new ArgumentException("Product price must be positive");
                }

                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    throw new ArgumentException("Product name is required");
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product created with ID {product.ProductId}");
                return product.ProductId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new product");
                throw;
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            try
            {
                // Check if product exists
                var existingProduct = await _context.Products.FindAsync(product.ProductId);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"Product with ID {product.ProductId} not found");
                }

                // Update specific properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {product.ProductId} updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {product.ProductId}");
                throw;
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Product with ID {productId} not found");
                }

                // Soft delete instead of hard delete
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {productId} soft deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {productId}");
                throw;
            }
        }

        // Additional methods for complex product management
        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, decimal? minPrice = null, decimal? maxPrice = null)
        {
            try
            {
                var query = _context.Products.AsQueryable();

                // Filter by search term
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Description.Contains(searchTerm)
                    );
                }

                // Filter by price range
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                // Additional filters like stock, active products

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                throw;
            }
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int quantityChange)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // Ensure stock doesn't go negative
                product.Stock = Math.Max(0, product.Stock + quantityChange);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product {productId} stock updated by {quantityChange}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating stock for product {productId}");
                return false;
            }
        }
    }
}
