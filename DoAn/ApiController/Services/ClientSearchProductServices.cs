using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.ApiController.Services
{
    public class ClientSearchProductServices
    {
        private readonly DlctContext _dbContext;

        public ClientSearchProductServices(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<object>> SearchProduct(string keyword)
        {
            var products = await _dbContext.Products
                .Include(p => p.ProductType)
                .Include(p => p.Provider)
                .Where(p => p.Name.Contains(keyword) || p.ProductId.ToString() == keyword)
                .ToListAsync();
            if (products == null || products.Count == 0)
            {
                return new List<object> { new { Message = "No products found for the given keyword." } };
            }
            return products.Select(p => new
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

            }).Cast<object>().ToList();
        }

        public async Task<ActionResult> FilterProductsByProductType(int productTypeId)
        {
            var productWithFullInfo = await _dbContext.Products
                .Include(p => p.ProductType)
                .Include(p => p.Provider)
                .Where(p => p.ProductTypeId == productTypeId)
                .ToListAsync();

            if (productWithFullInfo == null || productWithFullInfo.Count == 0)
            {
                return new NotFoundObjectResult($"No products found for ProductType with ID {productTypeId}.");
            }
            var response = productWithFullInfo.Select(p => new
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
            }).Cast<object>().ToList();

            return new OkObjectResult(response);
        }
    }
}
