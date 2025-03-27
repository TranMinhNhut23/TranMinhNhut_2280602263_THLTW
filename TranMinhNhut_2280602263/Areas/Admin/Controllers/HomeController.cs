using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranMinhNhut_2280602263.Models;
using TranMinhNhut_2280602263.Repositories;

namespace TranMinhNhut_2280602263.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;

        public HomeController(ApplicationDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy thống kê tổng quan
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice);

            // Lấy đơn hàng mới nhất
            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Lấy sản phẩm bán chạy
            ViewBag.TopProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Quantity)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
} 