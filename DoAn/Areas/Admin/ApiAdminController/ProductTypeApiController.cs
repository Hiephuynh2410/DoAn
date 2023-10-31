using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.ApiAdminController
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductTypeApiController : Controller
    {
        private readonly DlctContext _dbContext;

        public ProductTypeApiController(DlctContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductType()
        {
            var productType = await _dbContext.Producttypes
                .ToListAsync();

            var productTypesWithFullInfo = productType.Select(s => new
            {
                s.ProductTypeId,
                s.Name,
            }).ToList();

            return Ok(productTypesWithFullInfo);
        }

        [HttpGet("search")]
        public async Task<IActionResult> searchProductType(string keyword)
        {
            var productTypes = await _dbContext.Producttypes
                .Where(p => p.Name.Contains(keyword) || p.ProductTypeId.ToString() == keyword)
                .ToListAsync();
            var productTypesWithFullInfo = productTypes.Select(s => new
            {
                s.ProductTypeId,
                s.Name,
            }).ToList();
            return Ok(productTypesWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProductType(Producttype createModel)
        {
            if (ModelState.IsValid)
            {
                var ProductTypeExists = await _dbContext.Producttypes.AnyAsync(b => b.Name == createModel.Name);
                if (ProductTypeExists)
                {
                    return BadRequest(new { Message = "Producttype already exists." });
                }

                var newProductType = new Producttype
                {
                    Name = createModel.Name,
                };

                _dbContext.Producttypes.Add(newProductType);
                await _dbContext.SaveChangesAsync();

                var registrationSuccessResponse = new
                {
                    Message = "Producttype registration successful",
                    ProductTypeId = newProductType.ProductTypeId
                };
                return Ok(registrationSuccessResponse);
            }

            var invalidDataErrorResponse = new
            {
                Message = "Invalid ProductType data",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            };
            return BadRequest(invalidDataErrorResponse);
        }

        [HttpPut("update/{ProductTypeId}")]
        public async Task<IActionResult> UpdateProductTypeId(int ProductTypeId, Producttype updateModel)
        {
            var ProductType = await _dbContext.Producttypes.FindAsync(ProductTypeId);
            if (ProductType == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(updateModel.Name))
            {
                ProductType.Name = updateModel.Name;
            }
            _dbContext.Entry(ProductType).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var updateSuccessResponse = new
            {
                Message = "ProductType updated successfully"
            };

            return Ok(updateSuccessResponse);
        }

        [HttpDelete("delete/{ProductTypeId}")]
        public async Task<IActionResult> DeleteProductType(int ProductTypeId)
        {
            var ProductType = await _dbContext.Producttypes.FindAsync(ProductTypeId);
            if (ProductType == null)
            {
                return NotFound();
            }

            _dbContext.Producttypes.Remove(ProductType);
            await _dbContext.SaveChangesAsync();

            var deleteSuccessResponse = new
            {
                Message = "ProductTypeId deleted successfully"
            };

            return Ok(deleteSuccessResponse);
        }

        [HttpGet("detail/{ProductTypeId}")]
        public async Task<IActionResult> GetProductTypeDetail(int ProductTypeId)
        {
            var ProductType = await _dbContext.Producttypes.FindAsync(ProductTypeId);

            if (ProductType == null)
            {
                return NotFound();
            }
            var ProductTypeDetail = new
            {
                ProductType.ProductTypeId,
                ProductType.Name,
            };
            return Json(ProductTypeDetail);
        }
    }
}
