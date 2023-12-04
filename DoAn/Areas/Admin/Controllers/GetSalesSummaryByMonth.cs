using System;
using System.Linq;
using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GetSalesSummaryByMonth : Controller
    {
        private readonly DlctContext _dbContext;

        public GetSalesSummaryByMonth(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetSale()
        {
            var salesData = await _dbContext.Bills
                .Include(b => b.Billdetails)
                    .ThenInclude(d => d.Product)
                .Where(b => b.CreatedAt.HasValue)
                .ToListAsync();

            var bestSellingProducts = salesData
                .SelectMany(b => b.Billdetails)
                .GroupBy(d => d.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    ProductName = group.First().Product?.Name ?? "Unknown",
                    TotalQuantity = group.Sum(d => d.Quantity ?? 0)
                })
                .OrderByDescending(entry => entry.TotalQuantity)
                .ToList();

            foreach (var product in bestSellingProducts)
            {
                Console.WriteLine($"Product ID: {product.ProductId}, Product Name: {product.ProductName}");
            }

            ViewBag.BestSellingProducts = bestSellingProducts;

            return View();
        }

    }
}
