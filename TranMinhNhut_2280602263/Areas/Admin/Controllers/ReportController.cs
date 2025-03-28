using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TranMinhNhut_2280602263.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách báo cáo theo tháng
            var monthlyReports = await _context.Orders
                .Where(o => o.OrderDate.Year == currentYear)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new MonthlyReport
                {
                    Month = g.Key,
                    TotalOrders = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalPrice),
                    AverageOrder = g.Count() > 0 ? g.Sum(o => o.TotalPrice) / g.Count() : 0
                })
                .OrderBy(r => r.Month)
                .ToListAsync();

            // Tính tổng doanh thu và tổng số đơn hàng
            var totalRevenue = monthlyReports.Sum(r => r.TotalRevenue);
            var totalOrders = monthlyReports.Sum(r => r.TotalOrders);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Gán giá trị vào ViewBag để hiển thị trên View
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.AverageOrder = averageOrderValue;
            ViewBag.MonthlyReports = monthlyReports;

            return View();
        }
    }

    public class MonthlyReport
    {
        public int Month { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrder { get; set; }
        public decimal? ChangeFromPreviousMonth { get; set; }
    }
}
