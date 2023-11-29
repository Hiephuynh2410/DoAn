using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.ApiController
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientSearchProduct : Controller
    {
        private readonly DlctContext _dbContext;

        public ClientSearchProduct(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("search")]
        public async Task<ActionResult> SearchProduct(string keyword)
        {
            var products = await _dbContext.Products
                .Include(p => p.ProductType)
                .Include(p => p.Provider)
                .Where(p => p.Name.Contains(keyword) || p.ProductId.ToString() == keyword)
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound("No products found for the given keyword.");
            }

            var productWithFullInfo = products.Select(p => new
            {
                p.ProductId,
                p.Name,
                p.Description,
                p.Price,
                p.Quantity,
                p.Image,
                p.ProductTypeId,
                p.ProviderId,
                p.CreatedAt,
                p.UpdatedAt,
                p.CreatedBy,
                p.UpdatedBy,
                ProductType = new
                {
                    Name = p.ProductType?.Name
                },
                Provider = new
                {
                    p.Provider?.Name,
                    p.Provider?.Address,
                    p.Provider?.Email,
                    p.Provider?.Phone
                }
            }).ToList();
            return Ok(productWithFullInfo);
        }

        [HttpGet("filterProduct")]
        public async Task<ActionResult> FilterProductsByProductType(int productTypeId)
        {
            var products = await _dbContext.Products
                .Include(p => p.ProductType)
                .Include(p => p.Provider)
                .Where(p => p.ProductTypeId == productTypeId)
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound($"No products found for ProductType with ID {productTypeId}.");
            }

            var productWithFullInfo = products.Select(p => new
            {
                p.ProductId,
                p.Name,
                p.Description,
                p.Price,
                p.Quantity,
                p.Image,
                p.ProductTypeId,
                p.ProviderId,
                p.CreatedAt,
                p.UpdatedAt,
                p.CreatedBy,
                p.UpdatedBy,
                ProductType = new
                {
                    Name = p.ProductType?.Name
                },
                Provider = new
                {
                    p.Provider?.Name,
                    p.Provider?.Address,
                    p.Provider?.Email,
                    p.Provider?.Phone
                }
            }).ToList();

            return Ok(productWithFullInfo);
        }
    }
}
