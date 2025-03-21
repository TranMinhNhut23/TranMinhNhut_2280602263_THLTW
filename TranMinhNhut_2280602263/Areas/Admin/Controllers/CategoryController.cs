using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TranMinhNhut_2280602263.Models; // Đảm bảo namespace đúng
using TranMinhNhut_2280602263.Repositories; // Nếu có thư mục chứa repository
namespace TranMinhNhut_2280602263.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền truy cập
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository; // Khai báo biến

        // Inject Repository thông qua Constructor
        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_LayoutAdmin";
            }
            else
            {
                ViewData["Layout"] = "_LayoutUser";
            }
            return View();
        }
    }
}
