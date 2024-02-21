using DoAn.Areas.Admin.Services;
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
        private readonly ProductTypeServices _productTypeServices;
        public ProductTypeApiController(DlctContext dbContext, ProductTypeServices productTypeServices)
        {
            _dbContext = dbContext;
            _productTypeServices = productTypeServices;
        }
        [HttpGet] 
        public async Task<IActionResult> GetAllProductType()
        {
            var productTypeWithFullInfo = await _productTypeServices.GetAllProductTypes();
            return Ok(productTypeWithFullInfo);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProductsType(Producttype registrationModel)
        {

            var result = await _productTypeServices.CreateProductType(registrationModel);

            if (result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            else if (result is BadRequestObjectResult badRequestObjectResult)
            {
                return BadRequest(badRequestObjectResult.Value);
            }
            return StatusCode(500, "Internal Server Error");
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

        [HttpPut("update/{productTypeId}")]
        public async Task<IActionResult> UpdateProductTypesAsync(int productTypeId, Producttype producttype)
        {

            var result = await _productTypeServices.UpdateProductType(productTypeId, producttype);

            if (result is OkObjectResult okResult)
            {

                return Ok(okResult.Value);

            }
            else if (result is NotFoundObjectResult notFoundResult)
            {

                return NotFound(notFoundResult.Value);

            }
            else
            {

                return StatusCode(500, "Internal Server Error");

            }
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteProductTypessAsync([FromBody] List<int> productTypeId)
        {
            try
            {
                foreach (var ProductTypeId in productTypeId)
                {
                    var result = await _productTypeServices.DeleteAllProductTypeAsync(ProductTypeId);
                }

                var deleteSuccessResponse = new
                {
                    Message = "ProductType deleted successfully"
                };

                return new OkObjectResult(deleteSuccessResponse);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.Error.WriteLine($"Error deleting ProductType: {ex.Message}");
                return new StatusCodeResult(500);
            }
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
