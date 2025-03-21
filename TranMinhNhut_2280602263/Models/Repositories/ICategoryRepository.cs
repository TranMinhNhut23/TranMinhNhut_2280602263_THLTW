using TranMinhNhut_2280602263.Models;

namespace TranMinhNhut_2280602263.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);

        Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId);
        Task DeleteAsync(int id);
    }

}
