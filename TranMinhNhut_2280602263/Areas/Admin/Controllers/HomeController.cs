using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ApplicationDbContext context,
            IProductRepository productRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _productRepository = productRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Thống kê cơ bản
                ViewBag.TotalProducts = await _context.Products.CountAsync();
                ViewBag.TotalOrders = await _context.Orders.CountAsync();
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice);

                // Lấy danh sách đơn hàng gần đây (chỉ lấy các trường cần thiết)
                ViewBag.RecentOrders = await _context.Orders
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderDate,
                        o.TotalPrice,
                        o.PhoneNumber,
                        ApplicationUser = new
                        {
                            o.ApplicationUser.FullName
                        }
                    })
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

                // Lấy danh sách sản phẩm bán chạy (chỉ lấy các trường cần thiết)
                ViewBag.TopProducts = await _context.Products
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Quantity,
                        p.ImageUrl,
                        Category = new
                        {
                            p.Category.Name
                        }
                    })
                    .OrderByDescending(p => p.Quantity)
                    .Take(5)
                    .ToListAsync();

                // Lấy danh sách người dùng và vai trò
                var users = await _userManager.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.PhoneNumber
                    })
                    .ToListAsync();
                ViewBag.Users = users;

                var userRoles = new Dictionary<string, List<string>>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(user.Id));
                    userRoles[user.Id] = roles.ToList();
                }

                ViewBag.UserRoles = userRoles;

                return View();
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về view với thông báo lỗi
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải dữ liệu. Vui lòng thử lại sau.";
                return View();
            }
        }
    }
}
