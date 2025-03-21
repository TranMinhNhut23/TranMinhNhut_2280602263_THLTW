using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranMinhNhut_2280602263.Extensions;
using TranMinhNhut_2280602263.Models;
using TranMinhNhut_2280602263.Repositories;

[Authorize(Roles = "Customer")]
public class OrderController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IProductRepository _productRepository;

    public OrderController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IProductRepository productRepository)
    {
        _userManager = userManager;
        _context = context;
        _productRepository = productRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart == null || !cart.Items.Any())
        {
            TempData["Error"] = "Giỏ hàng của bạn đang trống!";
            return RedirectToAction("Index", "ShoppingCart");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = new Order
        {
            UserId = user.Id,
            ApplicationUser = user,
            OrderDetails = new List<OrderDetail>(),
            ShippingAddress = user.Address ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            OrderDate = DateTime.Now
        };

        ViewBag.Cart = cart; // Add this line to pass cart to view
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([Bind("ShippingAddress,PhoneNumber,Notes")] Order order)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "ShoppingCart");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                return View(order);
            }

            // Validate quantities and lock products
            foreach (var item in cart.Items)
            {
                var product = await _context.Products
                    .FromSqlRaw("SELECT * FROM Products WITH (UPDLOCK) WHERE Id = {0}", item.ProductId)
                    .FirstOrDefaultAsync();

                if (product == null || item.Quantity > product.Quantity)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Sản phẩm {item.Name} không đủ số lượng trong kho.";
                    return RedirectToAction("Index", "ShoppingCart");
                }
            }

            order.UserId = user.Id;
            order.ApplicationUser = user;
            order.OrderDate = DateTime.Now;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = new List<OrderDetail>();

            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Product = product,
                        Order = order
                    });

                    // Update product quantity
                    product.Quantity -= item.Quantity;
                    _context.Products.Update(product);
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            HttpContext.Session.Remove("Cart");
            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("OrderCompleted", new { orderId = order.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
            return RedirectToAction("Index", "ShoppingCart");
        }
    }

    public IActionResult OrderCompleted(int orderId)
    {
        return View(orderId);
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.UserId == user.Id)
            .ToListAsync();

        return View(orders);
    }
}
