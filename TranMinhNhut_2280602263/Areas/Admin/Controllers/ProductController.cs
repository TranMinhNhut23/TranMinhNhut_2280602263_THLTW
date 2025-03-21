using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using TranMinhNhut_2280602263.Models;
using TranMinhNhut_2280602263.Repositories;

namespace TranMinhNhut_2280602263.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")] // Cho phép cả Admin và Employee truy cập
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(string searchQuery, string sortOrder)
        {
            // Set layout dựa trên role
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            else if (User.IsInRole("Employee"))
            {
                ViewData["Layout"] = "_EmployeeLayout";
            }

            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products
                    .Where(p => p.Name?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ?? false)
                    .ToList();
            }

            products = sortOrder switch
            {
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                "name_asc" => products.OrderBy(p => p.Name).ToList(),
                "name_desc" => products.OrderByDescending(p => p.Name).ToList(),
                _ => products.OrderBy(p => p.Price).ToList()
            };

            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortOrder = sortOrder;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                await _productRepository.AddAsync(product);
                TempData["Success"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            }
            else if (User.IsInRole("Employee"))
            {
                ViewData["Layout"] = "_EmployeeLayout";
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (User.IsInRole("Employee"))
                    {
                        // Employee chỉ có thể cập nhật một số trường
                        var existingProduct = await _productRepository.GetByIdAsync(id);
                        existingProduct.Name = product.Name;
                        existingProduct.Price = product.Price;
                        existingProduct.Description = product.Description;
                        await _productRepository.UpdateAsync(existingProduct);
                    }
                    else if (User.IsInRole("Admin"))
                    {
                        // Admin có thể cập nhật tất cả
                        await _productRepository.UpdateAsync(product);
                    }

                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm.");
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới có quyền xóa
        public async Task<IActionResult> Delete(int id)
        {
            await _productRepository.DeleteAsync(id);
            TempData["Success"] = "Sản phẩm đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
