using TranMinhNhut_2280602263.Models;

namespace TranMinhNhut_2280602263.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<IEnumerable<Product>> GetAvailableProductsAsync(); // New method
    }


}
