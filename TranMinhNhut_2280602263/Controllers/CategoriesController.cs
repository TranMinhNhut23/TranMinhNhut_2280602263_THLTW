using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TranMinhNhut_2280602263.Models;
using TranMinhNhut_2280602263.Repositories;

namespace _2280600715_HoangAnhDuc_LTW_BUOI3.Controllers
{
    [Authorize(Roles = "Admin")] // Yêu cầu quyền Admin cho toàn bộ controller
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [AllowAnonymous] // Cho phép mọi người xem danh sách danh mục
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                TempData["SuccessMessage"] = "Category added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);

                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction(nameof(Index));
                }

                var productsInCategory = await _categoryRepository.GetProductsByCategoryIdAsync(id);
                if (productsInCategory.Any())
                {
                    TempData["ErrorMessage"] = "Đang có trong danh sách sản phẩm không thể xóa";
                    return RedirectToAction(nameof(Index));
                }

                await _categoryRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
                Console.WriteLine($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous] // Cho phép mọi người xem chi tiết danh mục
        public async Task<IActionResult> Display(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
    }
}
