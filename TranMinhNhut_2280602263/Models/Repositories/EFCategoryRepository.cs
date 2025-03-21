using Microsoft.EntityFrameworkCore;
using TranMinhNhut_2280602263.Models;
using TranMinhNhut_2280602263.Repositories;

public class EFCategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public EFCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Sửa lại phương thức GetAllAsync để include Products
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)  // Thêm dòng này để load Products
            .ToListAsync();
    }

    // Sửa lại phương thức GetByIdAsync để include Products
    public async Task<Category> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)  // Thêm dòng này để load Products
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Các phương thức khác giữ nguyên
    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }
}
