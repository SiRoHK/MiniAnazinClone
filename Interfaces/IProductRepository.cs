using MiniAmazonClone.Models;

namespace MiniAmazonClone.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int productId);
        Task<int> AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int productId);
    }

}
