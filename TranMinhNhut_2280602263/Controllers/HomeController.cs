using Microsoft.AspNetCore.Mvc;
using TranMinhNhut_2280602263.Repositories;

namespace TranMinhNhut_2280602263.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
    }
}
