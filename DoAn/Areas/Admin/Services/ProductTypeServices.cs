using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Services
{
    public class ProductTypeServices
    {
        private readonly DlctContext _dbContext;

        public ProductTypeServices(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<object>> GetAllProductTypes()
        {
            var productTypes = await _dbContext.Producttypes.ToListAsync();
            return productTypes.Select(p => new
            {
                p.ProductTypeId,
                p.Name
            }).Cast<object>().ToList();
        }

        public async Task<IActionResult> CreateProductType(Producttype productType)
        {
            try
            {
                _dbContext.Producttypes.Add(productType);
                await _dbContext.SaveChangesAsync();

                var CreatedproductType = await _dbContext.Producttypes
                    .FirstOrDefaultAsync(p => p.ProductTypeId == productType.ProductTypeId);

                if (CreatedproductType != null)
                {
                    var result = new
                    {
                        CreatedproductType.ProductTypeId,
                        CreatedproductType.Name,
                    };
                    return new OkObjectResult(result);
                }
                else
                {
                    return new NotFoundResult();
                }

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating product: {ex.Message}");
                return new StatusCodeResult(500);
            }


        }

        public async Task<IActionResult> UpdateProductType(int productTypeId, Producttype producttype)
        {

            var productTypeUpdate = await _dbContext.Producttypes
                .FirstOrDefaultAsync(x => x.ProductTypeId == productTypeId);

            if (productTypeUpdate == null)
            {
                return new NotFoundObjectResult("Not found Product");
            }

            if (!string.IsNullOrWhiteSpace(producttype.Name))
            {
                productTypeUpdate.Name = producttype.Name;
            }

            _dbContext.Entry(productTypeUpdate).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "Product type updated successfully",
                Name = producttype.Name
            };

            return new OkObjectResult(updateSuccessResponse);
        }

        public async Task<IActionResult> DeleteAllProductTypeAsync(int productTypeId)
        {
            var ProducttypesToDelete = await _dbContext.Producttypes.FindAsync(productTypeId);

            if (ProducttypesToDelete == null)
            {
                return new NotFoundObjectResult("Not found Producttypes");
            }

            _dbContext.Producttypes.Remove(ProducttypesToDelete);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "Producttypes deleted successfully"
            };

            return new OkObjectResult(deleteSuccessResponse);
        }
    }
}
