using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranMinhNhut_2280602263.Extensions;
using TranMinhNhut_2280602263.Models;
using TranMinhNhut_2280602263.Repositories;

[Authorize(Roles = "Customer")]
public class ShoppingCartController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    public ShoppingCartController(IProductRepository productRepository,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _productRepository = productRepository;
        _userManager = userManager;
        _context = context;
    }
    [HttpGet] // Change to HttpGet since we're using a link
    public async Task<IActionResult> AddToCart(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        cart.AddItem(new CartItem
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = quantity,
            ImageUrl = product.ImageUrl  // Add this line
        });

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        TempData["Success"] = "Sản phẩm đã được thêm vào giỏ hàng!";
        return Redirect(Request.Headers["Referer"].ToString() ?? "/");
    }

    public IActionResult Index()
    {
        var cart =
       HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new
       ShoppingCart();
        return View(cart);
    }
    // Các actions khác...
    private async Task<Product> GetProductFromDatabase(int productId)
    {
        // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
        var product = await _productRepository.GetByIdAsync(productId);
        return product;
    }
    [HttpGet]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart != null)
        {
            cart.RemoveItem(productId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["Success"] = "Sản phẩm đã được xóa khỏi giỏ hàng!";
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult GetCartCount()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        var count = cart?.Items.Sum(i => i.Quantity) ?? 0;
        return Json(count);
    }
    [HttpPost]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart != null)
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
        }
        return Json(new { success = true });
    }
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart == null || !cart.Items.Any())
        {
            TempData["Error"] = "Giỏ hàng của bạn đang trống!";
            return RedirectToAction("Index");
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
            OrderDate = DateTime.Now // Add this line to initialize OrderDate
        };

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([Bind("ShippingAddress,PhoneNumber,Notes")] Order order)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
        if (cart == null || !cart.Items.Any())
        {
            TempData["Error"] = "Giỏ hàng của bạn đang trống!";
            return RedirectToAction("Index");
        }

        // Initialize required properties before validation
        order.UserId = user.Id;
        order.ApplicationUser = user;
        order.OrderDate = DateTime.Now;
        order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

        if (!ModelState.IsValid)
        {
            return View(order);
        }

        order.OrderDetails = cart.Items.Select(item => new OrderDetail
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            Price = item.Price,
            Product = new Product { Id = item.ProductId },
            Order = order
        }).ToList();

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("Cart");
        TempData["Success"] = "Đặt hàng thành công!";
        return RedirectToAction("OrderCompleted", new { orderId = order.Id });
    }

    public IActionResult OrderCompleted(int orderId)
    {
        return View(orderId);
    }
    [HttpPost]
    public IActionResult ClearCart()
    {
        HttpContext.Session.Remove("Cart");
        TempData["Success"] = "Giỏ hàng đã được xóa!";
        return RedirectToAction("Index");
    }

}